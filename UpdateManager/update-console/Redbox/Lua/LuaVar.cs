namespace Redbox.Lua
{
    internal class LuaVar
    {
        public override string ToString() => LuaHelper.FormatLuaValue(this.Value);

        public int Index { get; private set; }

        public string Name { get; private set; }

        public object Value { get; internal set; }

        internal LuaVar(int index, string name, object value)
        {
            this.Index = index;
            this.Name = name;
            this.Value = value;
        }
    }
}
