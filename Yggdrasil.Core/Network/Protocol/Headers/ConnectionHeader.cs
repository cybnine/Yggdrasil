using Yggdrasil.Core.Foundation.Base;

namespace Yggdrasil.Core.Network.Protocol.Headers;

public class ConnectionHeader : HeaderBase
{
    public string ClientId { get; set; }
    public string ServerAddress { get; set; }
    public int Port { get; set; }

    public ConnectionHeader(int protocolVersion) : base(protocolVersion) {}

    public override byte[] Serialize()
    {
        throw new NotImplementedException();
    }
    
    public override void Deserialize(byte[] data)
    {
        // Implement deserialization logic
        throw new NotImplementedException();
    }
}