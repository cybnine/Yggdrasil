namespace Yggdrasil.Core.Foundation.Contracts;

public interface ISerializable
{
    byte[] Serialize();
    void Deserialize(byte[] data);
}