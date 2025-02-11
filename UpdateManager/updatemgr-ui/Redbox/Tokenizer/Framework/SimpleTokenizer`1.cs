namespace Redbox.Tokenizer.Framework
{
    internal class SimpleTokenizer<T> : TokenizerBase<T>
    {
        public SimpleTokenizer(int lineNumber, string statement)
          : base(lineNumber, statement)
        {
            this.Tokens = new SimpleTokenList();
        }

        public SimpleTokenList Tokens { get; protected set; }

        protected internal void AddTokenAndReset(SimpleTokenType type, bool isKeyValuePair)
        {
            SimpleTokenList tokens = this.Tokens;
            int type1 = (int)type;
            SimpleToken simpleToken = new SimpleToken((SimpleTokenType)type1, type1 == 3 ? this.m_tokenReader.RemainingTokens.Trim() : this.GetAccumulatedToken(), isKeyValuePair);
            tokens.Add(simpleToken);
            this.ResetAccumulator();
        }

        protected internal void AddTokenAndReset(SimpleTokenType type, string pairSeparator)
        {
            SimpleTokenList tokens = this.Tokens;
            int type1 = (int)type;
            SimpleToken simpleToken = new SimpleToken((SimpleTokenType)type1, type1 == 3 ? this.m_tokenReader.RemainingTokens.Trim() : this.GetAccumulatedToken(), pairSeparator);
            tokens.Add(simpleToken);
            this.ResetAccumulator();
        }
    }
}
