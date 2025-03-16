using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Redbox.ProductLookupCatalog
{
    public class Archive : IDisposable
    {
        private readonly FileStream m_file;
        private const int RecordLength = 18;
        private const int HeaderLength = 312;
        private readonly byte[] m_header = Encoding.ASCII.GetBytes("<~look~>");
        private const string m_headerString = "<~look~>";
        private const byte Version = 0;

        public static Archive Create(string name)
        {
            return Create(name, DateTime.UtcNow);
        }

        public static Archive Open(string name)
        {
            return File.Exists(name)
                ? new Archive(name, true)
                : throw new ArgumentException(string.Format("{0} does not exists.", (object)name), nameof(name));
        }

        public static Archive Open(string name, bool readOnly)
        {
            return File.Exists(name)
                ? new Archive(name, readOnly)
                : throw new ArgumentException(string.Format("{0} does not exists.", (object)name), nameof(name));
        }

        public static bool IsValid(string name)
        {
            using (var archive = Open(name))
            {
                return archive.IsValidArchive();
            }
        }

        public bool IsValidArchive()
        {
            m_file.Seek(0L, SeekOrigin.Begin);
            var binaryReader = new BinaryReader((Stream)m_file, Encoding.ASCII);
            var str = Encoding.ASCII.GetString(binaryReader.ReadBytes(8));
            var num = binaryReader.ReadByte();
            return str == "<~look~>" && num == (byte)0;
        }

        public byte GetVersion()
        {
            m_file.Seek(8L, SeekOrigin.Begin);
            return new BinaryReader((Stream)m_file, Encoding.ASCII).ReadByte();
        }

        public string GetOriginMachine()
        {
            m_file.Seek(10L, SeekOrigin.Begin);
            return Encoding.ASCII.GetString(new BinaryReader((Stream)m_file, Encoding.ASCII).ReadBytes(32));
        }

        public byte GetFlags()
        {
            m_file.Seek(9L, SeekOrigin.Begin);
            return new BinaryReader((Stream)m_file, Encoding.ASCII).ReadByte();
        }

        public long GetSizeInKB()
        {
            return m_file != null ? m_file.Length / 1024L : 0L;
        }

        public string GetCreatedOn()
        {
            m_file.Seek(42L, SeekOrigin.Begin);
            return Encoding.ASCII.GetString(new BinaryReader((Stream)m_file, Encoding.ASCII).ReadBytes(14));
        }

        public void Write(Inventory inventory)
        {
            var writer = new BinaryWriter((Stream)m_file, Encoding.ASCII);
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
            return (m_file.Length - 312L) / 18L;
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
            if (m_file == null)
                return;
            m_file.Close();
            m_file.Dispose();
        }

        public byte[] Header => m_header;

        internal static Archive Create(string name, DateTime createdOn)
        {
            var archive = !File.Exists(name)
                ? new Archive(name, false)
                : throw new ArgumentException(string.Format("{0} already exists.", (object)name), nameof(name));
            archive.WriteHeader(createdOn);
            return archive;
        }

        private void WriteHeader(DateTime createdOn)
        {
            var binaryWriter = new BinaryWriter((Stream)m_file, Encoding.ASCII);
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
            m_file.Seek(312L + index * 18L, SeekOrigin.Begin);
            var binaryReader = new BinaryReader((Stream)m_file, Encoding.ASCII);
            return DecompressBarcode(binaryReader.ReadByte(), binaryReader.ReadUInt64());
        }

        private Inventory ReadInventory(long index)
        {
            m_file.Seek(312L + index * 18L, SeekOrigin.Begin);
            return Decode(new BinaryReader((Stream)m_file, Encoding.ASCII));
        }

        private void WriteInventory(long index, Inventory i)
        {
            m_file.Seek(312L + index * 18L, SeekOrigin.Begin);
            var writer = new BinaryWriter((Stream)m_file, Encoding.ASCII);
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

        private Archive(string name, bool readOnly)
        {
            m_file = new FileStream(name, FileMode.OpenOrCreate, readOnly ? FileAccess.Read : FileAccess.ReadWrite);
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