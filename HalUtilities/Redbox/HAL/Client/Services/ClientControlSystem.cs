using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Redbox.HAL.Client.Services
{
  public sealed class ClientControlSystem : IControlSystem
  {
    private readonly HardwareService Service;
    private readonly byte[] ReadImmediateInstruction;

    public IControlResponse Initialize() => this.From("SERIALBOARD RESET");

    public bool Shutdown()
    {
      using (InstructionHelper instructionHelper = new InstructionHelper(this.Service))
        return instructionHelper.ExecuteGeneric("SERIALBOARD CLOSEPORT").Equals("SUCCESS", StringComparison.CurrentCultureIgnoreCase);
    }

    public IControlResponse SetAudio(AudioChannelState newState)
    {
      return this.From(string.Format("AUDIO {0}", (object) newState.ToString()));
    }

    public IControlResponse ToggleRingLight(bool on, int? sleepAfter)
    {
      return this.From(string.Format("CONTROLSYSTEM RINGLIGHT{0}", on ? (object) "ON" : (object) "OFF"));
    }

    public IControlResponse VendDoorRent() => this.From("VENDDOOR RENT");

    public IControlResponse VendDoorClose() => this.From("VENDDOOR CLOSE");

    public VendDoorState ReadVendDoorPosition()
    {
      using (InstructionHelper instructionHelper = new InstructionHelper(this.Service))
      {
        string str = instructionHelper.ExecuteGeneric("VENDDOOR STATUS");
        return string.IsNullOrEmpty(str) ? VendDoorState.Unknown : Enum<VendDoorState>.ParseIgnoringCase(str, VendDoorState.Unknown);
      }
    }

    public IControlResponse TrackOpen() => this.From("TRACK OPEN");

    public IControlResponse TrackClose() => this.From("TRACK CLOSE");

    public ErrorCodes TrackCycle()
    {
      using (InstructionHelper instructionHelper = new InstructionHelper(this.Service))
        return instructionHelper.ExecuteErrorCode("TRACK CYCLE");
    }

    public void TimedExtend()
    {
    }

    public void TimedExtend(int timeout)
    {
    }

    public IControlResponse ExtendArm() => this.From("GRIPPER EXTEND");

    public IControlResponse ExtendArm(int timeout) => throw new NotImplementedException();

    public IControlResponse RetractArm() => this.From("GRIPPER RETRACT");

    public IControlResponse SetFinger(GripperFingerState state)
    {
      return this.From(string.Format("GRIPPER {0}", GripperFingerState.Closed == state ? (object) "CLOSE" : (object) state.ToString()));
    }

    public ErrorCodes Center(CenterDiskMethod method)
    {
      using (InstructionHelper instructionHelper = new InstructionHelper(this.Service))
        return instructionHelper.ExecuteErrorCode("CONTROLSYSTEM CENTER");
    }

    public IBoardVersionResponse GetBoardVersion(ControlBoards board)
    {
      throw new NotImplementedException();
    }

    public IControlSystemRevision GetRevision() => throw new NotImplementedException();

    public IReadInputsResult<PickerInputs> ReadPickerInputs()
    {
      return (IReadInputsResult<PickerInputs>) new ClientReadPickerInputsResult(this.Service);
    }

    public IReadInputsResult<AuxInputs> ReadAuxInputs()
    {
      return (IReadInputsResult<AuxInputs>) new ClientReadAuxInputsResult(this.Service);
    }

    public void LogPickerSensorState()
    {
    }

    public void LogPickerSensorState(LogEntryType type)
    {
    }

    public void LogInputs(ControlBoards board)
    {
    }

    public void LogInputs(ControlBoards board, LogEntryType type)
    {
    }

    public IControlResponse SetSensors(bool on)
    {
      return this.From(string.Format("CONTROLSYSTEM PICKERSENSORS{0}", on ? (object) "ON" : (object) "OFF"));
    }

    public IPickerSensorReadResult ReadPickerSensors()
    {
      throw new NotImplementedException(nameof (ReadPickerSensors));
    }

    public IPickerSensorReadResult ReadPickerSensors(bool f)
    {
      HardwareJob job;
      if (!this.Service.ExecuteImmediate("SENSOR READ PICKER-SENSOR=1..6", out job).Success)
        return (IPickerSensorReadResult) new ClientPickerSensorReadResult(ErrorCodes.ServiceChannelError);
      Stack<string> stack;
      if (!job.GetStack(out stack).Success)
        return (IPickerSensorReadResult) new ClientPickerSensorReadResult(ErrorCodes.ServiceChannelError);
      if (stack.Count == 0)
        return (IPickerSensorReadResult) new ClientPickerSensorReadResult(ErrorCodes.CommunicationError);
      return int.Parse(stack.Pop()) >= 6 ? (IPickerSensorReadResult) new ClientPickerSensorReadResult(stack) : (IPickerSensorReadResult) new ClientPickerSensorReadResult(ErrorCodes.CommunicationError);
    }

    public IControlResponse StartRollerIn() => this.SetRollerState(RollerState.In);

    public IControlResponse StartRollerOut() => this.SetRollerState(RollerState.Out);

    public IControlResponse StopRoller() => this.SetRollerState(RollerState.Stop);

    public IControlResponse SetRollerState(RollerState s)
    {
      return this.From(string.Format("ROLLER {0}", (object) s.ToString().ToUpper()));
    }

    public IControlResponse RollerToPosition(RollerPosition position)
    {
      return this.RollerToPosition(position, 5000);
    }

    public IControlResponse RollerToPosition(RollerPosition position, int opTimeout)
    {
      return this.RollerToPosition(position, opTimeout, false);
    }

    public IControlResponse RollerToPosition(
      RollerPosition position,
      int opTimeout,
      bool logSensors)
    {
      return this.From(string.Format("ROLLER POS={0} TIMEOUT={1} WAIT=TRUE", (object) ((int) position).ToString(), (object) opTimeout));
    }

    public QlmStatus GetQlmStatus() => throw new NotImplementedException();

    public ErrorCodes EngageQlm(IFormattedLog log) => this.EngageQlm(true, log);

    public ErrorCodes EngageQlm(bool home, IFormattedLog log)
    {
      using (InstructionHelper instructionHelper = new InstructionHelper(this.Service))
        return instructionHelper.ExecuteErrorCode("QLM ENGAGE");
    }

    public ErrorCodes DisengageQlm(IFormattedLog log) => this.DisengageQlm(true, log);

    public ErrorCodes DisengageQlm(bool home, IFormattedLog log)
    {
      using (InstructionHelper instructionHelper = new InstructionHelper(this.Service))
        return instructionHelper.ExecuteErrorCode("QLM DISENGAGE");
    }

    public IControlResponse LockQlmDoor() => throw new NotImplementedException();

    public IControlResponse UnlockQlmDoor() => throw new NotImplementedException();

    public IControlResponse DropQlm() => this.From("CONTROLSYSTEM DROPQLM");

    public IControlResponse LiftQlm() => this.From("CONTROLSYSTEM LIFTQLM");

    public IControlResponse HaltQlmLifter() => this.From("CONTROLSYSTEM HALTQLMOPERATION");

    public VendDoorState VendDoorState => throw new NotImplementedException();

    public TrackState TrackState => throw new NotImplementedException();

    public ClientControlSystem(HardwareService s)
    {
      this.Service = s;
      StringBuilder stringBuilder = new StringBuilder();
      stringBuilder.AppendLine("CLEAR");
      stringBuilder.AppendLine(" SENSOR READ PICKER-SENSOR=1..6");
      this.ReadImmediateInstruction = Encoding.ASCII.GetBytes(stringBuilder.ToString());
    }

    private IControlResponse From(string instruction)
    {
      using (InstructionHelper instructionHelper = new InstructionHelper(this.Service))
        return instructionHelper.ExecuteWithResponse(instruction);
    }
  }
}
