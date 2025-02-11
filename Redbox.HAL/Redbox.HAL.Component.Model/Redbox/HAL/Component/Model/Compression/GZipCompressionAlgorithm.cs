using System;
using System.IO;
using ICSharpCode.SharpZipLib.GZip;

namespace Redbox.HAL.Component.Model.Compression
{
    internal sealed class GZipCompressionAlgorithm : ICompressionAlgorithm
    {
        public byte[] Compress(byte[] source)
        {
            byte[] destinationArray;
            using (var input = StreamBase.NewOn(source))
            {
                var binaryReader = new BinaryReader((StreamBase)input);
                using (var baseOutputStream = StreamBase.New())
                {
                    var gzipOutputStream = new GZipOutputStream((StreamBase)baseOutputStream);
                    gzipOutputStream.Write(binaryReader.ReadBytes((int)input.Length), 0, (int)input.Length);
                    gzipOutputStream.Finish();
                    gzipOutputStream.Flush();
                    destinationArray = new byte[baseOutputStream.Length];
                    Array.Copy(baseOutputStream.GetBuffer(), 0L, destinationArray, 0L, baseOutputStream.Length);
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
                using (var baseInputStream = StreamBase.NewOn(source))
                {
                    var buffer = new byte[4096];
                    var gzipInputStream = new GZipInputStream((StreamBase)baseInputStream, 4096);
                    while (true)
                    {
                        var count = gzipInputStream.Read(buffer, 0, 4096);
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