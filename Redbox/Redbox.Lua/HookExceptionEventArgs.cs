using System;

namespace Redbox.Lua
{
    public class HookExceptionEventArgs : EventArgs
    {
        public HookExceptionEventArgs(Exception ex)
        {
            Exception = ex;
        }

        public Exception Exception { get; private set; }
    }
}