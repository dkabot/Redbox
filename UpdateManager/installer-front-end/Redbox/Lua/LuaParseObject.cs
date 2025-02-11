using System.Collections;
using System.Collections.Generic;

namespace Redbox.Lua
{
    internal class LuaParseObject : IEnumerable
    {
        private readonly object m_instance;

        public LuaParseObject(object o) => this.m_instance = o;

        public IEnumerator GetEnumerator()
        {
            return !(this.m_instance is Dictionary<string, LuaParseObject> instance) ? (IEnumerator)null : (IEnumerator)instance.GetEnumerator();
        }

        public LuaParseObject this[int index]
        {
            get
            {
                string key = index.ToString();
                return !(this.m_instance is Dictionary<string, LuaParseObject> instance) || !instance.ContainsKey(key) ? (LuaParseObject)null : instance[key];
            }
        }

        public LuaParseObject this[string index]
        {
            get
            {
                return !(this.m_instance is Dictionary<string, LuaParseObject> instance) || !instance.ContainsKey(index) ? (LuaParseObject)null : instance[index];
            }
        }

        public static implicit operator string(LuaParseObject m) => m.m_instance as string;

        public static implicit operator int(LuaParseObject m) => m.m_instance as int? ?? 0;

        public static implicit operator double(LuaParseObject m) => m.m_instance as double? ?? 0.0;

        public static implicit operator Dictionary<string, object>(LuaParseObject m)
        {
            Dictionary<string, object> dictionary = new Dictionary<string, object>();
            if (m.m_instance is Dictionary<string, LuaParseObject> instance)
            {
                foreach (KeyValuePair<string, LuaParseObject> keyValuePair in instance)
                    dictionary[keyValuePair.Key] = (object)keyValuePair.Value;
            }
            return dictionary;
        }

        public static implicit operator LuaParseObject(string s) => new LuaParseObject((object)s);

        public static implicit operator LuaParseObject(int i) => new LuaParseObject((object)i);

        public static implicit operator LuaParseObject(double d) => new LuaParseObject((object)d);

        public static implicit operator LuaParseObject(Dictionary<string, LuaParseObject> dic)
        {
            return new LuaParseObject((object)dic);
        }

        public object Instance => this.m_instance;
    }
}
