using Yggdrasil.Core.Foundation.Base;
using Yggdrasil.Core.Foundation.Enums;
using Yggdrasil.Core.Network.Protocol.Headers;

namespace Yggdrasil.Core.Network.Protocol.Messages;

public class MessageHandshake : MessageBase
{
    public byte[] RsaPublicKey { get; set; }
    public string Version { get; set; }
    public string Algorithm { get; set; }

    public override MessageType MessageType => MessageType.Handshake;
    
    public MessageHandshake(int protocolVersion = 1, byte[] rsaPublicKey = null) : base(new HandshakeHeader(protocolVersion))
    {
        RsaPublicKey = rsaPublicKey ?? Array.Empty<byte>();
        Version = "1.0";
        Algorithm = "AES-256";
    }
    
    protected override byte[] SerializeWithoutHash()
    {
        using (MemoryStream ms = new MemoryStream())
        using (BinaryWriter writer = new BinaryWriter(ms))
        {
            writer.Write(MessageId.ToByteArray());
            writer.Write(Timestamp.ToBinary());
            writer.Write((int)MessageType);
            byte[] headerData = Header.Serialize();
            writer.Write(headerData.Length);
            writer.Write(headerData);
            writer.Write(RsaPublicKey.Length);
            writer.Write(RsaPublicKey);
            writer.Write(Version);
            writer.Write(Algorithm);
            return ms.ToArray();
        }
    }

    public override void Deserialize(byte[] data)
    {
        using (MemoryStream ms = new MemoryStream(data))
        using (BinaryReader reader = new BinaryReader(ms))
        {
            byte[] guidBytes = reader.ReadBytes(16);
            if (guidBytes.Length != 16)
            {
                throw new InvalidDataException("Invalid GUID data length");
            }
            MessageId = new Guid(guidBytes);
        
            Timestamp = DateTime.FromBinary(reader.ReadInt64());
        
            MessageType deserializedType = (MessageType)reader.ReadInt32();
            if (deserializedType != MessageType)
            {
                throw new InvalidDataException($"Expected message type {MessageType}, but got {deserializedType}");
            }
        
            int headerLength = reader.ReadInt32();
            byte[] headerData = reader.ReadBytes(headerLength);
            Header.Deserialize(headerData);
        
            int publicKeyLength = reader.ReadInt32();
            RsaPublicKey = reader.ReadBytes(publicKeyLength);
        
            Version = reader.ReadString();
            Algorithm = reader.ReadString();
        
            if (ms.Position < ms.Length)
            {
                Hash = reader.ReadString();
            }
        }
    }

    
    // public MessageHandshake(int protocolVersion, byte[] publicKey, string version, string algorithm) 
    //     : this(protocolVersion)
    // {
    //     PublicKey = publicKey;
    //     Version = version;
    //     Algorithm = algorithm;
    // }
}