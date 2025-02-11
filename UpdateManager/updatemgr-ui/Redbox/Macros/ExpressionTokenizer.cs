using System;
using System.Globalization;
using System.Text;

namespace Redbox.Macros
{
    class ExpressionTokenizer
    {
        public void InitTokenizer(string s)
        {
            this._text = s;
            this._position = 0;
            this.CurrentToken = ExpressionTokenizer.TokenType.BOF;
            this.GetNextToken();
        }

        public void GetNextToken()
        {
            if (this.CurrentToken == ExpressionTokenizer.TokenType.EOF)
            {
                throw new ExpressionParseException(ResourceUtils.GetString("String_CannotReadPastStream"), -1, -1);
            }
            if (this.IgnoreWhitespace)
            {
                this.SkipWhitespace();
            }
            this.CurrentPosition = new ExpressionTokenizer.Position(this._position);
            int num = this.PeekChar();
            if (num == -1)
            {
                this.CurrentToken = ExpressionTokenizer.TokenType.EOF;
                return;
            }
            char c = (char)num;
            if (!this.SingleCharacterMode)
            {
                if (!this.IgnoreWhitespace && char.IsWhiteSpace(c))
                {
                    StringBuilder stringBuilder = new StringBuilder();
                    while ((num = this.PeekChar()) != -1 && char.IsWhiteSpace((char)num))
                    {
                        stringBuilder.Append((char)num);
                        this.ReadChar();
                    }
                    this.CurrentToken = ExpressionTokenizer.TokenType.Whitespace;
                    this.TokenText = stringBuilder.ToString();
                    return;
                }
                if (char.IsDigit(c))
                {
                    this.CurrentToken = ExpressionTokenizer.TokenType.Number;
                    string text = string.Empty;
                    text += c.ToString();
                    this.ReadChar();
                    while ((num = this.PeekChar()) != -1)
                    {
                        c = (char)num;
                        if (!char.IsDigit(c))
                        {
                            break;
                        }
                        text += ((char)this.ReadChar()).ToString();
                    }
                    this.TokenText = text;
                    return;
                }
                if (c == '\'')
                {
                    this.CurrentToken = ExpressionTokenizer.TokenType.String;
                    string text2 = "";
                    this.ReadChar();
                    while ((num = this.ReadChar()) != -1)
                    {
                        c = (char)num;
                        if (c == '\'')
                        {
                            if (this.PeekChar() != 39)
                            {
                                break;
                            }
                            this.ReadChar();
                        }
                        text2 += c.ToString();
                    }
                    this.TokenText = text2;
                    return;
                }
                if (c == '_' || char.IsLetter(c))
                {
                    this.CurrentToken = ExpressionTokenizer.TokenType.Keyword;
                    StringBuilder stringBuilder2 = new StringBuilder();
                    stringBuilder2.Append(c);
                    this.ReadChar();
                    while ((num = this.PeekChar()) != -1)
                    {
                        char c2 = (char)num;
                        if (c2 == '_' || c2 == '-' || c2 == '.' || c2 == '\\' || char.IsLetterOrDigit(c2))
                        {
                            this.ReadChar();
                            stringBuilder2.Append(c2);
                        }
                        else
                        {
                            if (c2 != ':' || this.PeekChar2() == 58)
                            {
                                break;
                            }
                            this.ReadChar();
                            stringBuilder2.Append(':');
                        }
                    }
                    this.TokenText = stringBuilder2.ToString();
                    if (this.TokenText.EndsWith("-") || this.TokenText.EndsWith(".") || this.TokenText.EndsWith(":"))
                    {
                        throw new ExpressionParseException(string.Format(CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1182"), this.TokenText), this.CurrentPosition.CharIndex);
                    }
                    return;
                }
                else
                {
                    this.ReadChar();
                    num = this.PeekChar();
                    if (c == ':' && num == 58)
                    {
                        this.CurrentToken = ExpressionTokenizer.TokenType.DoubleColon;
                        this.TokenText = "::";
                        this.ReadChar();
                        return;
                    }
                    if (c == '!' && num == 61)
                    {
                        this.CurrentToken = ExpressionTokenizer.TokenType.NE;
                        this.TokenText = "!=";
                        this.ReadChar();
                        return;
                    }
                    if (c == '=' && num == 61)
                    {
                        this.CurrentToken = ExpressionTokenizer.TokenType.EQ;
                        this.TokenText = "==";
                        this.ReadChar();
                        return;
                    }
                    if (c == '<' && num == 61)
                    {
                        this.CurrentToken = ExpressionTokenizer.TokenType.LE;
                        this.TokenText = "<=";
                        this.ReadChar();
                        return;
                    }
                    if (c == '>' && num == 61)
                    {
                        this.CurrentToken = ExpressionTokenizer.TokenType.GE;
                        this.TokenText = ">=";
                        this.ReadChar();
                        return;
                    }
                }
            }
            else
            {
                this.ReadChar();
            }
            this.TokenText = new string(c, 1);
            if (c >= ' ' && c < '\u0080')
            {
                this.CurrentToken = ExpressionTokenizer.charIndexToTokenType[(int)c];
                return;
            }
            this.CurrentToken = ExpressionTokenizer.TokenType.Punctuation;
        }

        public bool IsKeyword(string k)
        {
            return this.CurrentToken == ExpressionTokenizer.TokenType.Keyword && this.TokenText == k;
        }

        public bool IgnoreWhitespace
        {
            get
            {
                return this._ignoreWhiteSpace;
            }
            set
            {
                this._ignoreWhiteSpace = value;
            }
        }

        public bool SingleCharacterMode { get; set; }

        public ExpressionTokenizer.TokenType CurrentToken { get; set; }

        public string TokenText { get; set; }

        public ExpressionTokenizer.Position CurrentPosition { get; set; }

        static ExpressionTokenizer()
        {
            for (int i = 0; i < 128; i++)
            {
                ExpressionTokenizer.charIndexToTokenType[i] = ExpressionTokenizer.TokenType.Punctuation;
            }
            foreach (ExpressionTokenizer.CharToTokenType charToTokenType in ExpressionTokenizer.charToTokenType)
            {
                ExpressionTokenizer.charIndexToTokenType[(int)charToTokenType.ch] = charToTokenType.tokenType;
            }
        }

        int ReadChar()
        {
            if (this._position < this._text.Length)
            {
                string text = this._text;
                int position = this._position;
                this._position = position + 1;
                return (int)text[position];
            }
            return -1;
        }

        int PeekChar()
        {
            if (this._position < this._text.Length)
            {
                return (int)this._text[this._position];
            }
            return -1;
        }

        int PeekChar2()
        {
            if (this._position + 1 < this._text.Length)
            {
                return (int)this._text[this._position + 1];
            }
            return -1;
        }

        void SkipWhitespace()
        {
            int num;
            while ((num = this.PeekChar()) != -1 && char.IsWhiteSpace((char)num))
            {
                this.ReadChar();
            }
        }

        static readonly ExpressionTokenizer.CharToTokenType[] charToTokenType = new ExpressionTokenizer.CharToTokenType[]
        {
            new ExpressionTokenizer.CharToTokenType('+', ExpressionTokenizer.TokenType.Plus),
            new ExpressionTokenizer.CharToTokenType('-', ExpressionTokenizer.TokenType.Minus),
            new ExpressionTokenizer.CharToTokenType('*', ExpressionTokenizer.TokenType.Mul),
            new ExpressionTokenizer.CharToTokenType('/', ExpressionTokenizer.TokenType.Div),
            new ExpressionTokenizer.CharToTokenType('%', ExpressionTokenizer.TokenType.Mod),
            new ExpressionTokenizer.CharToTokenType('<', ExpressionTokenizer.TokenType.LT),
            new ExpressionTokenizer.CharToTokenType('>', ExpressionTokenizer.TokenType.GT),
            new ExpressionTokenizer.CharToTokenType('(', ExpressionTokenizer.TokenType.LeftParen),
            new ExpressionTokenizer.CharToTokenType(')', ExpressionTokenizer.TokenType.RightParen),
            new ExpressionTokenizer.CharToTokenType('{', ExpressionTokenizer.TokenType.LeftCurlyBrace),
            new ExpressionTokenizer.CharToTokenType('}', ExpressionTokenizer.TokenType.RightCurlyBrace),
            new ExpressionTokenizer.CharToTokenType('!', ExpressionTokenizer.TokenType.Not),
            new ExpressionTokenizer.CharToTokenType('$', ExpressionTokenizer.TokenType.Dollar),
            new ExpressionTokenizer.CharToTokenType(',', ExpressionTokenizer.TokenType.Comma),
            new ExpressionTokenizer.CharToTokenType('.', ExpressionTokenizer.TokenType.Dot)
        };

        string _text;

        int _position;

        bool _ignoreWhiteSpace = true;

        static readonly ExpressionTokenizer.TokenType[] charIndexToTokenType = new ExpressionTokenizer.TokenType[128];

        public struct Position
        {
            public Position(int charIndex)
            {
                this._charIndex = charIndex;
            }

            public int CharIndex
            {
                get
                {
                    return this._charIndex;
                }
            }

            readonly int _charIndex;
        }

        public enum TokenType
        {
            BOF,
            EOF,
            Number,
            String,
            Keyword,
            EQ,
            NE,
            LT,
            GT,
            LE,
            GE,
            Plus,
            Minus,
            Mul,
            Div,
            Mod,
            LeftParen,
            RightParen,
            LeftCurlyBrace,
            RightCurlyBrace,
            Not,
            Punctuation,
            Whitespace,
            Dollar,
            Comma,
            Dot,
            DoubleColon
        }

        struct CharToTokenType
        {
            public CharToTokenType(char ch, ExpressionTokenizer.TokenType tokenType)
            {
                this.ch = ch;
                this.tokenType = tokenType;
            }

            public readonly char ch;

            public readonly ExpressionTokenizer.TokenType tokenType;
        }
    }
}
