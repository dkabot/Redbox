using System;
using System.IO;
using System.Runtime.InteropServices;

namespace SevenZip.Compression.LZMA
{
    [ComVisible(true)]
    public static class SevenZipHelper
    {
        private static int dictionary = 8388608;
        private static bool eos;

        private static CoderPropID[] propIDs = new CoderPropID[8]
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

        private static object[] properties = new object[8]
        {
            dictionary,
            2,
            3,
            0,
            2,
            128,
            "bt4",
            eos
        };

        public static byte[] Compress(byte[] inputBytes)
        {
            var inStream = new MemoryStream(inputBytes);
            var outStream = new MemoryStream();
            var encoder = new Encoder();
            encoder.SetCoderProperties(propIDs, properties);
            encoder.WriteCoderProperties(outStream);
            var length = inStream.Length;
            for (var index = 0; index < 8; ++index)
                outStream.WriteByte((byte)(length >> (8 * index)));
            encoder.Code(inStream, outStream, -1L, -1L, null);
            return outStream.ToArray();
        }

        public static byte[] Decompress(byte[] inputBytes)
        {
            var inStream = new MemoryStream(inputBytes);
            var decoder = new Decoder();
            inStream.Seek(0L, SeekOrigin.Begin);
            var outStream = new MemoryStream();
            var numArray = new byte[5];
            if (inStream.Read(numArray, 0, 5) != 5)
                throw new Exception("input .lzma is too short");
            long outSize = 0;
            for (var index = 0; index < 8; ++index)
            {
                var num = inStream.ReadByte();
                if (num < 0)
                    throw new Exception("Can't Read 1");
                outSize |= (long)(byte)num << (8 * index);
            }

            decoder.SetDecoderProperties(numArray);
            var inSize = inStream.Length - inStream.Position;
            decoder.Code(inStream, outStream, inSize, outSize, null);
            return outStream.ToArray();
        }
    }
}