using System.Collections;
using System.Collections.Generic;

namespace Redbox.Lua
{
    internal class LuaDebugFileContainer : IEnumerable<LuaDebugFile>, IEnumerable
    {
        private readonly List<LuaDebugFile> m_items = new List<LuaDebugFile>();

        public int Count => this.m_items.Count;

        public LuaDebugFile this[int index] => this.m_items[index];

        IEnumerator IEnumerable.GetEnumerator() => (IEnumerator)this.m_items.GetEnumerator();

        public IEnumerator<LuaDebugFile> GetEnumerator()
        {
            return (IEnumerator<LuaDebugFile>)this.m_items.GetEnumerator();
        }

        internal LuaDebugFileContainer()
        {
        }

        internal void Add(LuaDebugFile item) => this.m_items.Add(item);

        internal void Remove(LuaDebugFile item) => this.m_items.Remove(item);

        internal void RemoveAt(int index) => this.m_items.RemoveAt(index);

        internal void Clear() => this.m_items.Clear();
    }
}
