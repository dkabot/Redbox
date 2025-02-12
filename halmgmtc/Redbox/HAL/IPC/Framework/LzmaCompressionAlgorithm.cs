using Redbox.HAL.Component.Model;
using SevenZip.Compression.LZMA;

namespace Redbox.HAL.IPC.Framework
{
    internal sealed class LzmaCompressionAlgorithm : ICompressionAlgorithm
    {
        public byte[] Compress(byte[] source) => SevenZipHelper.Compress(source);

        public byte[] Decompress(byte[] source) => SevenZipHelper.Decompress(source);
    }
}