using System;
using System.Collections.Generic;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Extensions;

namespace Redbox.HAL.Client.Services;

internal abstract class AbstractClientReadInputsResult<T> : IReadInputsResult<T>
{
    private static readonly string[] ErrorInputs = new string[0];
    protected readonly string[] Inputs;

    protected AbstractClientReadInputsResult(HardwareService service, string instruction)
    {
        HardwareJob job;
        if (!service.ExecuteImmediate(instruction, out job).Success)
        {
            Error = ErrorCodes.ServiceChannelError;
        }
        else
        {
            Stack<string> stack;
            if (!job.GetStack(out stack).Success)
            {
                Error = ErrorCodes.ServiceChannelError;
            }
            else
            {
                var ignoringCase = Enum<ErrorCodes>.ParseIgnoringCase(stack.Pop(), ErrorCodes.CommunicationError);
                LogHelper.Instance.Log("[ClientReadInputsResult] Error = {0}", ignoringCase);
                if (ignoringCase != ErrorCodes.Success)
                {
                    Error = ignoringCase;
                }
                else
                {
                    var int32 = Convert.ToInt32(stack.Pop());
                    LogHelper.Instance.Log("[ClientReadInputsResult] Stack count = {0}", int32);
                    Error = ErrorCodes.Success;
                    Inputs = new string[int32];
                    for (var index = int32 - 1; index >= 0; --index)
                        Inputs[index] = stack.Pop();
                }
            }
        }
    }

    protected abstract string LogHeader { get; }

    public ErrorCodes Error { get; }

    public bool Success => Error == ErrorCodes.Success;

    public int InputCount => Inputs.Length;

    public void Log()
    {
        Log(LogEntryType.Info);
    }

    public void Log(LogEntryType type)
    {
        LogHelper.Instance.Log("--{0} inputs dump Error = {1} icount = {2}--", LogHeader, Error, Inputs.Length);
        var input = 0;
        Array.ForEach(Inputs, each =>
        {
            LogHelper.Instance.Log(" input {0} = {1}", input, Inputs[input]);
            ++input;
        });
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
}