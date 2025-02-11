using System.Collections.Generic;
using System.Threading;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Threading;

namespace Redbox.HAL.Controller.Framework
{
    internal sealed class MotionControlService_v2 : IConfigurationObserver, IMotionControlService
    {
        private readonly ReaderWriterLockSlim Lock = new ReaderWriterLockSlim();
        private readonly List<IMoveVeto> Vetoers = new List<IMoveVeto>();
        private AbstractMotionController Controller;
        private bool XAxisHomed;
        private bool YAxisHomed;

        internal MotionControlService_v2()
        {
            ControllerConfiguration.Instance.AddObserver(this);
        }

        public void NotifyConfigurationLoaded()
        {
            Controller = new ArcusMotionController2();
            Controller.OnConfigurationLoad();
        }

        public void NotifyConfigurationChangeStart()
        {
            Controller.OnConfigurationChangeStart();
        }

        public void NotifyConfigurationChangeEnd()
        {
            Controller.OnConfigurationChangeEnd();
        }

        public void AddVeto(IMoveVeto veto)
        {
            using (new WithWriteLock(Lock))
            {
                Vetoers.Add(veto);
            }
        }

        public void RemoveVeto(IMoveVeto veto)
        {
            using (new WithWriteLock(Lock))
            {
                Vetoers.Remove(veto);
            }
        }

        public bool Initialize()
        {
            return Controller.OnStartup();
        }

        public bool CommunicationOk()
        {
            return Controller.CommunicationOk();
        }

        public IControllerPosition ReadPositions()
        {
            return Controller.ReadPositions();
        }

        public IMotionControlLimitResponse ReadLimits()
        {
            return Controller.ReadLimits();
        }

        public ErrorCodes MoveAbsolute(Axis axis, int? xunits, int? yunits, bool checkSensors)
        {
            if (checkSensors)
            {
                var errorCodes = AxesInitialized();
                if (errorCodes != ErrorCodes.Success)
                    return errorCodes;
            }

            var target = new MoveTarget
            {
                Axis = axis,
                XCoordinate = xunits,
                YCoordinate = yunits
            };
            return checkSensors ? MoveWithCheck(ref target) : Controller.MoveToTarget(ref target);
        }

        public ErrorCodes MoveTo(int deck, int slot, MoveMode mode)
        {
            var service = ServiceLocator.Instance.GetService<IExecutionService>();
            return MoveTo(deck, slot, mode, service.GetActiveContext().ContextLog);
        }

        public ErrorCodes MoveTo(int deck, int slot, MoveMode mode, IFormattedLog log)
        {
            var data = new OffsetMoveData();
            return MoveTo(deck, slot, mode, log, ref data);
        }

        public ErrorCodes MoveTo(
            int deck,
            int slot,
            MoveMode mode,
            IFormattedLog _log,
            ref OffsetMoveData data)
        {
            var byNumber = ServiceLocator.Instance.GetService<IDecksService>().GetByNumber(deck);
            if (byNumber == null)
                return ErrorCodes.DeckOutOfRange;
            if (!byNumber.IsSlotValid(slot))
                return ErrorCodes.SlotOutOfRange;
            var error1 = AxesInitialized();
            if (error1 != ErrorCodes.Success)
            {
                LogMoveError(deck, slot, error1);
                return error1;
            }

            var num1 = byNumber.YOffset;
            var num2 = byNumber.IsQlm ? ControllerConfiguration.Instance.QlmYOffset : 50;
            switch (mode)
            {
                case MoveMode.Put:
                    num1 += num2 * (int)ControllerConfiguration.Instance.GearY.GetStepRatio();
                    break;
                case MoveMode.Get:
                    num1 -= num2 * (int)ControllerConfiguration.Instance.GearY.GetStepRatio();
                    break;
            }

            var num3 = byNumber.GetSlotOffset(slot);
            MoveTarget moveTarget;
            int? nullable;
            if (byNumber.IsQlm)
            {
                var controllerPosition = Controller.ReadPositions();
                if (controllerPosition.ReadOk)
                {
                    var num4 = num3 - ControllerConfiguration.Instance.QlmApproachOffset;
                    moveTarget = new MoveTarget();
                    moveTarget.Axis = Axis.XY;
                    moveTarget.XCoordinate = num4;
                    moveTarget.YCoordinate = num1;
                    var target = moveTarget;
                    switch (mode)
                    {
                        case MoveMode.None:
                        case MoveMode.Get:
                            var num5 = (int)MoveWithCheck(ref target);
                            break;
                        case MoveMode.Put:
                            var num6 = num3;
                            nullable = controllerPosition.XCoordinate;
                            var num7 = nullable.Value;
                            if (num6 < num7)
                            {
                                var num8 = (int)MoveWithCheck(ref target);
                            }

                            break;
                    }
                }
            }

            nullable = data.XOffset;
            if (nullable.HasValue)
            {
                var num9 = num3;
                nullable = data.XOffset;
                var num10 = nullable.Value;
                num3 = num9 + num10;
            }

            nullable = data.YOffset;
            if (nullable.HasValue)
            {
                var num11 = num1;
                nullable = data.YOffset;
                var num12 = nullable.Value;
                num1 = num11 + num12;
            }

            moveTarget = new MoveTarget();
            moveTarget.Axis = Axis.XY;
            moveTarget.XCoordinate = num3;
            moveTarget.YCoordinate = num1;
            var target1 = moveTarget;
            var error2 = MoveWithCheck(ref target1);
            switch (error2)
            {
                case ErrorCodes.ArcusNotResponsive:
                    if (ResetMotionControllerChecked()) error2 = MoveWithCheck(ref target1);
                    break;
                case ErrorCodes.LowerLimitError:
                    if (!ControllerConfiguration.Instance.LowerLimitAsError) error2 = ErrorCodes.Timeout;
                    break;
            }

            LogMoveError(deck, slot, error2);
            if (error2 == ErrorCodes.Success)
            {
                AtVendDoor = false;
                CurrentLocation = ServiceLocator.Instance.GetService<IInventoryService>().Get(deck, slot);
            }

            return error2;
        }

        public ErrorCodes MoveTo(ILocation location, MoveMode mode)
        {
            var service = ServiceLocator.Instance.GetService<IExecutionService>();
            return MoveTo(location.Deck, location.Slot, mode, service.GetActiveContext().ContextLog);
        }

        public ErrorCodes MoveTo(ILocation location, MoveMode mode, IFormattedLog log)
        {
            return MoveTo(location.Deck, location.Slot, mode, log);
        }

        public ErrorCodes MoveTo(
            ILocation location,
            MoveMode mode,
            IFormattedLog log,
            ref OffsetMoveData data)
        {
            return MoveTo(location.Deck, location.Slot, mode, log, ref data);
        }

        public ErrorCodes MoveVend(MoveMode mode, IFormattedLog writer)
        {
            var error1 = AxesInitialized();
            if (error1 != ErrorCodes.Success)
            {
                LogMoveVendError(error1);
                return error1;
            }

            var error2 = CheckAndReset();
            if (error2 != ErrorCodes.Success)
            {
                LogMoveVendError(error2);
                return error2;
            }

            var vend = Controller.MoveToVend(mode);
            if (ErrorCodes.ArcusNotResponsive == vend && ResetMotionControllerChecked())
                vend = Controller.MoveToVend(mode);
            LogMoveVendError(vend);
            AtVendDoor = vend == ErrorCodes.Success;
            return vend;
        }

        public ErrorCodes InitAxes()
        {
            return InitAxes(false);
        }

        public ErrorCodes InitAxes(bool fast)
        {
            if (ControllerConfiguration.Instance.RestartControllerDuringUserJobs && !CommunicationOk())
            {
                var failure = OnReset();
                InsertCorrectionStat(failure == ErrorCodes.Success);
                if (failure != ErrorCodes.Success)
                {
                    OnResetFailure(failure);
                    return failure;
                }
            }

            return OnInitAxes(fast);
        }

        public ErrorCodes HomeAxis(Axis axis)
        {
            return HomeAxisChecked(axis);
        }

        public ErrorCodes Reset(bool quick)
        {
            var failure = OnReset();
            if (failure == ErrorCodes.Success)
                failure = OnInitAxes(quick);
            if (failure != ErrorCodes.Success)
                OnResetFailure(failure);
            else
                LogHelper.Instance.WithContext("RESET of motion controller returned {0}", failure.ToString().ToUpper());
            return failure;
        }

        public bool TestAndReset()
        {
            return TestAndReset(true);
        }

        public bool TestAndReset(bool quick)
        {
            return CommunicationOk() || Reset(true) == ErrorCodes.Success;
        }

        public void Shutdown()
        {
            XAxisHomed = YAxisHomed = false;
            Controller.OnShutdown();
        }

        public string GetPrintableLocation()
        {
            var printableLocation = "Location unknown";
            var service = ServiceLocator.Instance.GetService<IMotionControlService>();
            if (service.CurrentLocation != null)
                printableLocation = service.CurrentLocation.ToString();
            else if (service.AtVendDoor)
                printableLocation = "Vend Door";
            return printableLocation;
        }

        public ILocation CurrentLocation { get; private set; }

        public bool AtVendDoor { get; private set; }

        public bool IsInitialized => AxesInitialized() == ErrorCodes.Success;

        private ErrorCodes MoveWithCheck(ref MoveTarget target)
        {
            var errorCodes = CheckAndReset();
            return errorCodes == ErrorCodes.Success ? Controller.MoveToTarget(ref target) : errorCodes;
        }

        private ErrorCodes HomeAxisChecked(Axis axis)
        {
            var errorCodes1 = CheckAndReset();
            if (errorCodes1 != ErrorCodes.Success)
                return errorCodes1;
            if (Axis.Y == axis)
                YAxisHomed = false;
            else if (axis == Axis.X)
                XAxisHomed = false;
            var errorCodes2 = Controller.HomeAxis(axis);
            if (errorCodes2 != ErrorCodes.Success)
                LogHelper.Instance.WithContext(LogEntryType.Error,
                    string.Format("HOME {0} returned an error status {1}", axis.ToString().ToUpper(),
                        errorCodes2.ToString().ToUpper()));
            else
                switch (axis)
                {
                    case Axis.X:
                        XAxisHomed = true;
                        break;
                    case Axis.Y:
                        YAxisHomed = true;
                        break;
                }

            return errorCodes2;
        }

        private ErrorCodes AxesInitialized()
        {
            if (!ControllerConfiguration.Instance.ValidateControllerHomeStatus)
                return ErrorCodes.Success;
            var num = 0;
            if (!XAxisHomed)
            {
                ++num;
                LogHelper.Instance.WithContext(false, LogEntryType.Info, "The X axis did not init.");
            }

            if (!YAxisHomed)
            {
                ++num;
                LogHelper.Instance.WithContext(false, LogEntryType.Info, "The Y axis did not init.");
            }

            return num != 0 ? ErrorCodes.MotorNotHomed : ErrorCodes.Success;
        }

        private ErrorCodes CheckAndReset()
        {
            using (new WithReadLock(Lock))
            {
                foreach (var vetoer in Vetoers)
                {
                    var errorCodes = vetoer.CanMove();
                    if (errorCodes != ErrorCodes.Success)
                    {
                        LogHelper.Instance.WithContext(false, LogEntryType.Error, "Move veto returned code {0}",
                            errorCodes.ToString());
                        return errorCodes;
                    }
                }
            }

            CurrentLocation = null;
            AtVendDoor = false;
            return ErrorCodes.Success;
        }

        private ErrorCodes OnInitAxes(bool fast)
        {
            using (var pickerFrontOperation = new ClearPickerFrontOperation())
            {
                if (ControllerConfiguration.Instance.ClearPickerOnHome)
                    pickerFrontOperation.Execute();
                var contextLog = ServiceLocator.Instance.GetService<IExecutionService>().GetActiveContext().ContextLog;
                contextLog.WriteFormatted("Home the X motor.");
                var num = fast ? 1 : 2;
                for (var index = 0; index < num; ++index)
                {
                    var errorCodes = HomeAxisChecked(Axis.X);
                    if (errorCodes != ErrorCodes.Success)
                        return errorCodes;
                    if (index == 0)
                    {
                        var target1 = new MoveTarget
                        {
                            Axis = Axis.X,
                            XCoordinate = -200,
                            YCoordinate = new int?()
                        };
                        var target2 = (int)Controller.MoveToTarget(ref target1);
                    }
                }

                contextLog.WriteFormatted("Home the Y Motor.");
                return HomeAxisChecked(Axis.Y);
            }
        }

        private bool ResetMotionControllerChecked()
        {
            if (!ControllerConfiguration.Instance.RestartControllerDuringUserJobs)
                return false;
            var activeContext = ServiceLocator.Instance.GetService<IExecutionService>().GetActiveContext();
            var e = new HardwareCorrectionEventArgs(HardwareCorrectionStatistic.Arcus);
            activeContext?.HardwareCorrectionStart(e);
            e.CorrectionOk = Reset(true) == ErrorCodes.Success;
            activeContext?.HardwareCorrectionEnd(e);
            InsertCorrectionStat(activeContext, e.CorrectionOk);
            return e.CorrectionOk;
        }

        private void InsertCorrectionStat(bool ok)
        {
            InsertCorrectionStat(ServiceLocator.Instance.GetService<IExecutionService>().GetActiveContext(), ok);
        }

        private void InsertCorrectionStat(IExecutionContext activeContext, bool ok)
        {
            if (activeContext == null)
                return;
            ServiceLocator.Instance.GetService<IHardwareCorrectionStatisticService>()
                .Insert(HardwareCorrectionStatistic.Arcus, activeContext, ok);
        }

        private void LogMoveVendError(ErrorCodes error)
        {
            if (error == ErrorCodes.Success)
                return;
            LogHelper.Instance.WithContext(LogEntryType.Error, "MOVEVEND returned an error status {0}",
                error.ToString().ToUpper());
        }

        private void LogMoveError(int deck, int slot, ErrorCodes error)
        {
            if (error == ErrorCodes.Success)
                return;
            LogHelper.Instance.WithContext(LogEntryType.Error,
                "MOVE Deck = {0} Slot = {1} returned an error status {2}", deck, slot, error.ToString().ToUpper());
        }

        private ErrorCodes OnReset()
        {
            Shutdown();
            if (!Controller.OnResetDeviceDriver())
                return ErrorCodes.ArcusNotResponsive;
            Initialize();
            return Controller.CommunicationOk() ? ErrorCodes.Success : ErrorCodes.ArcusNotResponsive;
        }

        private void OnResetFailure(ErrorCodes failure)
        {
            LogHelper.Instance.WithContext(false, LogEntryType.Error, "Reset of motion controller returned error {0}",
                failure);
        }
    }
}