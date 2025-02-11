using System;
using System.IO;
using System.Text;

namespace Redbox.Core
{
    internal class ASCII85
    {
        private const string PrefixMark = "<~";
        private const string SuffixMark = "~>";
        private const int m_asciiOffset = 33;
        private readonly byte[] m_encodedBlock = new byte[5];
        private readonly byte[] m_decodedBlock = new byte[4];
        private uint m_tuple;
        private int m_linePos;
        private readonly uint[] pow85 = new uint[5]
        {
      52200625U,
      614125U,
      7225U,
      85U,
      1U
        };

        public static byte[] Decode(string s) => new ASCII85().InternalDecode(s);

        public static string Encode(byte[] source) => new ASCII85().InternalEncode(source);

        private byte[] InternalDecode(string s)
        {
            if (!s.StartsWith("<~") | !s.EndsWith("~>"))
                throw new Exception("ASCII85 encoded data should begin with '<~' and end with '~>'");
            if (s.StartsWith("<~"))
                s = s.Substring("<~".Length);
            if (s.EndsWith("~>"))
                s = s.Substring(0, s.Length - "~>".Length);
            MemoryStream memoryStream = new MemoryStream();
            int index1 = 0;
            foreach (char ch in s)
            {
                bool flag;
                switch (ch)
                {
                    case char.MinValue:
                    case '\b':
                    case '\t':
                    case '\n':
                    case '\f':
                    case '\r':
                        flag = false;
                        break;
                    case 'z':
                        if (index1 != 0)
                            throw new Exception("The character 'z' is invalid inside an ASCII85 block.");
                        this.m_decodedBlock[0] = (byte)0;
                        this.m_decodedBlock[1] = (byte)0;
                        this.m_decodedBlock[2] = (byte)0;
                        this.m_decodedBlock[3] = (byte)0;
                        memoryStream.Write(this.m_decodedBlock, 0, this.m_decodedBlock.Length);
                        flag = false;
                        break;
                    default:
                        if (ch < '!' || ch > 'u')
                            throw new Exception("Bad character '" + ch.ToString() + "' found. ASCII85 only allows characters '!' to 'u'.");
                        flag = true;
                        break;
                }
                if (flag)
                {
                    this.m_tuple += ((uint)ch - 33U) * this.pow85[index1];
                    ++index1;
                    if (index1 == this.m_encodedBlock.Length)
                    {
                        this.DecodeBlock();
                        memoryStream.Write(this.m_decodedBlock, 0, this.m_decodedBlock.Length);
                        this.m_tuple = 0U;
                        index1 = 0;
                    }
                }
            }
            if (index1 != 0)
            {
                if (index1 == 1)
                    throw new Exception("The last block of ASCII85 data cannot be a single byte.");
                int bytes = index1 - 1;
                this.m_tuple += this.pow85[bytes];
                this.DecodeBlock(bytes);
                for (int index2 = 0; index2 < bytes; ++index2)
                    memoryStream.WriteByte(this.m_decodedBlock[index2]);
            }
            return memoryStream.ToArray();
        }

        private string InternalEncode(byte[] ba)
        {
            StringBuilder sb = new StringBuilder(ba.Length * (this.m_encodedBlock.Length / this.m_decodedBlock.Length));
            this.m_linePos = 0;
            this.AppendString(sb, "<~");
            int num1 = 0;
            this.m_tuple = 0U;
            foreach (byte num2 in ba)
            {
                if (num1 >= this.m_decodedBlock.Length - 1)
                {
                    this.m_tuple |= (uint)num2;
                    if (this.m_tuple == 0U)
                        this.AppendChar(sb, 'z');
                    else
                        this.EncodeBlock(sb);
                    this.m_tuple = 0U;
                    num1 = 0;
                }
                else
                {
                    this.m_tuple |= (uint)num2 << 24 - num1 * 8;
                    ++num1;
                }
            }
            if (num1 > 0)
                this.EncodeBlock(num1 + 1, sb);
            this.AppendString(sb, "~>");
            return sb.ToString();
        }

        private void EncodeBlock(StringBuilder sb) => this.EncodeBlock(this.m_encodedBlock.Length, sb);

        private void EncodeBlock(int count, StringBuilder sb)
        {
            for (int index = this.m_encodedBlock.Length - 1; index >= 0; --index)
            {
                this.m_encodedBlock[index] = (byte)(this.m_tuple % 85U + 33U);
                this.m_tuple /= 85U;
            }
            for (int index = 0; index < count; ++index)
            {
                char c = (char)this.m_encodedBlock[index];
                this.AppendChar(sb, c);
            }
        }

        private void DecodeBlock() => this.DecodeBlock(this.m_decodedBlock.Length);

        private void DecodeBlock(int bytes)
        {
            for (int index = 0; index < bytes; ++index)
                this.m_decodedBlock[index] = (byte)(this.m_tuple >> 24 - index * 8);
        }

        private void AppendString(StringBuilder sb, string s)
        {
            this.m_linePos += s.Length;
            sb.Append(s);
        }

        private void AppendChar(StringBuilder sb, char c)
        {
            sb.Append(c);
            ++this.m_linePos;
        }
    }
}
