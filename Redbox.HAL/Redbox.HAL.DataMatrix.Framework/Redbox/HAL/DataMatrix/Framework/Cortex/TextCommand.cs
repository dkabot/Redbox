using System.Collections.Generic;
using System.Text;

namespace Redbox.HAL.DataMatrix.Framework.Cortex
{
    internal abstract class TextCommand : AbstractCortexCommand
    {
        private byte[] m_command;

        protected override byte[] CoreCommand
        {
            get
            {
                if (m_command != null)
                    return m_command;
                var byteList = new List<byte>();
                try
                {
                    byteList.AddRange(Encoding.ASCII.GetBytes(Prefix));
                    if (Data == null)
                    {
                        byteList.Add(0);
                    }
                    else
                    {
                        byteList.Add((byte)Data.Length);
                        byteList.AddRange(Encoding.ASCII.GetBytes(Data));
                    }

                    byteList.Add(0);
                    return m_command = byteList.ToArray();
                }
                finally
                {
                    byteList.Clear();
                }
            }
        }

        protected abstract string Prefix { get; }

        protected abstract string Data { get; }
    }
}