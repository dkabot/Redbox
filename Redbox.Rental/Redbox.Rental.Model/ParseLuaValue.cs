using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Redbox.Rental.Model
{
    public class ParseLuaValue
    {
        private int _tokenPos;
        public List<string> _rawTokens = new List<string>();
        private Dictionary<string, object> _properties = new Dictionary<string, object>();

        public void Parse(string s)
        {
            _rawTokens.Clear();
            s = s.Replace("\\\"", "&quot");
            var str1 = string.Format("({0}[^{0}]*{0})", (object)"\"");
            foreach (var str2 in Regex.Split(s, str1 + "|(=)|(,)|(\\[)|(\\])|(\\{)|(\\})|(--[^\\n\\r]*)"))
                if (str2.Trim().Length != 0 && !str2.StartsWith("--"))
                    _rawTokens.Add(str2.Trim());
            _tokenPos = 0;
            LoadProperties();
        }

        private void LoadProperties()
        {
            _properties.Clear();
            var obj = ReadValue();
            _properties = obj is Dictionary<string, object>
                ? obj as Dictionary<string, object>
                : throw new Exception("expecting '{', a string or a number");
        }

        private object ReadValue()
        {
            if (IsNull)
                return ReadNull();
            if (IsToken("{"))
                return ReadObject();
            if (IsString)
                return (object)GetString();
            if (IsFloat)
                return (object)GetFloat();
            if (IsNumber)
                return (object)GetNumber();
            if (IsBool)
                return (object)GetBoolean();
            throw new Exception("expecting '{', a string or a number");
        }

        private object ReadObject()
        {
            var dictionary = new Dictionary<string, object>();
            var stringList = new List<string>();
            Next();
            while (!IsToken("}"))
            {
                if (IsLiteral)
                {
                    var current = GetCurrent();
                    Next();
                    dictionary.Add(current, ReadValue());
                }
                else if (IsToken("["))
                {
                    Next();
                    var key = GetString();
                    if (!IsToken("]"))
                        throw new Exception("expecting ']'");
                    Next();
                    if (!IsToken("="))
                        throw new Exception("expecting '='");
                    Next();
                    dictionary.Add(key, ReadValue());
                }
                else
                {
                    stringList.Add(GetString());
                }

                if (HasMore && !(Current == "}"))
                {
                    if (!IsToken(","))
                        throw new Exception("expecting ','");
                    Next();
                }
                else
                {
                    break;
                }
            }

            Next();
            return stringList.Count == 0 ? (object)dictionary : (object)stringList;
        }

        private bool IsLiteral => Regex.IsMatch(Current, "^[a-zA-Z]+[0-9a-zA-Z_]*");

        private bool IsBool => Current.Equals("true", StringComparison.CurrentCultureIgnoreCase) ||
                               Current.Equals("false", StringComparison.CurrentCultureIgnoreCase);

        private bool IsNull => string.IsNullOrEmpty(Current) ||
                               Current.Equals("null", StringComparison.CurrentCultureIgnoreCase) ||
                               Current.Equals("nil", StringComparison.CurrentCultureIgnoreCase);

        private bool IsString => Regex.IsMatch(Current, "^[\"']([^\"]*)[\"']");

        private bool IsNumber => Regex.IsMatch(Current, "^\\d+");

        private bool IsFloat => Regex.IsMatch(Current, "^\\d*\\.\\d+");

        private bool IsToken(string s)
        {
            return HasMore && Current == s;
        }

        private void Next()
        {
            ++_tokenPos;
        }

        private bool HasMore => _tokenPos < _rawTokens.Count;

        private string Current => !HasMore ? (string)null : _rawTokens[_tokenPos];

        private string GetCurrent()
        {
            var current = Current;
            Next();
            return current;
        }

        private object ReadNull()
        {
            Next();
            return (object)null;
        }

        private string GetString()
        {
            var current = GetCurrent();
            var match = Regex.Match(current, "^[\"']([^\"]*)[\"']");
            return match.Groups[0].Captures.Count == 0
                ? current
                : match.Groups[1].Captures[0].Value.Replace("&quot", "\"").Replace("\\r\\n", Environment.NewLine);
        }

        private int GetNumber()
        {
            return Convert.ToInt32(GetCurrent());
        }

        private bool GetBoolean()
        {
            return Convert.ToBoolean(GetCurrent());
        }

        private double GetFloat()
        {
            return Convert.ToDouble(GetCurrent());
        }

        public Dictionary<string, object> Properties => _properties;
    }
}