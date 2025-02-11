using ICSharpCode.SharpZipLib.GZip;
using System;
using System.IO;

namespace Redbox.Compression
{
    internal class GZipCompressionAlgorithm : CompressionAlgorithm
    {
        private const int BufferSize = 1024000;

        public override byte[] Compress(byte[] source)
        {
            byte[] destinationArray;
            using (MemoryStream input = new MemoryStream(source))
            {
                BinaryReader binaryReader = new BinaryReader((Stream)input);
                using (MemoryStream baseOutputStream = new MemoryStream())
                {
                    byte[] buffer = new byte[1024000];
                    using (GZipOutputStream gzipOutputStream = new GZipOutputStream((Stream)baseOutputStream))
                    {
                        while (true)
                        {
                            int count = binaryReader.Read(buffer, 0, buffer.Length);
                            if (count != 0)
                                gzipOutputStream.Write(buffer, 0, count);
                            else
                                break;
                        }
                        gzipOutputStream.Finish();
                        destinationArray = new byte[baseOutputStream.Length];
                        Array.Copy((Array)baseOutputStream.GetBuffer(), 0L, (Array)destinationArray, 0L, baseOutputStream.Length);
                    }
                }
                binaryReader.Close();
            }
            return destinationArray;
        }

        public override byte[] Decompress(byte[] source)
        {
            byte[] destinationArray;
            using (MemoryStream output = new MemoryStream())
            {
                BinaryWriter binaryWriter = new BinaryWriter((Stream)output);
                using (MemoryStream baseInputStream = new MemoryStream(source))
                {
                    byte[] buffer = new byte[1024000];
                    using (GZipInputStream gzipInputStream = new GZipInputStream((Stream)baseInputStream, buffer.Length))
                    {
                        while (true)
                        {
                            int count = gzipInputStream.Read(buffer, 0, buffer.Length);
                            if (count != 0)
                                binaryWriter.Write(buffer, 0, count);
                            else
                                break;
                        }
                    }
                }
                destinationArray = new byte[output.Length];
                Array.Copy((Array)output.GetBuffer(), 0L, (Array)destinationArray, 0L, output.Length);
                binaryWriter.Close();
            }
            return destinationArray;
        }

        public override void Compress(Stream source, Stream target)
        {
            using (BinaryReader binaryReader = new BinaryReader(source))
            {
                byte[] buffer = new byte[1024000];
                using (GZipOutputStream gzipOutputStream = new GZipOutputStream(target))
                {
                    while (true)
                    {
                        int count = binaryReader.Read(buffer, 0, buffer.Length);
                        if (count != 0)
                            gzipOutputStream.Write(buffer, 0, count);
                        else
                            break;
                    }
                    gzipOutputStream.Finish();
                }
            }
        }

        public override void Decompress(Stream source, Stream target)
        {
            byte[] buffer = new byte[1024000];
            using (GZipInputStream gzipInputStream = new GZipInputStream(source, buffer.Length))
            {
                while (true)
                {
                    int count = gzipInputStream.Read(buffer, 0, buffer.Length);
                    if (count != 0)
                        target.Write(buffer, 0, count);
                    else
                        break;
                }
            }
        }
    }
}
