using System;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Kdock.Wire;

namespace Kdock.App;

public sealed class SessionTransport : IWireSession
{
    private readonly TcpClient _client;
    private readonly NetworkStream _stream;
    private readonly SemaphoreSlim _sendLock = new(1, 1);
    private readonly CancellationTokenSource _stop = new();
    private readonly TaskCompletionSource<bool> _hello = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private Task? _receiveTask;
    private int _closed;

    private SessionTransport(TcpClient client)
    {
        _client = client;
        _client.NoDelay = true;
        _stream = client.GetStream();
        RemoteAddress = client.Client.RemoteEndPoint?.ToString() ?? string.Empty;
    }

    public string RemoteAddress { get; }
    public string PeerDeskId { get; private set; } = string.Empty;
    public bool IsConnected => _client.Connected && _stream.CanWrite && Volatile.Read(ref _closed) == 0;
    public bool ProtocolMatched { get; private set; }

    public event Action<WirePacket>? PacketReceived;
    public event Action<string>? StatusChanged;
    public event Action? ProtocolMismatch;

    public static async Task<SessionTransport> ConnectAsync(string host, int port, CancellationToken cancellationToken = default)
    {
        TcpClient client = new();
        await client.ConnectAsync(host, port, cancellationToken).ConfigureAwait(false);
        SessionTransport session = new(client);
        session._receiveTask = Task.Run(() => session.ReceiveLoopAsync(session._stop.Token), cancellationToken);
        await session.SendAsync(WirePacket.Text($"{Protocol.Hello}:{Protocol.Current}:{Protocol.Product}:{App.State.DeskId}")).ConfigureAwait(false);
        try
        {
            await session._hello.Task.WaitAsync(TimeSpan.FromSeconds(5), cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            await session.DisposeAsync().ConfigureAwait(false);
            throw new InvalidOperationException("The remote peer did not complete the Kdock protocol handshake.");
        }
        return session;
    }

    internal static async Task<SessionTransport> AcceptAsync(TcpClient client, CancellationToken cancellationToken = default)
    {
        SessionTransport session = new(client);
        session._receiveTask = Task.Run(() => session.ReceiveLoopAsync(session._stop.Token), cancellationToken);
        await session.SendAsync(WirePacket.Text($"{Protocol.Hello}:{Protocol.Current}:{Protocol.Product}:{App.State.DeskId}"), cancellationToken).ConfigureAwait(false);
        try
        {
            await session._hello.Task.WaitAsync(TimeSpan.FromSeconds(5), cancellationToken).ConfigureAwait(false);
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
            throw new InvalidOperationException("The session is not connected.");

        byte[] frame = WireFrameCodec.Encode(packet);
        await _sendLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            await _stream.WriteAsync(frame.AsMemory(), cancellationToken).ConfigureAwait(false);
            await _stream.FlushAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _sendLock.Release();
        }
    }

    private async Task ReceiveLoopAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                WirePacket? packet = await WireFrameCodec.ReadAsync(_stream, cancellationToken).ConfigureAwait(false);
                if (packet == null)
                    break;

                if (packet.Instruction.StartsWith($"{Protocol.Hello}:", StringComparison.OrdinalIgnoreCase))
                {
                    HandleHello(packet.Instruction);
                    continue;
                }

                if (!ProtocolMatched)
                    continue;
                PacketReceived?.Invoke(packet);
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

    private void HandleHello(string instruction)
    {
        string[] parts = instruction.Split(':', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 3 || !int.TryParse(parts[1], out int protocol) || protocol != Protocol.Current)
        {
            _hello.TrySetException(new InvalidOperationException("Update Kdock on both PCs."));
            ProtocolMismatch?.Invoke();
            Close("Update Kdock on both PCs.");
            return;
        }

        if (parts.Length >= 4)
        {
            string desk = new string(parts[3].Where(char.IsDigit).ToArray());
            if (desk.Length == 10)
                PeerDeskId = desk;
        }
        ProtocolMatched = true;
        _hello.TrySetResult(true);
        StatusChanged?.Invoke("Connected.");
    }

    private void Close(string status)
    {
        if (Interlocked.Exchange(ref _closed, 1) != 0)
            return;
        _stop.Cancel();
        try { _stream.Close(); } catch { }
        try { _client.Close(); } catch { }
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
        _stream.Dispose();
        _client.Dispose();
    }
}
