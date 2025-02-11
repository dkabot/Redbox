using System;
using System.Collections.Generic;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Core;

[Serializable]
public class TokenList : List<Token>, ICloneable<TokenList>
{
    public TokenList()
    {
    }

    public TokenList(IEnumerable<Token> tokens)
        : base(tokens)
    {
    }

    public TokenList Clone(params object[] parms)
    {
        var tokenList = new TokenList();
        foreach (var token in this)
            tokenList.Add(token.Clone());
        return tokenList;
    }

    public Token GetLabel()
    {
        return Find(each => each.Type == TokenType.Label);
    }

    public Token GetMnemonic()
    {
        return Find(each => each.Type == TokenType.Mnemonic);
    }

    public bool HasOnlyLabel()
    {
        return Count > 0 && FindAll(each => each.Type == TokenType.Label).Count == Count;
    }

    public bool HasOnlyComments()
    {
        return Count > 0 && FindAll(each => each.Type == TokenType.Comment).Count == Count;
    }

    public TokenList GetSymbols()
    {
        return new TokenList(FindAll(each =>
            (each.Type == TokenType.Symbol || each.Type == TokenType.ConstSymbol) && !each.Value.Equals("TRUE") &&
            !each.Value.Equals("FALSE")));
    }

    public TokenList GetSymbolsAndLiterals()
    {
        return new TokenList(FindAll(each =>
            each.Type == TokenType.Symbol || each.Type == TokenType.ConstSymbol ||
            each.Type == TokenType.StringLiteral || each.Type == TokenType.NumericLiteral));
    }

    public TokenList GetTokensAfterMnemonic()
    {
        var index1 = FindIndex(each => each.Type == TokenType.Mnemonic);
        if (index1 == -1)
            return new TokenList();
        var tokensAfterMnemonic = new TokenList();
        for (var index2 = index1 + 1; index2 < Count; ++index2)
            tokensAfterMnemonic.Add(this[index2]);
        return tokensAfterMnemonic;
    }

    public TokenList GetAllLiterals()
    {
        return new TokenList(FindAll(each =>
            each.Type == TokenType.StringLiteral || each.Type == TokenType.NumericLiteral));
    }

    public TokenList GetKeyValuePairs()
    {
        return new TokenList(FindAll(each => each.IsKeyValuePair));
    }

    public KeyValuePair GetKeyValuePair(string key)
    {
        var token = Find(each =>
            each.IsKeyValuePair && string.Compare(((KeyValuePair)each.ConvertValue()).Key, key, true) == 0);
        return token != null ? (KeyValuePair)token.ConvertValue() : null;
    }

    public TokenList GetNumericLiterals()
    {
        return new TokenList(FindAll(each => each.Type == TokenType.NumericLiteral));
    }

    public TokenList GetStringLiterals()
    {
        return new TokenList(FindAll(each => each.Type == TokenType.StringLiteral));
    }
}