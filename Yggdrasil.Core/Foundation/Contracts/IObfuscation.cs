namespace Yggdrasil.Core.Foundation.Contracts;

public interface IObfuscation
{
    byte[] Obfuscate(byte[] data);
    byte[] Deobfuscate(byte[] data);
}