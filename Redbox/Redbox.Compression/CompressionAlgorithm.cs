using System.IO;

namespace Redbox.Compression
{
    public abstract class CompressionAlgorithm
    {
        public static CompressionAlgorithm GetAlgorithm(CompressionType type)
        {
            switch (type)
            {
                case CompressionType.None:
                    return new NullCompressionAlgorithm();
                case CompressionType.GZip:
                    return new GZipCompressionAlgorithm();
                case CompressionType.Zip:
                    return new ZipCompressionAlgorithm();
                case CompressionType.BZip2:
                    return new BZip2CompressionAlgorithm();
                case CompressionType.LZMA:
                    return new LzmaCompressionAlgorithm();
                default:
                    return new NullCompressionAlgorithm();
            }
        }

        public abstract byte[] Compress(byte[] source);

        public abstract byte[] Decompress(byte[] source);

        public abstract void Compress(Stream source, Stream target);

        public abstract void Decompress(Stream source, Stream target);
    }
}