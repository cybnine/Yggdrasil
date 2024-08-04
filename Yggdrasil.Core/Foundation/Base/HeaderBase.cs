using Yggdrasil.Core.Foundation.Contracts;

namespace Yggdrasil.Core.Foundation.Base;

public abstract class HeaderBase : IHeader
{
    public Guid HeaderId { get; }
    public DateTime Timestamp { get; }
    public int ProtocolVersion { get; }

    protected HeaderBase(int protocolVersion)
    {
        HeaderId = Guid.NewGuid();
        Timestamp = DateTime.UtcNow;
        ProtocolVersion = protocolVersion;
    }

    public abstract byte[] Serialize();
    public abstract void Deserialize(byte[] data);
}