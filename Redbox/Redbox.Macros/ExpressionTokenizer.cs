using System.Globalization;
using System.Text;

namespace Redbox.Macros
{
    public class ExpressionTokenizer
    {
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

        private static readonly CharToTokenType[] charToTokenType = new CharToTokenType[15]
        {
            new CharToTokenType('+', TokenType.Plus),
            new CharToTokenType('-', TokenType.Minus),
            new CharToTokenType('*', TokenType.Mul),
            new CharToTokenType('/', TokenType.Div),
            new CharToTokenType('%', TokenType.Mod),
            new CharToTokenType('<', TokenType.LT),
            new CharToTokenType('>', TokenType.GT),
            new CharToTokenType('(', TokenType.LeftParen),
            new CharToTokenType(')', TokenType.RightParen),
            new CharToTokenType('{', TokenType.LeftCurlyBrace),
            new CharToTokenType('}', TokenType.RightCurlyBrace),
            new CharToTokenType('!', TokenType.Not),
            new CharToTokenType('$', TokenType.Dollar),
            new CharToTokenType(',', TokenType.Comma),
            new CharToTokenType('.', TokenType.Dot)
        };

        private static readonly TokenType[] charIndexToTokenType = new TokenType[128];
        private int _position;
        private string _text;

        static ExpressionTokenizer()
        {
            for (var index = 0; index < 128; ++index)
                charIndexToTokenType[index] = TokenType.Punctuation;
            foreach (var charToTokenType in charToTokenType)
                charIndexToTokenType[charToTokenType.ch] = charToTokenType.tokenType;
        }

        public bool IgnoreWhitespace { get; set; } = true;

        public bool SingleCharacterMode { get; set; }

        public TokenType CurrentToken { get; private set; }

        public string TokenText { get; private set; }

        public Position CurrentPosition { get; private set; }

        public void InitTokenizer(string s)
        {
            _text = s;
            _position = 0;
            CurrentToken = TokenType.BOF;
            GetNextToken();
        }

        public void GetNextToken()
        {
            if (CurrentToken == TokenType.EOF)
                throw new ExpressionParseException(ResourceUtils.GetString("String_CannotReadPastStream"), -1, -1);
            if (IgnoreWhitespace)
                SkipWhitespace();
            CurrentPosition = new Position(_position);
            var num1 = PeekChar();
            if (num1 == -1)
            {
                CurrentToken = TokenType.EOF;
            }
            else
            {
                var c1 = (char)num1;
                if (!SingleCharacterMode)
                {
                    if (!IgnoreWhitespace && char.IsWhiteSpace(c1))
                    {
                        var stringBuilder = new StringBuilder();
                        int c2;
                        while ((c2 = PeekChar()) != -1 && char.IsWhiteSpace((char)c2))
                        {
                            stringBuilder.Append((char)c2);
                            ReadChar();
                        }

                        CurrentToken = TokenType.Whitespace;
                        TokenText = stringBuilder.ToString();
                        return;
                    }

                    if (char.IsDigit(c1))
                    {
                        CurrentToken = TokenType.Number;
                        var str = string.Empty + c1;
                        ReadChar();
                        int c3;
                        while ((c3 = PeekChar()) != -1 && char.IsDigit((char)c3))
                            str += ((char)ReadChar()).ToString();
                        TokenText = str;
                        return;
                    }

                    switch (c1)
                    {
                        case '\'':
                            CurrentToken = TokenType.String;
                            var str1 = "";
                            ReadChar();
                            int num2;
                            while ((num2 = ReadChar()) != -1)
                            {
                                var ch = (char)num2;
                                if (ch == '\'')
                                {
                                    if (PeekChar() == 39)
                                        ReadChar();
                                    else
                                        break;
                                }

                                str1 += ch.ToString();
                            }

                            TokenText = str1;
                            return;
                        case '_':
                            CurrentToken = TokenType.Keyword;
                            var stringBuilder1 = new StringBuilder();
                            stringBuilder1.Append(c1);
                            ReadChar();
                            int num3;
                            while ((num3 = PeekChar()) != -1)
                            {
                                var c4 = (char)num3;
                                switch (c4)
                                {
                                    case '-':
                                    case '.':
                                    case '\\':
                                    case '_':
                                        ReadChar();
                                        stringBuilder1.Append(c4);
                                        continue;
                                    default:
                                        if (!char.IsLetterOrDigit(c4))
                                        {
                                            if (c4 == ':' && PeekChar2() != 58)
                                            {
                                                ReadChar();
                                                stringBuilder1.Append(':');
                                                continue;
                                            }

                                            goto label_33;
                                        }

                                        goto case '-';
                                }
                            }

                            label_33:
                            TokenText = stringBuilder1.ToString();
                            if (!TokenText.EndsWith("-") && !TokenText.EndsWith(".") && !TokenText.EndsWith(":"))
                                return;
                            throw new ExpressionParseException(
                                string.Format(CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1182"),
                                    TokenText), CurrentPosition.CharIndex);
                        default:
                            if (!char.IsLetter(c1))
                            {
                                ReadChar();
                                var num4 = PeekChar();
                                if (c1 == ':' && num4 == 58)
                                {
                                    CurrentToken = TokenType.DoubleColon;
                                    TokenText = "::";
                                    ReadChar();
                                    return;
                                }

                                if (c1 == '!' && num4 == 61)
                                {
                                    CurrentToken = TokenType.NE;
                                    TokenText = "!=";
                                    ReadChar();
                                    return;
                                }

                                if (c1 == '=' && num4 == 61)
                                {
                                    CurrentToken = TokenType.EQ;
                                    TokenText = "==";
                                    ReadChar();
                                    return;
                                }

                                if (c1 == '<' && num4 == 61)
                                {
                                    CurrentToken = TokenType.LE;
                                    TokenText = "<=";
                                    ReadChar();
                                    return;
                                }

                                if (c1 == '>' && num4 == 61)
                                {
                                    CurrentToken = TokenType.GE;
                                    TokenText = ">=";
                                    ReadChar();
                                    return;
                                }

                                break;
                            }

                            goto case '_';
                    }
                }
                else
                {
                    ReadChar();
                }

                TokenText = new string(c1, 1);
                if (c1 >= ' ' && c1 < '\u0080')
                    CurrentToken = charIndexToTokenType[c1];
                else
                    CurrentToken = TokenType.Punctuation;
            }
        }

        public bool IsKeyword(string k)
        {
            return CurrentToken == TokenType.Keyword && TokenText == k;
        }

        private int ReadChar()
        {
            return _position < _text.Length ? _text[_position++] : -1;
        }

        private int PeekChar()
        {
            return _position < _text.Length ? _text[_position] : -1;
        }

        private int PeekChar2()
        {
            return _position + 1 < _text.Length ? _text[_position + 1] : -1;
        }

        private void SkipWhitespace()
        {
            int c;
            while ((c = PeekChar()) != -1 && char.IsWhiteSpace((char)c))
                ReadChar();
        }

        public struct Position
        {
            public int CharIndex { get; }

            public Position(int charIndex)
            {
                CharIndex = charIndex;
            }
        }

        private struct CharToTokenType
        {
            public readonly char ch;
            public readonly TokenType tokenType;

            public CharToTokenType(char ch, TokenType tokenType)
            {
                this.ch = ch;
                this.tokenType = tokenType;
            }
        }
    }
}