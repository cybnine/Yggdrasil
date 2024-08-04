using System.Net;
using System.Net.Sockets;

namespace Yggdrasil.Core.Network.Sockets;

public class UDPSocketConnection : ISocketConnection
{
    private Socket _socket;
    private const int BufferSize = 8192;
    private IPEndPoint _remoteEndPoint;

    public bool IsConnected => _socket != null;
    public IPEndPoint LocalEndPoint => _socket?.LocalEndPoint as IPEndPoint;
    public IPEndPoint RemoteEndPoint => _remoteEndPoint;

    public UDPSocketConnection()
    {
        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
    }

    public Task ConnectAsync(IPEndPoint remoteEndPoint, CancellationToken cancellationToken = default)
    {
        _remoteEndPoint = remoteEndPoint;
        _socket.Bind(new IPEndPoint(IPAddress.Any, 0));
        return Task.CompletedTask;
    }

    public Task DisconnectAsync(CancellationToken cancellationToken = default)
    {
        _remoteEndPoint = null;
        return Task.CompletedTask;
    }

    public async Task<int> SendAsync(byte[] data, CancellationToken cancellationToken = default)
    {
        return await _socket.SendToAsync(new ArraySegment<byte>(data), SocketFlags.None, _remoteEndPoint);
    }

    public async Task<(int bytesRead, byte[] data)> ReceiveAsync(CancellationToken cancellationToken = default)
    {
        var buffer = new byte[BufferSize];
        var result = await _socket.ReceiveFromAsync(new ArraySegment<byte>(buffer), SocketFlags.None, _remoteEndPoint);
        return (result.ReceivedBytes, buffer);
    }

    public void Dispose()
    {
        _socket?.Close();
        _socket?.Dispose();
        _socket = null;
    }
}