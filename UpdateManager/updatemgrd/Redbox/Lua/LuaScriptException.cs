using System;

namespace Redbox.Lua
{
    internal class LuaScriptException : LuaException
    {
        private readonly string m_source;

        public LuaScriptException(string message, string source)
          : base(message)
        {
            this.m_source = source;
        }

        public LuaScriptException(Exception innerException, string source)
          : base("A .NET exception occured in user-code", innerException)
        {
            this.m_source = source;
            this.IsNetException = true;
        }

        public override string ToString()
        {
            return this.GetType().FullName + ": " + this.m_source + this.Message;
        }

        public bool IsNetException { get; private set; }

        public override string Source => this.m_source;
    }
}
