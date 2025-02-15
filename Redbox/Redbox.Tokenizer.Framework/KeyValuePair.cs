using System;

namespace Redbox.Tokenizer.Framework
{
    [Serializable]
    public class KeyValuePair
    {
        public KeyValuePair(string value, SimpleTokenType type)
            : this(value, type, "=")
        {
        }

        public KeyValuePair(string value, SimpleTokenType type, string pairSeparator)
        {
            Type = type;
            PairSeparator = pairSeparator;
            SetParts(value.Split(pairSeparator.ToCharArray(), 2, StringSplitOptions.RemoveEmptyEntries));
        }

        public string Key { get; private set; }

        public string Value { get; private set; }

        public SimpleTokenType Type { get; private set; }

        public string PairSeparator { get; private set; }

        public override string ToString()
        {
            return string.Format("{0}{1}{2}", Key, PairSeparator, Value);
        }

        private void SetParts(string[] parts)
        {
            if (parts.Length != 2)
                return;
            Key = parts[0].Trim();
            Value = parts[1].Trim();
        }
    }
}