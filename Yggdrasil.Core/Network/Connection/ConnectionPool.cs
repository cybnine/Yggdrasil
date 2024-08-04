using System.Collections.Concurrent;
using System.Net;
using Yggdrasil.Core.Network.Sockets;

namespace Yggdrasil.Core.Network.Connection;

    public class ConnectionPool
    {
        private readonly ConcurrentDictionary<IPEndPoint, ConcurrentQueue<ISocketConnection>> _pool;
        private readonly int _maxConnectionsPerEndpoint;

        public ConnectionPool(int maxConnectionsPerEndpoint = 10)
        {
            _pool = new ConcurrentDictionary<IPEndPoint, ConcurrentQueue<ISocketConnection>>();
            _maxConnectionsPerEndpoint = maxConnectionsPerEndpoint;
        }

        public async Task<ISocketConnection> GetConnectionAsync(IPEndPoint endpoint, CancellationToken cancellationToken = default)
        {
            if (_pool.TryGetValue(endpoint, out var queue))
            {
                if (queue.TryDequeue(out var connection))
                {
                    if (connection.IsConnected)
                    {
                        return connection;
                    }
                    else
                    {
                        await connection.DisconnectAsync(cancellationToken);
                    }
                }
            }

            // Create a new connection if none available
            var newConnection = new TcpSocketConnection();
            await newConnection.ConnectAsync(endpoint, cancellationToken);
            return newConnection;
        }

        public void ReturnConnection(IPEndPoint endpoint, ISocketConnection connection)
        {
            if (!_pool.TryGetValue(endpoint, out var queue))
            {
                queue = new ConcurrentQueue<ISocketConnection>();
                _pool[endpoint] = queue;
            }

            if (queue.Count < _maxConnectionsPerEndpoint)
            {
                queue.Enqueue(connection);
            }
            else
            {
                connection.Dispose();
            }
        }

        public async Task CloseAllConnectionsAsync(CancellationToken cancellationToken = default)
        {
            foreach (var queue in _pool.Values)
            {
                while (queue.TryDequeue(out var connection))
                {
                    await connection.DisconnectAsync(cancellationToken);
                    connection.Dispose();
                }
            }
        }
    }