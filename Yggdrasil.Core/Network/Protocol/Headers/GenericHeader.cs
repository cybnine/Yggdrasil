using Yggdrasil.Core.Foundation.Contracts;

namespace Yggdrasil.Core.Network.Protocol.Headers;

public class GenericHeader : IHeader
{
    public Guid HeaderId { get; private set; }
    public DateTime Timestamp { get; private set; }
    public int ProtocolVersion { get; private set; }

    public GenericHeader(int protocolVersion = 1)
    {
        HeaderId = Guid.NewGuid();
        Timestamp = DateTime.UtcNow;
        ProtocolVersion = protocolVersion;
    }

    public byte[] Serialize()
    {
        using (MemoryStream ms = new MemoryStream())
        using (BinaryWriter writer = new BinaryWriter(ms))
        {
            writer.Write(HeaderId.ToByteArray());
            writer.Write(Timestamp.ToBinary());
            writer.Write(ProtocolVersion);
            return ms.ToArray();
        }
    }

    public void Deserialize(byte[] data)
    {
        using (MemoryStream ms = new MemoryStream(data))
        using (BinaryReader reader = new BinaryReader(ms))
        {
            HeaderId = new Guid(reader.ReadBytes(16));
            Timestamp = DateTime.FromBinary(reader.ReadInt64());
            ProtocolVersion = reader.ReadInt32();
        }
    }
}