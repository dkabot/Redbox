using System.Collections.Generic;
using System.Threading;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Threading;

namespace Redbox.HAL.Controller.Framework
{
    internal sealed class ControlSystemBridgeModern :
        IConfigurationObserver,
        IControlSystemService,
        IControlSystem
    {
        private readonly ReaderWriterLockSlim ObserverLock = new ReaderWriterLockSlim();
        private readonly List<IControlSystemObserver> Observers = new List<IControlSystemObserver>();
        private readonly IPersistentCounterService PersistentCounterService;
        private readonly IRuntimeService RuntimeService;
        private AbstractRedboxControlSystem Implementor;

        internal ControlSystemBridgeModern(IRuntimeService rts, IPersistentCounterService pcs)
        {
            ControllerConfiguration.Instance.AddObserver(this);
            IsInitialized = false;
            RuntimeService = rts;
            PersistentCounterService = pcs;
        }

        public void NotifyConfigurationChangeStart()
        {
        }

        public void NotifyConfigurationChangeEnd()
        {
        }

        public void NotifyConfigurationLoaded()
        {
            LogHelper.Instance.Log("[ControlSystemBridge] Configuration load.");
            var instance = new FmeControlSystem(RuntimeService);
            Implementor = instance;
            ServiceLocator.Instance.AddService<ICoreCommandExecutor>(instance);
        }

        public IControlResponse Initialize()
        {
            var coreResponse = Implementor.Initialize();
            IsInitialized = coreResponse.Success;
            if (coreResponse.Success)
            {
                using (new WithReadLock(ObserverLock))
                {
                    Observers.ForEach(each => each.OnSystemInitialize(ErrorCodes.Success));
                }

                VendDoorState = Implementor.ReadVendDoorState();
            }

            return coreResponse;
        }

        public bool Shutdown()
        {
            using (new WithReadLock(ObserverLock))
            {
                Observers.ForEach(observer => observer.OnSystemShutdown());
            }

            IsInitialized = false;
            return Implementor.Shutdown();
        }

        public IControlResponse SetAudio(AudioChannelState newState)
        {
            return Implementor.SetAudio(newState);
        }

        public IControlResponse ToggleRingLight(bool on, int? sleepAfter)
        {
            var coreResponse = Implementor.SetRinglight(on);
            if (!coreResponse.Success)
                return coreResponse;
            if (!sleepAfter.HasValue)
                return coreResponse;
            RuntimeService.Wait(sleepAfter.Value);
            return coreResponse;
        }

        public IControlResponse VendDoorRent()
        {
            VendDoorState = VendDoorState.Unknown;
            var coreResponse = Implementor.SetVendDoor(VendDoorState.Rent);
            if (!coreResponse.Success)
            {
                PersistentCounterService.Increment(TimeoutCounters.VendDoorRent);
                return coreResponse;
            }

            VendDoorState = VendDoorState.Rent;
            return coreResponse;
        }

        public IControlResponse VendDoorClose()
        {
            VendDoorState = VendDoorState.Unknown;
            var coreResponse = Implementor.SetVendDoor(VendDoorState.Closed);
            if (!coreResponse.Success)
            {
                PersistentCounterService.Increment(TimeoutCounters.VendDoorClose);
                return coreResponse;
            }

            VendDoorState = VendDoorState.Closed;
            return coreResponse;
        }

        public VendDoorState ReadVendDoorPosition()
        {
            return Implementor.ReadVendDoorState();
        }

        public IControlResponse TrackOpen()
        {
            TrackState = TrackState.Unknown;
            var coreResponse = Implementor.SetTrack(TrackState.Open);
            if (coreResponse.Success)
            {
                TrackState = TrackState.Open;
                return coreResponse;
            }

            PersistentCounterService.Increment(TimeoutCounters.TrackOpen);
            return coreResponse;
        }

        public IControlResponse TrackClose()
        {
            TrackState = TrackState.Unknown;
            var coreResponse = Implementor.SetTrack(TrackState.Closed);
            if (coreResponse.Success)
            {
                TrackState = TrackState.Closed;
                return coreResponse;
            }

            PersistentCounterService.Increment(TimeoutCounters.TrackClose);
            return coreResponse;
        }

        public ErrorCodes TrackCycle()
        {
            if (!TrackOpen().Success)
                return ErrorCodes.TrackOpenTimeout;
            return !TrackClose().Success ? ErrorCodes.TrackCloseTimeout : ErrorCodes.Success;
        }

        public void TimedExtend()
        {
            TimedExtend(ControllerConfiguration.Instance.PushTime);
        }

        public void TimedExtend(int timeout)
        {
            Implementor.TimedArmExtend(timeout);
        }

        public IControlResponse ExtendArm()
        {
            var timeout = ControllerConfiguration.Instance.GripperArmExtendRetractTimeout;
            var service = ServiceLocator.Instance.GetService<IDecksService>();
            var currentLocation = ServiceLocator.Instance.GetService<IMotionControlService>().CurrentLocation;
            var flag = false;
            if (currentLocation != null)
                flag = service.GetByNumber(currentLocation.Deck).IsQlm;
            if (flag)
                timeout = ControllerConfiguration.Instance.QlmExtendTime;
            return ExtendArm(timeout);
        }

        public IControlResponse ExtendArm(int timeout)
        {
            var coreResponse = Implementor.ExtendArm(timeout);
            if (coreResponse.Success)
                return coreResponse;
            PersistentCounterService.Increment(TimeoutCounters.GripperExtend);
            return coreResponse;
        }

        public IControlResponse RetractArm()
        {
            var coreResponse = Implementor.RetractArm(ControllerConfiguration.Instance.GripperArmExtendRetractTimeout);
            if (coreResponse.Success)
                return coreResponse;
            PersistentCounterService.Increment(TimeoutCounters.GripperRetract);
            return coreResponse;
        }

        public IControlResponse SetFinger(GripperFingerState state)
        {
            var coreResponse = Implementor.SetFinger(state);
            if (coreResponse.Success)
                return coreResponse;
            var service = ServiceLocator.Instance.GetService<IPersistentCounterService>();
            if (state == GripperFingerState.Closed)
            {
                service.Increment(TimeoutCounters.FingerClose);
                return coreResponse;
            }

            if (state == GripperFingerState.Open)
            {
                service.Increment(TimeoutCounters.FingerOpen);
                return coreResponse;
            }

            service.Increment(TimeoutCounters.FingerRent);
            return coreResponse;
        }

        public ErrorCodes Center(CenterDiskMethod method)
        {
            var errorCodes = ErrorCodes.Success;
            if (method == CenterDiskMethod.None)
                return errorCodes;
            var service = ServiceLocator.Instance.GetService<IRuntimeService>();
            var milliseconds = 250;
            var position1 = CenterDiskMethod.DrumAndBack == method
                ? RollerPosition.Position1
                : RollerPosition.Position6;
            if (!RollerToPosition(position1, ControllerConfiguration.Instance.DefaultRollSensorTimeout, false).Success)
            {
                errorCodes = RollerPosition.Position1 == position1
                    ? ErrorCodes.RollerToPos1Timeout
                    : ErrorCodes.RollerToPos6Timeout;
                LogHelper.Instance.WithContext(false, LogEntryType.Error, "Center disk: Roller {0} timed out.",
                    position1);
            }

            service.SpinWait(milliseconds);
            var position2 = CenterDiskMethod.DrumAndBack == method
                ? RollerPosition.Position5
                : RollerPosition.Position3;
            if (!RollerToPosition(position2, ControllerConfiguration.Instance.DefaultRollSensorTimeout, false).Success)
            {
                errorCodes = RollerPosition.Position5 == position2
                    ? ErrorCodes.RollerToPos5Timeout
                    : ErrorCodes.RollerToPos3Timeout;
                LogHelper.Instance.WithContext(false, LogEntryType.Error, "Center disk: Roller {0} timed out.",
                    position2);
            }

            service.SpinWait(milliseconds);
            var num = (int)TrackCycle();
            return errorCodes;
        }

        public IBoardVersionResponse GetBoardVersion(ControlBoards board)
        {
            return Implementor.GetBoardVersion(board);
        }

        public IControlSystemRevision GetRevision()
        {
            return Implementor.GetRevision();
        }

        public IReadInputsResult<PickerInputs> ReadPickerInputs()
        {
            var pickerInputsResult = Implementor.ReadPickerInputs();
            if (!pickerInputsResult.Success)
                LogHelper.Instance.WithContext(LogEntryType.Error, "Read Picker inputs failed with error {0}",
                    pickerInputsResult.Error);
            return pickerInputsResult;
        }

        public IReadInputsResult<AuxInputs> ReadAuxInputs()
        {
            var readAuxInputsResult = Implementor.ReadAuxInputs();
            if (!readAuxInputsResult.Success)
                LogHelper.Instance.WithContext(LogEntryType.Error, "Read AUX inputs failed with error {0}",
                    readAuxInputsResult.Error);
            return readAuxInputsResult;
        }

        public void LogPickerSensorState()
        {
            LogPickerSensorState(LogEntryType.Info);
        }

        public void LogPickerSensorState(LogEntryType type)
        {
            ReadPickerSensors().Log(type);
        }

        public void LogInputs(ControlBoards board)
        {
            LogInputs(board, LogEntryType.Info);
        }

        public void LogInputs(ControlBoards board, LogEntryType type)
        {
            if (ControlBoards.Picker == board)
                Implementor.ReadPickerInputs().Log(type);
            else
                Implementor.ReadAuxInputs().Log(type);
        }

        public IControlResponse SetSensors(bool on)
        {
            return Implementor.SetPickerSensors(on);
        }

        public IPickerSensorReadResult ReadPickerSensors()
        {
            return ReadPickerSensors(true);
        }

        public IPickerSensorReadResult ReadPickerSensors(bool closeTrack)
        {
            if (closeTrack && TrackState.Closed != TrackState && !TrackClose().Success)
                return new PickerSensorReadResult(ErrorCodes.TrackCloseTimeout);
            try
            {
                var coreResponse = Implementor.SetPickerSensors(true);
                if (!coreResponse.Success)
                    return new PickerSensorReadResult(coreResponse.Error);
                var pickerSensorSpinTime = ControllerConfiguration.Instance.PickerSensorSpinTime;
                RuntimeService.SpinWait(pickerSensorSpinTime);
                var result = Implementor.ReadPickerInputs();
                RuntimeService.SpinWait(pickerSensorSpinTime);
                return new PickerSensorReadResult(result);
            }
            finally
            {
                Implementor.SetPickerSensors(false);
            }
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

        public IControlResponse SetRollerState(RollerState state)
        {
            return Implementor.SetRoller(state);
        }

        public IControlResponse RollerToPosition(RollerPosition position)
        {
            return RollerToPosition(position, ControllerConfiguration.Instance.DefaultRollSensorTimeout);
        }

        public IControlResponse RollerToPosition(RollerPosition position, int opTimeout)
        {
            return RollerToPosition(position, opTimeout, true);
        }

        public IControlResponse RollerToPosition(
            RollerPosition position,
            int opTimeout,
            bool logSensors)
        {
            var position1 = Implementor.RollerToPosition(position, opTimeout);
            if (position1.Success)
                return position1;
            if (logSensors && !position1.CommError)
            {
                LogHelper.Instance.WithContext(false, LogEntryType.Error, "Roller to {0} timed out.",
                    position.ToString());
                LogPickerSensorState(LogEntryType.Error);
            }

            return position1;
        }

        public QlmStatus GetQlmStatus()
        {
            return Implementor.GetQlmStatus();
        }

        public ErrorCodes EngageQlm(IFormattedLog log)
        {
            return EngageQlm(true, log);
        }

        public ErrorCodes EngageQlm(bool home, IFormattedLog log)
        {
            return OnLifterOperation(QlmOperation.Engage, log, home);
        }

        public ErrorCodes DisengageQlm(IFormattedLog log)
        {
            return DisengageQlm(true, log);
        }

        public ErrorCodes DisengageQlm(bool home, IFormattedLog log)
        {
            return OnLifterOperation(QlmOperation.Disengage, log, home);
        }

        public IControlResponse LockQlmDoor()
        {
            return Implementor.OnQlm(QlmOperation.LockDoor);
        }

        public IControlResponse UnlockQlmDoor()
        {
            return Implementor.OnQlm(QlmOperation.UnlockDoor);
        }

        public IControlResponse DropQlm()
        {
            return Implementor.OnQlm(QlmOperation.Drop);
        }

        public IControlResponse LiftQlm()
        {
            return Implementor.OnQlm(QlmOperation.Lift);
        }

        public IControlResponse HaltQlmLifter()
        {
            return Implementor.OnQlm(QlmOperation.Halt);
        }

        public VendDoorState VendDoorState { get; private set; }

        public TrackState TrackState { get; private set; }

        public void AddHandler(IControlSystemObserver observer)
        {
            using (new WithWriteLock(ObserverLock))
            {
                if (Observers.Contains(observer))
                    return;
                Observers.Add(observer);
            }
        }

        public void RemoveHandler(IControlSystemObserver observer)
        {
            using (new WithWriteLock(ObserverLock))
            {
                Observers.Remove(observer);
            }
        }

        public bool Restart()
        {
            if (!Shutdown())
                return false;
            RuntimeService.Wait(1200);
            return Initialize().Success;
        }

        public bool IsInitialized { get; private set; }

        private ErrorCodes OnLifterOperation(QlmOperation operation, IFormattedLog log, bool home)
        {
            if (ControllerConfiguration.Instance.IsVMZMachine)
                return ErrorCodes.Timeout;
            if (home && ServiceLocator.Instance.GetService<IMotionControlService>().HomeAxis(Axis.X) !=
                ErrorCodes.Success)
                return ErrorCodes.HomeXTimeout;
            var coreResponse = Implementor.OnQlm(operation);
            var errorCodes = !coreResponse.Success ? ErrorCodes.Timeout : ErrorCodes.Success;
            var msg = string.Format("{0} returned status {1}.", operation.ToString().ToUpper(),
                errorCodes.ToString().ToUpper());
            log.WriteFormatted(msg);
            if (!coreResponse.Success)
                PersistentCounterService.Increment(QlmOperation.Engage == operation
                    ? TimeoutCounters.QlmEngage
                    : TimeoutCounters.QlmDisengage);
            return errorCodes;
        }
    }
}