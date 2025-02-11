using Redbox.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.Text;

namespace Redbox.Lua
{
    internal class LuaTable : LuaObject
    {
        private readonly IDictionary<object, object> m_pairs = (IDictionary<object, object>)new Dictionary<object, object>();

        public LuaTable(LuaVirtualMachine virtualMachine)
          : base(virtualMachine, new int?())
        {
        }

        public LuaTable(LuaVirtualMachine virtualMachine, int reference)
          : base(virtualMachine, new int?(reference))
        {
        }

        public object this[object key]
        {
            get
            {
                if (this.Reference.HasValue)
                {
                    int count = this.VirtualMachineInstance.Stack.Count;
                    this.VirtualMachineInstance.Stack.GetReference(this.Reference.Value);
                    this.VirtualMachineInstance.Stack.Push(key);
                    this.VirtualMachineInstance.Stack.GetTable();
                    object top = this.VirtualMachineInstance.Stack.Top;
                    this.VirtualMachineInstance.Stack.SetTop(count);
                    return top;
                }
                return !this.m_pairs.ContainsKey(key) ? (object)null : this.m_pairs[key];
            }
            set
            {
                if (this.Reference.HasValue)
                {
                    int count = this.VirtualMachineInstance.Stack.Count;
                    this.VirtualMachineInstance.Stack.GetReference(this.Reference.Value);
                    this.VirtualMachineInstance.Stack.Push(key);
                    this.VirtualMachineInstance.Stack.Push(value);
                    this.VirtualMachineInstance.Stack.SetTable();
                    this.VirtualMachineInstance.Stack.SetTop(count);
                }
                else
                    this.m_pairs[key] = value;
            }
        }

        public override void Push()
        {
            if (this.Reference.HasValue)
            {
                base.Push();
            }
            else
            {
                this.VirtualMachineInstance.Stack.NewTable();
                foreach (KeyValuePair<object, object> pair in (IEnumerable<KeyValuePair<object, object>>)this.m_pairs)
                {
                    this.VirtualMachineInstance.Stack.Push(pair.Key);
                    this.VirtualMachineInstance.Stack.Push(pair.Value);
                    this.VirtualMachineInstance.Stack.SetTable();
                }
            }
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            this.FormatTable(builder, new List<LuaTable>());
            return builder.ToString();
        }

        public string ToJson()
        {
            Dictionary<string, object> dictionary = new Dictionary<string, object>();
            LuaTable.BuildPropertiesDictionary((IDictionary<string, object>)dictionary, this);
            return dictionary.ToJson();
        }

        public void FormatTable(StringBuilder builder, List<LuaTable> visited)
        {
            if (visited.Contains(this))
                return;
            visited.Add(this);
            builder.Append("{ ");
            bool flag = false;
            foreach (object key in (IEnumerable)this.Keys)
            {
                if (this[key] is LuaTable luaTable)
                {
                    if (!visited.Contains(luaTable))
                    {
                        if (flag)
                            builder.Append(", ");
                        builder.Append(LuaTable.FormatKey(key));
                        luaTable.FormatTable(builder, visited);
                    }
                }
                else
                {
                    if (flag)
                        builder.Append(", ");
                    builder.AppendFormat("{0}{1}", (object)LuaTable.FormatKey(key), (object)LuaHelper.FormatLuaValue(this[key]));
                }
                flag = true;
            }
            builder.Append(" }");
        }

        public Color? ToColor()
        {
            int int32_1 = Convert.ToInt32(this[(object)"r"] ?? (object)0);
            int int32_2 = Convert.ToInt32(this[(object)"g"] ?? (object)0);
            int int32_3 = Convert.ToInt32(this[(object)"b"] ?? (object)0);
            int green = int32_2;
            int blue = int32_3;
            return new Color?(Color.FromArgb(int32_1, green, blue));
        }

        public Point? ToPoint()
        {
            return new Point?(new Point(Convert.ToInt32(this[(object)"x"] ?? (object)0), Convert.ToInt32(this[(object)"y"] ?? (object)0)));
        }

        public Rectangle? ToRectangle()
        {
            int int32_1 = Convert.ToInt32(this[(object)"x"] ?? (object)0);
            int int32_2 = Convert.ToInt32(this[(object)"y"] ?? (object)0);
            int int32_3 = Convert.ToInt32(this[(object)"width"] ?? (object)0);
            int int32_4 = Convert.ToInt32(this[(object)"height"] ?? (object)0);
            int y = int32_2;
            int width = int32_3;
            int height = int32_4;
            return new Rectangle?(new Rectangle(int32_1, y, width, height));
        }

        public List<T> ToList<T>()
        {
            Type convertToType = typeof(T);
            List<T> list = new List<T>();
            foreach (object obj in (IEnumerable)this.Values)
                list.Add((T)ConversionHelper.ChangeType(obj, convertToType));
            return list;
        }

        public DateTime? ToDateTime()
        {
            DateTime result;
            return DateTime.TryParse(string.Format("{0}/{1}/{2} {3}:{4}:{5}", this[(object)"month"], this[(object)"day"], this[(object)"year"], this[(object)"hour"], this[(object)"min"], this[(object)"sec"]), out result) ? new DateTime?(result) : new DateTime?();
        }

        public void ImportDictionary(IDictionary<string, object> properties)
        {
            LuaTable.BuildLuaTable(this, (IEnumerable<KeyValuePair<string, object>>)properties);
        }

        public ICollection Keys
        {
            get
            {
                if (this.Reference.HasValue)
                    return this.GetTableDictionary().Keys;
                List<object> keys = new List<object>();
                this.m_pairs.Keys.ForEach<object>(new Action<object>(keys.Add));
                return (ICollection)keys;
            }
        }

        public ICollection Values
        {
            get
            {
                if (this.Reference.HasValue)
                    return this.GetTableDictionary().Values;
                List<object> values = new List<object>();
                this.m_pairs.Values.ForEach<object>(new Action<object>(values.Add));
                return (ICollection)values;
            }
        }

        private static void BuildPropertiesDictionary(
          IDictionary<string, object> properties,
          LuaTable table)
        {
            foreach (object key1 in (IEnumerable)table.Keys)
            {
                string key2 = key1.ToString();
                object obj = table[key1];
                if (obj is LuaTable table1)
                    LuaTable.BuildPropertiesDictionary(properties, table1);
                else
                    properties[key2] = obj;
            }
        }

        private static void BuildLuaTable(
          LuaTable table,
          IEnumerable<KeyValuePair<string, object>> properties)
        {
            foreach (KeyValuePair<string, object> property in properties)
            {
                if (property.Value is Dictionary<string, object> properties1)
                    LuaTable.BuildLuaTable(table, (IEnumerable<KeyValuePair<string, object>>)properties1);
                else
                    table[(object)property.Key] = property.Value;
            }
        }

        private static string FormatKey(object eachKey)
        {
            switch (eachKey)
            {
                case double _:
                    return string.Format("[{0}]=", eachKey);
                case string _:
                    return LuaTable.IsValidSymbol((string)eachKey) ? string.Format("{0}=", eachKey) : string.Format("['{0}']=", eachKey);
                case LuaTable _:
                    return string.Format("[{0}]=", (object)LuaHelper.FormatLuaValue(eachKey));
                default:
                    return (string)null;
            }
        }

        private static bool IsValidSymbol(string symbol)
        {
            return !string.IsNullOrEmpty(symbol) && (char.IsLetter(symbol[0]) || symbol[0] == '_') && !symbol.Contains(" ");
        }

        private ListDictionary GetTableDictionary()
        {
            ListDictionary tableDictionary = new ListDictionary();
            int count = this.VirtualMachineInstance.Stack.Count;
            this.Push();
            this.VirtualMachineInstance.Stack.Push((object)null);
            while (this.VirtualMachineInstance.Stack.NextKey() != 0)
            {
                object obj = this.VirtualMachineInstance.Stack.Pop();
                object top = this.VirtualMachineInstance.Stack.Top;
                tableDictionary[top] = obj;
            }
            this.VirtualMachineInstance.Stack.SetTop(count);
            return tableDictionary;
        }
    }
}
