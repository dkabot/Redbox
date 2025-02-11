using System;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;

namespace Redbox.HAL.Component.Model.Compression;

internal sealed class ZipCompressionAlgorithm : ICompressionAlgorithm
{
    public byte[] Compress(byte[] source)
    {
        byte[] destinationArray;
        using (var input = StreamBase.NewOn(source))
        {
            var binaryReader = new BinaryReader((StreamBase)input);
            using (var baseOutputStream = StreamBase.New())
            {
                var zipOutputStream = new ZipOutputStream((StreamBase)baseOutputStream);
                zipOutputStream.Write(binaryReader.ReadBytes((int)input.Length), 0, (int)input.Length);
                zipOutputStream.Finish();
                zipOutputStream.Flush();
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
                var zipInputStream = new ZipInputStream((StreamBase)baseInputStream);
                while (true)
                {
                    var count = zipInputStream.Read(buffer, 0, 4096);
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