using System;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework
{
    internal sealed class CoreResponse : IControlResponse
    {
        private static CoreResponse m_timeoutResponse = null;
        private static CoreResponse m_commErrorResponse = null;
        private static readonly char[] TrimChars = Environment.NewLine.ToCharArray();
        internal readonly AddressSelector Selector;
        private string m_commandResponse = string.Empty;
        private ErrorCodes m_errorCode;

        internal CoreResponse(ErrorCodes error, string diagnostic)
        {
            m_errorCode = error;
            Diagnostic = diagnostic;
        }

        internal CoreResponse(AddressSelector selector)
        {
            Selector = selector;
        }

        internal static CoreResponse CommErrorResponse
        {
            get
            {
                if (m_commErrorResponse == null)
                    m_commErrorResponse = new CoreResponse(AddressSelector.H101)
                    {
                        Error = ErrorCodes.CommunicationError
                    };
                return m_commErrorResponse;
            }
        }

        internal static CoreResponse TimedOutResponse
        {
            get
            {
                if (m_timeoutResponse == null)
                    m_timeoutResponse = new CoreResponse(AddressSelector.H101)
                    {
                        Error = ErrorCodes.Timeout
                    };
                return m_timeoutResponse;
            }
        }

        internal ErrorCodes Error
        {
            get => m_errorCode;
            set
            {
                m_errorCode = value;
                if (ErrorCodes.CommunicationError != m_errorCode)
                    return;
                OpCodeResponse = string.Empty;
            }
        }

        internal string OpCodeResponse
        {
            get => m_commandResponse;
            set
            {
                if (string.IsNullOrEmpty(value))
                    m_commandResponse = string.Empty;
                else
                    m_commandResponse = value.TrimEnd(TrimChars).TrimStart(TrimChars);
            }
        }

        public bool Success => Error == ErrorCodes.Success;

        public bool TimedOut => ErrorCodes.Timeout == Error;

        public bool CommError => ErrorCodes.CommunicationError == Error;

        public string Diagnostic { get; internal set; }

        public override string ToString()
        {
            return !CommError ? Error.ToString().ToUpper() : Diagnostic;
        }

        internal bool IsBitSet(int bit)
        {
            if (m_errorCode != ErrorCodes.Success)
                return false;
            if (OpCodeResponse.Length >= bit + 1)
                return OpCodeResponse[bit] == '1';
            LogHelper.Instance.Log("IsBitSet: opcode response is {0}; this is insufficient for bit test.",
                OpCodeResponse);
            return false;
        }
    }
}