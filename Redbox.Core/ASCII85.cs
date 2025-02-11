using System;
using System.IO;
using System.Text;

namespace Redbox.Core
{
    public class ASCII85
    {
        private const string PrefixMark = "<~";
        private const string SuffixMark = "~>";
        private const int m_asciiOffset = 33;
        private readonly byte[] m_decodedBlock = new byte[4];
        private readonly byte[] m_encodedBlock = new byte[5];

        private readonly uint[] pow85 = new uint[5]
        {
            52200625U,
            614125U,
            7225U,
            85U,
            1U
        };

        private int m_linePos;
        private uint m_tuple;

        public static byte[] Decode(string s)
        {
            return new ASCII85().InternalDecode(s);
        }

        public static string Encode(byte[] source)
        {
            return new ASCII85().InternalEncode(source);
        }

        private byte[] InternalDecode(string s)
        {
            if (!s.StartsWith("<~") | !s.EndsWith("~>"))
                throw new Exception("ASCII85 encoded data should begin with '<~' and end with '~>'");
            if (s.StartsWith("<~"))
                s = s.Substring("<~".Length);
            if (s.EndsWith("~>"))
                s = s.Substring(0, s.Length - "~>".Length);
            var memoryStream = new MemoryStream();
            var index1 = 0;
            foreach (var ch in s)
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
                        m_decodedBlock[0] = 0;
                        m_decodedBlock[1] = 0;
                        m_decodedBlock[2] = 0;
                        m_decodedBlock[3] = 0;
                        memoryStream.Write(m_decodedBlock, 0, m_decodedBlock.Length);
                        flag = false;
                        break;
                    default:
                        if (ch < '!' || ch > 'u')
                            throw new Exception("Bad character '" + ch +
                                                "' found. ASCII85 only allows characters '!' to 'u'.");
                        flag = true;
                        break;
                }

                if (flag)
                {
                    m_tuple += (ch - 33U) * pow85[index1];
                    ++index1;
                    if (index1 == m_encodedBlock.Length)
                    {
                        DecodeBlock();
                        memoryStream.Write(m_decodedBlock, 0, m_decodedBlock.Length);
                        m_tuple = 0U;
                        index1 = 0;
                    }
                }
            }

            if (index1 != 0)
            {
                if (index1 == 1)
                    throw new Exception("The last block of ASCII85 data cannot be a single byte.");
                var bytes = index1 - 1;
                m_tuple += pow85[bytes];
                DecodeBlock(bytes);
                for (var index2 = 0; index2 < bytes; ++index2)
                    memoryStream.WriteByte(m_decodedBlock[index2]);
            }

            return memoryStream.ToArray();
        }

        private string InternalEncode(byte[] ba)
        {
            var sb = new StringBuilder(ba.Length * (m_encodedBlock.Length / m_decodedBlock.Length));
            m_linePos = 0;
            AppendString(sb, "<~");
            var num1 = 0;
            m_tuple = 0U;
            foreach (var num2 in ba)
                if (num1 >= m_decodedBlock.Length - 1)
                {
                    m_tuple |= num2;
                    if (m_tuple == 0U)
                        AppendChar(sb, 'z');
                    else
                        EncodeBlock(sb);
                    m_tuple = 0U;
                    num1 = 0;
                }
                else
                {
                    m_tuple |= (uint)num2 << (24 - num1 * 8);
                    ++num1;
                }

            if (num1 > 0)
                EncodeBlock(num1 + 1, sb);
            AppendString(sb, "~>");
            return sb.ToString();
        }

        private void EncodeBlock(StringBuilder sb)
        {
            this.EncodeBlock(this.m_encodedBlock.Length, sb);
        }

        private void EncodeBlock(int count, StringBuilder sb)
        {
            for (var index = m_encodedBlock.Length - 1; index >= 0; --index)
            {
                m_encodedBlock[index] = (byte)(m_tuple % 85U + 33U);
                m_tuple /= 85U;
            }

            for (var index = 0; index < count; ++index)
            {
                var c = (char)m_encodedBlock[index];
                AppendChar(sb, c);
            }
        }

        private void DecodeBlock()
        {
            this.DecodeBlock(this.m_decodedBlock.Length);
        }

        private void DecodeBlock(int bytes)
        {
            for (var index = 0; index < bytes; ++index)
                m_decodedBlock[index] = (byte)(m_tuple >> (24 - index * 8));
        }

        private void AppendString(StringBuilder sb, string s)
        {
            m_linePos += s.Length;
            sb.Append(s);
        }

        private void AppendChar(StringBuilder sb, char c)
        {
            sb.Append(c);
            ++m_linePos;
        }
    }
}