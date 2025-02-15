using System.Collections;
using System.Collections.Generic;

namespace Redbox.Lua
{
    public class LuaParseObject : IEnumerable
    {
        public LuaParseObject(object o)
        {
            Instance = o;
        }

        public LuaParseObject this[int index]
        {
            get
            {
                var key = index.ToString();
                return !(Instance is Dictionary<string, LuaParseObject> instance) || !instance.ContainsKey(key)
                    ? null
                    : instance[key];
            }
        }

        public LuaParseObject this[string index] =>
            !(Instance is Dictionary<string, LuaParseObject> instance) || !instance.ContainsKey(index)
                ? null
                : instance[index];

        public object Instance { get; }

        public IEnumerator GetEnumerator()
        {
            return !(Instance is Dictionary<string, LuaParseObject> instance)
                ? null
                : (IEnumerator)instance.GetEnumerator();
        }

        public static implicit operator string(LuaParseObject m)
        {
            return m.Instance as string;
        }

        public static implicit operator int(LuaParseObject m)
        {
            return (m.Instance as int?).GetValueOrDefault();
        }

        public static implicit operator double(LuaParseObject m)
        {
            return (m.Instance as double?).GetValueOrDefault();
        }

        public static implicit operator Dictionary<string, object>(LuaParseObject m)
        {
            var dictionary = new Dictionary<string, object>();
            if (m.Instance is Dictionary<string, LuaParseObject> instance)
                foreach (var keyValuePair in instance)
                    dictionary[keyValuePair.Key] = keyValuePair.Value;
            return dictionary;
        }

        public static implicit operator LuaParseObject(string s)
        {
            return new LuaParseObject(s);
        }

        public static implicit operator LuaParseObject(int i)
        {
            return new LuaParseObject(i);
        }

        public static implicit operator LuaParseObject(double d)
        {
            return new LuaParseObject(d);
        }

        public static implicit operator LuaParseObject(Dictionary<string, LuaParseObject> dic)
        {
            return new LuaParseObject(dic);
        }
    }
}