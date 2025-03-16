using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Lua;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Redbox.Rental.Model
{
    public static class LuaExtensions
    {
        public static LuaTable ToLuaTable(this Dictionary<string, object> value)
        {
            var table = (LuaTable)ServiceLocator.Instance.GetService<IKernelService>().NewTable();
            value.ForEach<KeyValuePair<string, object>>((Action<KeyValuePair<string, object>>)(row =>
            {
                if (row.Value is ICollection)
                {
                    if (row.Value is Dictionary<string, object>)
                        table[(object)row.Key] = (object)ToLuaTable(row.Value as Dictionary<string, object>);
                    else if (row.Value is Dictionary<long, object>)
                        table[(object)row.Key] = (object)ToLuaTable(row.Value as Dictionary<long, object>);
                    else
                        table[(object)row.Key] = (object)(row.Value as ICollection).ToLuaTable();
                }
                else if (row.Value is DateTime)
                {
                    table[(object)row.Key] = (object)((DateTime)row.Value).ToString("yyyyMMddHHmmss");
                }
                else
                {
                    table[(object)row.Key] = row.Value;
                }
            }));
            return table;
        }

        public static LuaTable ToLuaTable(this Dictionary<long, object> value)
        {
            var table = (LuaTable)ServiceLocator.Instance.GetService<IKernelService>().NewTable();
            value.ForEach<KeyValuePair<long, object>>((Action<KeyValuePair<long, object>>)(row =>
            {
                if (row.Value is ICollection)
                {
                    if (row.Value is Dictionary<string, object>)
                        table[(object)row.Key] = (object)ToLuaTable(row.Value as Dictionary<string, object>);
                    else if (row.Value is Dictionary<long, object>)
                        table[(object)row.Key] = (object)ToLuaTable(row.Value as Dictionary<long, object>);
                    else
                        table[(object)row.Key] = (object)(row.Value as ICollection).ToLuaTable();
                }
                else if (row.Value is DateTime)
                {
                    table[(object)row.Key] = (object)((DateTime)row.Value).ToString("yyyyMMddHHmmss");
                }
                else
                {
                    table[(object)row.Key] = row.Value;
                }
            }));
            return table;
        }

        public static LuaTable ToLuaTable(this ICollection value)
        {
            var luaTable = (LuaTable)ServiceLocator.Instance.GetService<IKernelService>().NewTable();
            var num = 1;
            foreach (var obj in (IEnumerable)value)
                switch (obj)
                {
                    case ICollection _:
                        switch (obj)
                        {
                            case Dictionary<string, object> _:
                                luaTable[(object)num++] = (object)ToLuaTable(obj as Dictionary<string, object>);
                                continue;
                            case Dictionary<long, object> _:
                                luaTable[(object)num++] = (object)ToLuaTable(obj as Dictionary<long, object>);
                                continue;
                            default:
                                luaTable[(object)num++] = (object)(obj as ICollection).ToLuaTable();
                                continue;
                        }
                    case DateTime dateTime:
                        luaTable[(object)num++] = (object)dateTime.ToString("yyyyMMddHHmmss");
                        continue;
                    default:
                        luaTable[(object)num++] = obj;
                        continue;
                }

            return luaTable;
        }

        public static LuaTable ToLuaDataTable(this Dictionary<string, object> value)
        {
            var luaTable = ToLuaTable(value);
            luaTable[(object)"keys"] = (object)luaTable.Keys.ToLuaTable();
            return luaTable;
        }

        public static Dictionary<string, object> ParseLuaValue(this string value)
        {
            var parseLuaValue = new ParseLuaValue();
            parseLuaValue.Parse(value);
            return parseLuaValue.Properties;
        }

        public static string LuaDictToString(this Dictionary<string, object> dict)
        {
            var sb = new StringBuilder();
            DictionaryToStrings(dict, sb, string.Empty);
            return sb.ToString();
        }

        private static void DictionaryToStrings(
            Dictionary<string, object> dict,
            StringBuilder sb,
            string spaces)
        {
            foreach (var keyValuePair in dict)
                if (keyValuePair.Value is IDictionary<string, object>)
                {
                    sb.AppendLine(string.Format("{0}[{1} = {{", (object)spaces, (object)keyValuePair.Key));
                    DictionaryToStrings(keyValuePair.Value as Dictionary<string, object>, sb, spaces + "    ");
                    sb.AppendLine("}]");
                }
                else if (keyValuePair.Value is IList<string>)
                {
                    var sub = new StringBuilder();
                    sub.Append(string.Format("{0}[{1} = {{ ", (object)spaces, (object)keyValuePair.Key));
                    var pos = 0;
                    (keyValuePair.Value as IList<string>).ToList<string>().ForEach((Action<string>)(s =>
                    {
                        if (pos++ == 0)
                            sub.Append(string.Format("\"{0}\"", (object)s));
                        else
                            sub.Append(string.Format(", \"{0}\"", (object)s));
                    }));
                    sub.Append(" }]");
                    sb.AppendLine(sub.ToString());
                }
                else if (keyValuePair.Value is string)
                {
                    sb.AppendLine(string.Format("{0}[{1} = \"{2}\"]", (object)spaces, (object)keyValuePair.Key,
                        string.IsNullOrEmpty(keyValuePair.Value as string) ? (object)"null" : keyValuePair.Value));
                }
                else
                {
                    sb.AppendLine(string.Format("{0}[{1} = {2}]", (object)spaces, (object)keyValuePair.Key,
                        keyValuePair.Value ?? (object)"null"));
                }
        }
    }
}