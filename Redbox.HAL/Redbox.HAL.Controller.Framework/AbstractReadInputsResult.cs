using System;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework
{
    internal abstract class AbstractReadInputsResult<T> : IReadInputsResult<T>
    {
        protected readonly InputState[] Inputs;

        protected AbstractReadInputsResult(CoreResponse response)
        {
            RawResponse = response;
            if (!RawResponse.Success)
            {
                Error = ErrorCodes.CommunicationError;
            }
            else
            {
                Inputs = new InputState[20];
                Error = ErrorCodes.Success;
                if (ControllerConfiguration.Instance.ValidateInputsReadResponse)
                    FromValidated();
                else
                    From();
            }
        }

        protected abstract string LogHeader { get; }

        internal CoreResponse RawResponse { get; }

        public ErrorCodes Error { get; protected set; }

        public bool Success => Error == ErrorCodes.Success;

        public int InputCount => Inputs.Length;

        public void Log()
        {
            Log(LogEntryType.Info);
        }

        public void Log(LogEntryType type)
        {
            if (!Success)
                LogHelper.Instance.Log(RawResponse.Diagnostic);
            else
                LogHelper.Instance.Log(type, "{0} {1}", LogHeader, RawResponse.OpCodeResponse);
        }

        public bool IsInputActive(T input)
        {
            return IsInState(input, InputState.Active);
        }

        public bool IsInState(T input, InputState state)
        {
            if (!Success)
                throw new InvalidOperationException("Sensor read state is invalid");
            return OnGetInputState(input) == state;
        }

        public void Foreach(Action<T> action)
        {
            OnForeachInput(action);
        }

        protected abstract InputState OnGetInputState(T input);

        protected abstract void OnForeachInput(Action<T> a);

        protected InputState GetInputState(int input)
        {
            if (input < 0 || input >= Inputs.Length)
                throw new InvalidOperationException("Specified input exceeds bounds");
            return Inputs[input];
        }

        private void From()
        {
            try
            {
                var num = 0;
                for (var index = 0; index < RawResponse.OpCodeResponse.Length; ++index)
                    if (char.IsDigit(RawResponse.OpCodeResponse[index]))
                        Inputs[num++] = RawResponse.OpCodeResponse[index] == '1'
                            ? InputState.Active
                            : InputState.Inactive;
                if (20 == num)
                    return;
                OnBogusResponse();
            }
            catch (Exception ex)
            {
                OnResponseException(ex);
            }
        }

        private void FromValidated()
        {
            var num1 = RawResponse.OpCodeResponse.IndexOf("R");
            if (21 != num1 || !char.IsWhiteSpace(RawResponse.OpCodeResponse[16]))
            {
                OnBogusResponse();
            }
            else
            {
                var num2 = 0;
                try
                {
                    for (var index = 0; index < num1; ++index)
                        switch (RawResponse.OpCodeResponse[index])
                        {
                            case '0':
                                Inputs[num2++] = InputState.Inactive;
                                break;
                            case '1':
                                Inputs[num2++] = InputState.Active;
                                break;
                        }
                }
                catch (Exception ex)
                {
                    OnResponseException(ex);
                    num2 = 0;
                }

                if (num2 == 20)
                    return;
                OnBogusResponse();
            }
        }

        private void OnResponseException(Exception e)
        {
            LogHelper.Instance.Log("[ReadInputs] Unhandled exception parsing response '{0}'",
                RawResponse.OpCodeResponse);
            LogHelper.Instance.Log(e.Message);
            Error = ErrorCodes.CommunicationError;
        }

        private void OnBogusResponse()
        {
            LogHelper.Instance.WithContext(LogEntryType.Error, "[ReadInputs] Unexpected response from port {0}",
                RawResponse.OpCodeResponse);
            Error = ErrorCodes.CommunicationError;
        }
    }
}