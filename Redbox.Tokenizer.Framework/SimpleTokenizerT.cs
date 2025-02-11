namespace Redbox.Tokenizer.Framework
{
    public class SimpleTokenizer<T> : TokenizerBase<T>
    {
        public SimpleTokenizer(int lineNumber, string statement)
            : base(lineNumber, statement)
        {
            Tokens = new SimpleTokenList();
        }

        public SimpleTokenList Tokens { get; protected set; }

        protected internal void AddTokenAndReset(SimpleTokenType type, bool isKeyValuePair)
        {
            Tokens.Add(new SimpleToken(type,
                type == SimpleTokenType.Comment ? m_tokenReader.RemainingTokens.Trim() : GetAccumulatedToken(),
                isKeyValuePair));
            ResetAccumulator();
        }

        protected internal void AddTokenAndReset(SimpleTokenType type, string pairSeparator)
        {
            Tokens.Add(new SimpleToken(type,
                type == SimpleTokenType.Comment ? m_tokenReader.RemainingTokens.Trim() : GetAccumulatedToken(),
                pairSeparator));
            ResetAccumulator();
        }
    }
}