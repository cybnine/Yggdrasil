using Yggdrasil.Core.Foundation.Contracts;

namespace Yggdrasil.Core.Security.Obfuscation;

public class Xor : IObfuscation
{
    private readonly byte[] _key;

    public Xor(byte[] key)
    {
        _key = key;
    }

    public byte[] Obfuscate(byte[] data)
    {
        return Apply(data);
    }

    public byte[] Deobfuscate(byte[] data)
    {
        return Apply(data);
    }

    private byte[] Apply(byte[] data)
    {
        byte[] result = new byte[data.Length];

        for (int i = 0; i < data.Length; i++)
        {
            result[i] = (byte)(data[i] ^ _key[i % _key.Length]);
        }

        return result;
    }
}