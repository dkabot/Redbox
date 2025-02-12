using System;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Core;

namespace Redbox.IPC.Framework
{
    public class IPCProtocol : IIpcProtocol
    {
        public static IPCProtocol Parse(string protocolURI)
        {
            IPCProtocol ipcprotocol = new IPCProtocol(protocolURI)
            {
                Host = string.Empty,
                Port = string.Empty,
                IsSecure = false,
                Channel = ChannelType.Unknown
            };
            IPCProtocol.ProtocolTokenizer protocolTokenizer = new IPCProtocol.ProtocolTokenizer(protocolURI);
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
                foreach (Error error in protocolTokenizer.Errors)
                {
                    LogHelper.Instance.Log(error.Description, LogEntryType.Error);
                }
                throw new UriFormatException("URI is malformed: see log for details.");
            }
            ipcprotocol.Host = protocolTokenizer.Host;
            ipcprotocol.Port = protocolTokenizer.Port;
            ipcprotocol.IsSecure = protocolTokenizer.IsSecure;
            ipcprotocol.Channel = protocolTokenizer.Channel;
            ipcprotocol.Scheme = protocolTokenizer.Scheme;
            if (string.IsNullOrEmpty(ipcprotocol.Host) || string.IsNullOrEmpty(ipcprotocol.Port))
            {
                throw new UriFormatException("Host or port isn't set.");
            }
            if (ipcprotocol.Channel == ChannelType.Unknown)
            {
                throw new UriFormatException("The channel type is unknown; please correct your URI.");
            }
            int num;
            if (ipcprotocol.Channel == ChannelType.Socket && !int.TryParse(ipcprotocol.Port, out num))
            {
                throw new UriFormatException("Protocol is set up for sockets, but port isn't a valid address.");
            }
            return ipcprotocol;
        }

        public string GetPipeName()
        {
            if (this.PipeName == null)
            {
                this.PipeName = string.Format("{0}{1}", this.Host, this.Port);
            }
            return this.PipeName;
        }

        public bool IsSecure { get; set; }

        public ChannelType Channel { get; set; }

        public string Host { get; set; }

        public string Port { get; set; }

        public string Scheme { get; set; }

        public override string ToString()
        {
            return this.RawUri;
        }

        IPCProtocol(string rawUri)
        {
            this.RawUri = rawUri;
        }

        string PipeName;

        readonly string RawUri;

        enum ProtocolParserState
        {
            Start = 1,
            Scheme,
            Host,
            Port,
            Channel,
            Whitespace,
            Error
        }

        class ProtocolTokenizer : Tokenizer<IPCProtocol.ProtocolParserState>
        {
            public ProtocolTokenizer(string statement)
                : base(0, statement)
            {
            }

            protected override void OnReset()
            {
                base.CurrentState = IPCProtocol.ProtocolParserState.Start;
            }

            internal ChannelType Channel { get; set; }

            internal bool IsSecure { get; set; }

            internal string Host { get; set; }

            internal string Port { get; set; }

            internal string Scheme { get; set; }

            [StateHandler(State = IPCProtocol.ProtocolParserState.Error)]
            internal StateResult ProcessErrorState()
            {
                base.ResetAccumulator();
                base.Errors.Add(Error.NewError("T006", base.FormatError("An invalid token was detected."), "Correct your protocol syntax and resubmit."));
                return StateResult.Terminal;
            }

            [StateHandler(State = IPCProtocol.ProtocolParserState.Start)]
            internal StateResult ProcessStartState()
            {
                if (char.IsWhiteSpace(base.GetCurrentToken()))
                {
                    return StateResult.Continue;
                }
                if (char.IsLetter(base.GetCurrentToken()))
                {
                    base.CurrentState = IPCProtocol.ProtocolParserState.Scheme;
                    return StateResult.Restart;
                }
                base.CurrentState = IPCProtocol.ProtocolParserState.Error;
                return StateResult.Restart;
            }

            [StateHandler(State = IPCProtocol.ProtocolParserState.Host)]
            internal StateResult ProcessHostState()
            {
                char currentToken = base.GetCurrentToken();
                if (char.IsWhiteSpace(currentToken))
                {
                    return StateResult.Continue;
                }
                if (char.IsLetterOrDigit(currentToken) || currentToken == '.')
                {
                    base.AppendToAccumulator();
                    return StateResult.Continue;
                }
                if (':' == currentToken)
                {
                    this.Host = base.Accumulator.ToString();
                    base.AddTokenAndReset(TokenType.StringLiteral, false);
                    base.CurrentState = IPCProtocol.ProtocolParserState.Whitespace;
                    return StateResult.Restart;
                }
                base.CurrentState = IPCProtocol.ProtocolParserState.Error;
                return StateResult.Restart;
            }

            [StateHandler(State = IPCProtocol.ProtocolParserState.Scheme)]
            internal StateResult ProcessProtocolState()
            {
                char currentToken = base.GetCurrentToken();
                if (char.IsLetter(currentToken))
                {
                    if (base.Accumulator.Length == 0 && ('s' == currentToken || 'S' == currentToken))
                    {
                        this.IsSecure = true;
                    }
                    base.AppendToAccumulator();
                    return StateResult.Continue;
                }
                if ('-' != currentToken)
                {
                    if (':' == currentToken || char.IsWhiteSpace(currentToken))
                    {
                        this.Scheme = base.Accumulator.ToString();
                        if (this.ValidateProtocol())
                        {
                            base.AddTokenAndReset(TokenType.StringLiteral, false);
                            base.CurrentState = IPCProtocol.ProtocolParserState.Whitespace;
                            return StateResult.Restart;
                        }
                    }
                    base.CurrentState = IPCProtocol.ProtocolParserState.Error;
                    return StateResult.Restart;
                }
                base.MoveToNextToken();
                this.Scheme = base.Accumulator.ToString();
                if (this.ValidateProtocol())
                {
                    base.AddTokenAndReset(TokenType.StringLiteral, false);
                    base.CurrentState = IPCProtocol.ProtocolParserState.Channel;
                    return StateResult.Restart;
                }
                base.CurrentState = IPCProtocol.ProtocolParserState.Error;
                return StateResult.Restart;
            }

            [StateHandler(State = IPCProtocol.ProtocolParserState.Port)]
            internal StateResult ProcessPortState()
            {
                char currentToken = base.GetCurrentToken();
                if (char.IsWhiteSpace(currentToken))
                {
                    if (base.Accumulator.Length > 0)
                    {
                        this.Port = base.Accumulator.ToString();
                        base.AddTokenAndReset(TokenType.StringLiteral, false);
                        return StateResult.Terminal;
                    }
                    return StateResult.Continue;
                }
                else
                {
                    if (!char.IsLetterOrDigit(currentToken))
                    {
                        base.CurrentState = IPCProtocol.ProtocolParserState.Error;
                        return StateResult.Restart;
                    }
                    base.AppendToAccumulator();
                    if (base.PeekNextToken() == null)
                    {
                        this.Port = base.Accumulator.ToString();
                        base.AddTokenAndReset(TokenType.StringLiteral, false);
                        base.MoveToNextToken();
                        return StateResult.Terminal;
                    }
                    return StateResult.Continue;
                }
            }

            [StateHandler(State = IPCProtocol.ProtocolParserState.Channel)]
            internal StateResult ProcessChannelType()
            {
                char currentToken = base.GetCurrentToken();
                if (char.IsWhiteSpace(currentToken) || ':' == currentToken)
                {
                    this.DecodeChannel();
                    base.AddTokenAndReset(TokenType.StringLiteral, false);
                    base.CurrentState = IPCProtocol.ProtocolParserState.Whitespace;
                    return StateResult.Restart;
                }
                if (char.IsLetter(currentToken))
                {
                    base.AppendToAccumulator();
                    return StateResult.Continue;
                }
                base.CurrentState = IPCProtocol.ProtocolParserState.Error;
                return StateResult.Restart;
            }

            [StateHandler(State = IPCProtocol.ProtocolParserState.Whitespace)]
            internal StateResult ProcessWhitespaceState()
            {
                char currentToken = base.GetCurrentToken();
                if (char.IsWhiteSpace(currentToken))
                {
                    return StateResult.Continue;
                }
                if (currentToken == ':')
                {
                    this.m_semiColonsSeen++;
                    return StateResult.Continue;
                }
                if (currentToken == '/')
                {
                    this.m_slashesSeen++;
                    return StateResult.Continue;
                }
                if (char.IsLetterOrDigit(currentToken))
                {
                    if (this.m_slashesSeen == 0 && this.m_semiColonsSeen == 0)
                    {
                        base.CurrentState = IPCProtocol.ProtocolParserState.Scheme;
                        return StateResult.Restart;
                    }
                    if (this.m_semiColonsSeen == 1 && this.m_slashesSeen == 2)
                    {
                        base.CurrentState = IPCProtocol.ProtocolParserState.Host;
                        return StateResult.Restart;
                    }
                    if (this.m_semiColonsSeen == 2 && this.m_slashesSeen == 2)
                    {
                        base.CurrentState = IPCProtocol.ProtocolParserState.Port;
                        return StateResult.Restart;
                    }
                }
                base.CurrentState = IPCProtocol.ProtocolParserState.Error;
                return StateResult.Restart;
            }

            bool ValidateProtocol()
            {
                return base.Accumulator.ToString().Equals("rcp", StringComparison.CurrentCultureIgnoreCase) || base.Accumulator.ToString().Equals("srcp", StringComparison.CurrentCultureIgnoreCase);
            }

            bool ChannelIsValid(char protocol)
            {
                if ('p' == protocol || 'P' == protocol)
                {
                    this.Channel = ChannelType.NamedPipe;
                    return true;
                }
                return false;
            }

            bool DecodeChannel()
            {
                if (base.Accumulator.ToString().Equals("p", StringComparison.CurrentCultureIgnoreCase))
                {
                    this.Channel = ChannelType.NamedPipe;
                    return true;
                }
                this.Channel = ChannelType.Unknown;
                return false;
            }

            int m_slashesSeen;

            int m_semiColonsSeen;
        }
    }
}
