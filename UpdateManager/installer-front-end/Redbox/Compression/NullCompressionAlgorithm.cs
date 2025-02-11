using System.IO;

namespace Redbox.Compression
{
    internal class NullCompressionAlgorithm : CompressionAlgorithm
    {
        public override byte[] Compress(byte[] source) => source;

        public override byte[] Decompress(byte[] source) => source;

        public override void Compress(Stream source, Stream target)
        {
            byte[] buffer = new byte[4096];
            for (int count = source.Read(buffer, 0, buffer.Length); count > 0; count = source.Read(buffer, 0, buffer.Length))
                target.Write(buffer, 0, count);
        }

        public override void Decompress(Stream source, Stream target)
        {
            byte[] buffer = new byte[4096];
            for (int count = source.Read(buffer, 0, buffer.Length); count > 0; count = source.Read(buffer, 0, buffer.Length))
                target.Write(buffer, 0, count);
        }
    }
}
