using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Kdock.Wire;

namespace Kdock.App;

public sealed class BeaconEventArgs : EventArgs
{
    public required PersonRecord Person { get; init; }
}

public sealed class NetworkDiscovery : IAsyncDisposable
{
    private readonly AppState _state;
    private readonly UdpClient _receiver = new(AddressFamily.InterNetwork);
    private readonly UdpClient _sender = new(AddressFamily.InterNetwork);
    private readonly ConcurrentDictionary<string, DateTime> _lastSeen = new(StringComparer.OrdinalIgnoreCase);
    private readonly CancellationTokenSource _stop = new();
    private Task? _receiveTask;
    private Task? _broadcastTask;
    private bool _started;

    public NetworkDiscovery(AppState state)
    {
        _state = state;
        _receiver.EnableBroadcast = true;
        _sender.EnableBroadcast = true;
        _receiver.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        _receiver.Client.Bind(new IPEndPoint(IPAddress.Any, Protocol.BeaconPort));
    }

    public event EventHandler<BeaconEventArgs>? BeaconReceived;

    public void Start()
    {
        if (_started)
            return;
        _started = true;
        _receiveTask = ReceiveLoopAsync(_stop.Token);
        _broadcastTask = BroadcastLoopAsync(_stop.Token);
    }

    private async Task BroadcastLoopAsync(CancellationToken cancellationToken)
    {
        using PeriodicTimer timer = new(TimeSpan.FromSeconds(2));
        while (await timer.WaitForNextTickAsync(cancellationToken).ConfigureAwait(false))
        {
            try
            {
                Beacon beacon = new()
                {
                    Protocol = Protocol.Current,
                    Product = Protocol.Product,
                    DeskId = _state.DeskId,
                    MachineName = Environment.MachineName
                };
                byte[] payload = JsonSerializer.SerializeToUtf8Bytes(beacon);
                await _sender.SendAsync(payload, new IPEndPoint(IPAddress.Broadcast, Protocol.BeaconPort), cancellationToken).ConfigureAwait(false);
                ExpireStale();
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                break;
            }
            catch (SocketException)
            {
                ExpireStale();
            }
        }
    }

    private async Task ReceiveLoopAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                UdpReceiveResult result = await _receiver.ReceiveAsync(cancellationToken).ConfigureAwait(false);
                Beacon? beacon = JsonSerializer.Deserialize<Beacon>(result.Buffer);
                if (beacon == null || beacon.Protocol != Protocol.Current || string.IsNullOrWhiteSpace(beacon.DeskId) || beacon.DeskId == _state.DeskId)
                    continue;

                DateTime now = DateTime.UtcNow;
                _lastSeen[beacon.DeskId] = now;
                BeaconReceived?.Invoke(this, new BeaconEventArgs
                {
                    Person = new PersonRecord
                    {
                        DeskId = beacon.DeskId,
                        DisplayName = string.IsNullOrWhiteSpace(beacon.MachineName) ? beacon.DeskId : beacon.MachineName,
                        LastAddress = result.RemoteEndPoint.Address.ToString(),
                        Presence = Presence.Online,
                        LastSeenUtc = now
                    }
                });
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                break;
            }
            catch (JsonException)
            {
            }
            catch (SocketException)
            {
                if (!cancellationToken.IsCancellationRequested)
                    await Task.Delay(500, cancellationToken).ConfigureAwait(false);
            }
        }
    }

    private void ExpireStale()
    {
        DateTime cutoff = DateTime.UtcNow.AddSeconds(-6);
        foreach (KeyValuePair<string, DateTime> item in _lastSeen)
        {
            if (item.Value >= cutoff || !_lastSeen.TryRemove(item.Key, out _))
                continue;

            BeaconReceived?.Invoke(this, new BeaconEventArgs
            {
                Person = new PersonRecord
                {
                    DeskId = item.Key,
                    Presence = Presence.Offline,
                    LastSeenUtc = item.Value
                }
            });
        }
    }

    public async ValueTask DisposeAsync()
    {
        _stop.Cancel();
        _receiver.Dispose();
        _sender.Dispose();
        if (_receiveTask != null)
            await IgnoreCancellation(_receiveTask).ConfigureAwait(false);
        if (_broadcastTask != null)
            await IgnoreCancellation(_broadcastTask).ConfigureAwait(false);
        _stop.Dispose();
    }

    private static async Task IgnoreCancellation(Task task)
    {
        try { await task.ConfigureAwait(false); }
        catch (OperationCanceledException) { }
        catch (ObjectDisposedException) { }
    }

    private sealed class Beacon
    {
        public int Protocol { get; set; }
        public string Product { get; set; } = string.Empty;
        public string DeskId { get; set; } = string.Empty;
        public string MachineName { get; set; } = string.Empty;
    }
}
