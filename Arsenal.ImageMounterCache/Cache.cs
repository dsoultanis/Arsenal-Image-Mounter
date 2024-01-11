using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arsenal.ImageMounterCache
{
    public class Cache
    {
        public Cache() { }

        public void Initialize()
        {

        }
        public int Read(byte[] buffer, int bufferOffset, int count, long fileOffset)
        {
            return 0;
        }

        public int Read(Span<byte> span, long fileOffset = 0)
        {
            var path = Path.Combine("");
            var _stream = new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
            byte[] buffer = new byte[span.Length];
            _stream.Position = fileOffset;
            _stream.Read(buffer, 0, buffer.Length);
            buffer.CopyTo(span);

            return 0;
        }

        public void Write(byte[] buffer, int bufferOffset, int count, long fileOffset, int bytesRead)
        {

        }
    }
}
