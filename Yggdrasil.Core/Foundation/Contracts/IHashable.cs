namespace Yggdrasil.Core.Foundation.Contracts;

public interface IHashable
{
    string CalculateHash();
    bool VerifyHash(string hash);
}