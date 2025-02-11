using System;

namespace Redbox.Tokenizer.Framework
{
    [Serializable]
    internal class TokenBase<T>
    {
        public TokenBase(T type, string value)
        {
            this.Type = type;
            this.Value = value;
        }

        public override string ToString()
        {
            return string.Format("Type={0}, Value={1}", (object)this.Type, (object)this.Value);
        }

        public override int GetHashCode() => this.ToString().GetHashCode();

        public string Value { get; private set; }

        public T Type { get; private set; }
    }
}
