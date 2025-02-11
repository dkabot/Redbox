using System;
using System.IO.Ports;
using System.Text;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework
{
    internal sealed class FmeControlSystem : AbstractRedboxControlSystem, ICoreCommandExecutor
    {
        private const int LifterTimeout = 120000;

        private readonly ControlBoards[] ControllerBoards = new ControlBoards[3]
        {
            ControlBoards.Serial,
            ControlBoards.Picker,
            ControlBoards.Aux
        };

        private readonly ICommPort Port;

        internal FmeControlSystem(IRuntimeService rts)
            : base(rts)
        {
            var port = new SerialPort(ControllerConfiguration.Instance.ControllerPortName, 9600, Parity.None, 8,
                StopBits.One);
            Port = ServiceLocator.Instance.GetService<IPortManagerService>().Create(port);
            Port.WriteTerminator = new byte[1] { 13 };
            Port.ValidateResponse = response =>
            {
                var str = Encoding.ASCII.GetString(response.RawResponse);
                if (string.IsNullOrEmpty(str))
                    return false;
                return str.IndexOf("OK") != -1 || str.IndexOf("EER") != -1;
            };
        }

        public CoreResponse ExecuteControllerCommand(CommandType type)
        {
            return SendCommand(CoreCommand.Create(type, new int?(), Port));
        }

        public CoreResponse ExecuteControllerCommand(CommandType type, int? timeout)
        {
            return SendCommand(CoreCommand.Create(type, timeout, Port));
        }

        internal override CoreResponse Initialize()
        {
            var coreResponse = ExecuteControllerCommand(CommandType.Reset);
            RuntimeService.Wait(2000);
            SetAudio(AudioChannelState.Off);
            return coreResponse;
        }

        internal override bool Shutdown()
        {
            return Port.Close();
        }

        internal override CoreResponse SetAudio(AudioChannelState newState)
        {
            return ExecuteControllerCommand(AudioChannelState.On == newState
                ? CommandType.AudioOn
                : CommandType.AudioOff);
        }

        internal override CoreResponse SetRinglight(bool on)
        {
            return ExecuteControllerCommand(on ? CommandType.RinglightOn : CommandType.RinglightOff);
        }

        internal override CoreResponse SetPickerSensors(bool on)
        {
            return ExecuteControllerCommand(on ? CommandType.SensorBarOn : CommandType.SensorBarOff);
        }

        internal override CoreResponse SetFinger(GripperFingerState state)
        {
            var command = CommandType.GripperClose;
            switch (state)
            {
                case GripperFingerState.Closed:
                    command = CommandType.GripperClose;
                    break;
                case GripperFingerState.Open:
                    if (ControllerConfiguration.Instance.EnableSecureDiskValidator)
                        return CoreResponse.TimedOutResponse;
                    command = CommandType.GripperOpen;
                    break;
                case GripperFingerState.Rent:
                    command = !ControllerConfiguration.Instance.EnableSecureDiskValidator
                        ? CommandType.GripperRent
                        : CommandType.GripperOpen;
                    break;
            }

            var response = OnRetryable(command, 2, 5000);
            LogError(response, string.Format("RetryableFingerFunction {0}", state), ControlBoards.Picker);
            return response;
        }

        internal override CoreResponse SetRoller(RollerState state)
        {
            var type = CommandType.RollerStop;
            switch (state)
            {
                case RollerState.None:
                    throw new ArgumentException("Cannot be none");
                case RollerState.In:
                    type = CommandType.RollerIn;
                    break;
                case RollerState.Out:
                    type = CommandType.RollerOut;
                    break;
                case RollerState.Stop:
                    type = CommandType.RollerStop;
                    break;
            }

            var response = ExecuteControllerCommand(type);
            LogError(response, string.Format("Roller{0}", state), ControlBoards.Picker);
            return response;
        }

        internal override CoreResponse RollerToPosition(RollerPosition position, int opTimeout)
        {
            var type = CommandType.RollerToPos1;
            switch (position)
            {
                case RollerPosition.Position1:
                    type = CommandType.RollerToPos1;
                    break;
                case RollerPosition.Position2:
                    type = CommandType.RollerToPos2;
                    break;
                case RollerPosition.Position3:
                    type = CommandType.RollerToPos3;
                    break;
                case RollerPosition.Position4:
                    type = CommandType.RollerToPos4;
                    break;
                case RollerPosition.Position5:
                    type = CommandType.RollerToPos5;
                    break;
                case RollerPosition.Position6:
                    type = CommandType.RollerToPos6;
                    break;
            }

            return ExecuteControllerCommand(type, opTimeout);
        }

        internal override CoreResponse TimedArmExtend(int timeout)
        {
            try
            {
                LogHelper.Instance.Log(LogEntryType.Debug, "[FmeControlSystem] ExtendGripperArmForTime {0} ms",
                    timeout);
                var coreResponse = ExecuteControllerCommand(CommandType.ExtendGripperArmForTime);
                RuntimeService.Wait(timeout);
                return coreResponse;
            }
            finally
            {
                ExecuteControllerCommand(CommandType.GripperExtendHalt);
            }
        }

        internal override CoreResponse ExtendArm(int timeout)
        {
            var response = ExecuteControllerCommand(CommandType.GripperExtend, timeout);
            LogError(response, "SetGripperArm( Extend )", ControlBoards.Picker);
            if (response.TimedOut)
            {
                var pickerInputsResult = ReadPickerInputs();
                if (pickerInputsResult.Success && pickerInputsResult.IsInputActive(PickerInputs.Extend))
                {
                    LogHelper.Instance.WithContext(
                        "GripperExtend status returned timeout; however, picker sensors read shows the forward sensor reached.");
                    pickerInputsResult.Log();
                    response.Error = ErrorCodes.Success;
                }
            }

            return response;
        }

        internal override CoreResponse RetractArm(int timeout)
        {
            var response = OnRetryable(CommandType.GripperRetract, 2, timeout);
            LogError(response, "SetGripperArm( RETRACT )", ControlBoards.Picker);
            if (response.TimedOut)
            {
                var pickerInputsResult = ReadPickerInputs();
                if (pickerInputsResult.Success && pickerInputsResult.IsInputActive(PickerInputs.Retract))
                {
                    LogHelper.Instance.WithContext(
                        "GripperRetract status returned timeout; however, picker sensors read shows the sensor tripped.");
                    pickerInputsResult.Log();
                }
            }

            return response;
        }

        internal override CoreResponse SetTrack(TrackState state)
        {
            var response = OnRetryable(state == TrackState.Open ? CommandType.TrackOpen : CommandType.TrackClose, 2,
                5000);
            LogError(response, string.Format("Track {0}", state.ToString()), ControlBoards.Picker);
            return response;
        }

        internal override CoreResponse SetVendDoor(VendDoorState state)
        {
            var timeout = 5500;
            var response = VendDoorState.Rent != state
                ? OnCloseVendDoor(timeout)
                : OnRetryable(CommandType.VendDoorRent, 1, timeout);
            LogError(response, string.Format("VendDoor {0}", state), ControlBoards.Aux);
            return response;
        }

        internal override VendDoorState ReadVendDoorState()
        {
            var readAuxInputsResult = ReadAuxInputs();
            if (!readAuxInputsResult.Success)
            {
                LogHelper.Instance.WithContext(false, LogEntryType.Error, "Read AUX sensors returned error {0}",
                    readAuxInputsResult.Error.ToString().ToUpper());
                return VendDoorState.Unknown;
            }

            if (readAuxInputsResult.IsInputActive(AuxInputs.VendDoorClosed))
                return VendDoorState.Closed;
            return readAuxInputsResult.IsInputActive(AuxInputs.VendDoorRent)
                ? VendDoorState.Rent
                : VendDoorState.Unknown;
        }

        internal override QlmStatus GetQlmStatus()
        {
            var readAuxInputsResult = ReadAuxInputs();
            if (!readAuxInputsResult.Success)
                return QlmStatus.AuxNotResponsive;
            if (!readAuxInputsResult.IsInputActive(AuxInputs.QlmPresence))
                return QlmStatus.Empty;
            return !readAuxInputsResult.IsInputActive(AuxInputs.QlmUp) ? QlmStatus.Disengaged : QlmStatus.Engaged;
        }

        internal override CoreResponse OnQlm(QlmOperation op)
        {
            var response = (CoreResponse)null;
            switch (op)
            {
                case QlmOperation.None:
                    throw new ArgumentException("[FmeControlSystem] OnQlm: operation cannot be none");
                case QlmOperation.Engage:
                    response = ExecuteControllerCommand(CommandType.QlmEngage, 120000);
                    break;
                case QlmOperation.Disengage:
                    response = ExecuteControllerCommand(CommandType.QlmDisengage, 120000);
                    break;
                case QlmOperation.Lift:
                    response = ExecuteControllerCommand(CommandType.QlmLift);
                    break;
                case QlmOperation.Drop:
                    response = ExecuteControllerCommand(CommandType.QlmDrop);
                    break;
                case QlmOperation.Halt:
                    response = ExecuteControllerCommand(CommandType.QlmHalt);
                    break;
                case QlmOperation.LockDoor:
                    response = ExecuteControllerCommand(CommandType.QlmDoorLock);
                    break;
                case QlmOperation.UnlockDoor:
                    response = ExecuteControllerCommand(CommandType.QlmDoorUnlock);
                    break;
            }

            LogError(response, op.ToString().ToUpper(), ControlBoards.Aux);
            return response;
        }

        internal override ReadAuxInputsResult ReadAuxInputs()
        {
            return new ReadAuxInputsResult(ExecuteControllerCommand(CommandType.AuxSensorsRead));
        }

        internal override ReadPickerInputsResult ReadPickerInputs()
        {
            return new ReadPickerInputsResult(ExecuteControllerCommand(CommandType.ReadPickerInputs));
        }

        internal override BoardVersionResponse GetBoardVersion(ControlBoards board)
        {
            var type = CommandType.Version101;
            switch (board)
            {
                case ControlBoards.Picker:
                    type = CommandType.Version001;
                    break;
                case ControlBoards.Aux:
                    type = CommandType.Version002;
                    break;
                case ControlBoards.Serial:
                    type = CommandType.Version101;
                    break;
            }

            return new BoardVersionResponse(board, ExecuteControllerCommand(type));
        }

        internal override IControlSystemRevision GetRevision()
        {
            var boardVersionResponseArray = new IBoardVersionResponse[ControllerBoards.Length];
            var num = 0;
            for (var index = 0; index < ControllerBoards.Length; ++index)
            {
                boardVersionResponseArray[index] = GetBoardVersion(ControllerBoards[index]);
                if (!boardVersionResponseArray[index].ReadSuccess)
                    ++num;
            }

            return new FmeControllerRevision
            {
                Responses = boardVersionResponseArray,
                Success = num == 0
            };
        }

        private CoreResponse OnRetryable(CommandType command, int retryCount, int timeout)
        {
            var coreResponse = (CoreResponse)null;
            for (var index = 0; index < retryCount; ++index)
            {
                coreResponse = ExecuteControllerCommand(command, timeout);
                if (coreResponse.Success || coreResponse.CommError)
                    return coreResponse;
            }

            return coreResponse;
        }

        private CoreResponse SendCommand(CoreCommand command)
        {
            if (command.Port != null)
                if (command.Port.Open())
                    try
                    {
                        return command.Execute();
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Instance.Log(LogEntryType.Error,
                            string.Format("[FmeControlSystem] Send command on port {0} caught an exception.",
                                command.Port.DisplayName), ex);
                        return CoreResponse.CommErrorResponse;
                    }

            LogHelper.Instance.WithContext(LogEntryType.Error, "Unable to open control port {0}",
                command.Port.DisplayName);
            return CoreResponse.CommErrorResponse;
        }

        private CoreResponse OnCloseVendDoor(int timeout)
        {
            var coreResponse = (CoreResponse)null;
            for (var index = 0; index < 2; ++index)
            {
                coreResponse = ExecuteControllerCommand(CommandType.VendDoorClose, timeout);
                if (coreResponse.CommError)
                    return coreResponse;
                if (coreResponse.TimedOut)
                    OnCloseBackoff();
                else if (ReadVendDoorState() == VendDoorState.Closed || MoveToCloseSensor())
                    return coreResponse;
            }

            return coreResponse;
        }

        private bool MoveToCloseSensor()
        {
            if (!ControllerConfiguration.Instance.MoveVendDoorToAuxSensor)
                return true;
            OnCloseBackoff();
            var coreResponse = ExecuteControllerCommand(CommandType.VendDoorClose, 5000);
            if (coreResponse.CommError)
                return false;
            RuntimeService.SpinWait(100);
            return !coreResponse.TimedOut;
        }

        private void OnCloseBackoff()
        {
            RuntimeService.Wait(200);
            ExecuteControllerCommand(CommandType.UnknownVendDoorCloseCommand);
            RuntimeService.Wait(100);
            ExecuteControllerCommand(CommandType.VendDoorKillCommand);
            RuntimeService.Wait(100);
        }

        private void LogError(CoreResponse response, string command, ControlBoards board)
        {
            if (response.Success)
                return;
            LogHelper.Instance.WithContext(false, LogEntryType.Error, "{0} returned error status {1}.", command,
                response.Error.ToString().ToUpper());
            if (response.CommError)
                return;
            if (ControlBoards.Aux == board)
            {
                ReadAuxInputs().Log(LogEntryType.Error);
            }
            else
            {
                if (ControlBoards.Picker != board)
                    return;
                ReadPickerInputs().Log(LogEntryType.Error);
            }
        }

        private class FmeControllerRevision : IControlSystemRevision
        {
            public bool Success { get; internal set; }

            public IBoardVersionResponse[] Responses { get; internal set; }

            public string Revision => "A";
        }
    }
}