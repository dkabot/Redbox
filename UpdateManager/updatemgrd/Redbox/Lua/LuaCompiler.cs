namespace Redbox.Lua
{
    internal class LuaCompiler
    {
        private readonly LuaVirtualMachine m_virtualMachine;

        public LuaCompiler(LuaVirtualMachine virtualMachine)
        {
            this.m_virtualMachine = virtualMachine;
            if (this.m_virtualMachine["jit"] is LuaTable luaTable)
            {
                this.Version = luaTable[(object)"version"] as string;
                this.Architecture = luaTable[(object)"arch"] as string;
            }
            else
            {
                this.Version = "Not Installed";
                this.Architecture = "Not Installed";
            }
        }

        public string Version { get; private set; }

        public string Architecture { get; private set; }
    }
}
