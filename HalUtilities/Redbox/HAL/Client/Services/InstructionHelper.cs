using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Extensions;
using System;

namespace Redbox.HAL.Client.Services
{
  public struct InstructionHelper : IDisposable
  {
    private const int DefaultTimeout = 120000;
    private readonly HardwareService Service;

    public void Dispose()
    {
    }

    public InstructionHelper(HardwareService s)
      : this()
    {
      this.Service = s;
    }

    public HardwareJob Execute(string instruction) => this.Execute(instruction, 120000);

    public HardwareJob Execute(string instruction, int timeout)
    {
      HardwareJob job = (HardwareJob) null;
      HardwareCommandResult result = this.Service.ExecuteImmediate(instruction, new int?(timeout), out job);
      result.Dump();
      return !result.Success ? (HardwareJob) null : job;
    }

    public ErrorCodes ExecuteErrorCode(string instruction)
    {
      return this.ExecuteErrorCode(instruction, 120000);
    }

    public ErrorCodes ExecuteErrorCode(string instruction, int timeout)
    {
      string str = this.ExecuteGeneric(instruction, timeout);
      return string.IsNullOrEmpty(str) ? ErrorCodes.ServiceChannelError : Enum<ErrorCodes>.ParseIgnoringCase(str, ErrorCodes.ServiceChannelError);
    }

    public IControlResponse ExecuteWithResponse(string instruction)
    {
      return this.ExecuteWithResponse(instruction, 120000);
    }

    public IControlResponse ExecuteWithResponse(string instruction, int timeout)
    {
      return this.From(this.ExecuteGeneric(instruction, timeout));
    }

    public string ExecuteGeneric(string instruction) => this.ExecuteGeneric(instruction, 120000);

    public string ExecuteGeneric(string instruction, int timeout)
    {
      HardwareJob job = (HardwareJob) null;
      HardwareCommandResult result = this.Service.ExecuteImmediate(instruction, new int?(timeout), out job);
      result.Dump();
      return !result.Success ? string.Empty : job.GetTopOfStack();
    }

    private IControlResponse From(string serviceResponse)
    {
      InstructionHelper.ClientControlResponse clientControlResponse = new InstructionHelper.ClientControlResponse();
      if (string.IsNullOrEmpty(serviceResponse))
      {
        clientControlResponse.Diagnostic = ErrorCodes.ServiceChannelError.ToString().ToUpper();
        clientControlResponse.CommError = true;
      }
      else if (serviceResponse.Equals("SUCCESS", StringComparison.CurrentCultureIgnoreCase))
        clientControlResponse.Success = true;
      else if (serviceResponse.Equals("TIMEOUT", StringComparison.CurrentCultureIgnoreCase))
      {
        clientControlResponse.TimedOut = true;
      }
      else
      {
        clientControlResponse.CommError = clientControlResponse.TimedOut = true;
        clientControlResponse.Diagnostic = serviceResponse;
      }
      return (IControlResponse) clientControlResponse;
    }

    private class ClientControlResponse : IControlResponse
    {
      public bool Success { get; internal set; }

      public bool TimedOut { get; internal set; }

      public bool CommError { get; internal set; }

      public string Diagnostic { get; internal set; }

      internal ClientControlResponse() => this.Diagnostic = string.Empty;
    }
  }
}
