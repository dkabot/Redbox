using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework
{
    internal sealed class ContextSymbol : IContextSymbol
    {
        internal ContextSymbol(string s)
        {
            Key = s;
        }

        public string Key { get; }

        public object Value { get; set; }

        public bool IsReserved => Key.StartsWith("__");
    }
}