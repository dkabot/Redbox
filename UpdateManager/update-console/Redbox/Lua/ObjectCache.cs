using System;
using System.Collections.Generic;

namespace Redbox.Lua
{
    internal class ObjectCache
    {
        private int m_id;
        private readonly IDictionary<int, object> m_idToObject = (IDictionary<int, object>)new Dictionary<int, object>();
        private readonly IDictionary<object, int> m_objectToId = (IDictionary<object, int>)new Dictionary<object, int>();

        public ObjectCache() => this.m_id = 1;

        public object GetObject(int id)
        {
            return this.m_idToObject.ContainsKey(id) ? this.m_idToObject[id] : (object)null;
        }

        public void RemoveId(int id)
        {
            if (!this.m_idToObject.ContainsKey(id))
                return;
            object key = this.m_idToObject[id];
            this.m_objectToId.Remove(key);
            this.m_idToObject.Remove(id);
            if (!(key is IDisposable disposable))
                return;
            disposable.Dispose();
        }

        public int CacheObject(object instance)
        {
            if (this.m_objectToId.ContainsKey(instance))
                return this.m_objectToId[instance];
            this.m_idToObject[this.m_id] = instance;
            this.m_objectToId[instance] = this.m_id;
            int id = this.m_id;
            ++this.m_id;
            return id;
        }

        public void RemoveObject(object instance)
        {
            if (!this.m_objectToId.ContainsKey(instance))
                return;
            int key = this.m_objectToId[instance];
            this.m_objectToId.Remove(instance);
            this.m_idToObject.Remove(key);
            if (!(instance is IDisposable disposable))
                return;
            disposable.Dispose();
        }
    }
}
