using Yggdrasil.Core.Foundation.Enums;

namespace Yggdrasil.Core.Foundation.Contracts;

public interface IMessage : ISerializable
{
    Guid MessageId { get; }
    DateTime Timestamp { get; }
    MessageType MessageType { get; }
    IHeader Header { get; }
}