using Org.BouncyCastle.Crypto.Macs;
using Org.BouncyCastle.Crypto.Parameters;

namespace Yggdrasil.Core.Security.Integrity;

public class HmacHelper
{
    private readonly HMac _hMac;

    public HmacHelper(byte[] key)
    {
        _hMac = new HMac(new Org.BouncyCastle.Crypto.Digests.Sha256Digest());
        _hMac.Init(new KeyParameter(key));
    }

    public byte[] ComputeHmac(byte[] data)
    {
        _hMac.BlockUpdate(data, 0, data.Length);
        byte[] result = new byte[_hMac.GetMacSize()];
        _hMac.DoFinal(result, 0);
        return result;
    }

    public bool VerifyHmac(byte[] data, byte[] expectedHmac)
    {
        byte[] computedHmac = ComputeHmac(data);
        return computedHmac.SequenceEqual(expectedHmac);
    }
}