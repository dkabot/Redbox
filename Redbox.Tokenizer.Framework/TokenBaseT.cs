using System;

namespace Redbox.Tokenizer.Framework
{
    [Serializable]
    public class TokenBase<T>
    {
        public TokenBase(T type, string value)
        {
            Type = type;
            Value = value;
        }

        public string Value { get; private set; }

        public T Type { get; private set; }

        public override string ToString()
        {
            return string.Format("Type={0}, Value={1}", Type, Value);
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }
    }
}