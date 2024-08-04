namespace Yggdrasil.Core.Network.Framing;

public interface IFramer
{
    Task<byte[]> FrameMessageAsync(byte[] message, CancellationToken cancellationToken = default);
    Task<byte[]> ReadMessageAsync(Stream stream, CancellationToken cancellationToken = default);
}