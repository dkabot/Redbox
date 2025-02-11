using System;
using System.IO;
using ICSharpCode.SharpZipLib.BZip2;

namespace Redbox.Compression
{
    public class BZip2CompressionAlgorithm : CompressionAlgorithm
    {
        private const int BufferSize = 1024000;

        public override byte[] Compress(byte[] source)
        {
            byte[] destinationArray;
            using (var input = new MemoryStream(source))
            {
                var binaryReader = new BinaryReader(input);
                using (var memoryStream = new MemoryStream())
                {
                    var buffer = new byte[1024000];
                    using (var bzip2OutputStream = new BZip2OutputStream(memoryStream))
                    {
                        while (true)
                        {
                            var count = binaryReader.Read(buffer, 0, buffer.Length);
                            if (count != 0)
                                bzip2OutputStream.Write(buffer, 0, count);
                            else
                                break;
                        }

                        bzip2OutputStream.Flush();
                        destinationArray = new byte[memoryStream.Length];
                        Array.Copy(memoryStream.GetBuffer(), 0L, destinationArray, 0L, memoryStream.Length);
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
                using (var memoryStream = new MemoryStream(source))
                {
                    var buffer = new byte[1024000];
                    using (var bzip2InputStream = new BZip2InputStream(memoryStream))
                    {
                        while (true)
                        {
                            var count = bzip2InputStream.Read(buffer, 0, buffer.Length);
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
            var buffer = new byte[1024000];
            using (var binaryReader = new BinaryReader(source))
            {
                using (var bzip2OutputStream = new BZip2OutputStream(target))
                {
                    while (true)
                    {
                        var count = binaryReader.Read(buffer, 0, buffer.Length);
                        if (count != 0)
                            bzip2OutputStream.Write(buffer, 0, count);
                        else
                            break;
                    }

                    bzip2OutputStream.Flush();
                }
            }
        }

        public override void Decompress(Stream source, Stream target)
        {
            var buffer = new byte[1024000];
            using (var bzip2InputStream = new BZip2InputStream(source))
            {
                while (true)
                {
                    var count = bzip2InputStream.Read(buffer, 0, buffer.Length);
                    if (count != 0)
                        target.Write(buffer, 0, count);
                    else
                        break;
                }
            }
        }
    }
}