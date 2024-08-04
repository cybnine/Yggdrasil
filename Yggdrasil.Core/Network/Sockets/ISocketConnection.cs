using System.Net;

namespace Yggdrasil.Core.Network.Sockets;

public interface ISocketConnection : IDisposable
{
    bool IsConnected { get; }
    IPEndPoint LocalEndPoint { get; }
    IPEndPoint RemoteEndPoint { get; }
    
    Task ConnectAsync(IPEndPoint remoteEndPoint, CancellationToken cancellationToken = default);
    Task DisconnectAsync(CancellationToken cancellationToken = default);
    Task<int> SendAsync(byte[] data, CancellationToken cancellationToken = default);
    Task<(int bytesRead, byte[] data)> ReceiveAsync(CancellationToken cancellationToken = default);
}