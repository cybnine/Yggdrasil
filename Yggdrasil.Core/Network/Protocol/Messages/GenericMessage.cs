using Yggdrasil.Core.Foundation.Base;
using Yggdrasil.Core.Foundation.Contracts;
using Yggdrasil.Core.Foundation.Enums;
using Yggdrasil.Core.Network.Protocol.Headers;

namespace Yggdrasil.Core.Network.Protocol.Messages;

public class GenericMessage : MessageBase
{
    public string Content { get; set; }

    public override MessageType MessageType => MessageType.Generic;

    public GenericMessage(string content = "") : base(new GenericHeader())
    {
        Content = content;
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
            writer.Write(Content ?? "");
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
        
            Content = reader.ReadString();
        
            if (ms.Position < ms.Length)
            {
                Hash = reader.ReadString();
            }
        }
    }
}