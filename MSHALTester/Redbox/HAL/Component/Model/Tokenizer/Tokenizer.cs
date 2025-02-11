namespace Redbox.HAL.Component.Model.Tokenizer;

public class Tokenizer<T> : TokenizerBase<T>
{
    public Tokenizer(int lineNumber, string statement)
        : base(lineNumber, statement)
    {
        Tokens = new TokenList();
    }

    public TokenList Tokens { get; protected set; }

    protected internal void AddTokenAndReset(TokenType type, bool isKeyValuePair)
    {
        Tokens.Add(new Token(type,
            type == TokenType.Comment ? m_tokenReader.RemainingTokens.Trim() : GetAccumulatedToken(), isKeyValuePair));
        ResetAccumulator();
    }

    protected internal void AddTokenAndReset(TokenType type, string pairSeparator)
    {
        Tokens.Add(new Token(type,
            type == TokenType.Comment ? m_tokenReader.RemainingTokens.Trim() : GetAccumulatedToken(), pairSeparator));
        ResetAccumulator();
    }
}