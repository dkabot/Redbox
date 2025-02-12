using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Extensions;
using System;
using System.Collections.Generic;

namespace Redbox.HAL.Client.Services
{
  internal abstract class AbstractClientReadInputsResult<T> : IReadInputsResult<T>
  {
    protected readonly string[] Inputs;
    private static readonly string[] ErrorInputs = new string[0];

    public ErrorCodes Error { get; private set; }

    public bool Success => this.Error == ErrorCodes.Success;

    public int InputCount => this.Inputs.Length;

    public void Log() => this.Log(LogEntryType.Info);

    public void Log(LogEntryType type)
    {
      LogHelper.Instance.Log("--{0} inputs dump Error = {1} icount = {2}--", (object) this.LogHeader, (object) this.Error, (object) this.Inputs.Length);
      int input = 0;
      Array.ForEach<string>(this.Inputs, (Action<string>) (each =>
      {
        LogHelper.Instance.Log(" input {0} = {1}", (object) input, (object) this.Inputs[input]);
        ++input;
      }));
    }

    public bool IsInputActive(T input) => this.IsInState(input, InputState.Active);

    public bool IsInState(T input, InputState state)
    {
      if (!this.Success)
        throw new InvalidOperationException("Sensor read state is invalid");
      return this.OnGetInputState(input) == state;
    }

    public void Foreach(Action<T> action) => this.OnForeachInput(action);

    protected abstract InputState OnGetInputState(T input);

    protected abstract void OnForeachInput(Action<T> a);

    protected abstract string LogHeader { get; }

    protected AbstractClientReadInputsResult(HardwareService service, string instruction)
    {
      HardwareJob job;
      if (!service.ExecuteImmediate(instruction, out job).Success)
      {
        this.Error = ErrorCodes.ServiceChannelError;
      }
      else
      {
        Stack<string> stack;
        if (!job.GetStack(out stack).Success)
        {
          this.Error = ErrorCodes.ServiceChannelError;
        }
        else
        {
          ErrorCodes ignoringCase = Enum<ErrorCodes>.ParseIgnoringCase(stack.Pop(), ErrorCodes.CommunicationError);
          LogHelper.Instance.Log("[ClientReadInputsResult] Error = {0}", (object) ignoringCase);
          if (ignoringCase != ErrorCodes.Success)
          {
            this.Error = ignoringCase;
          }
          else
          {
            int int32 = Convert.ToInt32(stack.Pop());
            LogHelper.Instance.Log("[ClientReadInputsResult] Stack count = {0}", (object) int32);
            this.Error = ErrorCodes.Success;
            this.Inputs = new string[int32];
            for (int index = int32 - 1; index >= 0; --index)
              this.Inputs[index] = stack.Pop();
          }
        }
      }
    }
  }
}
