namespace Yggdrasil.Core.Network.Framing;

    public class DelimiterFramer : IFramer
    {
        private readonly byte[] _delimiter;

        public DelimiterFramer(byte[] delimiter)
        {
            _delimiter = delimiter ?? throw new ArgumentNullException(nameof(delimiter));
        }

        public async Task<byte[]> FrameMessageAsync(byte[] message, CancellationToken cancellationToken = default)
        {
            byte[] framedMessage = new byte[message.Length + _delimiter.Length];
            Buffer.BlockCopy(message, 0, framedMessage, 0, message.Length);
            Buffer.BlockCopy(_delimiter, 0, framedMessage, message.Length, _delimiter.Length);
            return framedMessage;
        }

        public async Task<byte[]> ReadMessageAsync(Stream stream, CancellationToken cancellationToken = default)
        {
            using MemoryStream messageStream = new MemoryStream();
            byte[] buffer = new byte[1];
            int delimiterIndex = 0;

            while (true)
            {
                int bytesRead = await stream.ReadAsync(buffer, 0, 1, cancellationToken);
                if (bytesRead == 0)
                    throw new EndOfStreamException("Connection closed while reading message.");

                if (buffer[0] == _delimiter[delimiterIndex])
                {
                    delimiterIndex++;
                    if (delimiterIndex == _delimiter.Length)
                    {
                        // We've found the full delimiter
                        return messageStream.ToArray();
                    }
                }
                else
                {
                    // If we were in the middle of a potential delimiter, write those bytes
                    if (delimiterIndex > 0)
                    {
                        await messageStream.WriteAsync(_delimiter, 0, delimiterIndex, cancellationToken);
                        delimiterIndex = 0;
                    }
                    await messageStream.WriteAsync(buffer, 0, 1, cancellationToken);
                }
            }
        }
    }