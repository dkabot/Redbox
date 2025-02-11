using System;

namespace Redbox.HAL.Component.Model.Tokenizer;

[Serializable]
public class KeyValuePair
{
    public KeyValuePair(string value, TokenType type)
        : this(value, type, "=")
    {
    }

    public KeyValuePair(string value, TokenType type, string pairSeparator)
    {
        Type = type;
        PairSeparator = pairSeparator;
        SetParts(value.Split(pairSeparator.ToCharArray(), 2, StringSplitOptions.RemoveEmptyEntries));
    }

    public string Key { get; private set; }

    public string Value { get; private set; }

    public TokenType Type { get; private set; }

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