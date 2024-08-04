using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;

namespace Yggdrasil.Core.Security.Encryption;

public class RsaHelper
{
    public static (string publicKey, string privateKey) GenerateKeyPair()
    {
        var generator = new RsaKeyPairGenerator();
        generator.Init(new KeyGenerationParameters(new SecureRandom(), 2048));
        var pair = generator.GenerateKeyPair();

        var publicKey = Convert.ToBase64String(SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(pair.Public).GetDerEncoded());
        var privateKey = Convert.ToBase64String(PrivateKeyInfoFactory.CreatePrivateKeyInfo(pair.Private).GetDerEncoded());

        return (publicKey, privateKey);
    }

    public static byte[] Encrypt(byte[] data, string publicKey)
    {
        var cipher = CipherUtilities.GetCipher("RSA/NONE/OAEPWITHSHA256ANDMGF1PADDING");
        var keyParams = PublicKeyFactory.CreateKey(Convert.FromBase64String(publicKey));
        cipher.Init(true, keyParams);
        return cipher.DoFinal(data);
    }

    public static byte[] Decrypt(byte[] data, string privateKey)
    {
        var cipher = CipherUtilities.GetCipher("RSA/NONE/OAEPWITHSHA256ANDMGF1PADDING");
        var keyParams = PrivateKeyFactory.CreateKey(Convert.FromBase64String(privateKey));
        cipher.Init(false, keyParams);
        return cipher.DoFinal(data);
    }
}