using System;
using System.Buffers.Binary;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Kdock.Wire;

namespace Kdock.App;

public sealed class MeetingSessionTransport : IWireSession
{
    private readonly ClientWebSocket _socket;
    private readonly CancellationTokenSource _stop = new();
    private readonly SemaphoreSlim _sendLock = new(1, 1);
    private readonly TaskCompletionSource<bool> _hello = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly MemoryStream _pending = new();
    private Task? _receiveTask;
    private int _closed;

    private MeetingSessionTransport(ClientWebSocket socket)
    {
        _socket = socket;
    }

    public bool IsConnected => _socket.State == WebSocketState.Open && Volatile.Read(ref _closed) == 0;
    public bool ProtocolMatched { get; private set; }

    public event Action<WirePacket>? PacketReceived;
    public event Action<string>? StatusChanged;
    public event Action? ProtocolMismatch;

    public static async Task<MeetingSessionTransport> ConnectAsync(Uri uri, CancellationToken cancellationToken = default)
    {
        ClientWebSocket socket = new();
        socket.Options.KeepAliveInterval = TimeSpan.FromSeconds(20);
        socket.Options.HttpVersion = HttpVersion.Version11;
        socket.Options.HttpVersionPolicy = HttpVersionPolicy.RequestVersionOrLower;
        await socket.ConnectAsync(uri, cancellationToken).ConfigureAwait(false);
        MeetingSessionTransport session = new(socket);
        session._receiveTask = Task.Run(() => session.ReceiveLoopAsync(session._stop.Token), cancellationToken);
        await session.SendAsync(WirePacket.Text($"{Protocol.Hello}:{Protocol.Current}:{Protocol.Product}")).ConfigureAwait(false);
        try
        {
            await session._hello.Task.WaitAsync(TimeSpan.FromSeconds(30), cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            await session.DisposeAsync().ConfigureAwait(false);
            throw new InvalidOperationException("The remote peer did not complete the Kdock protocol handshake.");
        }
        return session;
    }

    public async Task SendAsync(WirePacket packet, CancellationToken cancellationToken = default)
    {
        if (!IsConnected)
            throw new InvalidOperationException("The meeting session is not connected.");

        byte[] frame = WireFrameCodec.Encode(packet);
        await _sendLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            await _socket.SendAsync(frame, WebSocketMessageType.Binary, true, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _sendLock.Release();
        }
    }

    private async Task ReceiveLoopAsync(CancellationToken cancellationToken)
    {
        byte[] buffer = new byte[64 * 1024];
        try
        {
            while (!cancellationToken.IsCancellationRequested && _socket.State == WebSocketState.Open)
            {
                using MemoryStream message = new();
                WebSocketReceiveResult result;
                do
                {
                    result = await _socket.ReceiveAsync(buffer, cancellationToken).ConfigureAwait(false);
                    if (result.MessageType == WebSocketMessageType.Close)
                        return;
                    message.Write(buffer, 0, result.Count);
                }
                while (!result.EndOfMessage);

                AcceptBytes(message.ToArray());
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception ex)
        {
            StatusChanged?.Invoke(ex.Message);
        }
        finally
        {
            Close("Disconnected.");
        }
    }

    private void AcceptBytes(byte[] data)
    {
        _pending.Write(data, 0, data.Length);
        while (_pending.Length >= 4)
        {
            byte[] all = _pending.ToArray();
            int length = BinaryPrimitives.ReadInt32LittleEndian(all);
            if (length <= 0 || length > WireFrameCodec.MaximumPayloadLength)
            {
                _pending.SetLength(0);
                return;
            }
            if (all.Length < 4 + length)
                return;
            if (WireFrameCodec.TryDecode(all.AsSpan(4, length), out WirePacket packet))
                HandlePacket(packet);
            byte[] rest = all[(4 + length)..];
            _pending.SetLength(0);
            if (rest.Length > 0)
                _pending.Write(rest, 0, rest.Length);
        }
    }

    private void HandlePacket(WirePacket packet)
    {
        if (packet.Instruction.StartsWith($"{Protocol.Hello}:", StringComparison.OrdinalIgnoreCase))
        {
            string[] parts = packet.Instruction.Split(':', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 3 || !int.TryParse(parts[1], out int version) || version != Protocol.Current)
            {
                _hello.TrySetException(new InvalidOperationException("Update Kdock on both PCs."));
                ProtocolMismatch?.Invoke();
                Close("Update Kdock on both PCs.");
                return;
            }
            ProtocolMatched = true;
            _hello.TrySetResult(true);
            StatusChanged?.Invoke("Connected.");
            return;
        }

        if (ProtocolMatched)
            PacketReceived?.Invoke(packet);
    }

    private void Close(string status)
    {
        if (Interlocked.Exchange(ref _closed, 1) != 0)
            return;
        _stop.Cancel();
        try { _socket.Abort(); } catch { }
        StatusChanged?.Invoke(status);
    }

    public async ValueTask DisposeAsync()
    {
        Close("Disconnected.");
        if (_receiveTask != null)
        {
            try { await _receiveTask.ConfigureAwait(false); }
            catch (OperationCanceledException) { }
        }
        _sendLock.Dispose();
        _stop.Dispose();
        _socket.Dispose();
    }
}
