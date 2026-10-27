using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Kdock.Wire;

namespace Kdock.App;

public enum ConversationState
{
    Disconnected,
    Connecting,
    Connected,
    ProtocolMismatch,
    Error
}

public enum ConversationMessageKind
{
    Text,
    Attachment,
    System,
    Request
}

public sealed class ConversationMessage : System.ComponentModel.INotifyPropertyChanged
{
    private long _fileProgress;
    private string _savedPath = string.Empty;

    public DateTime TimestampUtc { get; init; } = DateTime.UtcNow;
    public string Sender { get; init; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public ConversationMessageKind Kind { get; init; }
    public string FileId { get; init; } = string.Empty;
    public string FileName { get; init; } = string.Empty;
    public long FileLength { get; init; }
    public long FileProgress
    {
        get => _fileProgress;
        set
        {
            if (_fileProgress == value)
                return;
            _fileProgress = value;
            Notify(nameof(FileProgress));
            Notify(nameof(IsSending));
            Notify(nameof(IsComplete));
            Notify(nameof(CanOpen));
            Notify(nameof(ProgressText));
        }
    }
    public string SavedPath
    {
        get => _savedPath;
        set
        {
            if (_savedPath == value)
                return;
            _savedPath = value;
            Notify(nameof(SavedPath));
            Notify(nameof(CanOpen));
        }
    }

    public bool IsMine => Sender == "You";
    public string TimeText => TimestampUtc.ToLocalTime().ToString("t");
    public bool IsAttachment => Kind == ConversationMessageKind.Attachment;
    public bool ShowText => !IsAttachment && !string.IsNullOrWhiteSpace(Text);
    public bool IsSending => IsAttachment && FileLength > 0 && FileProgress < FileLength;
    public bool IsComplete => IsAttachment && !IsSending;
    public bool CanOpen => IsComplete && File.Exists(SavedPath);
    public long ProgressMax => Math.Max(1, FileLength);
    public System.Windows.Media.ImageSource? FileIcon => IsAttachment ? ShellIconSource.ForName(FileName) : null;
    public string ProgressText => IsAttachment
        ? IsSending
            ? $"{(FileLength <= 0 ? 0 : FileProgress * 100 / FileLength)}%  ·  {FormatSize(FileProgress)} of {FormatSize(FileLength)}"
            : FormatSize(FileLength)
        : string.Empty;

    public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
    private void Notify(string name) => PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(name));

    private static string FormatSize(long bytes)
    {
        if (bytes < 1024)
            return bytes + " B";
        if (bytes < 1024 * 1024)
            return $"{bytes / 1024.0:0.0} KB";
        if (bytes < 1024L * 1024 * 1024)
            return $"{bytes / 1024.0 / 1024.0:0.0} MB";
        return $"{bytes / 1024.0 / 1024.0 / 1024.0:0.00} GB";
    }
}

public sealed class ConversationViewModel : IAsyncDisposable
{
    private readonly SynchronizationContext _uiContext;
    private readonly string _incomingDirectory;
    private readonly string _transcriptDirectory;
    private readonly InboxStore _inbox;
    private readonly ConcurrentDictionary<string, TaskCompletionSource<long>> _inboxResume = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, byte> _inboxSending = new(StringComparer.OrdinalIgnoreCase);
    private readonly object _historyGate = new();
    private System.Windows.Threading.DispatcherTimer? _saveTimer;
    private readonly Dictionary<string, IncomingFile> _incoming = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, IncomingFile> _uploads = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, ObservableCollection<ConversationMessage>> _transcripts = new(StringComparer.OrdinalIgnoreCase);
    private string _transcriptKey = string.Empty;
    private string _sessionKey = string.Empty;
    private readonly HashSet<string> _stopped = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _browseTransfers = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _clipTransfers = new(StringComparer.OrdinalIgnoreCase);
    private readonly object _clipGate = new();
    private readonly List<string> _explorerPaths = new();
    private readonly List<ClipOffer> _explorerOffers = new();
    private readonly List<ClipOffer> _remoteOffers = new();
    private readonly List<string> _hostClipFiles = new();
    private string? _downloadDirectory;
    private readonly List<PasteJob> _pastes = new();
    private readonly Channel<(WirePacket Packet, IWireSession Source)> _packetQueue =
        Channel.CreateUnbounded<(WirePacket, IWireSession)>(new UnboundedChannelOptions { SingleReader = true });
    private bool _explorerOffer;
    private string _clipSignature = string.Empty;
    private int _clipUsers;
    private System.Windows.Threading.DispatcherTimer? _clipTimer;
    private IWireSession? _session;
    private IWireSession? _peer;
    private ScreenShareService? _screenShare;
    private ScreenShareService? _peerShare;
    private int _expectSelfPeer;
    private int _wantLive;
    private int _restoring;
    private readonly SemaphoreSlim _liveGate = new(1, 1);
    private readonly string? _boundKey;

    public ConversationViewModel(string dataDirectory)
        : this(dataDirectory, new InboxStore(dataDirectory), boundKey: null)
    {
    }

    public ConversationViewModel(string dataDirectory, InboxStore inbox, string? boundKey)
    {
        _uiContext = SynchronizationContext.Current ?? new SynchronizationContext();
        _incomingDirectory = Path.Combine(dataDirectory, "Incoming");
        _transcriptDirectory = Path.Combine(dataDirectory, "Chats");
        _inbox = inbox;
        _boundKey = string.IsNullOrWhiteSpace(boundKey) ? null : CanonicalKey(boundKey);
        Directory.CreateDirectory(_incomingDirectory);
        Directory.CreateDirectory(_transcriptDirectory);
        Directory.CreateDirectory(InboxFolder);
        if (_boundKey != null)
        {
            LoadAll();
            _transcriptKey = _boundKey;
        }
        Transcript = string.IsNullOrWhiteSpace(_transcriptKey) ? new ObservableCollection<ConversationMessage>() : Bucket(_transcriptKey);
        State = ConversationState.Disconnected;
        _ = DrainPacketsAsync();
        _saveTimer = new System.Windows.Threading.DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
        _saveTimer.Tick += (_, _) =>
        {
            _saveTimer.Stop();
            SaveAll();
        };
    }

    public ObservableCollection<ConversationMessage> Transcript { get; private set; }
    public string RemoteFolder { get; private set; } = string.Empty;
    public string TransferStatus { get; private set; } = string.Empty;
    public int RemoteWidth { get; private set; }
    public int RemoteHeight { get; private set; }
    public ObservableCollection<PermissionRequest> Requests { get; } = new();
    public ObservableCollection<RemoteFileEntry> RemoteFiles { get; } = new();
    public ConversationState State { get; private set; }
    public string RemoteDeskId { get; private set; } = string.Empty;
    public string RemoteAddress { get; private set; } = string.Empty;
    public string Status { get; private set; } = "Disconnected";
    public string PeerKey => _boundKey ?? CanonicalKey(_sessionKey.Length > 0 ? _sessionKey : _transcriptKey);
    private IWireSession? LiveSession =>
        _session is { IsConnected: true } ? _session
        : _peer is { IsConnected: true } ? _peer
        : _session ?? _peer;

    public bool IsLive => LiveSession is { IsConnected: true };
    public Func<CancellationToken, Task>? RestoreSession { get; set; }
    public InboxStore Inbox => _inbox;
    public event Action? Changed;
    public event Action<string>? ChatTouched;
    public string? LastIncomingKey { get; private set; }
    public IEnumerable<string> ChatKeys => _transcripts.Keys.ToList();
    public event Action<byte[], int, int>? RemoteFrameReceived;
    public event Action<byte[]>? RemoteDeltaReceived;
    public event Action<byte[], int, int>? RemoteH264Received;
    public event Action<byte[], int, int, int, int>? RemoteRegionReceived;
    public event Action<int, int, bool, int, int, byte[]?>? RemoteCursorReceived;
    public event Action<string>? ShowPanel;
    public event Action? ViewEnded;
    public event Action? FilesClosed;
    public event Action? AccessChanged;
    public event Action<PermissionRequest>? PermissionAsked;
    public bool IsHostingScreen { get; private set; }
    public bool IsHostingFiles { get; private set; }
    public bool PeerFilesOpen { get; private set; }
    public bool ClipboardAllowed { get; private set; } = true;
    private bool _peerClipboard = true;
    private int _viewing;

    public void Activate(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return;
        _transcriptKey = CanonicalKey(key);
        Transcript = Bucket(_transcriptKey);
        Post(NotifyChanged);
    }

    public void ExpectSelfPeer() => Interlocked.Exchange(ref _expectSelfPeer, 1);

    public void CancelSelfPeer() => Interlocked.Exchange(ref _expectSelfPeer, 0);

    public bool TakeSelfPeer() => Interlocked.Exchange(ref _expectSelfPeer, 0) == 1;

    public void AttachPeer(IWireSession session)
    {
        _peer = session;
        _peerShare = new ScreenShareService(session);
        session.PacketReceived += packet => DispatchPacket(packet, session);
        session.StatusChanged += _ => { };
    }

    public async Task ConnectDirectAsync(string address, CancellationToken cancellationToken = default)
    {
        ParseAddress(address, out string host, out int port);
        if (string.IsNullOrWhiteSpace(host))
            throw new ArgumentException("A LAN host or host:port address is required.", nameof(address));
        await DisconnectAsync().ConfigureAwait(false);
        UpdateState(ConversationState.Connecting, "Connecting...");
        try
        {
            SessionTransport session = await SessionTransport.ConnectAsync(host, port, cancellationToken).ConfigureAwait(false);
            Attach(session, address);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("Update Kdock", StringComparison.OrdinalIgnoreCase))
        {
            UpdateState(ConversationState.ProtocolMismatch, "Update Kdock on both PCs.");
            throw;
        }
        catch (Exception ex)
        {
            UpdateState(ConversationState.Error, ex.Message);
            throw;
        }
    }

    public async Task AcceptSessionAsync(IWireSession session, string address)
    {
        if (ReferenceEquals(_session, session) || ReferenceEquals(_peer, session))
            return;
        if (IsLive)
        {
            AttachPeer(session);
            RememberPeerDesk(session);
            return;
        }

        await DisconnectAsync().ConfigureAwait(false);
        Attach(session, address);
        RememberPeerDesk(session);
    }

    public async Task RequestPermissionAsync(string kind, CancellationToken cancellationToken = default)
    {
        if (kind == "files" && PeerFilesOpen)
        {
            Post(() => ShowPanel?.Invoke("files"));
            await BrowseRemoteAsync("", cancellationToken).ConfigureAwait(false);
            return;
        }
        if (Requests.Any(item => item.Kind == kind && item.IsPending && item.ReplySession == null))
            return;
        if (!await PrepareSendAsync(cancellationToken).ConfigureAwait(false))
            return;
        string id = Guid.NewGuid().ToString("N");
        string waiting = kind switch
        {
            "files" => "Asked to browse their files.",
            "watch" => "Asked to watch their screen.",
            "share" => "Asked to share your screen.",
            _ => "Asked for permission."
        };
        Post(() =>
        {
            Requests.Add(new PermissionRequest { Id = id, Kind = kind });
            Append(new ConversationMessage { Sender = "You", Text = waiting, Kind = ConversationMessageKind.System });
        });
        if (_session == null)
            return;
        await _session.SendAsync(WirePacket.Text($"{Protocol.Permit}:{id}:{kind}"), cancellationToken).ConfigureAwait(false);
    }

    public async Task AnswerPermissionAsync(PermissionRequest request, bool allow, CancellationToken cancellationToken = default)
    {
        if (!request.IsPending)
            return;
        if (!await PrepareSendAsync(cancellationToken).ConfigureAwait(false))
            return;
        request.Result = allow ? "Allowed" : "Declined";
        Post(() =>
        {
            NotifyChanged();
            AccessChanged?.Invoke();
        });
        IWireSession? reply = request.ReplySession ?? _session;
        if (reply == null)
            return;
        await reply.SendAsync(WirePacket.Text($"{Protocol.PermitAnswer}:{request.Id}:{(allow ? "allow" : "deny")}"), cancellationToken).ConfigureAwait(false);
        if (allow && request.Kind == "watch")
            await StartShareOnAsync(reply, cancellationToken).ConfigureAwait(false);
        if (allow && request.Kind == "share")
        {
            if (!IsLoopbackSession)
                await StopCaptureQuietAsync().ConfigureAwait(false);
            OpenViewer();
        }
        if (request.Kind == "files")
        {
            SetHostingFiles(allow);
            if (!allow && reply.IsConnected)
                await reply.SendAsync(WirePacket.Text("files:closed"), cancellationToken).ConfigureAwait(false);
        }
        if (!allow && request.Kind == "watch")
            await StopScreenShareAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task BrowseRemoteAsync(string path = "", CancellationToken cancellationToken = default)
    {
        if (!await PrepareSendAsync(cancellationToken).ConfigureAwait(false))
            return;
        RemoteFolder = path ?? string.Empty;
        IWireSession? session = LiveSession;
        if (session == null)
            return;
        await session.SendAsync(WirePacket.Text($"files:getdrives:{Environment.MachineName}", path ?? string.Empty), cancellationToken).ConfigureAwait(false);
    }

    private void HandlePermissionPacket(WirePacket packet, IWireSession source)
    {
        if (packet.Instruction.StartsWith($"{Protocol.Permit}:", StringComparison.OrdinalIgnoreCase))
        {
            string[] parts = packet.Instruction.Split(':');
            if (parts.Length >= 3)
            {
                string kind = parts[2];
                if (kind.Equals("files", StringComparison.OrdinalIgnoreCase) && IsHostingFiles)
                {
                    _ = source.SendAsync(WirePacket.Text($"{Protocol.PermitAnswer}:{parts[1]}:allow"));
                    return;
                }
                PermissionRequest request = new() { Id = parts[1], Kind = kind, ReplySession = source };
                Post(() =>
                {
                    Requests.Add(request);
                    Append(new ConversationMessage
                    {
                        Sender = RemoteAddress,
                        Text = request.Description + " — answer in the access window.",
                        Kind = ConversationMessageKind.System,
                        FileId = request.Id
                    });
                    NotifyChanged();
                    PermissionAsked?.Invoke(request);
                    AccessChanged?.Invoke();
                });
            }
            return;
        }
        if (packet.Instruction.StartsWith($"{Protocol.PermitAnswer}:", StringComparison.OrdinalIgnoreCase))
        {
            string[] parts = packet.Instruction.Split(':');
            if (parts.Length >= 3)
            {
                string id = parts[1];
                string result = parts[2].Equals("allow", StringComparison.OrdinalIgnoreCase) ? "Allowed" : "Declined";
                Post(() =>
                {
                    PermissionRequest? request = null;
                    foreach (PermissionRequest item in Requests.Where(entry => entry.Id == id && entry.IsPending))
                    {
                        item.Result = result;
                        request ??= item;
                    }
                    Append(new ConversationMessage { Sender = RemoteAddress, Text = result, Kind = ConversationMessageKind.System });
                    NotifyChanged();
                    if (request != null && result == "Allowed" && request.Kind == "share")
                        _ = StartScreenShareAsync();
                    if (request != null && result == "Allowed" && request.Kind == "files")
                    {
                        PeerFilesOpen = true;
                        ShowPanel?.Invoke("files");
                        _ = BrowseRemoteAsync();
                    }
                    if (request != null && result == "Declined" && request.Kind == "files")
                        PeerFilesOpen = false;
                    if (request != null && result == "Allowed" && request.Kind == "watch")
                    {
                        if (!IsLoopbackSession)
                            _ = StopCaptureQuietAsync();
                        OpenViewer();
                    }
                    AccessChanged?.Invoke();
                });
            }
        }
    }

    private static List<RemoteFileEntry> ParseRemoteFiles(string raw)
    {
        List<RemoteFileEntry> result = new();
        string[] sections = raw.Split((char)243);
        for (int section = 0; section < sections.Length; section++)
        {
            string[] entries = sections[section].Split((char)240);
            for (int index = 0; index < entries.Length - 1; index++)
                if (!string.IsNullOrWhiteSpace(entries[index])) result.Add(RemoteFileEntry.Parse(entries[index], section < 4));
        }
        result.Sort((left, right) => left.IsFolder != right.IsFolder ? (left.IsFolder ? -1 : 1) : string.Compare(left.Name, right.Name, StringComparison.OrdinalIgnoreCase));
        return result;
    }

    public async Task SendTextAsync(string text, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(text))
            return;
        if (!await PrepareSendAsync(cancellationToken).ConfigureAwait(false))
            return;
        if (_session == null)
            return;
        string normalized = text.Replace("\r\n", "\n");
        await _session.SendAsync(WirePacket.Text($"{Protocol.Message}:{Environment.MachineName}", normalized), cancellationToken).ConfigureAwait(false);
        Append(new ConversationMessage { Sender = "You", Text = normalized, Kind = ConversationMessageKind.Text });
    }

    public async Task SendAttachmentAsync(string path, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(path) || !await PrepareSendAsync(cancellationToken).ConfigureAwait(false))
            return;

        string id = Guid.NewGuid().ToString("N");
        string name = Path.GetFileName(path).Replace(":", "_");
        long length = new FileInfo(path).Length;
        ConversationMessage message = new()
        {
            Sender = "You",
            Kind = ConversationMessageKind.Attachment,
            FileId = id,
            FileName = name,
            FileLength = length,
            SavedPath = path
        };
        Append(message);
        if (_session == null)
            return;
        await _session.SendAsync(WirePacket.Text($"{Protocol.ChatFile}:{id}", $"{name}\t{length}"), cancellationToken).ConfigureAwait(false);

        byte[] buffer = new byte[64 * 1024];
        long sent = 0;
        using FileStream stream = new(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        int read;
        while ((read = await stream.ReadAsync(buffer.AsMemory(), cancellationToken).ConfigureAwait(false)) > 0)
        {
            byte[] chunk = buffer[..read].ToArray();
            await _session.SendAsync(WirePacket.Binary($"File:{id}:{name}:{length}", chunk), cancellationToken).ConfigureAwait(false);
            sent += read;
            long progress = sent;
            Post(() => message.FileProgress = progress);
        }

        if (length == 0)
            await _session.SendAsync(WirePacket.Binary($"File:{id}:{name}:{length}", Array.Empty<byte>()), cancellationToken).ConfigureAwait(false);
        Post(NotifyChanged);
    }

    public async Task DisconnectAsync(bool userInitiated = false)
    {
        if (userInitiated)
            Volatile.Write(ref _wantLive, 0);
        if (_screenShare != null)
        {
            await _screenShare.DisposeAsync().ConfigureAwait(false);
            _screenShare = null;
        }
        if (_peerShare != null)
        {
            await _peerShare.DisposeAsync().ConfigureAwait(false);
            _peerShare = null;
        }
        if (_peer != null)
        {
            IWireSession peer = _peer;
            _peer = null;
            await peer.DisposeAsync().ConfigureAwait(false);
        }
        if (_session != null)
        {
            IWireSession session = _session;
            _session = null;
            await session.DisposeAsync().ConfigureAwait(false);
        }
        UpdateState(ConversationState.Disconnected, "Disconnected");
    }

    private void Attach(IWireSession session, string address)
    {
        _session = session;
        _screenShare = new ScreenShareService(session);
        RemoteAddress = address;
        RemoteDeskId = CanonicalKey(address);
        Volatile.Write(ref _wantLive, 1);
        _sessionKey = CanonicalKey(address);
        if (string.IsNullOrWhiteSpace(_transcriptKey))
            _transcriptKey = _sessionKey;
        session.PacketReceived += packet => DispatchPacket(packet, session);
        session.StatusChanged += status => SessionStatusChanged(session, status);
        session.ProtocolMismatch += SessionProtocolMismatch;
        RememberPeerDesk(session);
        UpdateState(ConversationState.Connected, "Connected");
        _ = ResumeInboxJobsAsync();
    }

    private void RememberPeerDesk(IWireSession session)
    {
        string desk = CanonicalKey(session.PeerDeskId);
        if (desk.Length == 10)
            RemoteDeskId = desk;
    }

    public void RequestLive() => Volatile.Write(ref _wantLive, 1);

    private async Task<bool> PrepareSendAsync(CancellationToken cancellationToken)
    {
        if (IsLive)
            return true;
        await RestoreLiveAsync(cancellationToken).ConfigureAwait(false);
        if (IsLive)
            return true;
        UpdateState(ConversationState.Disconnected, "Disconnected. Press Reconnect.");
        return false;
    }

    private async Task RestoreLiveAsync(CancellationToken cancellationToken)
    {
        if (IsLive || Volatile.Read(ref _wantLive) == 0 || RestoreSession == null)
            return;
        await _liveGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (IsLive || RestoreSession == null)
                return;
            UpdateState(ConversationState.Connecting, "Reconnecting...");
            await RestoreSession(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            UpdateState(ConversationState.Disconnected, ex.Message);
        }
        finally
        {
            _liveGate.Release();
        }
    }

    private async Task AutoRestoreAsync()
    {
        if (Interlocked.Exchange(ref _restoring, 1) == 1)
            return;
        try
        {
            for (int attempt = 1; attempt <= 8 && Volatile.Read(ref _wantLive) == 1 && !IsLive; attempt++)
            {
                UpdateState(ConversationState.Connecting, "Reconnecting (" + attempt + "/8)...");
                await RestoreLiveAsync(CancellationToken.None).ConfigureAwait(false);
                if (IsLive)
                {
                    UpdateState(ConversationState.Connected, "Reconnected");
                    return;
                }
                await Task.Delay(1000 * (1 << Math.Min(attempt - 1, 3))).ConfigureAwait(false);
            }
            if (!IsLive)
                UpdateState(ConversationState.Disconnected, "Disconnected. Press Reconnect.");
        }
        finally
        {
            Interlocked.Exchange(ref _restoring, 0);
        }
    }

    private async Task StartShareOnAsync(IWireSession session, CancellationToken cancellationToken)
    {
        if (!IsLoopbackSession)
            EndViewing();
        if (session == _peer)
        {
            BeginClipboardSync();
            if (_peerShare != null)
                await _peerShare.StartAsync(cancellationToken).ConfigureAwait(false);
            SetHostingScreen(true);
            return;
        }
        await StartScreenShareAsync(cancellationToken).ConfigureAwait(false);
    }

    private void DispatchPacket(WirePacket packet, IWireSession source)
    {
        if (packet.Instruction.StartsWith("remote:frame:", StringComparison.OrdinalIgnoreCase)
            || packet.Instruction.StartsWith("remote:delta", StringComparison.OrdinalIgnoreCase)
            || packet.Instruction.StartsWith("remote:h264:", StringComparison.OrdinalIgnoreCase)
            || packet.Instruction.StartsWith("remote:region:", StringComparison.OrdinalIgnoreCase)
            || packet.Instruction.StartsWith("remote:cursor:", StringComparison.OrdinalIgnoreCase)
            || packet.Instruction.StartsWith("remote:mark:", StringComparison.OrdinalIgnoreCase)
            || packet.Instruction.StartsWith("remote:ack:", StringComparison.OrdinalIgnoreCase)
            || packet.Instruction.StartsWith("remote:input", StringComparison.OrdinalIgnoreCase))
        {
            OnPacket(packet, source);
            return;
        }
        _packetQueue.Writer.TryWrite((packet, source));
    }

    private async Task DrainPacketsAsync()
    {
        try
        {
            await foreach ((WirePacket packet, IWireSession source) in _packetQueue.Reader.ReadAllAsync().ConfigureAwait(false))
            {
                try { OnPacket(packet, source); }
                catch { }
            }
        }
        catch (ChannelClosedException)
        {
        }
    }

    private void OnPacket(WirePacket packet, IWireSession source)
    {
        if (packet.Instruction.StartsWith("remote:frame:", StringComparison.OrdinalIgnoreCase))
        {
            if (Volatile.Read(ref _viewing) == 0)
                return;
            string[] size = packet.Instruction[13..].Split(',');
            int.TryParse(size.ElementAtOrDefault(0), out int width);
            int.TryParse(size.ElementAtOrDefault(1), out int height);
            RemoteWidth = width;
            RemoteHeight = height;
            RemoteFrameReceived?.Invoke(packet.Data, width, height);
            return;
        }

        if (packet.Instruction.StartsWith("remote:delta", StringComparison.OrdinalIgnoreCase))
        {
            if (Volatile.Read(ref _viewing) == 0 || packet.Data == null || packet.Data.Length == 0)
                return;
            RemoteDeltaReceived?.Invoke(packet.Data);
            return;
        }

        if (packet.Instruction.StartsWith("remote:h264:", StringComparison.OrdinalIgnoreCase))
        {
            if (Volatile.Read(ref _viewing) == 0 || packet.Data == null || packet.Data.Length == 0)
                return;
            string[] size = packet.Instruction["remote:h264:".Length..].Split(',');
            int.TryParse(size.ElementAtOrDefault(0), out int width);
            int.TryParse(size.ElementAtOrDefault(1), out int height);
            RemoteWidth = width;
            RemoteHeight = height;
            RemoteH264Received?.Invoke(packet.Data, width, height);
            return;
        }

        if (packet.Instruction.StartsWith("remote:region:", StringComparison.OrdinalIgnoreCase))
        {
            if (Volatile.Read(ref _viewing) == 0 || packet.Data == null || packet.Data.Length == 0)
                return;
            string[] part = packet.Instruction["remote:region:".Length..].Split(',');
            int.TryParse(part.ElementAtOrDefault(0), out int x);
            int.TryParse(part.ElementAtOrDefault(1), out int y);
            int.TryParse(part.ElementAtOrDefault(2), out int width);
            int.TryParse(part.ElementAtOrDefault(3), out int height);
            RemoteRegionReceived?.Invoke(packet.Data, x, y, width, height);
            return;
        }

        if (packet.Instruction.StartsWith("remote:cursor:", StringComparison.OrdinalIgnoreCase))
        {
            if (Volatile.Read(ref _viewing) == 0)
                return;
            string[] part = packet.Instruction["remote:cursor:".Length..].Split(',');
            int.TryParse(part.ElementAtOrDefault(0), out int x);
            int.TryParse(part.ElementAtOrDefault(1), out int y);
            int.TryParse(part.ElementAtOrDefault(2), out int shown);
            int.TryParse(part.ElementAtOrDefault(3), out int hotX);
            int.TryParse(part.ElementAtOrDefault(4), out int hotY);
            RemoteCursorReceived?.Invoke(x, y, shown != 0, hotX, hotY, packet.Data);
            return;
        }

        if (packet.Instruction.StartsWith("remote:mark:", StringComparison.OrdinalIgnoreCase))
        {
            _ = AckFrameAsync(source, packet.Instruction["remote:mark:".Length..]);
            return;
        }

        if (packet.Instruction.StartsWith("remote:ack:", StringComparison.OrdinalIgnoreCase))
        {
            if (int.TryParse(packet.Instruction["remote:ack:".Length..], out int seq))
            {
                ScreenShareService? share = ReferenceEquals(source, _peer) ? _peerShare : _screenShare;
                share?.Acknowledge(seq);
            }
            return;
        }

        if (packet.Instruction.StartsWith("remote:accept", StringComparison.OrdinalIgnoreCase))
        {
            _ = source.SendAsync(WirePacket.Text("remote:ready"));
            return;
        }

        if (packet.Instruction.StartsWith("remote:stop", StringComparison.OrdinalIgnoreCase))
        {
            _ = StopFromPeerAsync();
            return;
        }

        if (packet.Instruction.StartsWith($"{Protocol.Permit}:", StringComparison.OrdinalIgnoreCase) ||
            packet.Instruction.StartsWith($"{Protocol.PermitAnswer}:", StringComparison.OrdinalIgnoreCase))
        {
            HandlePermissionPacket(packet, source);
            return;
        }

        if (packet.Instruction.StartsWith("MSG:", StringComparison.OrdinalIgnoreCase))
        {
            string sender = packet.Instruction.Length > 4 ? packet.Instruction[4..] : RemoteAddress;
            string text = System.Text.Encoding.UTF8.GetString(packet.Data ?? Array.Empty<byte>());
            Append(new ConversationMessage { Sender = sender, Text = text, Kind = ConversationMessageKind.Text });
            return;
        }

        if (packet.Instruction.StartsWith("chatfile:", StringComparison.OrdinalIgnoreCase))
        {
            string id = packet.Instruction[9..];
            string metadata = System.Text.Encoding.UTF8.GetString(packet.Data ?? Array.Empty<byte>());
            string[] parts = metadata.Split('\t');
            long.TryParse(parts.ElementAtOrDefault(1), out long length);
            ConversationMessage message = new()
            {
                Sender = RemoteAddress,
                Kind = ConversationMessageKind.Attachment,
                FileId = id,
                FileName = parts.ElementAtOrDefault(0) ?? "file",
                FileLength = length
            };
            Append(message);
            _incoming[id] = new IncomingFile(id, message.FileName, length, _incomingDirectory);
            return;
        }

        if (packet.Instruction.StartsWith("transfer:cancel", StringComparison.OrdinalIgnoreCase))
        {
            string id = Encoding.UTF8.GetString(packet.Data ?? Array.Empty<byte>());
            _stopped.Add(id);
            return;
        }

        if (packet.Instruction.StartsWith("files:mkdir", StringComparison.OrdinalIgnoreCase)
            || packet.Instruction.StartsWith("files:copyfile", StringComparison.OrdinalIgnoreCase)
            || packet.Instruction.StartsWith("files:movefile", StringComparison.OrdinalIgnoreCase)
            || packet.Instruction.StartsWith("files:delete", StringComparison.OrdinalIgnoreCase))
        {
            ApplyFileCommand(source, packet);
            return;
        }

        if (packet.Instruction.StartsWith("files:access", StringComparison.OrdinalIgnoreCase)
            || packet.Instruction.StartsWith("files:closed", StringComparison.OrdinalIgnoreCase))
        {
            PeerFilesOpen = false;
            Post(() => FilesClosed?.Invoke());
            return;
        }

        if (packet.Instruction.StartsWith("files:getdrives", StringComparison.OrdinalIgnoreCase))
        {
            if (RefuseFilesIfLocked(source))
                return;
            string path = System.Text.Encoding.UTF8.GetString(packet.Data ?? Array.Empty<byte>());
            _ = Task.Run(async () =>
            {
                try
                {
                    string listing = BuildListing(path);
                    await source.SendAsync(WirePacket.Text("files:list", listing)).ConfigureAwait(false);
                }
                catch
                {
                }
            });
            return;
        }

        if (packet.Instruction.StartsWith("files:getfile", StringComparison.OrdinalIgnoreCase))
        {
            if (RefuseFilesIfLocked(source))
                return;
            string raw = Encoding.UTF8.GetString(packet.Data ?? Array.Empty<byte>());
            int tab = raw.IndexOf('\t');
            string path = tab >= 0 ? raw[..tab] : raw;
            string kind = tab >= 0 && raw[(tab + 1)..].Equals("clip", StringComparison.OrdinalIgnoreCase) ? "ClipFile" : "File";
            _ = SendDiskFileAsync(source, path, kind);
            return;
        }

        if (packet.Instruction.StartsWith("remote:cliptext", StringComparison.OrdinalIgnoreCase))
        {
            string text = Encoding.UTF8.GetString(packet.Data ?? Array.Empty<byte>());
            Post(() => ApplyClipText(text));
            return;
        }

        if (packet.Instruction.StartsWith("remote:clipflush", StringComparison.OrdinalIgnoreCase))
        {
            Post(() => _ = PublishClipboardAsync());
            return;
        }

        if (packet.Instruction.StartsWith("remote:clipoff", StringComparison.OrdinalIgnoreCase))
        {
            _peerClipboard = false;
            Post(() =>
            {
                if (!ExplorerOfferOnClipboard)
                    VirtualClipboard.ClearIfOurs();
            });
            return;
        }

        if (packet.Instruction.StartsWith("remote:clipon", StringComparison.OrdinalIgnoreCase))
        {
            _peerClipboard = true;
            return;
        }

        if (packet.Instruction.StartsWith("remote:clipboard", StringComparison.OrdinalIgnoreCase))
        {
            string text = Encoding.UTF8.GetString(packet.Data ?? Array.Empty<byte>());
            Post(() => ApplyRemoteClipboard(text));
            return;
        }

        if (packet.Instruction.StartsWith("remote:clipget", StringComparison.OrdinalIgnoreCase))
        {
            if (!ClipboardAllowed)
            {
                _ = source.SendAsync(WirePacket.Text("remote:clipempty"));
                return;
            }
            int.TryParse(Encoding.UTF8.GetString(packet.Data ?? Array.Empty<byte>()), out int index);
            string? path;
            lock (_clipGate)
                path = index >= 0 && index < _hostClipFiles.Count ? _hostClipFiles[index] : null;
            if (path == null)
                _ = source.SendAsync(WirePacket.Text("remote:clipempty"));
            else
                _ = SendDiskFileAsync(source, path, "ClipFile");
            return;
        }

        if (packet.Instruction.StartsWith("remote:clipempty", StringComparison.OrdinalIgnoreCase))
        {
            CancelPastes(explorer: false);
            return;
        }

        if (packet.Instruction.StartsWith("ClipFile:", StringComparison.OrdinalIgnoreCase))
        {
            ReceiveClip(packet);
            return;
        }

        if (packet.Instruction.StartsWith("files:inbox:", StringComparison.OrdinalIgnoreCase))
        {
            string id = packet.Instruction.Split(':').ElementAtOrDefault(2) ?? string.Empty;
            string[] meta = Encoding.UTF8.GetString(packet.Data ?? Array.Empty<byte>()).Split('\t');
            string name = meta.ElementAtOrDefault(0) ?? "file";
            long.TryParse(meta.ElementAtOrDefault(1), out long length);
            if (string.IsNullOrWhiteSpace(id))
                return;
            Directory.CreateDirectory(InboxFolder);
            if (!_uploads.TryGetValue(id, out IncomingFile? inboxFile))
            {
                inboxFile = new IncomingFile(id, name, length, InboxFolder, keepName: true, inbox: true);
                _uploads[id] = inboxFile;
                Append(new ConversationMessage
                {
                    Sender = RemoteAddress,
                    Kind = ConversationMessageKind.Attachment,
                    FileId = id,
                    FileName = name,
                    FileLength = length
                });
            }
            _ = source.SendAsync(WirePacket.Text($"files:resume:{id}", inboxFile.Received.ToString()));
            SetTransfer(inboxFile.Completed
                ? "Already in Inbox: " + name
                : "Saving " + name + " to Inbox...");
            TransferWindow.Report(id, name, inboxFile.Received, Math.Max(1, length), () => CancelTransfer(id));
            if (inboxFile.Completed)
                FinishInboxReceive(inboxFile);
            return;
        }

        if (packet.Instruction.StartsWith("files:resume:", StringComparison.OrdinalIgnoreCase))
        {
            string id = packet.Instruction.Split(':').ElementAtOrDefault(2) ?? string.Empty;
            long.TryParse(Encoding.UTF8.GetString(packet.Data ?? Array.Empty<byte>()), out long received);
            if (_inboxResume.TryGetValue(id, out TaskCompletionSource<long>? waiter))
                waiter.TrySetResult(Math.Max(0, received));
            return;
        }

        if (packet.Instruction.StartsWith("files:put:", StringComparison.OrdinalIgnoreCase))
        {
            if (RefuseFilesIfLocked(source))
                return;
            string id = packet.Instruction.Split(':').ElementAtOrDefault(2) ?? string.Empty;
            string[] meta = Encoding.UTF8.GetString(packet.Data ?? Array.Empty<byte>()).Split('\t');
            string folder = meta.ElementAtOrDefault(0) ?? string.Empty;
            string name = meta.ElementAtOrDefault(1) ?? "file";
            long.TryParse(meta.ElementAtOrDefault(2), out long length);
            if (!string.IsNullOrWhiteSpace(id) && Directory.Exists(folder))
            {
                TrackBrowseTransfer(id);
                _uploads[id] = new IncomingFile(id, name, length, folder, keepName: true);
            }
            return;
        }

        if (packet.Instruction.StartsWith("Put:", StringComparison.OrdinalIgnoreCase))
        {
            ReceiveUpload(packet);
            return;
        }

        if (packet.Instruction.StartsWith("remote:input", StringComparison.OrdinalIgnoreCase))
        {
            if (IsHostingScreen)
                RemoteControl.Apply(Encoding.UTF8.GetString(packet.Data ?? Array.Empty<byte>()), ReferenceEquals(source, _peer) ? _peerShare : _screenShare);
            return;
        }

        if (packet.Instruction.StartsWith("files:list", StringComparison.OrdinalIgnoreCase))
        {
            string raw = System.Text.Encoding.UTF8.GetString(packet.Data ?? Array.Empty<byte>());
            List<RemoteFileEntry> entries = ParseRemoteFiles(raw);
            if (entries.Count > 500)
                entries = entries.Take(500).ToList();
            Post(() =>
            {
                RemoteFiles.Clear();
                foreach (RemoteFileEntry entry in entries)
                    RemoteFiles.Add(entry);
                NotifyChanged();
            });
            return;
        }

        if (packet.Instruction.StartsWith("File:", StringComparison.OrdinalIgnoreCase))
        {
            ReceiveFile(packet);
        }
    }

    private static string BuildListing(string path)
    {
        const char section = (char)243;
        const char entrySep = (char)240;
        StringBuilder folders = new();
        StringBuilder files = new();
        try
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                foreach (DriveInfo drive in DriveInfo.GetDrives())
                {
                    if (!drive.IsReady)
                        continue;
                    folders.Append(drive.Name).Append('\t').Append('0').Append('\t').Append('0').Append(entrySep);
                }
            }
            else if (Directory.Exists(path))
            {
                int folderCount = 0;
                foreach (string directory in Directory.EnumerateDirectories(path))
                {
                    DirectoryInfo info = new(directory);
                    folders.Append(info.FullName).Append('\t').Append('0').Append('\t').Append(info.LastWriteTimeUtc.Ticks).Append(entrySep);
                    if (++folderCount >= 300)
                        break;
                }
                int fileCount = 0;
                foreach (string file in Directory.EnumerateFiles(path))
                {
                    FileInfo info = new(file);
                    files.Append(info.FullName).Append('\t').Append(info.Length).Append('\t').Append(info.LastWriteTimeUtc.Ticks).Append(entrySep);
                    if (++fileCount >= 400)
                        break;
                }
            }
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }

        return new StringBuilder().Append(folders).Append(section).Append(section).Append(section).Append(section).Append(files).ToString();
    }

    private bool IsLoopbackSession
    {
        get
        {
            string address = RemoteAddress ?? string.Empty;
            return Volatile.Read(ref _expectSelfPeer) == 1
                || address.StartsWith("127.0.0.1", StringComparison.OrdinalIgnoreCase)
                || address.StartsWith("localhost", StringComparison.OrdinalIgnoreCase);
        }
    }

    public async Task StartScreenShareAsync(CancellationToken cancellationToken = default)
    {
        if (!IsLoopbackSession)
            EndViewing();
        BeginClipboardSync();
        SetHostingScreen(true);
        if (_screenShare != null)
            await _screenShare.StartAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task StopScreenShareAsync(CancellationToken cancellationToken = default)
    {
        EndViewing();
        if (_screenShare != null)
            await _screenShare.StopAsync(cancellationToken).ConfigureAwait(false);
        if (_peerShare != null)
            await _peerShare.StopAsync(cancellationToken).ConfigureAwait(false);
        if (_session is { IsConnected: true })
            await _session.SendAsync(WirePacket.Text("remote:stop"), cancellationToken).ConfigureAwait(false);
        SetHostingScreen(false);
    }

    private async Task StopCaptureQuietAsync()
    {
        if (_screenShare != null)
            await _screenShare.StopQuietAsync().ConfigureAwait(false);
        if (_peerShare != null)
            await _peerShare.StopQuietAsync().ConfigureAwait(false);
        SetHostingScreen(false);
    }

    private void EndViewing()
    {
        if (Interlocked.Exchange(ref _viewing, 0) == 0)
            return;
        Post(() => ViewEnded?.Invoke());
    }

    public async Task RevokeScreenAsync(CancellationToken cancellationToken = default)
    {
        await StopScreenShareAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task RevokeFilesAsync(CancellationToken cancellationToken = default)
    {
        SetHostingFiles(false);
        if (_session is { IsConnected: true })
            await _session.SendAsync(WirePacket.Text("files:closed"), cancellationToken).ConfigureAwait(false);
    }

    public Task NotifyFilesWindowClosedAsync()
    {
        PeerFilesOpen = false;
        if (IsHostingFiles)
            return Task.CompletedTask;
        return _session is { IsConnected: true }
            ? _session.SendAsync(WirePacket.Text("files:closed"))
            : Task.CompletedTask;
    }

    public async Task SetClipboardAllowedAsync(bool allowed, CancellationToken cancellationToken = default)
    {
        ClipboardAllowed = allowed;
        Post(() => AccessChanged?.Invoke());
        IWireSession? session = LiveSession;
        if (session is not { IsConnected: true })
            return;
        await session.SendAsync(WirePacket.Text(allowed ? "remote:clipon" : "remote:clipoff"), cancellationToken).ConfigureAwait(false);
        if (!allowed)
            Post(() => VirtualClipboard.ClearIfOurs());
        else
            await FlushClipboardAsync().ConfigureAwait(false);
    }

    private void SetHostingScreen(bool value)
    {
        if (!value)
            AbortRemoteSession();
        if (IsHostingScreen == value)
        {
            if (!value)
                Post(() => AccessChanged?.Invoke());
            return;
        }
        IsHostingScreen = value;
        Post(() => AccessChanged?.Invoke());
    }

    private void AbortRemoteSession()
    {
        ClearPendingKind("watch");
        ClearPendingKind("share");
        EndClipboardSync();
        CancelPastes(explorer: false);
        lock (_clipGate)
            _hostClipFiles.Clear();
        string[] ids;
        lock (_clipTransfers)
            ids = _clipTransfers.ToArray();
        foreach (string id in ids)
            CancelTransfer(id);
        Post(() => VirtualClipboard.ClearIfOurs());
        if (ids.Length > 0)
            SetTransfer("Screen sharing stopped. Transfer cancelled.");
    }

    private void SetHostingFiles(bool value)
    {
        bool cleared = false;
        if (!value)
        {
            cleared = ClearPendingKind("files");
            AbortBrowseTransfers();
        }
        if (IsHostingFiles == value)
        {
            if (cleared)
                Post(() => AccessChanged?.Invoke());
            return;
        }
        IsHostingFiles = value;
        Post(() => AccessChanged?.Invoke());
    }

    private void AbortBrowseTransfers()
    {
        CancelPastes(explorer: true);
        string[] ids;
        lock (_browseTransfers)
            ids = _browseTransfers.ToArray();
        bool any = ids.Length > 0;
        foreach (string id in ids)
            CancelTransfer(id);
        foreach (string id in _uploads.Keys.Where(key => _uploads[key] is { Inbox: false }).ToList())
        {
            if (_uploads.Remove(id, out IncomingFile? file))
                file.Abort();
            CancelTransfer(id);
            any = true;
        }
        foreach (string id in _incoming.Keys.ToList())
        {
            if (!_incoming.TryGetValue(id, out IncomingFile? file))
                continue;
            bool chat = file.Path.StartsWith(_incomingDirectory, StringComparison.OrdinalIgnoreCase);
            if (chat)
                continue;
            file.Abort();
            _incoming.Remove(id);
            CancelTransfer(id);
            any = true;
        }
        if (any)
            SetTransfer("File access stopped. Transfer cancelled.");
    }

    private void TrackBrowseTransfer(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return;
        lock (_browseTransfers)
            _browseTransfers.Add(id);
    }

    private bool ClearPendingKind(string kind)
    {
        bool cleared = false;
        foreach (PermissionRequest request in Requests)
        {
            if (!request.Kind.Equals(kind, StringComparison.OrdinalIgnoreCase) || !request.IsPending)
                continue;
            request.Result = "Closed";
            cleared = true;
        }
        return cleared;
    }

    private void OpenViewer()
    {
        Volatile.Write(ref _viewing, 1);
        Post(() => ShowPanel?.Invoke("screen"));
    }

    private async Task StopFromPeerAsync()
    {
        EndViewing();
        await StopCaptureQuietAsync().ConfigureAwait(false);
        Append(new ConversationMessage { Sender = RemoteAddress, Text = "Screen sharing stopped.", Kind = ConversationMessageKind.System });
    }

    public void CancelTransfer(string id)
    {
        _stopped.Add(id);
        lock (_browseTransfers)
            _browseTransfers.Remove(id);
        lock (_clipTransfers)
            _clipTransfers.Remove(id);
        _inbox.RemoveJob(id);
        _ = _session?.SendAsync(WirePacket.Text("transfer:cancel", id));
        TransferWindow.Finish(id);
    }

    public void StageForLocalPaste(IReadOnlyList<RemoteFileEntry> files)
    {
        List<RemoteFileEntry> chosen = files.Where(file => !file.IsFolder).ToList();
        if (chosen.Count == 0 || !IsLive)
            return;
        _explorerPaths.Clear();
        _explorerOffers.Clear();
        foreach (RemoteFileEntry file in chosen)
        {
            _explorerPaths.Add(file.Path);
            _explorerOffers.Add(new ClipOffer(file.Name, file.Size));
        }
        List<ClipOffer> offers = _explorerOffers.ToList();
        _explorerOffer = true;
        Post(() => VirtualClipboard.Offer(offers, BeginExplorerPaste));
        SetTransfer("Copied. Paste in File Explorer on this PC to start the transfer.");
    }

    public void BeginClipboardSync()
    {
        Post(() =>
        {
            _clipUsers++;
            _clipTimer ??= new System.Windows.Threading.DispatcherTimer { Interval = TimeSpan.FromMilliseconds(400) };
            _clipTimer.Interval = TimeSpan.FromMilliseconds(400);
            _clipTimer.Tick -= ClipTick;
            _clipTimer.Tick += ClipTick;
            _clipTimer.Start();
            _ = PublishClipboardAsync();
        });
    }

    public void EndClipboardSync()
    {
        Post(() =>
        {
            _clipUsers = Math.Max(0, _clipUsers - 1);
            if (_clipUsers == 0)
                _clipTimer?.Stop();
        });
    }

    public async Task PasteLocalFilesAsync(string remoteFolder, IEnumerable<string> files, CancellationToken cancellationToken = default)
    {
        foreach (string file in files)
        {
            if (File.Exists(file))
                await UploadLocalAsync(file, remoteFolder, cancellationToken).ConfigureAwait(false);
        }
    }

    public Task MakeFolderAsync(string parent, string name, CancellationToken cancellationToken = default)
    {
        return SendCommandAsync($"files:mkdir:{Environment.MachineName}", parent + "\t" + name, cancellationToken);
    }

    public Task TransferRemoteAsync(string source, string destinationFolder, bool move, CancellationToken cancellationToken = default)
    {
        string instruction = move ? "files:movefile:" : "files:copyfile:";
        return SendCommandAsync(instruction + Environment.MachineName, source + "\t" + destinationFolder, cancellationToken);
    }

    public Task DeleteRemoteAsync(string path, CancellationToken cancellationToken = default)
    {
        return SendCommandAsync($"files:delete:{Environment.MachineName}", path, cancellationToken);
    }

    private async Task SendCommandAsync(string instruction, string data, CancellationToken cancellationToken)
    {
        if (!await PrepareSendAsync(cancellationToken).ConfigureAwait(false))
            return;
        IWireSession? session = LiveSession;
        if (session == null)
            return;
        await session.SendAsync(WirePacket.Text(instruction, data), cancellationToken).ConfigureAwait(false);
    }

    private bool RefuseFilesIfLocked(IWireSession source)
    {
        if (IsHostingFiles)
            return false;
        _ = source.SendAsync(WirePacket.Text("files:access", "</Denied/>"));
        return true;
    }

    private void ApplyFileCommand(IWireSession source, WirePacket packet)
    {
        if (RefuseFilesIfLocked(source))
            return;
        string data = Encoding.UTF8.GetString(packet.Data ?? Array.Empty<byte>());
        string instruction = packet.Instruction;
        string refresh = RemoteFolder;
        try
        {
            if (instruction.StartsWith("files:mkdir", StringComparison.OrdinalIgnoreCase))
            {
                string[] parts = data.Split('\t');
                refresh = parts.ElementAtOrDefault(0) ?? string.Empty;
                Directory.CreateDirectory(Path.Combine(refresh, parts.ElementAtOrDefault(1) ?? "New folder"));
            }
            else if (instruction.StartsWith("files:copyfile", StringComparison.OrdinalIgnoreCase) || instruction.StartsWith("files:movefile", StringComparison.OrdinalIgnoreCase))
            {
                string[] parts = data.Split('\t');
                string from = parts.ElementAtOrDefault(0) ?? string.Empty;
                refresh = parts.ElementAtOrDefault(1) ?? string.Empty;
                string to = Path.Combine(refresh, Path.GetFileName(from.TrimEnd('\\')));
                bool move = instruction.StartsWith("files:movefile", StringComparison.OrdinalIgnoreCase);
                if (Directory.Exists(from))
                {
                    if (move) Directory.Move(from, to);
                    else CopyDirectory(from, to);
                }
                else if (File.Exists(from))
                {
                    if (move) File.Move(from, to);
                    else File.Copy(from, to, true);
                }
            }
            else if (instruction.StartsWith("files:delete", StringComparison.OrdinalIgnoreCase))
            {
                refresh = Path.GetDirectoryName(data.TrimEnd('\\')) ?? string.Empty;
                if (Directory.Exists(data)) Directory.Delete(data, true);
                else if (File.Exists(data)) File.Delete(data);
            }
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
        _ = source.SendAsync(WirePacket.Text("files:list", BuildListing(refresh)));
    }

    private static void CopyDirectory(string source, string destination)
    {
        Directory.CreateDirectory(destination);
        foreach (string file in Directory.GetFiles(source))
            File.Copy(file, Path.Combine(destination, Path.GetFileName(file)), true);
        foreach (string directory in Directory.GetDirectories(source))
            CopyDirectory(directory, Path.Combine(destination, Path.GetFileName(directory)));
    }

    public async Task DownloadRemoteAsync(string remotePath, string localDirectory, CancellationToken cancellationToken = default)
    {
        if (!await PrepareSendAsync(cancellationToken).ConfigureAwait(false))
            return;
        Directory.CreateDirectory(localDirectory);
        _downloadDirectory = localDirectory;
        IWireSession? session = LiveSession;
        if (session == null)
            return;
        SetTransfer("Copying " + Path.GetFileName(remotePath) + " to this PC...");
        await session.SendAsync(WirePacket.Text($"files:getfile:{Environment.MachineName}", remotePath), cancellationToken).ConfigureAwait(false);
    }

    public static string InboxFolder =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Kdock Inbox");

    public async Task SendToInboxAsync(string peerId, IEnumerable<string> paths, CancellationToken cancellationToken = default)
    {
        string peer = CanonicalKey(peerId);
        if (string.IsNullOrWhiteSpace(peer))
            peer = string.IsNullOrWhiteSpace(RemoteDeskId) ? CanonicalKey(RemoteAddress) : RemoteDeskId;
        foreach (string path in paths)
        {
            if (!File.Exists(path))
                continue;
            InboxJob job = _inbox.AddJob(peer, path);
            await SendInboxJobAsync(job, cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task ResumeInboxJobsAsync()
    {
        string peer = CanonicalKey(string.IsNullOrWhiteSpace(RemoteDeskId) ? RemoteAddress : RemoteDeskId);
        if (peer.Length == 0)
            peer = PeerKey;
        foreach (InboxJob job in _inbox.Jobs)
        {
            string jobPeer = CanonicalKey(job.PeerId);
            if (jobPeer.Length == 0
                || (!jobPeer.Equals(peer, StringComparison.OrdinalIgnoreCase) && DigitsOnly(jobPeer) != DigitsOnly(peer)))
                continue;
            _ = SendInboxJobAsync(job, CancellationToken.None);
        }
    }

    private async Task SendInboxJobAsync(InboxJob job, CancellationToken cancellationToken)
    {
        if (!_inboxSending.TryAdd(job.Id, 1))
            return;
        try
        {
            if (!File.Exists(job.Path) || _stopped.Contains(job.Id))
            {
                _inbox.RemoveJob(job.Id);
                TransferWindow.Finish(job.Id);
                return;
            }

            TransferWindow.Report(job.Id, job.Name, job.Offset, Math.Max(1, job.Length), () => CancelTransfer(job.Id));
            if (!await PrepareSendAsync(cancellationToken).ConfigureAwait(false) || _session == null)
            {
                SetTransfer("Paused — reconnecting " + job.Name);
                return;
            }

            if (job.Offset == 0)
            {
                Append(new ConversationMessage
                {
                    Sender = "You",
                    Kind = ConversationMessageKind.Attachment,
                    FileId = job.Id,
                    FileName = job.Name,
                    FileLength = job.Length,
                    SavedPath = job.Path
                });
            }
            SetTransfer("Sending " + job.Name + " to their Inbox...");
            TaskCompletionSource<long> resume = new(TaskCreationOptions.RunContinuationsAsynchronously);
            _inboxResume[job.Id] = resume;
            await _session.SendAsync(WirePacket.Text($"files:inbox:{job.Id}", $"{job.Name}\t{job.Length}"), cancellationToken).ConfigureAwait(false);
            long have = job.Offset;
            try
            {
                have = await resume.Task.WaitAsync(TimeSpan.FromSeconds(12), cancellationToken).ConfigureAwait(false);
            }
            catch (TimeoutException)
            {
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            _inboxResume.TryRemove(job.Id, out _);
            if (have >= job.Length)
            {
                _inbox.RemoveJob(job.Id);
                Post(() => UpdateFileProgress(job.Id, job.Length, job.Path));
                TransferWindow.Finish(job.Id);
                SetTransfer("Already on their Inbox: " + job.Name);
                return;
            }

            _inbox.UpdateJob(job.Id, have);
            await SendChunksAsync(_session, "Put", job.Id, job.Name, job.Length, job.Path, cancellationToken, have, offset =>
            {
                _inbox.UpdateJob(job.Id, offset);
                Post(() => UpdateFileProgress(job.Id, offset, job.Path));
            }).ConfigureAwait(false);
            _inbox.RemoveJob(job.Id);
            Post(() => UpdateFileProgress(job.Id, job.Length, job.Path));
            SetTransfer("Sent " + job.Name + " to their Inbox.");
        }
        catch (OperationCanceledException)
        {
            SetTransfer("Paused — " + job.Name);
        }
        catch (IOException)
        {
            SetTransfer("Paused — reconnecting " + job.Name);
            TransferWindow.Report(job.Id, job.Name, job.Offset, Math.Max(1, job.Length), () => CancelTransfer(job.Id));
        }
        catch (ObjectDisposedException)
        {
            SetTransfer("Paused — reconnecting " + job.Name);
        }
        catch (Exception)
        {
            SetTransfer("Paused — reconnecting " + job.Name);
            TransferWindow.Report(job.Id, job.Name, job.Offset, Math.Max(1, job.Length), () => CancelTransfer(job.Id));
        }
        finally
        {
            _inboxSending.TryRemove(job.Id, out _);
            _inboxResume.TryRemove(job.Id, out _);
        }
    }

    public async Task UploadLocalAsync(string localPath, string remoteFolder, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(localPath) || string.IsNullOrWhiteSpace(remoteFolder) || !await PrepareSendAsync(cancellationToken).ConfigureAwait(false))
            return;
        string id = Guid.NewGuid().ToString("N");
        string name = Path.GetFileName(localPath);
        long length = new FileInfo(localPath).Length;
        IWireSession? session = LiveSession;
        if (session == null)
            return;
        SetTransfer("Copying " + name + " to their PC...");
        TrackBrowseTransfer(id);
        await session.SendAsync(WirePacket.Text($"files:put:{id}", $"{remoteFolder}\t{name}\t{length}"), cancellationToken).ConfigureAwait(false);
        await SendChunksAsync(session, "Put", id, name, length, localPath, cancellationToken).ConfigureAwait(false);
        SetTransfer("Copied " + name + " to their PC.");
    }

    private static async Task AckFrameAsync(IWireSession source, string seq)
    {
        try
        {
            if (source.IsConnected)
                await source.SendAsync(WirePacket.Text("remote:ack:" + seq)).ConfigureAwait(false);
        }
        catch (Exception)
        {
        }
    }

    public async Task SendControlAsync(string payload, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(payload) || Volatile.Read(ref _viewing) == 0)
            return;
        if (!await PrepareSendAsync(cancellationToken).ConfigureAwait(false))
            return;
        IWireSession? session = LiveSession;
        if (session == null || !session.IsConnected)
            return;
        await session.SendAsync(WirePacket.Text("remote:input", payload), cancellationToken).ConfigureAwait(false);
    }

    private async Task SendDiskFileAsync(IWireSession session, string path, string kind)
    {
        if (!File.Exists(path))
            return;
        string id = Guid.NewGuid().ToString("N");
        string name = Path.GetFileName(path);
        long length = new FileInfo(path).Length;
        if (kind.Equals("ClipFile", StringComparison.OrdinalIgnoreCase))
        {
            lock (_clipTransfers)
                _clipTransfers.Add(id);
        }
        else
            TrackBrowseTransfer(id);
        await SendChunksAsync(session, kind, id, name, length, path, CancellationToken.None).ConfigureAwait(false);
    }

    private async Task SendChunksAsync(
        IWireSession session,
        string kind,
        string id,
        string name,
        long length,
        string path,
        CancellationToken cancellationToken,
        long offset = 0,
        Action<long>? progress = null)
    {
        byte[] buffer = new byte[64 * 1024];
        await using FileStream stream = new(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        if (offset > 0)
            stream.Seek(Math.Min(offset, stream.Length), SeekOrigin.Begin);
        int read;
        long sent = Math.Max(0, offset);
        TransferWindow.Report(id, name, sent, Math.Max(1, length), () => CancelTransfer(id));
        while ((read = await stream.ReadAsync(buffer.AsMemory(), cancellationToken).ConfigureAwait(false)) > 0)
        {
            if (_stopped.Contains(id))
            {
                TransferWindow.Finish(id);
                return;
            }
            byte[] chunk = buffer[..read].ToArray();
            await session.SendAsync(WirePacket.Binary($"{kind}:{id}:{name}:{length}:{sent}", chunk), cancellationToken).ConfigureAwait(false);
            sent += read;
            progress?.Invoke(sent);
            TransferWindow.Report(id, name, sent, Math.Max(1, length), () => CancelTransfer(id));
        }
        if (length == 0)
            await session.SendAsync(WirePacket.Binary($"{kind}:{id}:{name}:{length}:0", Array.Empty<byte>()), cancellationToken).ConfigureAwait(false);
        TransferWindow.Finish(id);
    }

    private void ReceiveUpload(WirePacket packet)
    {
        string[] parts = packet.Instruction.Split(':');
        if (parts.Length < 2 || !_uploads.TryGetValue(parts[1], out IncomingFile? file))
            return;
        if (_stopped.Contains(file.Id) || (!file.Inbox && !IsHostingFiles))
        {
            file.Abort();
            _uploads.Remove(file.Id);
            TransferWindow.Finish(file.Id);
            return;
        }
        if (TryWireFile(packet.Instruction, out _, out _, out _, out long at))
            file.Seek(at);
        else if (parts.Length >= 5 && long.TryParse(parts[4], out at))
            file.Seek(at);
        file.Write(packet.Data);
        TransferWindow.Report(file.Id, file.Name, file.Received, Math.Max(1, file.Length), () => CancelTransfer(file.Id));
        if (file.Completed)
            FinishInboxReceive(file);
    }

    private void FinishInboxReceive(IncomingFile file)
    {
        if (!_uploads.Remove(file.Id))
            return;
        file.Complete();
        SetTransfer("Received " + file.Name);
        TransferWindow.Finish(file.Id);
        Post(() =>
        {
            ConversationMessage? message = Bucket(_sessionKey.Length > 0 ? _sessionKey : _transcriptKey)
                .FirstOrDefault(item => item.FileId == file.Id);
            if (message != null)
            {
                message.FileProgress = file.Length;
                message.SavedPath = file.Path;
            }
            if (file.Inbox)
            {
                _inbox.RememberArrival(new InboxArrival
                {
                    Name = file.Name,
                    From = _inbox.NameFor(RemoteDeskId.Length > 0 ? RemoteDeskId : RemoteAddress),
                    Path = file.Path,
                    Size = file.Length,
                    Utc = DateTime.UtcNow
                });
            }
            NotifyChanged();
        });
        if (!string.IsNullOrWhiteSpace(RemoteFolder))
            _ = SendListingAsync(file);
    }

    private void UpdateFileProgress(string id, long progress, string path)
    {
        ConversationMessage? message = Bucket(_sessionKey.Length > 0 ? _sessionKey : _transcriptKey)
            .FirstOrDefault(item => item.FileId == id);
        if (message == null)
            return;
        message.FileProgress = progress;
        message.SavedPath = path;
    }

    private async Task SendListingAsync(IncomingFile file)
    {
        string folder = Path.GetDirectoryName(file.Path) ?? RemoteFolder;
        IWireSession? session = _peer ?? _session;
        if (session == null || string.IsNullOrWhiteSpace(folder))
            return;
        await session.SendAsync(WirePacket.Text("files:list", BuildListing(folder))).ConfigureAwait(false);
    }

    private void ReceiveFile(WirePacket packet)
    {
        string[] parts = packet.Instruction.Split(':');
        if (parts.Length < 4)
            return;
        string id = parts[1];
        if (!_incoming.TryGetValue(id, out IncomingFile? file))
        {
            long.TryParse(parts[3], out long length);
            string folder = _downloadDirectory ?? _incomingDirectory;
            bool keepName = _downloadDirectory != null;
            file = new IncomingFile(id, parts[2], length, folder, keepName);
            _incoming[id] = file;
            if (keepName)
            {
                TrackBrowseTransfer(id);
                _downloadDirectory = null;
                SetTransfer("Saving " + file.Name + " on this PC...");
            }
            else
            {
                Append(new ConversationMessage { Sender = RemoteAddress, Kind = ConversationMessageKind.Attachment, FileId = id, FileName = file.Name, FileLength = file.Length });
            }
        }

        bool chatFile = file.Path.StartsWith(_incomingDirectory, StringComparison.OrdinalIgnoreCase);
        if (_stopped.Contains(id))
        {
            file.Abort();
            _incoming.Remove(id);
            if (!chatFile)
                TransferWindow.Finish(id);
            return;
        }
        file.Write(packet.Data);
        if (!chatFile)
            TransferWindow.Report(file.Id, file.Name, file.Received, Math.Max(1, file.Length), () => CancelTransfer(file.Id));
        Post(() =>
        {
            ConversationMessage? message = Transcript.FirstOrDefault(item => item.FileId == id);
            if (message != null)
            {
                message.FileProgress = file.Received;
                if (file.Completed)
                    message.SavedPath = file.Path;
            }
            if (file.Completed && !chatFile)
                TransferStatus = "Saved " + file.Name;
            NotifyChanged();
        });
        if (file.Completed)
        {
            _incoming.Remove(id);
            if (!chatFile)
            {
                SetTransfer("Saved " + file.Name + " on this PC.");
                TransferWindow.Finish(id);
            }
        }
    }

    private void Append(ConversationMessage message)
    {
        string key = CanonicalKey(_sessionKey.Length > 0 ? _sessionKey : _transcriptKey);
        if (key.Length == 0)
            key = "session";
        void Add()
        {
            Bucket(key).Add(message);
            ChatTouched?.Invoke(key);
            if (!string.Equals(message.Sender, "You", StringComparison.OrdinalIgnoreCase) && message.Kind != ConversationMessageKind.System)
            {
                LastIncomingKey = key;
                bool viewingThis = CanonicalKey(_transcriptKey).Equals(key, StringComparison.OrdinalIgnoreCase);
                if (!App.IsPeopleOpen || !viewingThis)
                    NoticeBoard.Push();
            }
            ScheduleSave();
            NotifyChanged();
        }
        if (SynchronizationContext.Current == _uiContext)
            Add();
        else
            Post(Add);
    }

    private ObservableCollection<ConversationMessage> Bucket(string key)
    {
        key = CanonicalKey(key);
        if (_transcripts.TryGetValue(key, out ObservableCollection<ConversationMessage>? list))
            return list;
        string digits = DigitsOnly(key);
        if (digits.Length == 10)
        {
            string? alias = _transcripts.Keys.FirstOrDefault(item => DigitsOnly(item) == digits);
            if (alias != null)
                return _transcripts[alias];
        }
        list = new ObservableCollection<ConversationMessage>();
        _transcripts[key] = list;
        return list;
    }

    public static string CanonicalKey(string value)
    {
        string text = (value ?? string.Empty).Trim();
        string digits = DigitsOnly(text);
        return digits.Length == 10 ? digits : text;
    }

    private static string DigitsOnly(string value) => new string((value ?? string.Empty).Where(char.IsDigit).ToArray());

    public void DeleteHistory(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return;
        string digits = new string(key.Where(char.IsDigit).ToArray());
        List<string> match = _transcripts.Keys
            .Where(item => item.Equals(key, StringComparison.OrdinalIgnoreCase)
                || (digits.Length == 10 && new string(item.Where(char.IsDigit).ToArray()) == digits))
            .ToList();
        foreach (string item in match)
            _transcripts.Remove(item);
        if (match.Contains(_transcriptKey) || _transcriptKey.Equals(key, StringComparison.OrdinalIgnoreCase))
        {
            _transcriptKey = string.Empty;
            Transcript = new ObservableCollection<ConversationMessage>();
        }
        try
        {
            Directory.CreateDirectory(_transcriptDirectory);
            foreach (string file in Directory.GetFiles(_transcriptDirectory, "*.json"))
            {
                string name = Path.GetFileNameWithoutExtension(file);
                string fileDigits = new string(name.Where(char.IsDigit).ToArray());
                if (name.Equals(SafeKey(key), StringComparison.OrdinalIgnoreCase)
                    || match.Any(item => name.Equals(SafeKey(item), StringComparison.OrdinalIgnoreCase))
                    || (digits.Length == 10 && fileDigits == digits))
                {
                    File.Delete(file);
                }
            }
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
        if (LastIncomingKey != null && match.Contains(LastIncomingKey))
            LastIncomingKey = null;
        Post(NotifyChanged);
    }

    private static string SafeKey(string key) =>
        string.Concat((key ?? string.Empty).Select(ch => char.IsLetterOrDigit(ch) || ch is '.' or '-' or '_' ? ch : '_'));

    private void ScheduleSave()
    {
        Post(() =>
        {
            _saveTimer ??= new System.Windows.Threading.DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
            _saveTimer.Stop();
            _saveTimer.Start();
        });
    }

    private void LoadAll()
    {
        try
        {
            foreach (string file in Directory.GetFiles(_transcriptDirectory, "*.json"))
            {
                string key = Path.GetFileNameWithoutExtension(file);
                if (string.IsNullOrWhiteSpace(key))
                    continue;
                if (_boundKey != null
                    && !CanonicalKey(key).Equals(_boundKey, StringComparison.OrdinalIgnoreCase)
                    && DigitsOnly(key) != DigitsOnly(_boundKey))
                    continue;
                StoredChat? stored = System.Text.Json.JsonSerializer.Deserialize<StoredChat>(File.ReadAllText(file));
                if (stored?.Messages == null)
                    continue;
                ObservableCollection<ConversationMessage> list = new();
                foreach (StoredMessage item in stored.Messages.TakeLast(500))
                {
                    list.Add(new ConversationMessage
                    {
                        TimestampUtc = item.TimestampUtc == default ? DateTime.UtcNow : item.TimestampUtc,
                        Sender = item.Sender,
                        Text = item.Text,
                        Kind = item.Kind,
                        FileId = item.FileId,
                        FileName = item.FileName,
                        FileLength = item.FileLength,
                        FileProgress = item.FileLength,
                        SavedPath = item.SavedPath
                    });
                }
                _transcripts[key] = list;
            }
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
        catch (System.Text.Json.JsonException)
        {
        }
    }

    private void SaveAll()
    {
        lock (_historyGate)
        {
            try
            {
                Directory.CreateDirectory(_transcriptDirectory);
                foreach (KeyValuePair<string, ObservableCollection<ConversationMessage>> pair in _transcripts)
                {
                    if (_boundKey != null
                        && !CanonicalKey(pair.Key).Equals(_boundKey, StringComparison.OrdinalIgnoreCase)
                        && DigitsOnly(pair.Key) != DigitsOnly(_boundKey))
                        continue;
                    string safe = SafeKey(pair.Key);
                    if (safe.Length == 0)
                        continue;
                    StoredChat stored = new()
                    {
                        Messages = pair.Value
                            .Where(item => item.Kind is ConversationMessageKind.Text or ConversationMessageKind.Attachment)
                            .Select(item => new StoredMessage
                            {
                                TimestampUtc = item.TimestampUtc,
                                Sender = item.Sender,
                                Text = item.Text,
                                Kind = item.Kind,
                                FileId = item.FileId,
                                FileName = item.FileName,
                                FileLength = item.FileLength,
                                SavedPath = item.SavedPath
                            })
                            .TakeLast(500)
                            .ToList()
                    };
                    File.WriteAllText(Path.Combine(_transcriptDirectory, safe + ".json"), System.Text.Json.JsonSerializer.Serialize(stored));
                }
            }
            catch (IOException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
        }
    }

    private sealed class StoredChat
    {
        public List<StoredMessage> Messages { get; set; } = new();
    }

    private sealed class StoredMessage
    {
        public DateTime TimestampUtc { get; set; }
        public string Sender { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public ConversationMessageKind Kind { get; set; }
        public string FileId { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public long FileLength { get; set; }
        public string SavedPath { get; set; } = string.Empty;
    }

    private void SetTransfer(string status)
    {
        TransferStatus = status;
        Post(NotifyChanged);
    }

    private void ClipTick(object? sender, EventArgs e) => _ = PublishClipboardAsync();

    public Task FlushClipboardAsync()
    {
        TaskCompletionSource tcs = new(TaskCreationOptions.RunContinuationsAsynchronously);
        Post(async () =>
        {
            try { await PublishClipboardAsync().ConfigureAwait(true); }
            catch { }
            tcs.TrySetResult();
        });
        return tcs.Task;
    }

    public async Task RequestPeerClipboardAsync()
    {
        IWireSession? session = LiveSession;
        if (session is not { IsConnected: true })
            return;
        await session.SendAsync(WirePacket.Text("remote:clipflush")).ConfigureAwait(false);
    }

    private async Task PublishClipboardAsync()
    {
        try
        {
            IWireSession? session = LiveSession;
            if (_clipUsers <= 0 || session is not { IsConnected: true })
                return;
            if (!ClipboardAllowed)
                return;
            if (VirtualClipboard.IsCurrent())
                return;
            if (System.Windows.Clipboard.ContainsFileDropList())
            {
                System.Collections.Specialized.StringCollection dropped = System.Windows.Clipboard.GetFileDropList();
                List<string> files = new();
                foreach (string? path in dropped)
                {
                    if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
                        files.Add(path);
                }
                if (files.Count == 0)
                    return;
                string signature = "f:" + string.Join("|", files);
                if (signature == _clipSignature)
                    return;
                _clipSignature = signature;
                lock (_clipGate)
                {
                    _hostClipFiles.Clear();
                    _hostClipFiles.AddRange(files);
                }
                string payload = string.Join("\n", files.Select(path => Path.GetFileName(path) + "\t" + new FileInfo(path).Length));
                await session.SendAsync(WirePacket.Text("remote:clipboard", payload)).ConfigureAwait(true);
                return;
            }

            if (System.Windows.Clipboard.ContainsText())
            {
                string text = System.Windows.Clipboard.GetText();
                if (string.IsNullOrEmpty(text) || text.Length > 256 * 1024)
                    return;
                string signature = ClipTextSignature(text);
                if (signature == _clipSignature)
                    return;
                _clipSignature = signature;
                await session.SendAsync(WirePacket.Text("remote:cliptext", text)).ConfigureAwait(true);
                return;
            }

            if (_clipSignature.Length > 0)
            {
                _clipSignature = string.Empty;
                lock (_clipGate)
                    _hostClipFiles.Clear();
                await session.SendAsync(WirePacket.Text("remote:clipboard")).ConfigureAwait(true);
            }
        }
        catch (System.Runtime.InteropServices.ExternalException)
        {
        }
    }

    private static string ClipTextSignature(string text)
    {
        int take = Math.Min(96, text.Length);
        return "t:" + text.Length + ":" + text[..take];
    }

    private void ApplyClipText(string text)
    {
        if (string.IsNullOrEmpty(text))
            return;
        try
        {
            _clipSignature = ClipTextSignature(text);
            System.Windows.Clipboard.SetText(text);
        }
        catch (System.Runtime.InteropServices.ExternalException)
        {
        }
    }

    private bool ExplorerOfferOnClipboard => _explorerOffer && VirtualClipboard.IsCurrent();

    private void ApplyRemoteClipboard(string text)
    {
        if (_explorerOffer && !VirtualClipboard.IsCurrent())
            _explorerOffer = false;
        if (!_peerClipboard || _explorerOffer)
            return;
        if (string.IsNullOrWhiteSpace(text))
        {
            bool busy;
            lock (_clipGate)
                busy = _pastes.Any(job => !job.Done);
            if (!busy)
                VirtualClipboard.ClearIfOurs();
            return;
        }
        _remoteOffers.Clear();
        foreach (string line in text.Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            string[] parts = line.Split('\t');
            long.TryParse(parts.ElementAtOrDefault(1), out long size);
            _remoteOffers.Add(new ClipOffer(parts.ElementAtOrDefault(0) ?? "file", size));
        }
        if (_remoteOffers.Count == 0)
            return;
        _clipSignature = "f:" + string.Join("|", _remoteOffers.Select(item => item.Name + ":" + item.Size));
        List<ClipOffer> offers = _remoteOffers.ToList();
        VirtualClipboard.Offer(offers, BeginRemotePaste);
    }

    private ChunkStream? BeginExplorerPaste(int index)
    {
        IWireSession? session = LiveSession;
        if (index < 0 || index >= _explorerPaths.Count || session == null)
            return null;
        return BeginPaste(_explorerOffers[index], explorer: true, () => session.SendAsync(WirePacket.Text($"files:getfile:{Environment.MachineName}", _explorerPaths[index] + "\tclip")));
    }

    private ChunkStream? BeginRemotePaste(int index)
    {
        IWireSession? session = LiveSession;
        if (index < 0 || index >= _remoteOffers.Count || session == null)
            return null;
        return BeginPaste(_remoteOffers[index], explorer: false, () => session.SendAsync(WirePacket.Text("remote:clipget", index.ToString())));
    }

    private ChunkStream BeginPaste(ClipOffer offer, bool explorer, Func<Task> request)
    {
        lock (_clipGate)
        {
            PasteJob? existing = _pastes.FirstOrDefault(job =>
                !job.Done
                && job.Explorer == explorer
                && job.Size == offer.Size
                && string.Equals(job.Name, offer.Name, StringComparison.OrdinalIgnoreCase));
            if (existing != null)
                return existing.Stream;
        }

        ChunkStream stream = new(offer.Size);
        PasteJob created = new(stream, "paste-" + Guid.NewGuid().ToString("N"), offer.Name, offer.Size, explorer);
        lock (_clipGate)
        {
            PasteJob? existing = _pastes.FirstOrDefault(job =>
                !job.Done
                && job.Explorer == explorer
                && job.Size == offer.Size
                && string.Equals(job.Name, offer.Name, StringComparison.OrdinalIgnoreCase));
            if (existing != null)
                return existing.Stream;
            _pastes.Add(created);
        }
        TransferWindow.Report(created.Progress, created.Name, 0, Math.Max(1, created.Size), () => CancelPaste(created));
        _ = request();
        return stream;
    }

    private void ReceiveClip(WirePacket packet)
    {
        if (!TryWireFile(packet.Instruction, out string id, out string wireName, out long total, out _))
            return;
        PasteJob? job;
        lock (_clipGate)
        {
            job = _pastes.FirstOrDefault(item => !item.Done && string.Equals(item.NetworkId, id, StringComparison.Ordinal))
                ?? _pastes.FirstOrDefault(item => !item.Done && item.NetworkId == null && string.Equals(item.Name, wireName, StringComparison.OrdinalIgnoreCase));
            if (job == null)
            {
                List<PasteJob> waiting = _pastes.Where(item => !item.Done && item.NetworkId == null).ToList();
                if (waiting.Count == 1)
                    job = waiting[0];
            }
            if (job == null)
                return;
            job.NetworkId ??= id;
        }
        if (_stopped.Contains(id))
        {
            CancelPaste(job);
            return;
        }
        if (packet.Data is { Length: > 0 })
            job.Stream.Add(packet.Data);
        long size = Math.Max(job.Size, Math.Max(0, total));
        long written = job.Stream.Written;
        TransferWindow.Report(job.Progress, job.Name, written, Math.Max(1, size), () => CancelPaste(job));
        if (size > 0)
        {
            if (written < size)
                return;
        }
        else if (packet.Data is { Length: > 0 })
        {
            return;
        }
        lock (_clipGate)
            job.Done = true;
        job.Stream.Complete();
        TransferWindow.Finish(job.Progress);
        SetTransfer("Pasted " + job.Name + " on this PC.");
    }

    private static bool TryWireFile(string instruction, out string id, out string name, out long total, out long offset)
    {
        id = string.Empty;
        name = string.Empty;
        total = 0;
        offset = 0;
        int head = instruction.IndexOf(':');
        int idEnd = head >= 0 ? instruction.IndexOf(':', head + 1) : -1;
        int offsetColon = instruction.LastIndexOf(':');
        int lengthColon = offsetColon > 0 ? instruction.LastIndexOf(':', offsetColon - 1) : -1;
        if (idEnd <= head || lengthColon <= idEnd)
            return false;
        id = instruction[(head + 1)..idEnd];
        name = instruction[(idEnd + 1)..lengthColon];
        if (!long.TryParse(instruction[(lengthColon + 1)..offsetColon], out total))
            return false;
        long.TryParse(instruction[(offsetColon + 1)..], out offset);
        return true;
    }

    private void CancelPastes(bool explorer)
    {
        List<PasteJob> jobs;
        lock (_clipGate)
            jobs = _pastes.Where(job => !job.Done && job.Explorer == explorer).ToList();
        foreach (PasteJob job in jobs)
            CancelPaste(job);
    }

    private void CancelPaste(PasteJob job)
    {
        string? networkId;
        lock (_clipGate)
        {
            if (job.Done)
                return;
            job.Done = true;
            networkId = job.NetworkId;
        }
        job.Stream.Complete();
        if (!string.IsNullOrEmpty(networkId))
        {
            _stopped.Add(networkId);
            _ = LiveSession?.SendAsync(WirePacket.Text("transfer:cancel", networkId));
        }
        TransferWindow.Finish(job.Progress);
    }

    private void SessionStatusChanged(IWireSession session, string status)
    {
        if (!ReferenceEquals(_session, session))
            return;
        if (status == "Disconnected.")
        {
            UpdateState(ConversationState.Disconnected, Volatile.Read(ref _wantLive) == 1 ? "Disconnected. Reconnecting..." : "Disconnected");
            if (Volatile.Read(ref _wantLive) == 1)
                Post(() => _ = AutoRestoreAsync());
            return;
        }
        UpdateState(State, status);
    }

    private void SessionProtocolMismatch()
    {
        UpdateState(ConversationState.ProtocolMismatch, "Update Kdock on both PCs.");
    }

    private void UpdateState(ConversationState state, string status)
    {
        State = state;
        Status = status;
        Post(NotifyChanged);
    }

    private void Post(Action action)
    {
        _uiContext.Post(_ => action(), null);
    }

    private void NotifyChanged() => Changed?.Invoke();

    private static void ParseAddress(string address, out string host, out int port)
    {
        host = (address ?? string.Empty).Trim();
        port = Protocol.LanPort;
        if (host.StartsWith("[", StringComparison.Ordinal))
        {
            int end = host.IndexOf(']');
            if (end > 0)
            {
                string remainder = host[(end + 1)..];
                string value = host[1..end];
                host = value;
                if (remainder.StartsWith(":") && int.TryParse(remainder[1..], out int parsed))
                    port = parsed;
            }
            return;
        }
        int colon = host.LastIndexOf(':');
        if (colon > 0 && host.IndexOf(':') == colon && int.TryParse(host[(colon + 1)..], out int portValue))
        {
            host = host[..colon];
            port = portValue;
        }
    }

    public async ValueTask DisposeAsync()
    {
        SaveAll();
        _packetQueue.Writer.TryComplete();
        await DisconnectAsync(userInitiated: true).ConfigureAwait(false);
        _liveGate.Dispose();
    }

    private sealed class PasteJob
    {
        public PasteJob(ChunkStream stream, string progress, string name, long size, bool explorer)
        {
            Stream = stream;
            Progress = progress;
            Name = name;
            Size = size;
            Explorer = explorer;
        }

        public ChunkStream Stream { get; }
        public string Progress { get; }
        public string Name { get; }
        public long Size { get; }
        public bool Explorer { get; }
        public string? NetworkId { get; set; }
        public bool Done { get; set; }
    }

    private sealed class IncomingFile
    {
        private FileStream? _stream;
        public IncomingFile(string id, string name, long length, string directory, bool keepName = false, bool inbox = false)
        {
            Id = id;
            Name = Sanitize(name);
            Length = length;
            Inbox = inbox;
            if (inbox)
            {
                string final = System.IO.Path.Combine(directory, Name);
                Path = final + ".kdockpart";
                if (File.Exists(final) && new FileInfo(final).Length == length)
                {
                    Path = final;
                    Received = length;
                    return;
                }
                _stream = new FileStream(Path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);
                if (_stream.Length > length)
                    _stream.SetLength(0);
                _stream.Seek(0, SeekOrigin.End);
                Received = _stream.Length;
                return;
            }
            Path = System.IO.Path.Combine(directory, keepName ? Name : id + "-" + Name);
            _stream = new FileStream(Path, FileMode.Create, FileAccess.Write, FileShare.None);
        }
        public string Id { get; }
        public string Name { get; }
        public long Length { get; }
        public string Path { get; private set; }
        public bool Inbox { get; }
        public long Received { get; private set; }
        public bool Completed => Received >= Length;
        public void Abort()
        {
            try { _stream?.Dispose(); } catch { }
            _stream = null;
            try { if (File.Exists(Path)) File.Delete(Path); } catch { }
        }
        public void Seek(long at)
        {
            if (_stream == null)
                return;
            at = Math.Clamp(at, 0, Math.Max(0, Length));
            _stream.Seek(at, SeekOrigin.Begin);
            Received = at;
        }
        public void Write(byte[]? data)
        {
            data ??= Array.Empty<byte>();
            if (_stream == null || data.Length == 0 && Length > 0)
                return;
            _stream.Write(data, 0, data.Length);
            Received += data.Length;
            if (Completed)
                Complete();
        }
        public void Complete()
        {
            try { _stream?.Dispose(); } catch { }
            _stream = null;
            if (!Inbox || !Path.EndsWith(".kdockpart", StringComparison.OrdinalIgnoreCase))
                return;
            string final = Path[..^".kdockpart".Length];
            try
            {
                if (File.Exists(Path))
                    File.Move(Path, final, true);
                Path = final;
            }
            catch (IOException)
            {
            }
        }
        private static string Sanitize(string name)
        {
            foreach (char invalid in System.IO.Path.GetInvalidFileNameChars())
                name = name.Replace(invalid, '_');
            return name;
        }
    }
}
