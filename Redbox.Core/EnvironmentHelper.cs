using System;

namespace Redbox.Core
{
    public static class EnvironmentHelper
    {
        private static byte[] m_newLineBytes;

        public static byte[] GetNewLineBytes()
        {
            if (m_newLineBytes == null)
            {
                m_newLineBytes = new byte[Environment.NewLine.Length];
                var num = 0;
                foreach (var ch in Environment.NewLine)
                    m_newLineBytes[num++] = (byte)ch;
            }

            return m_newLineBytes;
        }
    }
}