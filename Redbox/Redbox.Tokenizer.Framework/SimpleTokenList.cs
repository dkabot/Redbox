using System;
using System.Collections.Generic;
using Redbox.Core;

namespace Redbox.Tokenizer.Framework
{
    [Serializable]
    public class SimpleTokenList : List<SimpleToken>, ICloneable<SimpleTokenList>
    {
        public SimpleTokenList()
        {
        }

        public SimpleTokenList(IEnumerable<SimpleToken> tokens)
            : base(tokens)
        {
        }

        public SimpleTokenList Clone(params object[] parms)
        {
            var simpleTokenList = new SimpleTokenList();
            foreach (var simpleToken in this)
                simpleTokenList.Add(simpleToken.Clone());
            return simpleTokenList;
        }

        public SimpleToken GetLabel()
        {
            return Find(each => each.Type == SimpleTokenType.Label);
        }

        public SimpleToken GetMnemonic()
        {
            return Find(each => each.Type == SimpleTokenType.Mnemonic);
        }

        public bool HasOnlyLabel()
        {
            return Count > 0 && FindAll(each => each.Type == SimpleTokenType.Label).Count == Count;
        }

        public bool HasOnlyComments()
        {
            return Count > 0 && FindAll(each => each.Type == SimpleTokenType.Comment).Count == Count;
        }

        public SimpleTokenList GetSymbols()
        {
            return new SimpleTokenList(FindAll(each => each.Type == SimpleTokenType.Symbol));
        }

        public SimpleTokenList GetSymbolsAndLiterals()
        {
            return new SimpleTokenList(FindAll(each =>
                each.Type == SimpleTokenType.Symbol || each.Type == SimpleTokenType.StringLiteral ||
                each.Type == SimpleTokenType.NumericLiteral));
        }

        public SimpleTokenList GetTokensAfterMnemonic()
        {
            var index1 = FindIndex(each => each.Type == SimpleTokenType.Mnemonic);
            if (index1 == -1)
                return new SimpleTokenList();
            var tokensAfterMnemonic = new SimpleTokenList();
            for (var index2 = index1 + 1; index2 < Count; ++index2)
                tokensAfterMnemonic.Add(this[index2]);
            return tokensAfterMnemonic;
        }

        public SimpleTokenList GetAllLiterals()
        {
            return new SimpleTokenList(FindAll(each =>
                each.Type == SimpleTokenType.StringLiteral || each.Type == SimpleTokenType.NumericLiteral));
        }

        public SimpleTokenList GetKeyValuePairs()
        {
            return new SimpleTokenList(FindAll(each => each.IsKeyValuePair));
        }

        public KeyValuePair GetKeyValuePair(string key)
        {
            var simpleToken = Find(each =>
                each.IsKeyValuePair && string.Compare(((KeyValuePair)each.ConvertValue()).Key, key, true) == 0);
            return simpleToken != null ? (KeyValuePair)simpleToken.ConvertValue() : null;
        }

        public SimpleTokenList GetNumericLiterals()
        {
            return new SimpleTokenList(FindAll(each => each.Type == SimpleTokenType.NumericLiteral));
        }

        public SimpleTokenList GetStringLiterals()
        {
            return new SimpleTokenList(FindAll(each => each.Type == SimpleTokenType.StringLiteral));
        }
    }
}