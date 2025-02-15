using System;
using Redbox.Core;

namespace Redbox.Tokenizer.Framework
{
    [Serializable]
    public class SimpleToken : TokenBase<SimpleTokenType>, ICloneable<SimpleToken>
    {
        public SimpleToken(SimpleTokenType type, string value, string pairSeparator)
            : this(type, value, true)
        {
            PairSeparator = pairSeparator;
        }

        public SimpleToken(SimpleTokenType type, string value, bool isKeyValuePair)
            : base(type, value)
        {
            IsKeyValuePair = isKeyValuePair;
            PairSeparator = "=";
        }

        public bool IsKeyValuePair { get; private set; }

        public string PairSeparator { get; private set; }

        public SimpleToken Clone(params object[] parms)
        {
            return new SimpleToken(Type, Value, IsKeyValuePair)
            {
                PairSeparator = PairSeparator
            };
        }

        public override bool Equals(object obj)
        {
            if (this == obj)
                return true;
            return obj is SimpleToken simpleToken && Value == simpleToken.Value && Type == simpleToken.Type &&
                   IsKeyValuePair == simpleToken.IsKeyValuePair;
        }

        public override string ToString()
        {
            return string.Format("Type={0}, Value={1}, IsKeyValuePair={2}", Type, Value, IsKeyValuePair);
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public object ConvertValue()
        {
            if (IsKeyValuePair)
                return new KeyValuePair(Value, Type, PairSeparator);
            if (Type == SimpleTokenType.NumericLiteral)
                return Value.IndexOf(".") != -1 ? decimal.Parse(Value) : (object)int.Parse(Value);
            if (Type == SimpleTokenType.Operator && Value.IndexOf("..") != -1)
                return new Range(Value);
            if (string.Compare(Value, bool.TrueString, true) == 0)
                return true;
            return string.Compare(Value, bool.FalseString, true) == 0 ? false : (object)Value;
        }
    }
}