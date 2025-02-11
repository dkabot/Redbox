using Redbox.Core;
using System;

namespace Redbox.Tokenizer.Framework
{
    [Serializable]
    internal class SimpleToken : TokenBase<SimpleTokenType>, ICloneable<SimpleToken>
    {
        public SimpleToken(SimpleTokenType type, string value, string pairSeparator)
          : this(type, value, true)
        {
            this.PairSeparator = pairSeparator;
        }

        public SimpleToken(SimpleTokenType type, string value, bool isKeyValuePair)
          : base(type, value)
        {
            this.IsKeyValuePair = isKeyValuePair;
            this.PairSeparator = "=";
        }

        public override bool Equals(object obj)
        {
            if (this == obj)
                return true;
            return obj is SimpleToken simpleToken && this.Value == simpleToken.Value && this.Type == simpleToken.Type && this.IsKeyValuePair == simpleToken.IsKeyValuePair;
        }

        public override string ToString()
        {
            return string.Format("Type={0}, Value={1}, IsKeyValuePair={2}", (object)this.Type, (object)this.Value, (object)this.IsKeyValuePair);
        }

        public override int GetHashCode() => this.ToString().GetHashCode();

        public object ConvertValue()
        {
            if (this.IsKeyValuePair)
                return (object)new KeyValuePair(this.Value, this.Type, this.PairSeparator);
            if (this.Type == SimpleTokenType.NumericLiteral)
                return this.Value.IndexOf(".") != -1 ? (object)Decimal.Parse(this.Value) : (object)int.Parse(this.Value);
            if (this.Type == SimpleTokenType.Operator && this.Value.IndexOf("..") != -1)
                return (object)new Range(this.Value);
            if (string.Compare(this.Value, bool.TrueString, true) == 0)
                return (object)true;
            return string.Compare(this.Value, bool.FalseString, true) == 0 ? (object)false : (object)this.Value;
        }

        public SimpleToken Clone(params object[] parms)
        {
            return new SimpleToken(this.Type, this.Value, this.IsKeyValuePair)
            {
                PairSeparator = this.PairSeparator
            };
        }

        public bool IsKeyValuePair { get; private set; }

        public string PairSeparator { get; private set; }
    }
}
