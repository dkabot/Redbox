using System;

namespace Redbox.Core
{
    public class DefaultConsole : IConsole, IDisposable
    {
        public DefaultConsole()
        {
            IsAttachedToConsole = ConsoleTool.AttachConsoleToParentProcess();
            try
            {
                if (!IsAttachedToConsole || Console.CursorTop + 1 >= Console.BufferHeight)
                    return;
                Console.SetCursorPosition(0, Console.CursorTop + 1);
            }
            catch
            {
            }
        }

        public void Write(string value)
        {
            Console.Write(value);
        }

        public void Write(string format, params object[] parms)
        {
            Console.Write(format, parms);
        }

        public void WriteLine()
        {
            Console.WriteLine();
        }

        public void WriteLine(object value)
        {
            Console.WriteLine(value);
        }

        public void WriteLine(string value)
        {
            Console.WriteLine(value);
        }

        public void WriteLine(string format, params object[] parms)
        {
            Console.WriteLine(format, parms);
        }

        public void Dispose()
        {
            if (!IsAttachedToConsole)
                return;
            ConsoleTool.FreeAttachedConsole();
        }

        public bool IsAttachedToConsole { get; }
    }
}