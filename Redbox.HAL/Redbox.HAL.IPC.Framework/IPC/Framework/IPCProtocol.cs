using System;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Core;

namespace Redbox.IPC.Framework
{
    public class IPCProtocol : IIpcProtocol
    {
        private readonly string RawUri;

        private string PipeName;

        private IPCProtocol(string rawUri)
        {
            RawUri = rawUri;
        }

        public string GetPipeName()
        {
            if (PipeName == null) PipeName = string.Format("{0}{1}", Host, Port);
            return PipeName;
        }

        public bool IsSecure { get; set; }

        public ChannelType Channel { get; set; }

        public string Host { get; set; }

        public string Port { get; set; }

        public string Scheme { get; set; }

        public static IPCProtocol Parse(string protocolURI)
        {
            var ipcprotocol = new IPCProtocol(protocolURI)
            {
                Host = string.Empty,
                Port = string.Empty,
                IsSecure = false,
                Channel = ChannelType.Unknown
            };
            var protocolTokenizer = new ProtocolTokenizer(protocolURI);
            try
            {
                protocolTokenizer.Tokenize();
            }
            catch (Exception ex)
            {
                throw new UriFormatException(ex.Message);
            }

            if (protocolTokenizer.Errors.ContainsError())
            {
                foreach (var error in protocolTokenizer.Errors)
                    LogHelper.Instance.Log(error.Description, LogEntryType.Error);
                throw new UriFormatException("URI is malformed: see log for details.");
            }

            ipcprotocol.Host = protocolTokenizer.Host;
            ipcprotocol.Port = protocolTokenizer.Port;
            ipcprotocol.IsSecure = protocolTokenizer.IsSecure;
            ipcprotocol.Channel = protocolTokenizer.Channel;
            ipcprotocol.Scheme = protocolTokenizer.Scheme;
            if (string.IsNullOrEmpty(ipcprotocol.Host) || string.IsNullOrEmpty(ipcprotocol.Port))
                throw new UriFormatException("Host or port isn't set.");
            if (ipcprotocol.Channel == ChannelType.Unknown)
                throw new UriFormatException("The channel type is unknown; please correct your URI.");
            int num;
            if (ipcprotocol.Channel == ChannelType.Socket && !int.TryParse(ipcprotocol.Port, out num))
                throw new UriFormatException("Protocol is set up for sockets, but port isn't a valid address.");
            return ipcprotocol;
        }

        public override string ToString()
        {
            return RawUri;
        }

        private enum ProtocolParserState
        {
            Start = 1,
            Scheme,
            Host,
            Port,
            Channel,
            Whitespace,
            Error
        }

        private class ProtocolTokenizer : Tokenizer<ProtocolParserState>
        {
            private int m_semiColonsSeen;

            private int m_slashesSeen;

            public ProtocolTokenizer(string statement)
                : base(0, statement)
            {
            }

            internal ChannelType Channel { get; set; }

            internal bool IsSecure { get; set; }

            internal string Host { get; set; }

            internal string Port { get; set; }

            internal string Scheme { get; set; }

            protected override void OnReset()
            {
                CurrentState = ProtocolParserState.Start;
            }

            [StateHandler(State = ProtocolParserState.Error)]
            internal StateResult ProcessErrorState()
            {
                ResetAccumulator();
                Errors.Add(Error.NewError("T006", FormatError("An invalid token was detected."),
                    "Correct your protocol syntax and resubmit."));
                return StateResult.Terminal;
            }

            [StateHandler(State = ProtocolParserState.Start)]
            internal StateResult ProcessStartState()
            {
                if (char.IsWhiteSpace(GetCurrentToken())) return StateResult.Continue;
                if (char.IsLetter(GetCurrentToken()))
                {
                    CurrentState = ProtocolParserState.Scheme;
                    return StateResult.Restart;
                }

                CurrentState = ProtocolParserState.Error;
                return StateResult.Restart;
            }

            [StateHandler(State = ProtocolParserState.Host)]
            internal StateResult ProcessHostState()
            {
                var currentToken = GetCurrentToken();
                if (char.IsWhiteSpace(currentToken)) return StateResult.Continue;
                if (char.IsLetterOrDigit(currentToken) || currentToken == '.')
                {
                    AppendToAccumulator();
                    return StateResult.Continue;
                }

                if (':' == currentToken)
                {
                    Host = Accumulator.ToString();
                    AddTokenAndReset(TokenType.StringLiteral, false);
                    CurrentState = ProtocolParserState.Whitespace;
                    return StateResult.Restart;
                }

                CurrentState = ProtocolParserState.Error;
                return StateResult.Restart;
            }

            [StateHandler(State = ProtocolParserState.Scheme)]
            internal StateResult ProcessProtocolState()
            {
                var currentToken = GetCurrentToken();
                if (char.IsLetter(currentToken))
                {
                    if (Accumulator.Length == 0 && ('s' == currentToken || 'S' == currentToken)) IsSecure = true;
                    AppendToAccumulator();
                    return StateResult.Continue;
                }

                if ('-' != currentToken)
                {
                    if (':' == currentToken || char.IsWhiteSpace(currentToken))
                    {
                        Scheme = Accumulator.ToString();
                        if (ValidateProtocol())
                        {
                            AddTokenAndReset(TokenType.StringLiteral, false);
                            CurrentState = ProtocolParserState.Whitespace;
                            return StateResult.Restart;
                        }
                    }

                    CurrentState = ProtocolParserState.Error;
                    return StateResult.Restart;
                }

                MoveToNextToken();
                Scheme = Accumulator.ToString();
                if (ValidateProtocol())
                {
                    AddTokenAndReset(TokenType.StringLiteral, false);
                    CurrentState = ProtocolParserState.Channel;
                    return StateResult.Restart;
                }

                CurrentState = ProtocolParserState.Error;
                return StateResult.Restart;
            }

            [StateHandler(State = ProtocolParserState.Port)]
            internal StateResult ProcessPortState()
            {
                var currentToken = GetCurrentToken();
                if (char.IsWhiteSpace(currentToken))
                {
                    if (Accumulator.Length > 0)
                    {
                        Port = Accumulator.ToString();
                        AddTokenAndReset(TokenType.StringLiteral, false);
                        return StateResult.Terminal;
                    }

                    return StateResult.Continue;
                }

                if (!char.IsLetterOrDigit(currentToken))
                {
                    CurrentState = ProtocolParserState.Error;
                    return StateResult.Restart;
                }

                AppendToAccumulator();
                if (PeekNextToken() == null)
                {
                    Port = Accumulator.ToString();
                    AddTokenAndReset(TokenType.StringLiteral, false);
                    MoveToNextToken();
                    return StateResult.Terminal;
                }

                return StateResult.Continue;
            }

            [StateHandler(State = ProtocolParserState.Channel)]
            internal StateResult ProcessChannelType()
            {
                var currentToken = GetCurrentToken();
                if (char.IsWhiteSpace(currentToken) || ':' == currentToken)
                {
                    DecodeChannel();
                    AddTokenAndReset(TokenType.StringLiteral, false);
                    CurrentState = ProtocolParserState.Whitespace;
                    return StateResult.Restart;
                }

                if (char.IsLetter(currentToken))
                {
                    AppendToAccumulator();
                    return StateResult.Continue;
                }

                CurrentState = ProtocolParserState.Error;
                return StateResult.Restart;
            }

            [StateHandler(State = ProtocolParserState.Whitespace)]
            internal StateResult ProcessWhitespaceState()
            {
                var currentToken = GetCurrentToken();
                if (char.IsWhiteSpace(currentToken)) return StateResult.Continue;
                if (currentToken == ':')
                {
                    m_semiColonsSeen++;
                    return StateResult.Continue;
                }

                if (currentToken == '/')
                {
                    m_slashesSeen++;
                    return StateResult.Continue;
                }

                if (char.IsLetterOrDigit(currentToken))
                {
                    if (m_slashesSeen == 0 && m_semiColonsSeen == 0)
                    {
                        CurrentState = ProtocolParserState.Scheme;
                        return StateResult.Restart;
                    }

                    if (m_semiColonsSeen == 1 && m_slashesSeen == 2)
                    {
                        CurrentState = ProtocolParserState.Host;
                        return StateResult.Restart;
                    }

                    if (m_semiColonsSeen == 2 && m_slashesSeen == 2)
                    {
                        CurrentState = ProtocolParserState.Port;
                        return StateResult.Restart;
                    }
                }

                CurrentState = ProtocolParserState.Error;
                return StateResult.Restart;
            }

            private bool ValidateProtocol()
            {
                return Accumulator.ToString().Equals("rcp", StringComparison.CurrentCultureIgnoreCase) ||
                       Accumulator.ToString().Equals("srcp", StringComparison.CurrentCultureIgnoreCase);
            }

            private bool ChannelIsValid(char protocol)
            {
                if ('p' == protocol || 'P' == protocol)
                {
                    Channel = ChannelType.NamedPipe;
                    return true;
                }

                return false;
            }

            private bool DecodeChannel()
            {
                if (Accumulator.ToString().Equals("p", StringComparison.CurrentCultureIgnoreCase))
                {
                    Channel = ChannelType.NamedPipe;
                    return true;
                }

                Channel = ChannelType.Unknown;
                return false;
            }
        }
    }
}