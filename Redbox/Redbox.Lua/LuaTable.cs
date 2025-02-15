using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.Text;
using Redbox.Core;

namespace Redbox.Lua
{
    public class LuaTable : LuaObject
    {
        private readonly IDictionary<object, object> m_pairs = new Dictionary<object, object>();

        public LuaTable(LuaVirtualMachine virtualMachine)
            : base(virtualMachine, new int?())
        {
        }

        public LuaTable(LuaVirtualMachine virtualMachine, int reference)
            : base(virtualMachine, reference)
        {
        }

        public object this[object key]
        {
            get
            {
                if (Reference.HasValue)
                {
                    var count = VirtualMachineInstance.Stack.Count;
                    VirtualMachineInstance.Stack.GetReference(Reference.Value);
                    VirtualMachineInstance.Stack.Push(key);
                    VirtualMachineInstance.Stack.GetTable();
                    var top = VirtualMachineInstance.Stack.Top;
                    VirtualMachineInstance.Stack.SetTop(count);
                    return top;
                }

                return !m_pairs.ContainsKey(key) ? null : m_pairs[key];
            }
            set
            {
                if (Reference.HasValue)
                {
                    var count = VirtualMachineInstance.Stack.Count;
                    VirtualMachineInstance.Stack.GetReference(Reference.Value);
                    VirtualMachineInstance.Stack.Push(key);
                    VirtualMachineInstance.Stack.Push(value);
                    VirtualMachineInstance.Stack.SetTable();
                    VirtualMachineInstance.Stack.SetTop(count);
                }
                else
                {
                    m_pairs[key] = value;
                }
            }
        }

        public ICollection Keys
        {
            get
            {
                if (Reference.HasValue)
                    return GetTableDictionary().Keys;
                var keys = new List<object>();
                m_pairs.Keys.ForEach(keys.Add);
                return keys;
            }
        }

        public ICollection Values
        {
            get
            {
                if (Reference.HasValue)
                    return GetTableDictionary().Values;
                var values = new List<object>();
                m_pairs.Values.ForEach(values.Add);
                return values;
            }
        }

        public override void Push()
        {
            if (Reference.HasValue)
            {
                base.Push();
            }
            else
            {
                VirtualMachineInstance.Stack.NewTable();
                foreach (var pair in m_pairs)
                {
                    VirtualMachineInstance.Stack.Push(pair.Key);
                    VirtualMachineInstance.Stack.Push(pair.Value);
                    VirtualMachineInstance.Stack.SetTable();
                }
            }
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            FormatTable(builder, new List<LuaTable>());
            return builder.ToString();
        }

        public string ToJson()
        {
            var dictionary = new Dictionary<string, object>();
            BuildPropertiesDictionary(dictionary, this);
            return dictionary.ToJson();
        }

        public void FormatTable(StringBuilder builder, List<LuaTable> visited)
        {
            if (visited.Contains(this))
                return;
            visited.Add(this);
            builder.Append("{ ");
            var flag = false;
            foreach (var key in Keys)
            {
                if (this[key] is LuaTable luaTable)
                {
                    if (!visited.Contains(luaTable))
                    {
                        if (flag)
                            builder.Append(", ");
                        builder.Append(FormatKey(key));
                        luaTable.FormatTable(builder, visited);
                    }
                }
                else
                {
                    if (flag)
                        builder.Append(", ");
                    builder.AppendFormat("{0}{1}", FormatKey(key), LuaHelper.FormatLuaValue(this[key]));
                }

                flag = true;
            }

            builder.Append(" }");
        }

        public Color? ToColor()
        {
            var int32_1 = Convert.ToInt32(this["r"] ?? 0);
            var int32_2 = Convert.ToInt32(this["g"] ?? 0);
            var int32_3 = Convert.ToInt32(this["b"] ?? 0);
            var green = int32_2;
            var blue = int32_3;
            return Color.FromArgb(int32_1, green, blue);
        }

        public Point? ToPoint()
        {
            return new Point(Convert.ToInt32(this["x"] ?? 0), Convert.ToInt32(this["y"] ?? 0));
        }

        public Rectangle? ToRectangle()
        {
            var int32_1 = Convert.ToInt32(this["x"] ?? 0);
            var int32_2 = Convert.ToInt32(this["y"] ?? 0);
            var int32_3 = Convert.ToInt32(this["width"] ?? 0);
            var int32_4 = Convert.ToInt32(this["height"] ?? 0);
            var y = int32_2;
            var width = int32_3;
            var height = int32_4;
            return new Rectangle(int32_1, y, width, height);
        }

        public List<T> ToList<T>()
        {
            var convertToType = typeof(T);
            var list = new List<T>();
            foreach (var obj in Values)
                list.Add((T)ConversionHelper.ChangeType(obj, convertToType));
            return list;
        }

        public DateTime? ToDateTime()
        {
            DateTime result;
            return DateTime.TryParse(
                string.Format("{0}/{1}/{2} {3}:{4}:{5}", this["month"], this["day"], this["year"], this["hour"],
                    this["min"], this["sec"]), out result)
                ? result
                : new DateTime?();
        }

        public void ImportDictionary(IDictionary<string, object> properties)
        {
            BuildLuaTable(this, properties);
        }

        private static void BuildPropertiesDictionary(
            IDictionary<string, object> properties,
            LuaTable table)
        {
            foreach (var key1 in table.Keys)
            {
                var key2 = key1.ToString();
                var obj = table[key1];
                if (obj is LuaTable table1)
                    BuildPropertiesDictionary(properties, table1);
                else
                    properties[key2] = obj;
            }
        }

        private static void BuildLuaTable(
            LuaTable table,
            IEnumerable<KeyValuePair<string, object>> properties)
        {
            foreach (var property in properties)
                if (property.Value is Dictionary<string, object> properties1)
                    BuildLuaTable(table, properties1);
                else
                    table[property.Key] = property.Value;
        }

        private static string FormatKey(object eachKey)
        {
            switch (eachKey)
            {
                case double _:
                    return string.Format("[{0}]=", eachKey);
                case string _:
                    return IsValidSymbol((string)eachKey)
                        ? string.Format("{0}=", eachKey)
                        : string.Format("['{0}']=", eachKey);
                case LuaTable _:
                    return string.Format("[{0}]=", LuaHelper.FormatLuaValue(eachKey));
                default:
                    return null;
            }
        }

        private static bool IsValidSymbol(string symbol)
        {
            return !string.IsNullOrEmpty(symbol) && (char.IsLetter(symbol[0]) || symbol[0] == '_') &&
                   !symbol.Contains(" ");
        }

        private ListDictionary GetTableDictionary()
        {
            var tableDictionary = new ListDictionary();
            var count = VirtualMachineInstance.Stack.Count;
            Push();
            VirtualMachineInstance.Stack.Push(null);
            while (VirtualMachineInstance.Stack.NextKey() != 0)
            {
                var obj = VirtualMachineInstance.Stack.Pop();
                var top = VirtualMachineInstance.Stack.Top;
                tableDictionary[top] = obj;
            }

            VirtualMachineInstance.Stack.SetTop(count);
            return tableDictionary;
        }
    }
}