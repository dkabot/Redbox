using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Redbox.ProductLookupCatalog
{
    public class MemoryArchive : IDisposable
    {
        private readonly MemoryStream _stream;
        private const int RecordLength = 18;
        private const int HeaderLength = 312;
        private readonly byte[] m_header = Encoding.ASCII.GetBytes("<~look~>");
        private const string m_headerString = "<~look~>";
        private const byte Version = 0;

        public static MemoryArchive Create()
        {
            var memoryArchive = new MemoryArchive();
            memoryArchive.WriteHeader(DateTime.UtcNow);
            return memoryArchive;
        }

        public static MemoryArchive Open(string name)
        {
            if (!File.Exists(name))
                throw new ArgumentException(string.Format("{0} does not exists.", (object)name), nameof(name));
            using (var fileStream = new FileStream(name, FileMode.Open, FileAccess.Read))
            {
                return new MemoryArchive((Stream)fileStream);
            }
        }

        public static MemoryArchive Open(Stream stream)
        {
            return new MemoryArchive(stream);
        }

        public static bool IsValid(string name)
        {
            using (var memoryArchive = Open(name))
            {
                return memoryArchive.IsValidArchive();
            }
        }

        public static bool IsValid(Stream stream)
        {
            using (var memoryArchive = Open(stream))
            {
                return memoryArchive.IsValidArchive();
            }
        }

        public byte[] ToArray()
        {
            return _stream.ToArray();
        }

        public bool IsValidArchive()
        {
            _stream.Seek(0L, SeekOrigin.Begin);
            var binaryReader = new BinaryReader((Stream)_stream, Encoding.ASCII);
            var str = Encoding.ASCII.GetString(binaryReader.ReadBytes(8));
            var num = binaryReader.ReadByte();
            return str == "<~look~>" && num == (byte)0;
        }

        public byte GetVersion()
        {
            _stream.Seek(8L, SeekOrigin.Begin);
            return new BinaryReader((Stream)_stream, Encoding.ASCII).ReadByte();
        }

        public string GetOriginMachine()
        {
            _stream.Seek(10L, SeekOrigin.Begin);
            return Encoding.ASCII.GetString(new BinaryReader((Stream)_stream, Encoding.ASCII).ReadBytes(32));
        }

        public byte GetFlags()
        {
            _stream.Seek(9L, SeekOrigin.Begin);
            return new BinaryReader((Stream)_stream, Encoding.ASCII).ReadByte();
        }

        public long GetSizeInKB()
        {
            return _stream != null ? _stream.Length / 1024L : 0L;
        }

        public string GetCreatedOn()
        {
            _stream.Seek(42L, SeekOrigin.Begin);
            return Encoding.ASCII.GetString(new BinaryReader((Stream)_stream, Encoding.ASCII).ReadBytes(14));
        }

        public void Write(Inventory inventory)
        {
            var writer = new BinaryWriter((Stream)_stream, Encoding.ASCII);
            Encode(inventory, writer);
        }

        public Inventory this[long i]
        {
            get => ReadInventory(i);
            set => WriteInventory(i, value);
        }

        public long Count => GetNumberOfRecords();

        public long GetNumberOfRecords()
        {
            return (_stream.Length - 312L) / 18L;
        }

        public Inventory Find(string barcode)
        {
            long num1 = 0;
            var num2 = GetNumberOfRecords() - 1L;
            while (num1 <= num2)
            {
                var index = (num1 + num2) / 2L;
                var str = ReadBarcode(index);
                if (str.CompareTo(barcode) < 0)
                {
                    num1 = index + 1L;
                }
                else
                {
                    if (str.CompareTo(barcode) <= 0)
                        return ReadInventory(index);
                    num2 = index - 1L;
                }
            }

            return (Inventory)null;
        }

        public long FindIndex(string barcode)
        {
            long num1 = 0;
            var num2 = GetNumberOfRecords() - 1L;
            while (num1 <= num2)
            {
                var index = (num1 + num2) / 2L;
                var str = ReadBarcode(index);
                if (str.CompareTo(barcode) < 0)
                {
                    num1 = index + 1L;
                }
                else
                {
                    if (str.CompareTo(barcode) <= 0)
                        return index;
                    num2 = index - 1L;
                }
            }

            return -1;
        }

        public void Dispose()
        {
            if (_stream == null)
                return;
            _stream.Close();
            _stream.Dispose();
        }

        public byte[] Header => m_header;

        private void WriteHeader(DateTime createdOn)
        {
            var binaryWriter = new BinaryWriter((Stream)_stream, Encoding.ASCII);
            binaryWriter.Write(Header);
            binaryWriter.Write((byte)0);
            binaryWriter.Write((byte)0);
            var machineName = Environment.MachineName;
            if (machineName.Length > 32)
            {
                var str = machineName.Substring(0, 32);
                binaryWriter.Write(str);
            }
            else
            {
                binaryWriter.Write(Encoding.ASCII.GetBytes(new string(' ', 32 - machineName.Length)));
                binaryWriter.Write(Encoding.ASCII.GetBytes(machineName));
            }

            binaryWriter.Write(Encoding.ASCII.GetBytes(createdOn.ToString("yyyyMMddHHmmss")));
            var buffer = new byte[256];
            for (var index = 0; index < buffer.Length; ++index)
                buffer[index] = (byte)0;
            binaryWriter.Write(buffer);
        }

        private string ReadBarcode(long index)
        {
            _stream.Seek(312L + index * 18L, SeekOrigin.Begin);
            var binaryReader = new BinaryReader((Stream)_stream, Encoding.ASCII);
            return DecompressBarcode(binaryReader.ReadByte(), binaryReader.ReadUInt64());
        }

        private Inventory ReadInventory(long index)
        {
            _stream.Seek(312L + index * 18L, SeekOrigin.Begin);
            return Decode(new BinaryReader((Stream)_stream, Encoding.ASCII));
        }

        private void WriteInventory(long index, Inventory i)
        {
            _stream.Seek(312L + index * 18L, SeekOrigin.Begin);
            var writer = new BinaryWriter((Stream)_stream, Encoding.ASCII);
            Encode(i, writer);
        }

        private static Inventory Decode(BinaryReader reader)
        {
            return new Inventory()
            {
                Barcode = DecompressBarcode(reader.ReadByte(), reader.ReadUInt64()),
                TitleId = reader.ReadUInt32(),
                Code = (InventoryStatusCode)reader.ReadByte(),
                TotalRentalCount = reader.ReadUInt32()
            };
        }

        private static void Encode(Inventory i, BinaryWriter writer)
        {
            var keyValuePair = CompressBarcode(i.Barcode);
            writer.Write(keyValuePair.Key);
            writer.Write(keyValuePair.Value);
            writer.Write(i.TitleId);
            writer.Write((byte)i.Code);
            writer.Write(i.TotalRentalCount);
        }

        private MemoryArchive()
        {
            _stream = new MemoryStream();
        }

        private MemoryArchive(Stream stream)
        {
            _stream = new MemoryStream();
            StreamCopy(stream, (Stream)_stream, 4096);
        }

        private void StreamCopy(Stream source, Stream destination, int bufferSize)
        {
            var buffer = new byte[bufferSize];
            int count;
            while ((count = source.Read(buffer, 0, buffer.Length)) != 0)
                destination.Write(buffer, 0, count);
        }

        private static string DecompressBarcode(byte prefix, ulong integerPart)
        {
            return string.Format("{0}{1}", (object)new string('0', (int)prefix), (object)integerPart);
        }

        private static KeyValuePair<byte, ulong> CompressBarcode(string barcode)
        {
            if (!ulong.TryParse(barcode, out var _))
                throw new FormatException(string.Format("{0} can not be encoded because it is not a natural number.",
                    (object)barcode));
            byte num = 0;
            while ((int)num < barcode.Length && barcode[(int)num] == '0')
                ++num;
            if ((int)num == barcode.Length)
                --num;
            return new KeyValuePair<byte, ulong>(num, Convert.ToUInt64(barcode.Substring((int)num)));
        }
    }
}