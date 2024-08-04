using System.Net;
using Yggdrasil.Core.Network.Connection;
using Yggdrasil.Core.Network.Protocol.Messages;
using Yggdrasil.Core.Network.Sockets;
using Yggdrasil.Core.Security.Encryption;
using Yggdrasil.Core.Security.Integrity;
using Yggdrasil.Core.Support.Logging;

namespace Yggdrasil.Asgard;

public class Server
{
    private byte[] _rsaPublicKey;
    private string _rsaPrivateKey;
    private AesEncryption _aes;
    private HmacHelper _hmac;
    private readonly TcpSocketConnection _listener;
    private readonly ConnectionManager _connectionManager;
    private readonly CancellationTokenSource _cts = new();
    private readonly Logger _logger = Logger.Instance;

    public Server(IPEndPoint endPoint, bool verboseLogging = false)
    {
        _logger.VerboseLoggingEnabled = verboseLogging;
        _connectionManager = new ConnectionManager();

        var keyPair = RsaHelper.GenerateKeyPair();
        _rsaPublicKey = Convert.FromBase64String(keyPair.Item1);
        _rsaPrivateKey = keyPair.Item2;
        _logger.Log(LogLevel.Info, "Server RSA key pair generated.");
        _logger.LogVerbose($"Server public key: {keyPair.Item1}");

        _listener = new TcpSocketConnection();
        _logger.Log(LogLevel.Info, $"Server initializing on endpoint: {endPoint}");
        _listener.ListenAsync(endPoint, _cts.Token).Wait();
        _logger.Log(LogLevel.Info, "Server is now listening for connections");
    }

    public async Task StartAsync()
    {
        _logger.Log(LogLevel.Info, "Server starting to accept client connections");
        while (!_cts.IsCancellationRequested)
        {
            var clientConnection = await _listener.AcceptAsync(_cts.Token);
            _logger.Log(LogLevel.Info, $"New client connected: {clientConnection.RemoteEndPoint}");
            _ = HandleClientAsync(clientConnection);
        }
    }

    private async Task HandleClientAsync(ISocketConnection client)
    {
        try
        {
            _logger.Log(LogLevel.Debug, $"Handling new client: {client.RemoteEndPoint}");

            // Receive client's handshake
            var (_, handshakeData) = await client.ReceiveAsync(_cts.Token);
            var clientHandshake = new MessageHandshake();
            clientHandshake.Deserialize(handshakeData);
            _logger.Log(LogLevel.Debug, $"Received client handshake from {client.RemoteEndPoint}");
            _logger.LogVerbose($"Client handshake data: {Convert.ToBase64String(handshakeData)}");

            // Send server's handshake with RSA public key
            var serverHandshake = new MessageHandshake(1, _rsaPublicKey);
            await client.SendAsync(serverHandshake.Serialize(), _cts.Token);
            _logger.Log(LogLevel.Debug, $"Sent server handshake to {client.RemoteEndPoint}");

            // Generate AES and HMAC keys
            var aesKey = AesEncryption.GenerateKey();
            var hmacKey = AesEncryption.GenerateKey();
            _logger.Log(LogLevel.Debug, $"Generated AES and HMAC keys for {client.RemoteEndPoint}");
            _logger.LogVerbose($"Generated AES key: {Convert.ToBase64String(aesKey)}");
            _logger.LogVerbose($"Generated HMAC key: {Convert.ToBase64String(hmacKey)}");

            // Encrypt and send AES and HMAC keys
            var keys = aesKey.Concat(hmacKey).ToArray();
            string clientPublicKeyString = Convert.ToBase64String(clientHandshake.RsaPublicKey);
            var encryptedKeys = RsaHelper.Encrypt(keys, clientPublicKeyString);
            await client.SendAsync(encryptedKeys, _cts.Token);
            _logger.Log(LogLevel.Debug, $"Sent encrypted AES and HMAC keys to {client.RemoteEndPoint}");

            // Initialize AES encryption and HMAC
            _aes = new AesEncryption(aesKey);
            _hmac = new HmacHelper(hmacKey);
            _logger.Log(LogLevel.Debug, $"Initialized AES encryption and HMAC for {client.RemoteEndPoint}");

            while (!_cts.IsCancellationRequested)
            {
                var (_, receivedData) = await client.ReceiveAsync(_cts.Token);
                _logger.Log(LogLevel.Debug, $"Received encrypted data from {client.RemoteEndPoint}");
                _logger.LogVerbose($"Received encrypted data: {Convert.ToBase64String(receivedData)}");

                // Separate the encrypted data and nonce
                var nonce = receivedData.Take(12).ToArray();
                var encryptedData = receivedData.Skip(12).ToArray();
                _logger.LogVerbose($"Nonce: {Convert.ToBase64String(nonce)}");
                _logger.LogVerbose($"Encrypted data: {Convert.ToBase64String(encryptedData)}");

                var decryptedData = _aes.Decrypt(encryptedData, nonce);
                _logger.Log(LogLevel.Debug, $"Decrypted data from {client.RemoteEndPoint}");
                _logger.LogVerbose($"Decrypted data: {Convert.ToBase64String(decryptedData)}");

                var message = new GenericMessage();
                message.Deserialize(decryptedData);
                _logger.Log(LogLevel.Info, $"Received message from {client.RemoteEndPoint}: {message.Content}");

                // Echo the message back
                var responseData = message.Serialize();
                var (encryptedResponse, responseNonce) = _aes.Encrypt(responseData);
                await client.SendAsync(responseNonce.Concat(encryptedResponse).ToArray(), _cts.Token);
                _logger.Log(LogLevel.Debug, $"Sent encrypted response to {client.RemoteEndPoint}");
                _logger.LogVerbose(
                    $"Sent encrypted response: {Convert.ToBase64String(responseNonce.Concat(encryptedResponse).ToArray())}");
            }
        }
        catch (Exception ex)
        {
            _logger.Log(LogLevel.Error, $"Error handling client {client.RemoteEndPoint}: {ex.Message}");
        }
        finally
        {
            _connectionManager.ReturnConnection(client.RemoteEndPoint, client);
            _logger.Log(LogLevel.Info, $"Client connection returned to pool: {client.RemoteEndPoint}");
        }
    }

    public async Task StopAsync()
    {
        _logger.Log(LogLevel.Info, "Stopping server");
        _cts.Cancel();
        await _connectionManager.CloseAllConnectionsAsync();
        await _listener.DisconnectAsync();
        _logger.Log(LogLevel.Info, "Server stopped");
    }
}