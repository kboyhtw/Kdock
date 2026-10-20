using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Kdock.Wire;

namespace Kdock.App;

public sealed class SessionAcceptedEventArgs : EventArgs
{
    public required IWireSession Session { get; init; }
    public required string RemoteAddress { get; init; }
}

public sealed class SessionListener : IAsyncDisposable
{
    private readonly TcpListener _listener;
    private readonly CancellationTokenSource _stop = new();
    private Task? _acceptTask;

    public SessionListener(int port = Protocol.LanPort)
    {
        _listener = new TcpListener(IPAddress.Any, port);
    }

    public event EventHandler<SessionAcceptedEventArgs>? SessionAccepted;
    public event Action<string>? StatusChanged;

    public bool IsListening { get; private set; }

    public int Port => _listener.LocalEndpoint is IPEndPoint endpoint ? endpoint.Port : 0;

    public void Start()
    {
        if (IsListening)
            return;
        try
        {
            _listener.Start();
            IsListening = true;
            _acceptTask = AcceptLoopAsync(_stop.Token);
            StatusChanged?.Invoke($"Listening on port {((IPEndPoint)_listener.LocalEndpoint).Port}.");
        }
        catch (SocketException ex)
        {
            StatusChanged?.Invoke($"LAN listener unavailable: {ex.Message}");
        }
    }

    private async Task AcceptLoopAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                TcpClient client = await _listener.AcceptTcpClientAsync(cancellationToken).ConfigureAwait(false);
                _ = HandleClientAsync(client, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                break;
            }
            catch (ObjectDisposedException)
            {
                break;
            }
            catch (SocketException ex)
            {
                if (!cancellationToken.IsCancellationRequested)
                    StatusChanged?.Invoke($"LAN listener error: {ex.Message}");
            }
        }
    }

    private async Task HandleClientAsync(TcpClient client, CancellationToken cancellationToken)
    {
        string remoteAddress = client.Client.RemoteEndPoint?.ToString() ?? string.Empty;
        try
        {
            SessionTransport session = await SessionTransport.AcceptAsync(client, cancellationToken).ConfigureAwait(false);
            SessionAccepted?.Invoke(this, new SessionAcceptedEventArgs { Session = session, RemoteAddress = remoteAddress });
        }
        catch (Exception ex)
        {
            StatusChanged?.Invoke($"Rejected LAN session from {remoteAddress}: {ex.Message}");
            client.Dispose();
        }
    }

    public async ValueTask DisposeAsync()
    {
        _stop.Cancel();
        _listener.Stop();
        if (_acceptTask != null)
        {
            try { await _acceptTask.ConfigureAwait(false); }
            catch (OperationCanceledException) { }
        }
        _stop.Dispose();
    }
}
