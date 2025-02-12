using System;
using System.Collections.Generic;
using System.Text;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Extensions;

namespace Redbox.HAL.Client.Services
{
    public sealed class ClientControlSystem : IControlSystem
    {
        private readonly byte[] ReadImmediateInstruction;
        private readonly HardwareService Service;

        public ClientControlSystem(HardwareService s)
        {
            Service = s;
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("CLEAR");
            stringBuilder.AppendLine(" SENSOR READ PICKER-SENSOR=1..6");
            ReadImmediateInstruction = Encoding.ASCII.GetBytes(stringBuilder.ToString());
        }

        public IControlResponse Initialize()
        {
            return From("SERIALBOARD RESET");
        }

        public bool Shutdown()
        {
            using (var instructionHelper = new InstructionHelper(Service))
            {
                return instructionHelper.ExecuteGeneric("SERIALBOARD CLOSEPORT")
                    .Equals("SUCCESS", StringComparison.CurrentCultureIgnoreCase);
            }
        }

        public IControlResponse SetAudio(AudioChannelState newState)
        {
            return From(string.Format("AUDIO {0}", newState.ToString()));
        }

        public IControlResponse ToggleRingLight(bool on, int? sleepAfter)
        {
            return From(string.Format("CONTROLSYSTEM RINGLIGHT{0}", on ? "ON" : (object)"OFF"));
        }

        public IControlResponse VendDoorRent()
        {
            return From("VENDDOOR RENT");
        }

        public IControlResponse VendDoorClose()
        {
            return From("VENDDOOR CLOSE");
        }

        public VendDoorState ReadVendDoorPosition()
        {
            using (var instructionHelper = new InstructionHelper(Service))
            {
                var str = instructionHelper.ExecuteGeneric("VENDDOOR STATUS");
                return string.IsNullOrEmpty(str)
                    ? VendDoorState.Unknown
                    : Enum<VendDoorState>.ParseIgnoringCase(str, VendDoorState.Unknown);
            }
        }

        public IControlResponse TrackOpen()
        {
            return From("TRACK OPEN");
        }

        public IControlResponse TrackClose()
        {
            return From("TRACK CLOSE");
        }

        public ErrorCodes TrackCycle()
        {
            using (var instructionHelper = new InstructionHelper(Service))
            {
                return instructionHelper.ExecuteErrorCode("TRACK CYCLE");
            }
        }

        public void TimedExtend()
        {
        }

        public void TimedExtend(int timeout)
        {
        }

        public IControlResponse ExtendArm()
        {
            return From("GRIPPER EXTEND");
        }

        public IControlResponse ExtendArm(int timeout)
        {
            throw new NotImplementedException();
        }

        public IControlResponse RetractArm()
        {
            return From("GRIPPER RETRACT");
        }

        public IControlResponse SetFinger(GripperFingerState state)
        {
            return From(string.Format("GRIPPER {0}",
                GripperFingerState.Closed == state ? "CLOSE" : (object)state.ToString()));
        }

        public ErrorCodes Center(CenterDiskMethod method)
        {
            using (var instructionHelper = new InstructionHelper(Service))
            {
                return instructionHelper.ExecuteErrorCode("CONTROLSYSTEM CENTER");
            }
        }

        public IBoardVersionResponse GetBoardVersion(ControlBoards board)
        {
            throw new NotImplementedException();
        }

        public IControlSystemRevision GetRevision()
        {
            throw new NotImplementedException();
        }

        public IReadInputsResult<PickerInputs> ReadPickerInputs()
        {
            return new ClientReadPickerInputsResult(Service);
        }

        public IReadInputsResult<AuxInputs> ReadAuxInputs()
        {
            return new ClientReadAuxInputsResult(Service);
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
            return From(string.Format("CONTROLSYSTEM PICKERSENSORS{0}", on ? "ON" : (object)"OFF"));
        }

        public IPickerSensorReadResult ReadPickerSensors()
        {
            throw new NotImplementedException(nameof(ReadPickerSensors));
        }

        public IPickerSensorReadResult ReadPickerSensors(bool f)
        {
            HardwareJob job;
            if (!Service.ExecuteImmediate("SENSOR READ PICKER-SENSOR=1..6", out job).Success)
                return new ClientPickerSensorReadResult(ErrorCodes.ServiceChannelError);
            Stack<string> stack;
            if (!job.GetStack(out stack).Success)
                return new ClientPickerSensorReadResult(ErrorCodes.ServiceChannelError);
            if (stack.Count == 0)
                return new ClientPickerSensorReadResult(ErrorCodes.CommunicationError);
            return int.Parse(stack.Pop()) >= 6
                ? new ClientPickerSensorReadResult(stack)
                : (IPickerSensorReadResult)new ClientPickerSensorReadResult(ErrorCodes.CommunicationError);
        }

        public IControlResponse StartRollerIn()
        {
            return SetRollerState(RollerState.In);
        }

        public IControlResponse StartRollerOut()
        {
            return SetRollerState(RollerState.Out);
        }

        public IControlResponse StopRoller()
        {
            return SetRollerState(RollerState.Stop);
        }

        public IControlResponse SetRollerState(RollerState s)
        {
            return From(string.Format("ROLLER {0}", s.ToString().ToUpper()));
        }

        public IControlResponse RollerToPosition(RollerPosition position)
        {
            return RollerToPosition(position, 5000);
        }

        public IControlResponse RollerToPosition(RollerPosition position, int opTimeout)
        {
            return RollerToPosition(position, opTimeout, false);
        }

        public IControlResponse RollerToPosition(
            RollerPosition position,
            int opTimeout,
            bool logSensors)
        {
            return From(string.Format("ROLLER POS={0} TIMEOUT={1} WAIT=TRUE", ((int)position).ToString(), opTimeout));
        }

        public QlmStatus GetQlmStatus()
        {
            throw new NotImplementedException();
        }

        public ErrorCodes EngageQlm(IFormattedLog log)
        {
            return EngageQlm(true, log);
        }

        public ErrorCodes EngageQlm(bool home, IFormattedLog log)
        {
            using (var instructionHelper = new InstructionHelper(Service))
            {
                return instructionHelper.ExecuteErrorCode("QLM ENGAGE");
            }
        }

        public ErrorCodes DisengageQlm(IFormattedLog log)
        {
            return DisengageQlm(true, log);
        }

        public ErrorCodes DisengageQlm(bool home, IFormattedLog log)
        {
            using (var instructionHelper = new InstructionHelper(Service))
            {
                return instructionHelper.ExecuteErrorCode("QLM DISENGAGE");
            }
        }

        public IControlResponse LockQlmDoor()
        {
            throw new NotImplementedException();
        }

        public IControlResponse UnlockQlmDoor()
        {
            throw new NotImplementedException();
        }

        public IControlResponse DropQlm()
        {
            return From("CONTROLSYSTEM DROPQLM");
        }

        public IControlResponse LiftQlm()
        {
            return From("CONTROLSYSTEM LIFTQLM");
        }

        public IControlResponse HaltQlmLifter()
        {
            return From("CONTROLSYSTEM HALTQLMOPERATION");
        }

        public VendDoorState VendDoorState => throw new NotImplementedException();

        public TrackState TrackState => throw new NotImplementedException();

        private IControlResponse From(string instruction)
        {
            using (var instructionHelper = new InstructionHelper(Service))
            {
                return instructionHelper.ExecuteWithResponse(instruction);
            }
        }
    }
}