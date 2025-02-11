using System;

namespace Redbox.HAL.Component.Model.Tokenizer
{
    [Serializable]
    public class Token : ICloneable<Token>
    {
        public Token(TokenType type, string value, string pairSeparator)
            : this(type, value, true)
        {
            PairSeparator = pairSeparator;
        }

        public Token(TokenType type, string value, bool isKeyValuePair)
        {
            Type = type;
            Value = value;
            IsKeyValuePair = isKeyValuePair;
            PairSeparator = "=";
        }

        public bool IsSymbolOrConst => Type == TokenType.Symbol || Type == TokenType.ConstSymbol;

        public string Value { get; private set; }

        public TokenType Type { get; private set; }

        public bool IsKeyValuePair { get; private set; }

        public string PairSeparator { get; private set; }

        public Token Clone(params object[] parms)
        {
            return new Token(Type, Value, IsKeyValuePair)
            {
                PairSeparator = PairSeparator
            };
        }

        public override bool Equals(object obj)
        {
            if (this == obj)
                return true;
            return obj is Token token && Value == token.Value && Type == token.Type &&
                   IsKeyValuePair == token.IsKeyValuePair;
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
            if (Type == TokenType.NumericLiteral)
                return Value.IndexOf(".") != -1 ? decimal.Parse(Value) : (object)int.Parse(Value);
            if (Type == TokenType.Operator && Value.IndexOf("..") != -1)
                return new Range(Value);
            if (string.Compare(Value, bool.TrueString, true) == 0)
                return true;
            return string.Compare(Value, bool.FalseString, true) == 0 ? false : (object)Value;
        }
    }
}