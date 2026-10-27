using System;
using System.Threading;
using System.Threading.Tasks;
using Kdock.Wire;

namespace Kdock.App;

public interface IWireSession : IAsyncDisposable
{
    bool IsConnected { get; }
    bool ProtocolMatched { get; }
    string PeerDeskId { get; }
    event Action<WirePacket>? PacketReceived;
    event Action<string>? StatusChanged;
    event Action? ProtocolMismatch;
    Task SendAsync(WirePacket packet, CancellationToken cancellationToken = default);
}
