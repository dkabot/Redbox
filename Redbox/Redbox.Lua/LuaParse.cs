using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Redbox.Lua
{
    public class LuaParse
    {
        private readonly List<string> m_tokens = new List<string>();

        public string Id { get; set; }

        public LuaParseObject Value { get; set; }

        protected bool IsLiteral => Regex.IsMatch(m_tokens[0], "^[a-zA-Z]+[0-9a-zA-Z_]*");

        protected bool IsString => Regex.IsMatch(m_tokens[0], "^[\"']([^\"]*)[\"']");

        protected bool IsNumber => Regex.IsMatch(m_tokens[0], "^\\d+");

        protected bool IsFloat => Regex.IsMatch(m_tokens[0], "^\\d*\\.\\d+");

        public void Parse(string s, bool allowAnonymous)
        {
            var str1 = string.Format("({0}[^{0}]*{0})", "\"");
            foreach (var str2 in Regex.Split(s, str1 + "|(=)|(,)|(\\[)|(\\])|(\\{)|(\\})|(--[^\\n\\r]*)"))
                if (str2.Trim().Length != 0 && !str2.StartsWith("--"))
                    m_tokens.Add(str2.Trim());
            Assign(!allowAnonymous);
        }

        protected void Assign(bool requiresIdentifier)
        {
            if (requiresIdentifier)
            {
                if (!IsLiteral)
                    throw new Exception("expect identifier");
                Id = GetToken();
                if (!IsToken("="))
                    throw new Exception("expect '='");
                NextToken();
            }

            Value = RVal();
        }

        protected LuaParseObject RVal()
        {
            if (IsToken("{"))
                return LuaObject();
            if (IsString)
                return GetString();
            if (IsNumber)
                return GetNumber();
            if (IsFloat)
                return GetFloat();
            throw new Exception("expecting '{', a string or a number");
        }

        protected LuaParseObject LuaObject()
        {
            var dictionary = new Dictionary<string, LuaParseObject>();
            NextToken();
            while (!IsToken("}"))
            {
                if (IsLiteral)
                {
                    var token = GetToken();
                    NextToken();
                    dictionary.Add(token, RVal());
                }
                else if (IsToken("["))
                {
                    NextToken();
                    var key = (string)GetString();
                    if (!IsToken("]"))
                        throw new Exception("expecting ']'");
                    NextToken();
                    if (!IsToken("="))
                        throw new Exception("expecting '='");
                    NextToken();
                    dictionary.Add(key, RVal());
                }
                else
                {
                    dictionary.Add(dictionary.Count.ToString(), RVal());
                }

                if (HasMoreTokens() && !(PeekToken() == "}"))
                {
                    if (!IsToken(","))
                        throw new Exception("expecting ','");
                    NextToken();
                }
                else
                {
                    break;
                }
            }

            NextToken();
            return dictionary;
        }

        protected string GetToken()
        {
            var token = m_tokens[0];
            m_tokens.RemoveAt(0);
            return token;
        }

        protected LuaParseObject GetString()
        {
            var str = Regex.Match(m_tokens[0], "^[\"']([^\"]*)[\"']").Groups[1].Captures[0].Value;
            m_tokens.RemoveAt(0);
            return str;
        }

        protected LuaParseObject GetNumber()
        {
            var int32 = Convert.ToInt32(m_tokens[0]);
            m_tokens.RemoveAt(0);
            return int32;
        }

        protected LuaParseObject GetFloat()
        {
            var num = Convert.ToDouble(m_tokens[0]);
            m_tokens.RemoveAt(0);
            return num;
        }

        protected void NextToken()
        {
            if (m_tokens.Count == 0)
                return;
            m_tokens.RemoveAt(0);
        }

        protected string PeekToken()
        {
            return m_tokens.Count <= 0 ? null : m_tokens[0];
        }

        protected bool IsToken(string s)
        {
            return m_tokens.Count > 0 && m_tokens[0] == s;
        }

        protected bool HasMoreTokens()
        {
            return m_tokens.Count > 0;
        }
    }
}