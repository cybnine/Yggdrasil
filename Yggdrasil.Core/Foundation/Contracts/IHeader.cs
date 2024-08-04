namespace Yggdrasil.Core.Foundation.Contracts;

public interface IHeader : ISerializable
{
    Guid HeaderId { get; }
    DateTime Timestamp { get; }
    int ProtocolVersion { get; }
}