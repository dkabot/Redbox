using System;

namespace Redbox.Tokenizer.Framework
{
    [Serializable]
    internal class KeyValuePair
    {
        public KeyValuePair(string value, SimpleTokenType type)
          : this(value, type, "=")
        {
        }

        public KeyValuePair(string value, SimpleTokenType type, string pairSeparator)
        {
            this.Type = type;
            this.PairSeparator = pairSeparator;
            this.SetParts(value.Split(pairSeparator.ToCharArray(), 2, StringSplitOptions.RemoveEmptyEntries));
        }

        public override string ToString()
        {
            return string.Format("{0}{1}{2}", (object)this.Key, (object)this.PairSeparator, (object)this.Value);
        }

        public string Key { get; private set; }

        public string Value { get; private set; }

        public SimpleTokenType Type { get; private set; }

        public string PairSeparator { get; private set; }

        private void SetParts(string[] parts)
        {
            if (parts.Length != 2)
                return;
            this.Key = parts[0].Trim();
            this.Value = parts[1].Trim();
        }
    }
}
