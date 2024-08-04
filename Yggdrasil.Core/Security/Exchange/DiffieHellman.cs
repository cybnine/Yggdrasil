using System.Security.Cryptography;

namespace Yggdrasil.Core.Security.Exchange;

public class DiffieHellman : IDisposable
{
    private ECDiffieHellman _dh;
    
    public DiffieHellman()
    {
        _dh = ECDiffieHellman.Create();
    }

    public byte[] GetPublicKey()
    {
        return _dh.PublicKey.ExportSubjectPublicKeyInfo();
    }

    public byte[] DeriveKey(byte[] otherPartyPublicKey)
    {
        using var otherPartyPublicKeyImported = ECDiffieHellman.Create();
        otherPartyPublicKeyImported.ImportSubjectPublicKeyInfo(otherPartyPublicKey, out _);
        return _dh.DeriveKeyMaterial(otherPartyPublicKeyImported.PublicKey);
    }

    public void Dispose()
    {
        _dh?.Dispose();
    }
}