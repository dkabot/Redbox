using ICSharpCode.SharpZipLib.BZip2;
using Redbox.HAL.Component.Model;
using System;
using System.IO;

namespace Redbox.HAL.IPC.Framework
{
    internal sealed class BZip2CompressionAlgorithm : ICompressionAlgorithm
    {
        public byte[] Compress(byte[] source)
        {
            byte[] destinationArray;
            using (MemoryStream input = StreamBase.NewOn(source))
            {
                BinaryReader binaryReader = new BinaryReader(input);
                using (MemoryStream memoryStream = StreamBase.New())
                {
                    BZip2OutputStream bzip2OutputStream = new BZip2OutputStream(memoryStream);
                    bzip2OutputStream.Write(binaryReader.ReadBytes((int)input.Length), 0, (int)input.Length);
                    bzip2OutputStream.Flush();
                    destinationArray = new byte[memoryStream.Length];
                    Array.Copy((Array)memoryStream.GetBuffer(), 0L, (Array)destinationArray, 0L, memoryStream.Length);
                }
                binaryReader.Close();
            }
            return destinationArray;
        }

        public byte[] Decompress(byte[] source)
        {
            byte[] destinationArray;
            using (MemoryStream output = StreamBase.New())
            {
                BinaryWriter binaryWriter = new BinaryWriter(output);
                using (MemoryStream memoryStream = StreamBase.NewOn(source))
                {
                    byte[] buffer = new byte[4096];
                    BZip2InputStream bzip2InputStream = new BZip2InputStream(memoryStream);
                    while (true)
                    {
                        int count = bzip2InputStream.Read(buffer, 0, 4096);
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
    }
}
