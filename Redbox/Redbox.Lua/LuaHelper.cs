using System.Collections.Generic;
using System.Text;

namespace Redbox.Lua
{
    public static class LuaHelper
    {
        public static string FormatLuaValue(object value)
        {
            string str;
            switch (value)
            {
                case null:
                    return "nil";
                case string _:
                    var stringBuilder = new StringBuilder();
                    foreach (var c in (string)value)
                    {
                        if (char.IsControl(c))
                            switch (c)
                            {
                                case '\t':
                                    stringBuilder.Append("\\t");
                                    continue;
                                case '\n':
                                    stringBuilder.Append("\\n");
                                    continue;
                                case '\r':
                                    stringBuilder.Append("\\r");
                                    continue;
                                default:
                                    continue;
                            }

                        if (char.IsLetterOrDigit(c) || char.IsWhiteSpace(c))
                        {
                            stringBuilder.Append(c);
                        }
                        else if (char.IsPunctuation(c))
                        {
                            if (c == '\'')
                                stringBuilder.Append("\\'");
                            else
                                stringBuilder.Append(c);
                        }
                        else
                        {
                            stringBuilder.AppendFormat("\\{0:000}", (byte)c);
                        }
                    }

                    return string.Format("'{0}'", stringBuilder);
                case LuaTable luaTable:
                    var builder = new StringBuilder();
                    luaTable.FormatTable(builder, new List<LuaTable>());
                    str = builder.ToString();
                    break;
                case LuaFunction _:
                    str = "(function)";
                    break;
                case bool _:
                    str = value.ToString().ToLower();
                    break;
                default:
                    str = value.GetType().ToString() == "Lua511.LuaCSFunction" ? "(kernel function)" : value.ToString();
                    break;
            }

            return str;
        }
    }
}