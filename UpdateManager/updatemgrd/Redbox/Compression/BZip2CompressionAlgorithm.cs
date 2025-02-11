using ICSharpCode.SharpZipLib.BZip2;
using System;
using System.IO;

namespace Redbox.Compression
{
    internal class BZip2CompressionAlgorithm : CompressionAlgorithm
    {
        private const int BufferSize = 1024000;

        public override byte[] Compress(byte[] source)
        {
            byte[] destinationArray;
            using (MemoryStream input = new MemoryStream(source))
            {
                BinaryReader binaryReader = new BinaryReader((Stream)input);
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    byte[] buffer = new byte[1024000];
                    using (BZip2OutputStream bzip2OutputStream = new BZip2OutputStream((Stream)memoryStream))
                    {
                        while (true)
                        {
                            int count = binaryReader.Read(buffer, 0, buffer.Length);
                            if (count != 0)
                                bzip2OutputStream.Write(buffer, 0, count);
                            else
                                break;
                        }
                        bzip2OutputStream.Flush();
                        destinationArray = new byte[memoryStream.Length];
                        Array.Copy((Array)memoryStream.GetBuffer(), 0L, (Array)destinationArray, 0L, memoryStream.Length);
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
                using (MemoryStream memoryStream = new MemoryStream(source))
                {
                    byte[] buffer = new byte[1024000];
                    using (BZip2InputStream bzip2InputStream = new BZip2InputStream((Stream)memoryStream))
                    {
                        while (true)
                        {
                            int count = bzip2InputStream.Read(buffer, 0, buffer.Length);
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
            byte[] buffer = new byte[1024000];
            using (BinaryReader binaryReader = new BinaryReader(source))
            {
                using (BZip2OutputStream bzip2OutputStream = new BZip2OutputStream(target))
                {
                    while (true)
                    {
                        int count = binaryReader.Read(buffer, 0, buffer.Length);
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
            byte[] buffer = new byte[1024000];
            using (BZip2InputStream bzip2InputStream = new BZip2InputStream(source))
            {
                while (true)
                {
                    int count = bzip2InputStream.Read(buffer, 0, buffer.Length);
                    if (count != 0)
                        target.Write(buffer, 0, count);
                    else
                        break;
                }
            }
        }
    }
}
