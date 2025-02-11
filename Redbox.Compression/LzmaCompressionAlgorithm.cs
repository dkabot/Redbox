using System;
using System.IO;
using SevenZip;
using SevenZip.Compression.LZMA;

namespace Redbox.Compression
{
    public class LzmaCompressionAlgorithm : CompressionAlgorithm
    {
        public override byte[] Compress(byte[] source)
        {
            using (var target = new MemoryStream())
            {
                using (var source1 = new MemoryStream(source))
                {
                    Compress(source1, target);
                    return target.ToArray();
                }
            }
        }

        public override byte[] Decompress(byte[] source)
        {
            using (var target = new MemoryStream())
            {
                using (var source1 = new MemoryStream(source))
                {
                    Decompress(source1, target);
                    return target.ToArray();
                }
            }
        }

        public override void Compress(Stream source, Stream target)
        {
            var propIDs = new CoderPropID[8]
            {
                CoderPropID.DictionarySize,
                CoderPropID.PosStateBits,
                CoderPropID.LitContextBits,
                CoderPropID.LitPosBits,
                CoderPropID.Algorithm,
                CoderPropID.NumFastBytes,
                CoderPropID.MatchFinder,
                CoderPropID.EndMarker
            };
            var properties = new object[8]
            {
                2097152,
                2,
                4,
                0,
                2,
                273,
                "bt4",
                true
            };
            var encoder = new Encoder();
            encoder.SetCoderProperties(propIDs, properties);
            encoder.WriteCoderProperties(target);
            var length = source.Length;
            for (var index = 0; index < 8; ++index)
                target.WriteByte((byte)(length >> (8 * index)));
            encoder.Code(source, target, -1L, -1L, null);
        }

        public override void Decompress(Stream source, Stream target)
        {
            var numArray = new byte[5];
            if (source.Read(numArray, 0, 5) != 5)
                throw new Exception("input .lzma is too short");
            var decoder = new Decoder();
            decoder.SetDecoderProperties(numArray);
            long outSize = 0;
            for (var index = 0; index < 8; ++index)
            {
                var num = source.ReadByte();
                if (num < 0)
                    throw new Exception("Can't Read 1");
                outSize |= (long)(byte)num << (8 * index);
            }

            var inSize = source.Length - source.Position;
            decoder.Code(source, target, inSize, outSize, null);
        }
    }
}