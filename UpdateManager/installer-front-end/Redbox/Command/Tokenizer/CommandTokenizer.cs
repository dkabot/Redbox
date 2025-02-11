using System;
using Redbox.Tokenizer.Framework;

namespace Redbox.Command.Tokenizer
{
    class CommandTokenizer : SimpleTokenizer<CommandParserState>
    {
        public CommandTokenizer(int lineNumber, string statement)
            : base(lineNumber, statement)
        {
        }

        protected override void OnReset()
        {
            base.CurrentState = CommandParserState.Start;
        }

        [StateHandler(State = CommandParserState.Error)]
        internal StateResult ProcessErrorState()
        {
            base.ResetAccumulator();
            base.Errors.Add(Error.NewError("T006", base.FormatError("An invalid token was detected."), "Correct your command syntax and resubmit."));
            return StateResult.Terminal;
        }

        [StateHandler(State = CommandParserState.Start)]
        internal StateResult ProcessStartState()
        {
            if (char.IsWhiteSpace(base.GetCurrentToken()))
            {
                base.CurrentState = CommandParserState.Whitespace;
                return StateResult.Restart;
            }
            if (base.GetCurrentToken() == '-')
            {
                char? c = base.PeekNextToken();
                if (((c != null) ? new int?((int)c.GetValueOrDefault()) : null) == 45)
                {
                    base.CurrentState = CommandParserState.Comment;
                    return StateResult.Restart;
                }
            }
            if (char.IsLetter(base.GetCurrentToken()))
            {
                base.CurrentState = CommandParserState.Command;
                return StateResult.Restart;
            }
            if (char.IsPunctuation(base.GetCurrentToken()))
            {
                base.CurrentState = CommandParserState.Error;
                return StateResult.Restart;
            }
            return StateResult.Continue;
        }

        [StateHandler(State = CommandParserState.Comment)]
        internal StateResult ProcessCommentState()
        {
            base.AddTokenAndReset(SimpleTokenType.Comment, false);
            return StateResult.Terminal;
        }

        [StateHandler(State = CommandParserState.Command)]
        internal StateResult ProcessCommandState()
        {
            char currentToken = base.GetCurrentToken();
            if (char.IsLetter(currentToken) || currentToken == '-')
            {
                base.AppendToAccumulator();
            }
            if (char.IsWhiteSpace(currentToken))
            {
                base.AddTokenAndReset(SimpleTokenType.Mnemonic, false);
                base.CurrentState = CommandParserState.Whitespace;
                return StateResult.Restart;
            }
            if (base.PeekNextToken() == null)
            {
                base.AddTokenAndReset(SimpleTokenType.Mnemonic, false);
                return StateResult.Continue;
            }
            return StateResult.Continue;
        }

        [StateHandler(State = CommandParserState.Symbol)]
        internal StateResult ProcessSymbolState()
        {
            char currentToken = base.GetCurrentToken();
            if (currentToken == ':')
            {
                base.AppendToAccumulator();
                base.CurrentState = CommandParserState.Key;
                return StateResult.Restart;
            }
            if (char.IsWhiteSpace(currentToken))
            {
                base.AddTokenAndReset(SimpleTokenType.Symbol, false);
                base.CurrentState = CommandParserState.Whitespace;
                return StateResult.Restart;
            }
            if (base.PeekNextToken() == null)
            {
                base.AppendToAccumulator();
                base.AddTokenAndReset(SimpleTokenType.Symbol, false);
                base.CurrentState = CommandParserState.Start;
                return StateResult.Continue;
            }
            base.AppendToAccumulator();
            return StateResult.Continue;
        }

        [StateHandler(State = CommandParserState.Key)]
        internal StateResult ProcessKeyState()
        {
            this.m_expectingValue = true;
            base.CurrentState = CommandParserState.Whitespace;
            return StateResult.Continue;
        }

        [StateHandler(State = CommandParserState.Value)]
        internal StateResult ProcessValueState()
        {
            char currentToken = base.GetCurrentToken();
            if (currentToken == '\\' && this.m_inQuotedValue && !this.m_isEscaped)
            {
                char? c = base.PeekNextToken();
                char? c2 = c;
                if (!(((c2 != null) ? new int?((int)c2.GetValueOrDefault()) : null) == 39))
                {
                    c2 = c;
                    if (!(((c2 != null) ? new int?((int)c2.GetValueOrDefault()) : null) == 34))
                    {
                        c2 = c;
                        if (!(((c2 != null) ? new int?((int)c2.GetValueOrDefault()) : null) == 92))
                        {
                            base.CurrentState = CommandParserState.Error;
                            return StateResult.Restart;
                        }
                    }
                }
                this.m_isEscaped = true;
            }
            else if ((currentToken == this.m_expectedClosingQuote && !this.m_isEscaped) || (char.IsWhiteSpace(currentToken) && !this.m_inQuotedValue))
            {
                this.m_inQuotedValue = false;
                this.m_expectingValue = false;
                if (this.m_inPathExpression)
                {
                    base.AddTokenAndReset(SimpleTokenType.StringLiteral, false);
                    this.m_inPathExpression = false;
                }
                else
                {
                    base.AddTokenAndReset(SimpleTokenType.StringLiteral, ":");
                }
                base.CurrentState = CommandParserState.Whitespace;
            }
            else if (base.PeekNextToken() == null)
            {
                if (this.m_inQuotedValue)
                {
                    base.CurrentState = CommandParserState.Error;
                    return StateResult.Restart;
                }
                this.m_expectingValue = false;
                base.AppendToAccumulator();
                if (this.m_inPathExpression)
                {
                    base.AddTokenAndReset(SimpleTokenType.StringLiteral, false);
                    this.m_inPathExpression = false;
                }
                else
                {
                    base.AddTokenAndReset(SimpleTokenType.StringLiteral, ":");
                }
                base.CurrentState = CommandParserState.Whitespace;
            }
            else
            {
                base.AppendToAccumulator();
                if (this.m_isEscaped)
                {
                    this.m_isEscaped = false;
                }
            }
            return StateResult.Continue;
        }

        [StateHandler(State = CommandParserState.Whitespace)]
        internal StateResult ProcessWhitespaceState()
        {
            char currentToken = base.GetCurrentToken();
            if (char.IsLetter(currentToken) || char.IsDigit(currentToken))
            {
                base.CurrentState = (this.m_expectingValue ? CommandParserState.Value : CommandParserState.Symbol);
                return StateResult.Restart;
            }
            if (currentToken == '\'' || currentToken == '"')
            {
                if (!this.m_expectingValue)
                {
                    base.CurrentState = CommandParserState.Error;
                    return StateResult.Restart;
                }
                this.m_expectedClosingQuote = currentToken;
                base.CurrentState = CommandParserState.Value;
                this.m_inQuotedValue = true;
            }
            else if (currentToken == '>')
            {
                if (this.m_inPathExpression)
                {
                    base.Errors.Add(Error.NewError("T005", base.FormatError("Cannot nest redirection operators."), "Correct your command syntax and resubmit."));
                    return StateResult.Terminal;
                }
                base.AppendToAccumulator();
                char? c = base.PeekNextToken();
                if (((c != null) ? new int?((int)c.GetValueOrDefault()) : null) == 62)
                {
                    base.AppendToAccumulator();
                    base.MoveToNextToken();
                }
                base.AddTokenAndReset(SimpleTokenType.Operator, false);
                this.m_inPathExpression = true;
                this.m_expectingValue = true;
                return StateResult.Continue;
            }
            else if (this.m_inQuotedValue)
            {
                base.AppendToAccumulator();
            }
            return StateResult.Continue;
        }

        bool m_isEscaped;

        bool m_inQuotedValue;

        bool m_expectingValue;

        bool m_inPathExpression;

        char m_expectedClosingQuote;
    }
}
