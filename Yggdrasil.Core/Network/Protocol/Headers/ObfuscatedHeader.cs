using Yggdrasil.Core.Foundation.Contracts;

namespace Yggdrasil.Core.Network.Protocol.Headers;

public class ObfuscatedHeader : IHeader
{
    private readonly IHeader _header;
    private readonly IObfuscation _obfuscation;
    
    public ObfuscatedHeader(IHeader header, IObfuscation obfuscation)
    {
        _header = header;
        _obfuscation = obfuscation;
    }
    
    public Guid HeaderId => _header.HeaderId;
    public DateTime Timestamp => _header.Timestamp;
    public int ProtocolVersion => _header.ProtocolVersion;
    
    public byte[] Serialize()
    {
        byte[] data = _header.Serialize();
        return _obfuscation.Obfuscate(data);
    }
    
    public void Deserialize(byte[] data)
    {
        byte[] deobfuscatedData = _obfuscation.Deobfuscate(data);
        _header.Deserialize(deobfuscatedData);
    }
}