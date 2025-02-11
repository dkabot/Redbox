using System;

namespace Redbox.Lua
{
    internal class HookExceptionEventArgs : EventArgs
    {
        public HookExceptionEventArgs(Exception ex) => this.Exception = ex;

        public Exception Exception { get; private set; }
    }
}
