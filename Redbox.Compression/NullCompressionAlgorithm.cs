using System.IO;

namespace Redbox.Compression
{
    public class NullCompressionAlgorithm : CompressionAlgorithm
    {
        public override byte[] Compress(byte[] source)
        {
            return source;
        }

        public override byte[] Decompress(byte[] source)
        {
            return source;
        }

        public override void Compress(Stream source, Stream target)
        {
            var buffer = new byte[4096];
            for (var count = source.Read(buffer, 0, buffer.Length);
                 count > 0;
                 count = source.Read(buffer, 0, buffer.Length))
                target.Write(buffer, 0, count);
        }

        public override void Decompress(Stream source, Stream target)
        {
            var buffer = new byte[4096];
            for (var count = source.Read(buffer, 0, buffer.Length);
                 count > 0;
                 count = source.Read(buffer, 0, buffer.Length))
                target.Write(buffer, 0, count);
        }
    }
}