using System;

namespace Redbox.Core
{
    internal class NullConsole : IConsole, IDisposable
    {
        public void Write(string value)
        {
        }

        public void Write(string format, params object[] parms)
        {
        }

        public void WriteLine()
        {
        }

        public void WriteLine(object value)
        {
        }

        public void WriteLine(string value)
        {
        }

        public void WriteLine(string format, params object[] parms)
        {
        }

        public void Dispose()
        {
        }

        public bool IsAttachedToConsole => false;
    }
}
