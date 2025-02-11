using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Redbox.Lua
{
    internal class LuaParse
    {
        private readonly List<string> m_tokens = new List<string>();

        public void Parse(string s, bool allowAnonymous)
        {
            string str1 = string.Format("({0}[^{0}]*{0})", (object)"\"");
            foreach (string str2 in Regex.Split(s, str1 + "|(=)|(,)|(\\[)|(\\])|(\\{)|(\\})|(--[^\\n\\r]*)"))
            {
                if (str2.Trim().Length != 0 && !str2.StartsWith("--"))
                    this.m_tokens.Add(str2.Trim());
            }
            this.Assign(!allowAnonymous);
        }

        public string Id { get; set; }

        public LuaParseObject Value { get; set; }

        protected void Assign(bool requiresIdentifier)
        {
            if (requiresIdentifier)
            {
                if (!this.IsLiteral)
                    throw new Exception("expect identifier");
                this.Id = this.GetToken();
                if (!this.IsToken("="))
                    throw new Exception("expect '='");
                this.NextToken();
            }
            this.Value = this.RVal();
        }

        protected LuaParseObject RVal()
        {
            if (this.IsToken("{"))
                return this.LuaObject();
            if (this.IsString)
                return this.GetString();
            if (this.IsNumber)
                return this.GetNumber();
            if (this.IsFloat)
                return this.GetFloat();
            throw new Exception("expecting '{', a string or a number");
        }

        protected LuaParseObject LuaObject()
        {
            Dictionary<string, LuaParseObject> dictionary1 = new Dictionary<string, LuaParseObject>();
            this.NextToken();
            while (!this.IsToken("}"))
            {
                if (this.IsLiteral)
                {
                    string token = this.GetToken();
                    this.NextToken();
                    dictionary1.Add(token, this.RVal());
                }
                else if (this.IsToken("["))
                {
                    this.NextToken();
                    string key = (string)this.GetString();
                    if (!this.IsToken("]"))
                        throw new Exception("expecting ']'");
                    this.NextToken();
                    if (!this.IsToken("="))
                        throw new Exception("expecting '='");
                    this.NextToken();
                    dictionary1.Add(key, this.RVal());
                }
                else
                {
                    Dictionary<string, LuaParseObject> dictionary2 = dictionary1;
                    dictionary2.Add(dictionary2.Count.ToString(), this.RVal());
                }
                if (this.HasMoreTokens() && !(this.PeekToken() == "}"))
                {
                    if (!this.IsToken(","))
                        throw new Exception("expecting ','");
                    this.NextToken();
                }
                else
                    break;
            }
            this.NextToken();
            return (LuaParseObject)dictionary1;
        }

        protected bool IsLiteral => Regex.IsMatch(this.m_tokens[0], "^[a-zA-Z]+[0-9a-zA-Z_]*");

        protected bool IsString => Regex.IsMatch(this.m_tokens[0], "^[\"']([^\"]*)[\"']");

        protected bool IsNumber => Regex.IsMatch(this.m_tokens[0], "^\\d+");

        protected bool IsFloat => Regex.IsMatch(this.m_tokens[0], "^\\d*\\.\\d+");

        protected string GetToken()
        {
            string token = this.m_tokens[0];
            this.m_tokens.RemoveAt(0);
            return token;
        }

        protected LuaParseObject GetString()
        {
            string str = Regex.Match(this.m_tokens[0], "^[\"']([^\"]*)[\"']").Groups[1].Captures[0].Value;
            this.m_tokens.RemoveAt(0);
            return (LuaParseObject)str;
        }

        protected LuaParseObject GetNumber()
        {
            int int32 = Convert.ToInt32(this.m_tokens[0]);
            this.m_tokens.RemoveAt(0);
            return (LuaParseObject)int32;
        }

        protected LuaParseObject GetFloat()
        {
            double num = Convert.ToDouble(this.m_tokens[0]);
            this.m_tokens.RemoveAt(0);
            return (LuaParseObject)num;
        }

        protected void NextToken()
        {
            if (this.m_tokens.Count == 0)
                return;
            this.m_tokens.RemoveAt(0);
        }

        protected string PeekToken() => this.m_tokens.Count <= 0 ? (string)null : this.m_tokens[0];

        protected bool IsToken(string s) => this.m_tokens.Count > 0 && this.m_tokens[0] == s;

        protected bool HasMoreTokens() => this.m_tokens.Count > 0;
    }
}
