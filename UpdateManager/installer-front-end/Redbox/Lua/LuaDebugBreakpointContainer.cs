using System.Collections;
using System.Collections.Generic;

namespace Redbox.Lua
{
    internal class LuaDebugBreakpointContainer : IEnumerable<LuaDebugBreakpoint>, IEnumerable
    {
        private readonly List<LuaDebugBreakpoint> m_items = new List<LuaDebugBreakpoint>();

        public void Clear() => this.m_items.Clear();

        public int Count => this.m_items.Count;

        public LuaDebugBreakpoint this[int index] => this.m_items[index];

        public void Remove(LuaDebugBreakpoint item) => this.m_items.Remove(item);

        IEnumerator IEnumerable.GetEnumerator() => (IEnumerator)this.m_items.GetEnumerator();

        public IEnumerator<LuaDebugBreakpoint> GetEnumerator()
        {
            return (IEnumerator<LuaDebugBreakpoint>)this.m_items.GetEnumerator();
        }

        internal LuaDebugBreakpointContainer()
        {
        }

        internal void Add(LuaDebugBreakpoint item) => this.m_items.Add(item);

        internal void RemoveAt(int index) => this.m_items.RemoveAt(index);
    }
}
