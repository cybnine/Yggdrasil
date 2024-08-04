using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace Yggdrasil.Core.Security.Encryption;

public class AesEncryption
{
    private byte[] _key;
    private readonly object _keyLock = new object();
    private int _messageCount;
    private DateTime _lastRotation;
    private const int MaxMessagesPerKey = 1000;
    private const int KeyRotationIntervalMinutes = 30;

    public AesEncryption(byte[] initialKey)
    {
        _key = initialKey;
        _lastRotation = DateTime.UtcNow;
    }

    public (byte[] ciphertext, byte[] nonce) Encrypt(byte[] plaintext)
    {
        lock (_keyLock)
        {
            CheckAndRotateKey();
            var nonce = new byte[12];
            new SecureRandom().NextBytes(nonce);

            var cipher = new GcmBlockCipher(new AesEngine());
            var parameters = new AeadParameters(new KeyParameter(_key), 128, nonce);
            cipher.Init(true, parameters);

            var ciphertext = new byte[cipher.GetOutputSize(plaintext.Length)];
            var len = cipher.ProcessBytes(plaintext, 0, plaintext.Length, ciphertext, 0);
            cipher.DoFinal(ciphertext, len);

            return (ciphertext, nonce);
        }
    }

    public byte[] Decrypt(byte[] ciphertext, byte[] nonce)
    {
        lock (_keyLock)
        {
            CheckAndRotateKey();
            var cipher = new GcmBlockCipher(new AesEngine());
            var parameters = new AeadParameters(new KeyParameter(_key), 128, nonce);
            cipher.Init(false, parameters);

            var plaintext = new byte[cipher.GetOutputSize(ciphertext.Length)];
            var len = cipher.ProcessBytes(ciphertext, 0, ciphertext.Length, plaintext, 0);
            cipher.DoFinal(plaintext, len);

            return plaintext;
        }
    }

    private void CheckAndRotateKey()
    {
        _messageCount++;
        if (_messageCount >= MaxMessagesPerKey || (DateTime.UtcNow - _lastRotation).TotalMinutes >= KeyRotationIntervalMinutes)
        {
            _key = GenerateKey();
            _messageCount = 0;
            _lastRotation = DateTime.UtcNow;
        }
    }

    public static byte[] GenerateKey()
    {
        var key = new byte[32]; // 256 bits
        new SecureRandom().NextBytes(key);
        return key;
    }
}