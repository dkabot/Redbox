using ICSharpCode.SharpZipLib.Zip;
using System;
using System.IO;

namespace Redbox.Compression
{
    internal class ZipCompressionAlgorithm : CompressionAlgorithm
    {
        private const int BufferSize = 1024000;

        public override byte[] Compress(byte[] source)
        {
            byte[] buffer = new byte[1024000];
            byte[] destinationArray;
            using (MemoryStream input = new MemoryStream(source))
            {
                BinaryReader binaryReader = new BinaryReader((Stream)input);
                using (MemoryStream baseOutputStream = new MemoryStream())
                {
                    ZipOutputStream zipOutputStream = new ZipOutputStream((Stream)baseOutputStream);
                    ZipEntry entry = new ZipEntry("file")
                    {
                        DateTime = DateTime.UtcNow,
                        Size = (long)source.Length,
                        CompressionMethod = CompressionMethod.Deflated
                    };
                    zipOutputStream.PutNextEntry(entry);
                    while (true)
                    {
                        int count = binaryReader.Read(buffer, 0, buffer.Length);
                        if (count != 0)
                            zipOutputStream.Write(buffer, 0, count);
                        else
                            break;
                    }
                    zipOutputStream.Finish();
                    zipOutputStream.Flush();
                    destinationArray = new byte[baseOutputStream.Length];
                    Array.Copy((Array)baseOutputStream.GetBuffer(), 0L, (Array)destinationArray, 0L, baseOutputStream.Length);
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
                    ZipInputStream zipInputStream = new ZipInputStream((Stream)baseInputStream);
                    while (true)
                    {
                        int count = zipInputStream.Read(buffer, 0, buffer.Length);
                        if (count != 0)
                            binaryWriter.Write(buffer, 0, count);
                        else
                            break;
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
                using (ZipOutputStream zipOutputStream = new ZipOutputStream(target))
                {
                    ZipEntry entry = new ZipEntry("file")
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
            byte[] buffer = new byte[1024000];
            using (ZipInputStream zipInputStream = new ZipInputStream(source))
            {
                while (true)
                {
                    int count = zipInputStream.Read(buffer, 0, buffer.Length);
                    if (count != 0)
                        target.Write(buffer, 0, count);
                    else
                        break;
                }
            }
        }
    }
}
