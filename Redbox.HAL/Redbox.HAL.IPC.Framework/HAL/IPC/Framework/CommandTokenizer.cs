using Redbox.HAL.Component.Model;
using Redbox.HAL.Core;

namespace Redbox.HAL.IPC.Framework
{
    public class CommandTokenizer : Tokenizer<CommandParserState>
    {
        private char m_expectedClosingQuote;
        private bool m_expectingValue;
        private bool m_inQuotedValue;
        private bool m_isEscaped;
        private bool m_treatAsLiteral;

        public CommandTokenizer(int lineNumber, string statement) : base(lineNumber, statement)
        {
        }

        protected override void OnReset()
        {
            CurrentState = CommandParserState.Start;
        }

        [StateHandler(State = CommandParserState.Error)]
        internal StateResult ProcessErrorState()
        {
            ResetAccumulator();
            Errors.Add(Error.NewError("T006", FormatError("An invalid token was detected."),
                "Correct your command syntax and resubmit."));
            return StateResult.Terminal;
        }

        [StateHandler(State = CommandParserState.Start)]
        internal StateResult ProcessStartState()
        {
            if (char.IsWhiteSpace(GetCurrentToken()))
            {
                CurrentState = CommandParserState.Whitespace;
                return StateResult.Restart;
            }

            if (GetCurrentToken() == '-')
            {
                var nullable1 = PeekNextToken();
                var nullable2 = nullable1.HasValue ? nullable1.GetValueOrDefault() : new int?();
                var num = 45;
                if ((nullable2.GetValueOrDefault() == num) & nullable2.HasValue)
                {
                    CurrentState = CommandParserState.Comment;
                    return StateResult.Restart;
                }
            }

            if (char.IsLetter(GetCurrentToken()))
            {
                CurrentState = CommandParserState.Command;
                return StateResult.Restart;
            }

            if (!char.IsPunctuation(GetCurrentToken()))
                return StateResult.Continue;
            CurrentState = CommandParserState.Error;
            return StateResult.Restart;
        }

        [StateHandler(State = CommandParserState.Comment)]
        internal StateResult ProcessCommentState()
        {
            AddTokenAndReset(TokenType.Comment, false);
            return StateResult.Terminal;
        }

        [StateHandler(State = CommandParserState.Command)]
        internal StateResult ProcessCommandState()
        {
            var currentToken = GetCurrentToken();
            if (char.IsLetter(currentToken) || currentToken == '-')
                AppendToAccumulator();
            if (char.IsWhiteSpace(currentToken))
            {
                AddTokenAndReset(TokenType.Mnemonic, false);
                CurrentState = CommandParserState.Whitespace;
                return StateResult.Restart;
            }

            if (PeekNextToken().HasValue)
                return StateResult.Continue;
            AddTokenAndReset(TokenType.Mnemonic, false);
            return StateResult.Continue;
        }

        [StateHandler(State = CommandParserState.Symbol)]
        internal StateResult ProcessSymbolState()
        {
            var currentToken = GetCurrentToken();
            if (currentToken == ':')
            {
                AppendToAccumulator();
                CurrentState = CommandParserState.Key;
                return StateResult.Restart;
            }

            if (char.IsWhiteSpace(currentToken))
            {
                AddTokenAndReset(TokenType.Symbol, false);
                CurrentState = CommandParserState.Whitespace;
                return StateResult.Restart;
            }

            if (!PeekNextToken().HasValue)
            {
                AppendToAccumulator();
                AddTokenAndReset(TokenType.Symbol, false);
                CurrentState = CommandParserState.Start;
                return StateResult.Continue;
            }

            AppendToAccumulator();
            return StateResult.Continue;
        }

        [StateHandler(State = CommandParserState.Key)]
        internal StateResult ProcessKeyState()
        {
            m_expectingValue = true;
            CurrentState = CommandParserState.Whitespace;
            return StateResult.Continue;
        }

        [StateHandler(State = CommandParserState.StringLiteral)]
        internal StateResult ProcessStringLiteralState()
        {
            var currentToken = GetCurrentToken();
            if (m_inQuotedValue && currentToken == m_expectedClosingQuote)
            {
                m_inQuotedValue = false;
                m_expectingValue = false;
                m_treatAsLiteral = false;
                AddTokenAndReset(TokenType.StringLiteral, ":");
                CurrentState = CommandParserState.Whitespace;
                return StateResult.Continue;
            }

            AppendToAccumulator();
            return StateResult.Continue;
        }

        [StateHandler(State = CommandParserState.Value)]
        internal StateResult ProcessValueState()
        {
            var currentToken = GetCurrentToken();
            if (currentToken == '\\' && m_inQuotedValue && !m_isEscaped)
            {
                var nullable1 = PeekNextToken();
                var nullable2 = nullable1;
                var nullable3 = nullable2.HasValue ? nullable2.GetValueOrDefault() : new int?();
                var num1 = 39;
                if (!((nullable3.GetValueOrDefault() == num1) & nullable3.HasValue))
                {
                    nullable2 = nullable1;
                    var nullable4 = nullable2.HasValue ? nullable2.GetValueOrDefault() : new int?();
                    var num2 = 34;
                    if (!((nullable4.GetValueOrDefault() == num2) & nullable4.HasValue))
                    {
                        nullable2 = nullable1;
                        var nullable5 = nullable2.HasValue ? nullable2.GetValueOrDefault() : new int?();
                        var num3 = 92;
                        if (!((nullable5.GetValueOrDefault() == num3) & nullable5.HasValue))
                        {
                            CurrentState = CommandParserState.Error;
                            return StateResult.Restart;
                        }
                    }
                }

                m_isEscaped = true;
            }
            else if ((currentToken == m_expectedClosingQuote && !m_isEscaped) ||
                     (char.IsWhiteSpace(currentToken) && !m_inQuotedValue))
            {
                m_inQuotedValue = false;
                m_expectingValue = false;
                AddTokenAndReset(TokenType.StringLiteral, ":");
                CurrentState = CommandParserState.Whitespace;
            }
            else if (!PeekNextToken().HasValue)
            {
                if (m_inQuotedValue)
                {
                    CurrentState = CommandParserState.Error;
                    return StateResult.Restart;
                }

                m_expectingValue = false;
                AppendToAccumulator();
                AddTokenAndReset(TokenType.StringLiteral, ":");
                CurrentState = CommandParserState.Whitespace;
            }
            else
            {
                AppendToAccumulator();
                if (m_isEscaped)
                    m_isEscaped = false;
            }

            return StateResult.Continue;
        }

        [StateHandler(State = CommandParserState.Whitespace)]
        internal StateResult ProcessWhitespaceState()
        {
            var currentToken = GetCurrentToken();
            if (char.IsLetter(currentToken) || char.IsDigit(currentToken))
            {
                CurrentState = m_expectingValue
                    ? m_treatAsLiteral ? CommandParserState.StringLiteral : CommandParserState.Value
                    : CommandParserState.Symbol;
                return StateResult.Restart;
            }

            switch (currentToken)
            {
                case '"':
                case '\'':
                    m_expectedClosingQuote = currentToken;
                    CurrentState = m_treatAsLiteral ? CommandParserState.StringLiteral : CommandParserState.Value;
                    m_inQuotedValue = true;
                    break;
                case '@':
                    m_treatAsLiteral = true;
                    break;
                default:
                    if (m_inQuotedValue) AppendToAccumulator();
                    break;
            }

            return StateResult.Continue;
        }
    }
}