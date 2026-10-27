using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Kdock.Wire;

namespace Kdock.App;

public sealed class ConversationHub : IAsyncDisposable
{
    private readonly string _dataDirectory;
    private readonly string _chatDirectory;
    private readonly object _gate = new();
    private readonly Dictionary<string, ConversationViewModel> _sessions = new(StringComparer.OrdinalIgnoreCase);

    public ConversationHub(string dataDirectory)
    {
        _dataDirectory = dataDirectory;
        _chatDirectory = Path.Combine(dataDirectory, "Chats");
        Directory.CreateDirectory(_chatDirectory);
        Inbox = new InboxStore(dataDirectory);
        Shell = new ConversationViewModel(dataDirectory, Inbox, boundKey: null);
        Active = Shell;
    }

    public InboxStore Inbox { get; }
    public ConversationViewModel Shell { get; }
    public ConversationViewModel Active { get; private set; }
    public string? LastIncomingKey { get; private set; }
    public event Action<string>? ChatTouched;
    public event Action<ConversationViewModel>? SessionCreated;
    public event Action<ConversationViewModel, byte[], int, int>? RemoteFrame;
    public event Action<ConversationViewModel, byte[]>? RemoteDelta;
    public event Action<ConversationViewModel, byte[], int, int>? RemoteH264;
    public event Action<ConversationViewModel, byte[], int, int, int, int>? RemoteRegion;
    public event Action<ConversationViewModel, int, int, bool, int, int, byte[]?>? RemoteCursor;
    public event Action<ConversationViewModel, string>? Panel;
    public event Action<ConversationViewModel>? ViewEnded;
    public event Action<ConversationViewModel>? FilesClosed;
    public event Action<ConversationViewModel, PermissionRequest>? PermissionAsked;
    public event Action<ConversationViewModel>? AccessChanged;
    public event Action? Changed;

    public IEnumerable<ConversationViewModel> All
    {
        get
        {
            lock (_gate)
                return _sessions.Values.ToList();
        }
    }

    public IEnumerable<string> ChatKeys
    {
        get
        {
            HashSet<string> keys = new(StringComparer.OrdinalIgnoreCase);
            try
            {
                foreach (string file in Directory.GetFiles(_chatDirectory, "*.json"))
                {
                    string name = Path.GetFileNameWithoutExtension(file);
                    if (!string.IsNullOrWhiteSpace(name))
                        keys.Add(name);
                }
            }
            catch (IOException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
            lock (_gate)
            {
                foreach (string key in _sessions.Keys)
                    keys.Add(key);
            }
            return keys;
        }
    }

    public ConversationViewModel Get(string key)
    {
        string canonical = ConversationViewModel.CanonicalKey(key);
        if (canonical.Length == 0)
            canonical = "session";
        ConversationViewModel? created = null;
        lock (_gate)
        {
            ConversationViewModel? existing = FindLocked(canonical);
            if (existing != null)
                return existing;
            ConversationViewModel vm = new(_dataDirectory, Inbox, canonical);
            _sessions[canonical] = vm;
            Wire(vm);
            created = vm;
        }
        SessionCreated?.Invoke(created);
        return created;
    }

    public ConversationViewModel ForIncoming(string remoteAddress, string? peerDeskId = null)
    {
        string desk = Digits(peerDeskId ?? string.Empty);
        string primary = desk.Length == 10 ? desk : remoteAddress;
        lock (_gate)
        {
            ConversationViewModel? live = FindLocked(primary) ?? FindLocked(remoteAddress);
            if (live != null)
            {
                AliasLocked(live, primary);
                AliasLocked(live, remoteAddress);
                return live;
            }
        }

        ConversationViewModel created = Get(primary);
        lock (_gate)
            AliasLocked(created, remoteAddress);
        return created;
    }

    private void AliasLocked(ConversationViewModel live, string key)
    {
        string canonical = ConversationViewModel.CanonicalKey(key);
        if (canonical.Length > 0)
            _sessions[canonical] = live;
    }

    public ConversationViewModel? FindLive(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return null;
        lock (_gate)
        {
            ConversationViewModel? vm = FindLocked(key);
            return vm is { IsLive: true } ? vm : null;
        }
    }

    private ConversationViewModel? FindLocked(string key)
    {
        string canonical = ConversationViewModel.CanonicalKey(key);
        if (_sessions.TryGetValue(canonical, out ConversationViewModel? existing))
            return existing;
        string digits = Digits(canonical);
        string host = EndpointHost(canonical);
        foreach (KeyValuePair<string, ConversationViewModel> pair in _sessions)
        {
            if (digits.Length == 10 && Digits(pair.Key) == digits)
                return pair.Value;
            if (host.Length > 0 &&
                (EndpointHost(pair.Key).Equals(host, StringComparison.OrdinalIgnoreCase)
                 || EndpointHost(pair.Value.RemoteAddress).Equals(host, StringComparison.OrdinalIgnoreCase)
                 || EndpointHost(pair.Value.PeerKey).Equals(host, StringComparison.OrdinalIgnoreCase)))
                return pair.Value;
        }
        return null;
    }

    private static string EndpointHost(string value)
    {
        string text = (value ?? string.Empty).Trim();
        if (text.StartsWith('['))
        {
            int end = text.IndexOf(']');
            if (end > 1)
                return text[1..end];
        }
        int colon = text.LastIndexOf(':');
        if (colon > 1 && text.Contains('.') && int.TryParse(text[(colon + 1)..], out _))
            return text[..colon];
        return text;
    }

    public ConversationViewModel Activate(string key)
    {
        ConversationViewModel vm = FindLive(key) ?? Get(key);
        vm.Activate(key);
        Active = vm;
        return vm;
    }

    public bool IsLive(string key)
    {
        string canonical = ConversationViewModel.CanonicalKey(key);
        string digits = Digits(canonical);
        lock (_gate)
        {
            foreach (KeyValuePair<string, ConversationViewModel> pair in _sessions)
            {
                if (!pair.Value.IsLive)
                    continue;
                if (pair.Key.Equals(canonical, StringComparison.OrdinalIgnoreCase))
                    return true;
                if (digits.Length == 10 && Digits(pair.Key) == digits)
                    return true;
                if (pair.Value.PeerKey.Equals(canonical, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
        }
        return false;
    }

    public bool TryTakeSelfPeer(out ConversationViewModel? session)
    {
        lock (_gate)
        {
            foreach (ConversationViewModel vm in _sessions.Values)
            {
                if (!vm.TakeSelfPeer())
                    continue;
                session = vm;
                return true;
            }
        }
        session = null;
        return false;
    }

    public async ValueTask DisposeAsync()
    {
        List<ConversationViewModel> sessions;
        lock (_gate)
            sessions = _sessions.Values.ToList();
        foreach (ConversationViewModel vm in sessions)
            await vm.DisposeAsync();
        await Shell.DisposeAsync();
    }

    private void Wire(ConversationViewModel vm)
    {
        vm.ChatTouched += key =>
        {
            LastIncomingKey = vm.LastIncomingKey ?? key;
            ChatTouched?.Invoke(key);
        };
        vm.Changed += () => Changed?.Invoke();
        vm.RemoteFrameReceived += (data, width, height) => RemoteFrame?.Invoke(vm, data, width, height);
        vm.RemoteDeltaReceived += data => RemoteDelta?.Invoke(vm, data);
        vm.RemoteH264Received += (data, width, height) => RemoteH264?.Invoke(vm, data, width, height);
        vm.RemoteRegionReceived += (data, x, y, width, height) => RemoteRegion?.Invoke(vm, data, x, y, width, height);
        vm.RemoteCursorReceived += (x, y, visible, hotX, hotY, png) => RemoteCursor?.Invoke(vm, x, y, visible, hotX, hotY, png);
        vm.ShowPanel += panel => Panel?.Invoke(vm, panel);
        vm.ViewEnded += () => ViewEnded?.Invoke(vm);
        vm.FilesClosed += () => FilesClosed?.Invoke(vm);
        vm.PermissionAsked += request => PermissionAsked?.Invoke(vm, request);
        vm.AccessChanged += () => AccessChanged?.Invoke(vm);
    }

    private static string Digits(string value) => new string((value ?? string.Empty).Where(char.IsDigit).ToArray());
}
