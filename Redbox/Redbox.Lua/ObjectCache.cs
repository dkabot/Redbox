using System;
using System.Collections.Generic;

namespace Redbox.Lua
{
    public class ObjectCache
    {
        private readonly IDictionary<int, object> m_idToObject = new Dictionary<int, object>();
        private readonly IDictionary<object, int> m_objectToId = new Dictionary<object, int>();
        private int m_id;

        public ObjectCache()
        {
            m_id = 1;
        }

        public object GetObject(int id)
        {
            return m_idToObject.ContainsKey(id) ? m_idToObject[id] : null;
        }

        public void RemoveId(int id)
        {
            if (!m_idToObject.ContainsKey(id))
                return;
            var key = m_idToObject[id];
            m_objectToId.Remove(key);
            m_idToObject.Remove(id);
            if (!(key is IDisposable disposable))
                return;
            disposable.Dispose();
        }

        public int CacheObject(object instance)
        {
            if (m_objectToId.ContainsKey(instance))
                return m_objectToId[instance];
            m_idToObject[m_id] = instance;
            m_objectToId[instance] = m_id;
            var id = m_id;
            ++m_id;
            return id;
        }

        public void RemoveObject(object instance)
        {
            if (!m_objectToId.ContainsKey(instance))
                return;
            var key = m_objectToId[instance];
            m_objectToId.Remove(instance);
            m_idToObject.Remove(key);
            if (!(instance is IDisposable disposable))
                return;
            disposable.Dispose();
        }
    }
}