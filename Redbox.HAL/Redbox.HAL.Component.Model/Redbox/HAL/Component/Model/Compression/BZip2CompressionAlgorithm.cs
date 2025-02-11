using System;
using System.IO;
using ICSharpCode.SharpZipLib.BZip2;

namespace Redbox.HAL.Component.Model.Compression
{
    internal sealed class BZip2CompressionAlgorithm : ICompressionAlgorithm
    {
        public byte[] Compress(byte[] source)
        {
            byte[] destinationArray;
            using (var input = StreamBase.NewOn(source))
            {
                var binaryReader = new BinaryReader((StreamBase)input);
                using (var memoryStream = StreamBase.New())
                {
                    var bzip2OutputStream = new BZip2OutputStream((StreamBase)memoryStream);
                    bzip2OutputStream.Write(binaryReader.ReadBytes((int)input.Length), 0, (int)input.Length);
                    bzip2OutputStream.Flush();
                    destinationArray = new byte[memoryStream.Length];
                    Array.Copy(memoryStream.GetBuffer(), 0L, destinationArray, 0L, memoryStream.Length);
                }

                binaryReader.Close();
            }

            return destinationArray;
        }

        public byte[] Decompress(byte[] source)
        {
            byte[] destinationArray;
            using (var output = StreamBase.New())
            {
                var binaryWriter = new BinaryWriter((StreamBase)output);
                using (var memoryStream = StreamBase.NewOn(source))
                {
                    var buffer = new byte[4096];
                    var bzip2InputStream = new BZip2InputStream((StreamBase)memoryStream);
                    while (true)
                    {
                        var count = bzip2InputStream.Read(buffer, 0, 4096);
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
    }
}