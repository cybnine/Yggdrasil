using System;
using System.Net;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using Yggdrasil.Core.Foundation.Enums;
using Yggdrasil.Core.Network.Connection;
using Yggdrasil.Core.Network.Protocol.Messages;
using Yggdrasil.Core.Network.Sockets;
using Yggdrasil.Core.Security.Encryption;
using Yggdrasil.Core.Security.Exchange;
using Yggdrasil.Core.Security.Integrity;
using Yggdrasil.Core.Support.Logging;

public class Client
{
    private readonly CancellationTokenSource _cts = new();
    private byte[] _rsaPublicKey;
    private string _rsaPrivateKey;
    private AesEncryption _aes;
    private HmacHelper _hmac;
    private readonly Logger _logger = Logger.Instance;
    private readonly ConnectionManager _connectionManager;
    private readonly IPEndPoint _serverEndPoint;

    public Client(IPEndPoint serverEndPoint, bool verboseLogging = false)
    {
        _logger.VerboseLoggingEnabled = verboseLogging;
        _serverEndPoint = serverEndPoint;
        _connectionManager = new ConnectionManager();

        var keyPair = RsaHelper.GenerateKeyPair();
        _rsaPublicKey = Convert.FromBase64String(keyPair.Item1);
        _rsaPrivateKey = keyPair.Item2;
        _logger.Log(LogLevel.Info, "Client RSA key pair generated");
        _logger.LogVerbose($"Client public key: {keyPair.Item1}");
    }

    public async Task StartAsync()
    {
        ISocketConnection connection = null;
        try
        {
            _logger.Log(LogLevel.Info, $"Connecting to server at {_serverEndPoint}");
            connection = await _connectionManager.GetConnectionAsync(_serverEndPoint, _cts.Token);
            _logger.Log(LogLevel.Info, "Connected to server");

            // Send handshake with RSA public key
            var handshake = new MessageHandshake(1, _rsaPublicKey);
            await connection.SendAsync(handshake.Serialize(), _cts.Token);
            _logger.Log(LogLevel.Debug, "Sent handshake to server");

            // Receive server's handshake
            var (_, serverHandshakeData) = await connection.ReceiveAsync(_cts.Token);
            var serverHandshake = new MessageHandshake();
            serverHandshake.Deserialize(serverHandshakeData);
            _logger.Log(LogLevel.Debug, "Received server handshake");
            _logger.LogVerbose($"Server handshake data: {Convert.ToBase64String(serverHandshakeData)}");

            // Receive encrypted AES and HMAC keys
            var (_, encryptedKeys) = await connection.ReceiveAsync(_cts.Token);
            var keys = RsaHelper.Decrypt(encryptedKeys, _rsaPrivateKey);
            var aesKey = keys.Take(32).ToArray();
            var hmacKey = keys.Skip(32).ToArray();
            _logger.Log(LogLevel.Debug, "Received and decrypted AES and HMAC keys");
            _logger.LogVerbose($"AES key: {Convert.ToBase64String(aesKey)}");
            _logger.LogVerbose($"HMAC key: {Convert.ToBase64String(hmacKey)}");

            // Initialize AES encryption and HMAC
            _aes = new AesEncryption(aesKey);
            _hmac = new HmacHelper(hmacKey);
            _logger.Log(LogLevel.Info, "Secure connection established");

            while (!_cts.IsCancellationRequested)
            {
                Console.Write("Enter message (or 'exit' to quit): ");
                var input = Console.ReadLine();
                if (input?.ToLower() == "exit") break;

                var message = new GenericMessage(input);
                var serializedMessage = message.Serialize();
                var (encryptedData, nonce) = _aes.Encrypt(serializedMessage);
                _logger.LogVerbose($"Encrypted message: {Convert.ToBase64String(encryptedData)}");
                _logger.LogVerbose($"Nonce: {Convert.ToBase64String(nonce)}");

                await connection.SendAsync(nonce.Concat(encryptedData).ToArray(), _cts.Token);
                _logger.Log(LogLevel.Debug, "Sent encrypted message to server");

                var (_, responseData) = await connection.ReceiveAsync(_cts.Token);
                _logger.Log(LogLevel.Debug, "Received encrypted response from server");

                // Separate the nonce and encrypted data
                var responseNonce = responseData.Take(12).ToArray();
                var responseEncryptedData = responseData.Skip(12).ToArray();
                _logger.LogVerbose($"Response nonce: {Convert.ToBase64String(responseNonce)}");
                _logger.LogVerbose($"Response encrypted data: {Convert.ToBase64String(responseEncryptedData)}");

                var decryptedResponse = _aes.Decrypt(responseEncryptedData, responseNonce);
                var responseMessage = new GenericMessage();
                responseMessage.Deserialize(decryptedResponse);
                _logger.Log(LogLevel.Info, $"Server response: {responseMessage.Content}");
            }
        }
        catch (Exception ex)
        {
            _logger.Log(LogLevel.Error, $"Error in client: {ex.Message}");
        }
        finally
        {
            if (connection != null)
            {
                _connectionManager.ReturnConnection(_serverEndPoint, connection);
                _logger.Log(LogLevel.Debug, "Returned connection to pool");
            }
        }
    }

    public async Task StopAsync()
    {
        _cts.Cancel();
        await _connectionManager.CloseAllConnectionsAsync();
        _logger.Log(LogLevel.Info, "Client stopped");
    }
}