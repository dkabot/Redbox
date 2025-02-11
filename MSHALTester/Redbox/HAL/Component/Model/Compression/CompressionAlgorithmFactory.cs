namespace Redbox.HAL.Component.Model.Compression;

public static class CompressionAlgorithmFactory
{
    public static ICompressionAlgorithm GetAlgorithm(CompressionType type)
    {
        switch (type)
        {
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
}