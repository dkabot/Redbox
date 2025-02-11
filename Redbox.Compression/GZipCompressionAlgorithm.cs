using System;
using System.IO;
using ICSharpCode.SharpZipLib.GZip;

namespace Redbox.Compression
{
    public class GZipCompressionAlgorithm : CompressionAlgorithm
    {
        private const int BufferSize = 1024000;

        public override byte[] Compress(byte[] source)
        {
            byte[] destinationArray;
            using (var input = new MemoryStream(source))
            {
                var binaryReader = new BinaryReader(input);
                using (var baseOutputStream = new MemoryStream())
                {
                    var buffer = new byte[1024000];
                    using (var gzipOutputStream = new GZipOutputStream(baseOutputStream))
                    {
                        while (true)
                        {
                            var count = binaryReader.Read(buffer, 0, buffer.Length);
                            if (count != 0)
                                gzipOutputStream.Write(buffer, 0, count);
                            else
                                break;
                        }

                        gzipOutputStream.Finish();
                        destinationArray = new byte[baseOutputStream.Length];
                        Array.Copy(baseOutputStream.GetBuffer(), 0L, destinationArray, 0L, baseOutputStream.Length);
                    }
                }

                binaryReader.Close();
            }

            return destinationArray;
        }

        public override byte[] Decompress(byte[] source)
        {
            byte[] destinationArray;
            using (var output = new MemoryStream())
            {
                var binaryWriter = new BinaryWriter(output);
                using (var baseInputStream = new MemoryStream(source))
                {
                    var buffer = new byte[1024000];
                    using (var gzipInputStream = new GZipInputStream(baseInputStream, buffer.Length))
                    {
                        while (true)
                        {
                            var count = gzipInputStream.Read(buffer, 0, buffer.Length);
                            if (count != 0)
                                binaryWriter.Write(buffer, 0, count);
                            else
                                break;
                        }
                    }
                }

                destinationArray = new byte[output.Length];
                Array.Copy(output.GetBuffer(), 0L, destinationArray, 0L, output.Length);
                binaryWriter.Close();
            }

            return destinationArray;
        }

        public override void Compress(Stream source, Stream target)
        {
            using (var binaryReader = new BinaryReader(source))
            {
                var buffer = new byte[1024000];
                using (var gzipOutputStream = new GZipOutputStream(target))
                {
                    while (true)
                    {
                        var count = binaryReader.Read(buffer, 0, buffer.Length);
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
            var buffer = new byte[1024000];
            using (var gzipInputStream = new GZipInputStream(source, buffer.Length))
            {
                while (true)
                {
                    var count = gzipInputStream.Read(buffer, 0, buffer.Length);
                    if (count != 0)
                        target.Write(buffer, 0, count);
                    else
                        break;
                }
            }
        }
    }
}