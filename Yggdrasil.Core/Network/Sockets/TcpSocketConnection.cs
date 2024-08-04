using System.Net;
using System.Net.Sockets;
using Yggdrasil.Core.Network.Framing;

namespace Yggdrasil.Core.Network.Sockets;

public class TcpSocketConnection : ISocketConnection
{
    private Socket _socket;
    private NetworkStream _stream;
    private IFramer _framer;

    public bool IsConnected => _socket?.Connected ?? false;
    public IPEndPoint LocalEndPoint => _socket?.LocalEndPoint as IPEndPoint;
    public IPEndPoint RemoteEndPoint => _socket?.RemoteEndPoint as IPEndPoint;

    public TcpSocketConnection(IFramer framer = null)
    {
        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        _framer = framer ?? new LengthPrefixFramer();
    }

    public async Task ConnectAsync(IPEndPoint remoteEndPoint, CancellationToken cancellationToken = default)
    {
        await _socket.ConnectAsync(remoteEndPoint, cancellationToken);
        _stream = new NetworkStream(_socket, true);
    }

    public async Task DisconnectAsync(CancellationToken cancellationToken = default)
    {
        if (IsConnected)
        {
            await Task.Run(() => _socket.Disconnect(false), cancellationToken);
        }

        _stream?.Dispose();
    }

    public async Task<int> SendAsync(byte[] data, CancellationToken cancellationToken = default)
    {
        byte[] framedData = await _framer.FrameMessageAsync(data, cancellationToken);
        await _stream.WriteAsync(framedData, 0, framedData.Length, cancellationToken);
        return framedData.Length;
    }

    public async Task<(int bytesRead, byte[] data)> ReceiveAsync(CancellationToken cancellationToken = default)
    {
        byte[] data = await _framer.ReadMessageAsync(_stream, cancellationToken);
        return (data.Length, data);
    }

    public async Task ListenAsync(IPEndPoint localEndPoint, CancellationToken cancellationToken = default)
    {
        _socket.Bind(localEndPoint);
        _socket.Listen(100);
    }

    public async Task<TcpSocketConnection> AcceptAsync(CancellationToken cancellationToken = default)
    {
        Socket clientSocket = await _socket.AcceptAsync(cancellationToken);
        return new TcpSocketConnection(_framer)
            { _socket = clientSocket, _stream = new NetworkStream(clientSocket, true) };
    }

    public void Dispose()
    {
        _stream?.Dispose();
        _socket?.Dispose();
        _socket = null;
    }
}