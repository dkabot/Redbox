using System;

namespace Redbox.Core
{
    internal static class EnvironmentHelper
    {
        private static byte[] m_newLineBytes;

        public static byte[] GetNewLineBytes()
        {
            if (EnvironmentHelper.m_newLineBytes == null)
            {
                EnvironmentHelper.m_newLineBytes = new byte[Environment.NewLine.Length];
                int num = 0;
                foreach (char ch in Environment.NewLine)
                    EnvironmentHelper.m_newLineBytes[num++] = (byte)ch;
            }
            return EnvironmentHelper.m_newLineBytes;
        }
    }
}
