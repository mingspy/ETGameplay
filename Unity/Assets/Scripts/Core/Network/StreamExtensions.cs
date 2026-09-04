using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ET
{
    
    public static class StreamExtensions
    {
        /// <summary>
        /// 兼容 .NET Standard 2.1 的 ReadExactly 实现
        /// </summary>
        public static void ReadExactly(this Stream stream, byte[] buffer, int offset, int count)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            if (buffer == null) throw new ArgumentNullException(nameof(buffer));
            if (offset < 0 || count < 0 || offset + count > buffer.Length)
                throw new ArgumentOutOfRangeException();

            int totalRead = 0;
            while (totalRead < count)
            {
                int read = stream.Read(buffer, offset + totalRead, count - totalRead);
                if (read == 0)
                {
                    // 流意外结束，抛出异常，模拟 ReadExactly 的行为
                    throw new EndOfStreamException($"Unable to read beyond the end of the stream. Expected {count} bytes, but only read {totalRead}.");
                }
                totalRead += read;
            }
        }

        /// <summary>
        /// 异步版本
        /// </summary>
        public static async Task ReadExactlyAsync(this Stream stream, byte[] buffer, int offset, int count, CancellationToken cancellationToken = default)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            if (buffer == null) throw new ArgumentNullException(nameof(buffer));
            if (offset < 0 || count < 0 || offset + count > buffer.Length)
                throw new ArgumentOutOfRangeException();

            int totalRead = 0;
            while (totalRead < count)
            {
                int read = await stream.ReadAsync(buffer, offset + totalRead, count - totalRead, cancellationToken).ConfigureAwait(false);
                if (read == 0)
                {
                    throw new EndOfStreamException($"Unable to read beyond the end of the stream. Expected {count} bytes, but only read {totalRead}.");
                }
                totalRead += read;
            }
        }
    }

}