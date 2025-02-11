using System;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;

namespace Redbox.Compression
{
    public class ZipCompressionAlgorithm : CompressionAlgorithm
    {
        private const int BufferSize = 1024000;

        public override byte[] Compress(byte[] source)
        {
            var buffer = new byte[1024000];
            byte[] destinationArray;
            using (var input = new MemoryStream(source))
            {
                var binaryReader = new BinaryReader(input);
                using (var baseOutputStream = new MemoryStream())
                {
                    var zipOutputStream = new ZipOutputStream(baseOutputStream);
                    var entry = new ZipEntry("file")
                    {
                        DateTime = DateTime.UtcNow,
                        Size = source.Length,
                        CompressionMethod = CompressionMethod.Deflated
                    };
                    zipOutputStream.PutNextEntry(entry);
                    while (true)
                    {
                        var count = binaryReader.Read(buffer, 0, buffer.Length);
                        if (count != 0)
                            zipOutputStream.Write(buffer, 0, count);
                        else
                            break;
                    }

                    zipOutputStream.Finish();
                    zipOutputStream.Flush();
                    destinationArray = new byte[baseOutputStream.Length];
                    Array.Copy(baseOutputStream.GetBuffer(), 0L, destinationArray, 0L, baseOutputStream.Length);
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
                    var zipInputStream = new ZipInputStream(baseInputStream);
                    while (true)
                    {
                        var count = zipInputStream.Read(buffer, 0, buffer.Length);
                        if (count != 0)
                            binaryWriter.Write(buffer, 0, count);
                        else
                            break;
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
                using (var zipOutputStream = new ZipOutputStream(target))
                {
                    var entry = new ZipEntry("file")
                    {
                        DateTime = DateTime.UtcNow,
                        Size = source.Length,
                        CompressionMethod = CompressionMethod.Deflated
                    };
                    zipOutputStream.PutNextEntry(entry);
                    zipOutputStream.Write(binaryReader.ReadBytes((int)source.Length), 0, (int)source.Length);
                    zipOutputStream.Finish();
                }
            }
        }

        public override void Decompress(Stream source, Stream target)
        {
            var buffer = new byte[1024000];
            using (var zipInputStream = new ZipInputStream(source))
            {
                while (true)
                {
                    var count = zipInputStream.Read(buffer, 0, buffer.Length);
                    if (count != 0)
                        target.Write(buffer, 0, count);
                    else
                        break;
                }
            }
        }
    }
}