using Redbox.Core;
using System;
using System.Collections.Generic;

namespace Redbox.Tokenizer.Framework
{
    [Serializable]
    internal class SimpleTokenList : List<SimpleToken>, ICloneable<SimpleTokenList>
    {
        public SimpleTokenList()
        {
        }

        public SimpleTokenList(IEnumerable<SimpleToken> tokens)
          : base(tokens)
        {
        }

        public SimpleToken GetLabel()
        {
            return this.Find((Predicate<SimpleToken>)(each => each.Type == SimpleTokenType.Label));
        }

        public SimpleToken GetMnemonic()
        {
            return this.Find((Predicate<SimpleToken>)(each => each.Type == SimpleTokenType.Mnemonic));
        }

        public bool HasOnlyLabel()
        {
            return this.Count > 0 && this.FindAll((Predicate<SimpleToken>)(each => each.Type == SimpleTokenType.Label)).Count == this.Count;
        }

        public bool HasOnlyComments()
        {
            return this.Count > 0 && this.FindAll((Predicate<SimpleToken>)(each => each.Type == SimpleTokenType.Comment)).Count == this.Count;
        }

        public SimpleTokenList GetSymbols()
        {
            return new SimpleTokenList((IEnumerable<SimpleToken>)this.FindAll((Predicate<SimpleToken>)(each => each.Type == SimpleTokenType.Symbol)));
        }

        public SimpleTokenList GetSymbolsAndLiterals()
        {
            return new SimpleTokenList((IEnumerable<SimpleToken>)this.FindAll((Predicate<SimpleToken>)(each => each.Type == SimpleTokenType.Symbol || each.Type == SimpleTokenType.StringLiteral || each.Type == SimpleTokenType.NumericLiteral)));
        }

        public SimpleTokenList GetTokensAfterMnemonic()
        {
            int index1 = this.FindIndex((Predicate<SimpleToken>)(each => each.Type == SimpleTokenType.Mnemonic));
            if (index1 == -1)
                return new SimpleTokenList();
            SimpleTokenList tokensAfterMnemonic = new SimpleTokenList();
            for (int index2 = index1 + 1; index2 < this.Count; ++index2)
                tokensAfterMnemonic.Add(this[index2]);
            return tokensAfterMnemonic;
        }

        public SimpleTokenList GetAllLiterals()
        {
            return new SimpleTokenList((IEnumerable<SimpleToken>)this.FindAll((Predicate<SimpleToken>)(each => each.Type == SimpleTokenType.StringLiteral || each.Type == SimpleTokenType.NumericLiteral)));
        }

        public SimpleTokenList GetKeyValuePairs()
        {
            return new SimpleTokenList((IEnumerable<SimpleToken>)this.FindAll((Predicate<SimpleToken>)(each => each.IsKeyValuePair)));
        }

        public KeyValuePair GetKeyValuePair(string key)
        {
            SimpleToken simpleToken = this.Find((Predicate<SimpleToken>)(each => each.IsKeyValuePair && string.Compare(((KeyValuePair)each.ConvertValue()).Key, key, true) == 0));
            return simpleToken != null ? (KeyValuePair)simpleToken.ConvertValue() : (KeyValuePair)null;
        }

        public SimpleTokenList GetNumericLiterals()
        {
            return new SimpleTokenList((IEnumerable<SimpleToken>)this.FindAll((Predicate<SimpleToken>)(each => each.Type == SimpleTokenType.NumericLiteral)));
        }

        public SimpleTokenList GetStringLiterals()
        {
            return new SimpleTokenList((IEnumerable<SimpleToken>)this.FindAll((Predicate<SimpleToken>)(each => each.Type == SimpleTokenType.StringLiteral)));
        }

        public SimpleTokenList Clone(params object[] parms)
        {
            SimpleTokenList simpleTokenList = new SimpleTokenList();
            foreach (SimpleToken simpleToken in (List<SimpleToken>)this)
                simpleTokenList.Add(simpleToken.Clone(Array.Empty<object>()));
            return simpleTokenList;
        }
    }
}
