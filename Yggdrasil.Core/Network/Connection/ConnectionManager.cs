using System.Net;
using Yggdrasil.Core.Network.Sockets;
using Yggdrasil.Core.Support.Logging;

namespace Yggdrasil.Core.Network.Connection;

public class ConnectionManager
{
    private readonly ConnectionPool _pool;
    private readonly Logger _logger = Logger.Instance;

    public ConnectionManager(int maxConnectionsPerEndpoint = 10)
    {
        _pool = new ConnectionPool(maxConnectionsPerEndpoint);
    }

    public async Task<ISocketConnection> GetConnectionAsync(IPEndPoint endpoint, CancellationToken cancellationToken = default)
    {
        try
        {
            var connection = await _pool.GetConnectionAsync(endpoint, cancellationToken);
            _logger.Log(LogLevel.Debug, $"Retrieved connection to {endpoint}");
            return connection;
        }
        catch (Exception ex)
        {
            _logger.Log(LogLevel.Error, $"Failed to get connection to {endpoint}: {ex.Message}");
            throw;
        }
    }

    public void ReturnConnection(IPEndPoint endpoint, ISocketConnection connection)
    {
        _pool.ReturnConnection(endpoint, connection);
        _logger.Log(LogLevel.Debug, $"Returned connection to pool for {endpoint}");
    }

    public async Task CloseAllConnectionsAsync(CancellationToken cancellationToken = default)
    {
        await _pool.CloseAllConnectionsAsync(cancellationToken);
        _logger.Log(LogLevel.Info, "Closed all connections in the pool");
    }
}