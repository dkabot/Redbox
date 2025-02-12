using Redbox.HAL.Component.Model;

namespace Redbox.HAL.IPC.Framework
{
    public static class CompressionAlgorithm
    {
        public static ICompressionAlgorithm GetAlgorithm(CompressionType type)
        {
            switch (type)
            {
                case CompressionType.GZip:
                    return (ICompressionAlgorithm)new GZipCompressionAlgorithm();
                case CompressionType.Zip:
                    return (ICompressionAlgorithm)new ZipCompressionAlgorithm();
                case CompressionType.BZip2:
                    return (ICompressionAlgorithm)new BZip2CompressionAlgorithm();
                case CompressionType.LZMA:
                    return (ICompressionAlgorithm)new LzmaCompressionAlgorithm();
                default:
                    return (ICompressionAlgorithm)new NullCompressionAlgorithm();
            }
        }
    }
}