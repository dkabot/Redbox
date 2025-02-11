using System.IO;

namespace Redbox.Compression
{
    internal abstract class CompressionAlgorithm
    {
        public static CompressionAlgorithm GetAlgorithm(CompressionType type)
        {
            switch (type)
            {
                case CompressionType.None:
                    return (CompressionAlgorithm)new NullCompressionAlgorithm();
                case CompressionType.GZip:
                    return (CompressionAlgorithm)new GZipCompressionAlgorithm();
                case CompressionType.Zip:
                    return (CompressionAlgorithm)new ZipCompressionAlgorithm();
                case CompressionType.BZip2:
                    return (CompressionAlgorithm)new BZip2CompressionAlgorithm();
                case CompressionType.LZMA:
                    return (CompressionAlgorithm)new LzmaCompressionAlgorithm();
                default:
                    return (CompressionAlgorithm)new NullCompressionAlgorithm();
            }
        }

        public abstract byte[] Compress(byte[] source);

        public abstract byte[] Decompress(byte[] source);

        public abstract void Compress(Stream source, Stream target);

        public abstract void Decompress(Stream source, Stream target);
    }
}
