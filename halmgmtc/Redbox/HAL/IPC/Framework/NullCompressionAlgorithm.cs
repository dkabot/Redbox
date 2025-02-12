using Redbox.HAL.Component.Model;

namespace Redbox.HAL.IPC.Framework
{
    internal sealed class NullCompressionAlgorithm : ICompressionAlgorithm
    {
        public byte[] Compress(byte[] source) => source;

        public byte[] Decompress(byte[] source) => source;
    }
}