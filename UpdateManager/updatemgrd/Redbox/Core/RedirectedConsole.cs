using System;
using System.IO;

namespace Redbox.Core
{
    internal class RedirectedConsole : IConsole, IDisposable
    {
        private readonly StreamWriter m_writer;

        public RedirectedConsole(string fileName)
        {
            string directoryName = Path.GetDirectoryName(fileName);
            if (!Directory.Exists(directoryName))
                Directory.CreateDirectory(directoryName);
            this.m_writer = File.CreateText(fileName);
            this.m_writer.AutoFlush = true;
        }

        public void Dispose()
        {
            if (this.m_writer == null)
                return;
            this.m_writer.Close();
            this.m_writer.Dispose();
        }

        public bool IsAttachedToConsole => true;

        public void Write(string value) => this.m_writer.Write(value);

        public void Write(string format, params object[] parms) => this.m_writer.Write(format, parms);

        public void WriteLine() => this.m_writer.WriteLine();

        public void WriteLine(object value) => this.m_writer.WriteLine(value);

        public void WriteLine(string value) => this.m_writer.WriteLine(value);

        public void WriteLine(string format, params object[] parms)
        {
            this.m_writer.WriteLine(format, parms);
        }
    }
}
