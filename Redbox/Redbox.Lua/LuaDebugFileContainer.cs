using System.Collections;
using System.Collections.Generic;

namespace Redbox.Lua
{
    public class LuaDebugFileContainer : IEnumerable<LuaDebugFile>, IEnumerable
    {
        private readonly List<LuaDebugFile> m_items = new List<LuaDebugFile>();

        internal LuaDebugFileContainer()
        {
        }

        public int Count => m_items.Count;

        public LuaDebugFile this[int index] => m_items[index];

        IEnumerator IEnumerable.GetEnumerator()
        {
            return m_items.GetEnumerator();
        }

        public IEnumerator<LuaDebugFile> GetEnumerator()
        {
            return m_items.GetEnumerator();
        }

        internal void Add(LuaDebugFile item)
        {
            m_items.Add(item);
        }

        internal void Remove(LuaDebugFile item)
        {
            m_items.Remove(item);
        }

        internal void RemoveAt(int index)
        {
            m_items.RemoveAt(index);
        }

        internal void Clear()
        {
            m_items.Clear();
        }
    }
}