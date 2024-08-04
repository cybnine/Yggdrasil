using System.Security.Cryptography;
using Yggdrasil.Core.Foundation.Contracts;
using Yggdrasil.Core.Foundation.Enums;

namespace Yggdrasil.Core.Foundation.Base;

public abstract class MessageBase : IMessage
{
    public Guid MessageId { get; set; }
    public DateTime Timestamp { get; set; }
    public abstract MessageType MessageType { get; }
    public IHeader Header { get; }
    public string Hash { get; set; }
    
    protected MessageBase(IHeader header)
    {
        MessageId = Guid.NewGuid();
        Timestamp = DateTime.UtcNow;
        Header = header ?? throw new ArgumentNullException(nameof(header));
    }
    
    // public string CalculateHash()
    // {
    //     using SHA256 sha256 = SHA256.Create();
    //     byte[] hashBytes = sha256.ComputeHash(this.SerializeWithoutHash());
    //     return Convert.ToBase64String(hashBytes);
    // }
    //
    // public bool VerifyHash(string hash)
    // {
    //     return this.CalculateHash() == hash;
    // }
    //
    public virtual byte[] Serialize()
    {
        return SerializeWithoutHash();
    }

    protected abstract byte[] SerializeWithoutHash();

    public abstract void Deserialize(byte[] data);
}
