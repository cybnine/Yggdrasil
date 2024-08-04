namespace Yggdrasil.Core.Network.Framing;

public class LengthPrefixFramer : IFramer
{
    public async Task<byte[]> FrameMessageAsync(byte[] message, CancellationToken cancellationToken = default)
    {
        byte[] lengthPrefix = BitConverter.GetBytes(message.Length);
        byte[] framedMessage = new byte[lengthPrefix.Length + message.Length];
        Buffer.BlockCopy(lengthPrefix, 0, framedMessage, 0, lengthPrefix.Length);
        Buffer.BlockCopy(message, 0, framedMessage, lengthPrefix.Length, message.Length);
        return framedMessage;
    }

    
    public async Task<byte[]> ReadMessageAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        byte[] lengthBuffer = new byte[4];
        await stream.ReadAsync(lengthBuffer, 0, 4, cancellationToken);
        int messageLength = BitConverter.ToInt32(lengthBuffer, 0);

        byte[] messageBuffer = new byte[messageLength];
        int bytesRead = 0;
        while (bytesRead < messageLength)
        {
            int read = await stream.ReadAsync(messageBuffer, bytesRead, messageLength - bytesRead, cancellationToken);
            if (read == 0)
                throw new EndOfStreamException("Connection closed while reading message.");
            bytesRead += read;
        }

        return messageBuffer;
    }
}