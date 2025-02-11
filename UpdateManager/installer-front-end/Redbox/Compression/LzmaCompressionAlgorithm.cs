using SevenZip;
using SevenZip.Compression.LZMA;
using System;
using System.IO;

namespace Redbox.Compression
{
    internal class LzmaCompressionAlgorithm : CompressionAlgorithm
    {
        public override byte[] Compress(byte[] source)
        {
            using (MemoryStream target = new MemoryStream())
            {
                using (MemoryStream source1 = new MemoryStream(source))
                {
                    this.Compress((Stream)source1, (Stream)target);
                    return target.ToArray();
                }
            }
        }

        public override byte[] Decompress(byte[] source)
        {
            using (MemoryStream target = new MemoryStream())
            {
                using (MemoryStream source1 = new MemoryStream(source))
                {
                    this.Decompress((Stream)source1, (Stream)target);
                    return target.ToArray();
                }
            }
        }

        public override void Compress(Stream source, Stream target)
        {
            CoderPropID[] propIDs = new CoderPropID[8]
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
            object[] properties = new object[8]
            {
        (object) 2097152,
        (object) 2,
        (object) 4,
        (object) 0,
        (object) 2,
        (object) 273,
        (object) "bt4",
        (object) true
            };
            Encoder encoder = new Encoder();
            encoder.SetCoderProperties(propIDs, properties);
            encoder.WriteCoderProperties(target);
            long length = source.Length;
            for (int index = 0; index < 8; ++index)
                target.WriteByte((byte)(length >> 8 * index));
            encoder.Code(source, target, -1L, -1L, (ICodeProgress)null);
        }

        public override void Decompress(Stream source, Stream target)
        {
            byte[] numArray = new byte[5];
            if (source.Read(numArray, 0, 5) != 5)
                throw new Exception("input .lzma is too short");
            Decoder decoder = new Decoder();
            decoder.SetDecoderProperties(numArray);
            long outSize = 0;
            for (int index = 0; index < 8; ++index)
            {
                int num = source.ReadByte();
                if (num < 0)
                    throw new Exception("Can't Read 1");
                outSize |= (long)(byte)num << 8 * index;
            }
            long inSize = source.Length - source.Position;
            decoder.Code(source, target, inSize, outSize, (ICodeProgress)null);
        }
    }
}
