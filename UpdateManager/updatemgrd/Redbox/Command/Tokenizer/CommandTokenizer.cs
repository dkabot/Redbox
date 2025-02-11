using Redbox.Tokenizer.Framework;

namespace Redbox.Command.Tokenizer
{
    internal class CommandTokenizer :
      SimpleTokenizer<CommandParserState>
    {
        private bool m_isEscaped;
        private bool m_inQuotedValue;
        private bool m_expectingValue;
        private bool m_inPathExpression;
        private char m_expectedClosingQuote;

        public CommandTokenizer(int lineNumber, string statement) : base(lineNumber, statement)
        {
        }

        protected override void OnReset() => this.CurrentState = CommandParserState.Start;

        [Redbox.Tokenizer.Framework.StateHandler(State = CommandParserState.Error)]
        internal StateResult ProcessErrorState()
        {
            this.ResetAccumulator();
            this.Errors.Add(Error.NewError("T006", this.FormatError("An invalid token was detected."), "Correct your command syntax and resubmit."));
            return StateResult.Terminal;
        }

        [Redbox.Tokenizer.Framework.StateHandler(State = CommandParserState.Start)]
        internal StateResult ProcessStartState()
        {
            if (char.IsWhiteSpace(this.GetCurrentToken()))
            {
                this.CurrentState = CommandParserState.Whitespace;
                return StateResult.Restart;
            }
            if (this.GetCurrentToken() == '-')
            {
                char? nullable1 = this.PeekNextToken();
                int? nullable2 = nullable1.HasValue ? new int?((int)nullable1.GetValueOrDefault()) : new int?();
                int num = 45;
                if ((nullable2.GetValueOrDefault() == num ? (nullable2.HasValue ? 1 : 0) : 0) != 0)
                {
                    this.CurrentState = CommandParserState.Comment;
                    return StateResult.Restart;
                }
            }
            if (char.IsLetter(this.GetCurrentToken()))
            {
                this.CurrentState = CommandParserState.Command;
                return StateResult.Restart;
            }
            if (!char.IsPunctuation(this.GetCurrentToken()))
                return StateResult.Continue;
            this.CurrentState = CommandParserState.Error;
            return StateResult.Restart;
        }

        [Redbox.Tokenizer.Framework.StateHandler(State = CommandParserState.Comment)]
        internal StateResult ProcessCommentState()
        {
            this.AddTokenAndReset(SimpleTokenType.Comment, false);
            return StateResult.Terminal;
        }

        [Redbox.Tokenizer.Framework.StateHandler(State = CommandParserState.Command)]
        internal StateResult ProcessCommandState()
        {
            char currentToken = this.GetCurrentToken();
            if (char.IsLetter(currentToken) || currentToken == '-')
                this.AppendToAccumulator();
            if (char.IsWhiteSpace(currentToken))
            {
                this.AddTokenAndReset(SimpleTokenType.Mnemonic, false);
                this.CurrentState = CommandParserState.Whitespace;
                return StateResult.Restart;
            }
            if (this.PeekNextToken().HasValue)
                return StateResult.Continue;
            this.AddTokenAndReset(SimpleTokenType.Mnemonic, false);
            return StateResult.Continue;
        }

        [Redbox.Tokenizer.Framework.StateHandler(State = CommandParserState.Symbol)]
        internal StateResult ProcessSymbolState()
        {
            char currentToken = this.GetCurrentToken();
            if (currentToken == ':')
            {
                this.AppendToAccumulator();
                this.CurrentState = CommandParserState.Key;
                return StateResult.Restart;
            }
            if (char.IsWhiteSpace(currentToken))
            {
                this.AddTokenAndReset(SimpleTokenType.Symbol, false);
                this.CurrentState = CommandParserState.Whitespace;
                return StateResult.Restart;
            }
            if (!this.PeekNextToken().HasValue)
            {
                this.AppendToAccumulator();
                this.AddTokenAndReset(SimpleTokenType.Symbol, false);
                this.CurrentState = CommandParserState.Start;
                return StateResult.Continue;
            }
            this.AppendToAccumulator();
            return StateResult.Continue;
        }

        [Redbox.Tokenizer.Framework.StateHandler(State = CommandParserState.Key)]
        internal StateResult ProcessKeyState()
        {
            this.m_expectingValue = true;
            this.CurrentState = CommandParserState.Whitespace;
            return StateResult.Continue;
        }

        [Redbox.Tokenizer.Framework.StateHandler(State = CommandParserState.Value)]
        internal StateResult ProcessValueState()
        {
            char currentToken = this.GetCurrentToken();
            if (currentToken == '\\' && this.m_inQuotedValue && !this.m_isEscaped)
            {
                char? nullable1 = this.PeekNextToken();
                char? nullable2 = nullable1;
                int? nullable3 = nullable2.HasValue ? new int?((int)nullable2.GetValueOrDefault()) : new int?();
                int num1 = 39;
                if ((nullable3.GetValueOrDefault() == num1 ? (nullable3.HasValue ? 1 : 0) : 0) == 0)
                {
                    nullable2 = nullable1;
                    nullable3 = nullable2.HasValue ? new int?((int)nullable2.GetValueOrDefault()) : new int?();
                    int num2 = 34;
                    if ((nullable3.GetValueOrDefault() == num2 ? (nullable3.HasValue ? 1 : 0) : 0) == 0)
                    {
                        nullable2 = nullable1;
                        nullable3 = nullable2.HasValue ? new int?((int)nullable2.GetValueOrDefault()) : new int?();
                        int num3 = 92;
                        if ((nullable3.GetValueOrDefault() == num3 ? (nullable3.HasValue ? 1 : 0) : 0) == 0)
                        {
                            this.CurrentState = CommandParserState.Error;
                            return StateResult.Restart;
                        }
                    }
                }
                this.m_isEscaped = true;
            }
            else if ((int)currentToken == (int)this.m_expectedClosingQuote && !this.m_isEscaped || char.IsWhiteSpace(currentToken) && !this.m_inQuotedValue)
            {
                this.m_inQuotedValue = false;
                this.m_expectingValue = false;
                if (this.m_inPathExpression)
                {
                    this.AddTokenAndReset(SimpleTokenType.StringLiteral, false);
                    this.m_inPathExpression = false;
                }
                else
                    this.AddTokenAndReset(SimpleTokenType.StringLiteral, ":");
                this.CurrentState = CommandParserState.Whitespace;
            }
            else if (!this.PeekNextToken().HasValue)
            {
                if (this.m_inQuotedValue)
                {
                    this.CurrentState = CommandParserState.Error;
                    return StateResult.Restart;
                }
                this.m_expectingValue = false;
                this.AppendToAccumulator();
                if (this.m_inPathExpression)
                {
                    this.AddTokenAndReset(SimpleTokenType.StringLiteral, false);
                    this.m_inPathExpression = false;
                }
                else
                    this.AddTokenAndReset(SimpleTokenType.StringLiteral, ":");
                this.CurrentState = CommandParserState.Whitespace;
            }
            else
            {
                this.AppendToAccumulator();
                if (this.m_isEscaped)
                    this.m_isEscaped = false;
            }
            return StateResult.Continue;
        }

        [Redbox.Tokenizer.Framework.StateHandler(State = CommandParserState.Whitespace)]
        internal StateResult ProcessWhitespaceState()
        {
            char currentToken = this.GetCurrentToken();
            if (char.IsLetter(currentToken) || char.IsDigit(currentToken))
            {
                this.CurrentState = this.m_expectingValue ? CommandParserState.Value : CommandParserState.Symbol;
                return StateResult.Restart;
            }
            switch (currentToken)
            {
                case '"':
                case '\'':
                    if (this.m_expectingValue)
                    {
                        this.m_expectedClosingQuote = currentToken;
                        this.CurrentState = CommandParserState.Value;
                        this.m_inQuotedValue = true;
                        break;
                    }
                    this.CurrentState = CommandParserState.Error;
                    return StateResult.Restart;
                case '>':
                    if (this.m_inPathExpression)
                    {
                        this.Errors.Add(Error.NewError("T005", this.FormatError("Cannot nest redirection operators."), "Correct your command syntax and resubmit."));
                        return StateResult.Terminal;
                    }
                    this.AppendToAccumulator();
                    char? nullable1 = this.PeekNextToken();
                    int? nullable2 = nullable1.HasValue ? new int?((int)nullable1.GetValueOrDefault()) : new int?();
                    int num = 62;
                    if ((nullable2.GetValueOrDefault() == num ? (nullable2.HasValue ? 1 : 0) : 0) != 0)
                    {
                        this.AppendToAccumulator();
                        this.MoveToNextToken();
                    }
                    this.AddTokenAndReset(SimpleTokenType.Operator, false);
                    this.m_inPathExpression = true;
                    this.m_expectingValue = true;
                    return StateResult.Continue;
                default:
                    if (this.m_inQuotedValue)
                    {
                        this.AppendToAccumulator();
                        break;
                    }
                    break;
            }
            return StateResult.Continue;
        }
    }
}
