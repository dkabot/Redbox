using Redbox.HAL.Component.Model;
using System;
using System.Text;

namespace Redbox.HAL.Client.Services
{
  public sealed class CameraAdapter
  {
    private CameraState State;
    private readonly byte[] CameraSnapImmediate;
    private readonly HardwareService Service;

    public bool LegacyCamera { get; private set; }

    public bool CameraInError()
    {
      string s = this.RunInstruction("CAMERA RETURNERRORCOUNT");
      if (string.IsNullOrEmpty(s))
        return false;
      int num;
      try
      {
        num = int.Parse(s);
      }
      catch
      {
        num = 0;
      }
      return num > 0;
    }

    public CameraState ToggleState()
    {
      if (this.State == CameraState.Unknown)
        return this.State;
      string str = this.RunInstruction(CameraState.Started == this.State ? "CAMERA STOP FORCE=TRUE" : "CAMERA START");
      if (string.IsNullOrEmpty(str))
        this.State = CameraState.Unknown;
      else if ("TRUE".Equals(str, StringComparison.CurrentCultureIgnoreCase))
        this.State = CameraState.Started == this.State ? CameraState.Stopped : CameraState.Started;
      return this.State;
    }

    public CameraState GetCameraStatus()
    {
      if (this.State == CameraState.Unknown)
      {
        string str = this.RunInstruction("CAMERA STATUS");
        if (!string.IsNullOrEmpty(str))
          this.State = str == "RUNNING" ? CameraState.Started : CameraState.Stopped;
      }
      return this.State;
    }

    public void Reset(bool legacy)
    {
      this.LegacyCamera = legacy;
      this.State = CameraState.Unknown;
    }

    public ISnapResult Snap()
    {
      HardwareJob job;
      if (!this.Service.ExecuteImmediateProgram(this.CameraSnapImmediate, out job).Success)
        return (ISnapResult) null;
      CameraAdapter._SnapResult snapResult = new CameraAdapter._SnapResult();
      if ("SUCCESS" == job.GetTopOfStack())
      {
        string[] stackEntries = job.GetStackEntries(2);
        snapResult.SnapOk = true;
        snapResult.Path = stackEntries[1];
      }
      return (ISnapResult) snapResult;
    }

    public bool ResetReturnCounter()
    {
      return this.Service.ExecuteImmediate("CAMERA RESETRETURNCOUNTER", out HardwareJob _).Success;
    }

    public CameraAdapter(HardwareService service)
    {
      this.Service = service;
      StringBuilder stringBuilder = new StringBuilder();
      stringBuilder.AppendLine("CLEAR");
      stringBuilder.AppendLine(" RINGLIGHT ON");
      stringBuilder.AppendLine(" CAMERA SNAP");
      stringBuilder.AppendLine(" RINGLIGHT OFF");
      this.CameraSnapImmediate = Encoding.ASCII.GetBytes(stringBuilder.ToString());
      this.State = CameraState.Unknown;
    }

    private string RunInstruction(string instruction)
    {
      using (InstructionHelper instructionHelper = new InstructionHelper(this.Service))
        return instructionHelper.ExecuteGeneric(instruction, 120000);
    }

    private class _SnapResult : ISnapResult, IDisposable
    {
      public void Dispose()
      {
      }

      public bool SnapOk { get; internal set; }

      public string Path { get; internal set; }

      internal _SnapResult()
      {
        this.SnapOk = false;
        this.Path = string.Empty;
      }
    }
  }
}
