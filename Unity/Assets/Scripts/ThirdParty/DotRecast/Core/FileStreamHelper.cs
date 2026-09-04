namespace DotRecast.Core
{
    using System;
    using System.IO;

    public static class FileStreamHelper
    {
        /// <summary>
        /// 兼容低版本 .NET 的 ReadExactly 实现
        /// </summary>
        public static void ReadExactly(this FileStream fs, byte[] buffer, int offset, int count)
        {
            if (fs == null) throw new ArgumentNullException(nameof(fs));
            if (buffer == null) throw new ArgumentNullException(nameof(buffer));
            if (offset < 0 || count < 0 || offset + count > buffer.Length)
                throw new ArgumentOutOfRangeException();

            int totalBytesRead = 0;
            while (totalBytesRead < count)
            {
                // Read 可能返回小于请求的字节数，甚至 0（文件结束）
                int bytesRead = fs.Read(buffer, offset + totalBytesRead, count - totalBytesRead);
            
                if (bytesRead == 0)
                {
                    // 如果提前到达文件末尾，说明数据不足，抛出异常（模拟 ReadExactly 行为）
                    throw new EndOfStreamException($"无法从流中读取足够的字节。期望: {count}, 实际读取: {totalBytesRead}");
                }
            
                totalBytesRead += bytesRead;
            }
        }
    }
}