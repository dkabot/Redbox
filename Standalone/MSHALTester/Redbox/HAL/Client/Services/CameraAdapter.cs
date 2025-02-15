using System;
using System.Text;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Client.Services;

public sealed class CameraAdapter
{
    private readonly byte[] CameraSnapImmediate;
    private readonly HardwareService Service;
    private CameraState State;

    public CameraAdapter(HardwareService service)
    {
        Service = service;
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine("CLEAR");
        stringBuilder.AppendLine(" RINGLIGHT ON");
        stringBuilder.AppendLine(" CAMERA SNAP");
        stringBuilder.AppendLine(" RINGLIGHT OFF");
        CameraSnapImmediate = Encoding.ASCII.GetBytes(stringBuilder.ToString());
        State = CameraState.Unknown;
    }

    public bool LegacyCamera { get; private set; }

    public bool CameraInError()
    {
        var s = RunInstruction("CAMERA RETURNERRORCOUNT");
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
        if (State == CameraState.Unknown)
            return State;
        var str = RunInstruction(CameraState.Started == State ? "CAMERA STOP FORCE=TRUE" : "CAMERA START");
        if (string.IsNullOrEmpty(str))
            State = CameraState.Unknown;
        else if ("TRUE".Equals(str, StringComparison.CurrentCultureIgnoreCase))
            State = CameraState.Started == State ? CameraState.Stopped : CameraState.Started;
        return State;
    }

    public CameraState GetCameraStatus()
    {
        if (State == CameraState.Unknown)
        {
            var str = RunInstruction("CAMERA STATUS");
            if (!string.IsNullOrEmpty(str))
                State = str == "RUNNING" ? CameraState.Started : CameraState.Stopped;
        }

        return State;
    }

    public void Reset(bool legacy)
    {
        LegacyCamera = legacy;
        State = CameraState.Unknown;
    }

    public ISnapResult Snap()
    {
        HardwareJob job;
        if (!Service.ExecuteImmediateProgram(CameraSnapImmediate, out job).Success)
            return null;
        var snapResult = new _SnapResult();
        if ("SUCCESS" == job.GetTopOfStack())
        {
            var stackEntries = job.GetStackEntries(2);
            snapResult.SnapOk = true;
            snapResult.Path = stackEntries[1];
        }

        return snapResult;
    }

    public bool ResetReturnCounter()
    {
        return Service.ExecuteImmediate("CAMERA RESETRETURNCOUNTER", out var _).Success;
    }

    private string RunInstruction(string instruction)
    {
        using (var instructionHelper = new InstructionHelper(Service))
        {
            return instructionHelper.ExecuteGeneric(instruction, 120000);
        }
    }

    private class _SnapResult : ISnapResult, IDisposable
    {
        internal _SnapResult()
        {
            SnapOk = false;
            Path = string.Empty;
        }

        public void Dispose()
        {
        }

        public bool SnapOk { get; internal set; }

        public string Path { get; internal set; }
    }
}