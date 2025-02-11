using System.Collections.Generic;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework.Operations;
using Redbox.HAL.Controller.Framework.Services;

namespace Redbox.HAL.Controller.Framework
{
    public sealed class ControllerService : IMoveVeto, IControllerService
    {
        private readonly DumpbinPutObserver DumpObserver = new DumpbinPutObserver();
        private readonly DefaultGetObserver GetObserver = new DefaultGetObserver();
        private readonly DefaultPutObserver PutObserver = new DefaultPutObserver();

        public void Initialize(ErrorList errors, IDictionary<string, object> initProperties)
        {
            ControllerConfiguration.Instance.Initialize();
            var service1 = ServiceLocator.Instance.GetService<IDataTableService>();
            ServiceLocator.Instance.AddService<IKioskConfiguration>(new KioskConfigurationService());
            var inventoryService = (IInventoryService)new InventoryService();
            ServiceLocator.Instance.AddService<IInventoryService>(inventoryService);
            var persistentCounterService = new PersistentCounterService(service1);
            ServiceLocator.Instance.AddService<IPersistentCounterService>(persistentCounterService);
            ServiceLocator.Instance.AddService<IHardwareCorrectionStatisticService>(
                new HardwareCorrectionStatisticService(service1));
            ServiceLocator.Instance.AddService<IKioskFunctionCheckService>(new KioskFunctionCheckService());
            ServiceLocator.Instance.AddService<IEmptySearchPatternService>(
                new EmptySearchPatternService(inventoryService));
            var instance1 = new MotionControlService_v2();
            ServiceLocator.Instance.AddService<IMotionControlService>(instance1);
            instance1.AddVeto(this);
            var service2 = ServiceLocator.Instance.GetService<IRuntimeService>();
            var instance2 = new ControlSystemBridgeModern(service2, persistentCounterService);
            ServiceLocator.Instance.AddService<IControlSystemService>(instance2);
            ServiceLocator.Instance.AddService<IControlSystem>(instance2);
            ServiceLocator.Instance.AddService<IAirExchangerService>(new IceQubeAirExchangerService());
            ServiceLocator.Instance.AddService<IDumpbinService>(new DumpbinServiceBridge());
            ServiceLocator.Instance.AddService<IDoorSensorService>(new DoorSensorService());
            ServiceLocator.Instance.AddService<IPowerCycleDeviceService>(new PowerCycleDeviceService(service2));
        }

        public void Shutdown()
        {
            ServiceLocator.Instance.GetService<IMotionControlService>().Shutdown();
            ServiceLocator.Instance.GetService<IControlSystem>().Shutdown();
        }

        public IGetResult Get()
        {
            return Get(GetObserver);
        }

        public IGetResult Get(IGetObserver observer)
        {
            var service = ServiceLocator.Instance.GetService<IMotionControlService>();
            return OnGet(observer, service.CurrentLocation);
        }

        public IGetFromResult GetFrom(ILocation location)
        {
            return GetFrom(GetObserver, location);
        }

        public IGetFromResult GetFrom(IGetFromObserver observer, ILocation location)
        {
            var service = ServiceLocator.Instance.GetService<IMotionControlService>();
            var from = new GetFromResult();
            from.MoveResult = service.MoveTo(location, MoveMode.Get);
            if (from.MoveResult == ErrorCodes.Success)
                from.GetResult = OnGet(observer, location);
            else
                observer?.OnMoveError(from.MoveResult);
            return from;
        }

        public IPutResult Put(string id)
        {
            var service1 = ServiceLocator.Instance.GetService<IDumpbinService>();
            var service2 = ServiceLocator.Instance.GetService<IMotionControlService>();
            var observer = (IPutObserver)PutObserver;
            var currentLocation = service2.CurrentLocation;
            if (service1.IsBin(currentLocation))
                observer = DumpObserver;
            return OnPut(observer, id, service2.CurrentLocation);
        }

        public IPutResult Put(IPutObserver observer, string id)
        {
            var service = ServiceLocator.Instance.GetService<IMotionControlService>();
            return OnPut(observer, id, service.CurrentLocation);
        }

        public IPutToResult PutTo(string id, ILocation location)
        {
            var service = ServiceLocator.Instance.GetService<IDumpbinService>();
            var observer = (IPutToObserver)PutObserver;
            var loc = location;
            if (service.IsBin(loc))
                observer = DumpObserver;
            return PutTo(observer, id, location);
        }

        public IPutToResult PutTo(IPutToObserver observer, string id, ILocation location)
        {
            var service = ServiceLocator.Instance.GetService<IMotionControlService>();
            var putToResult = new PutToResult();
            putToResult.MoveResult = service.MoveTo(location, MoveMode.Put);
            if (putToResult.MoveResult == ErrorCodes.Success)
            {
                var num = (int)ServiceLocator.Instance.GetService<IControlSystem>().TrackCycle();
                putToResult.PutResult = OnPut(observer, id, location);
            }
            else
            {
                observer?.OnMoveError(putToResult.MoveResult);
            }

            return putToResult;
        }

        public ITransferResult Transfer(ILocation source, ILocation destination)
        {
            return Transfer(source, destination, true);
        }

        public ITransferResult Transfer(ILocation source, ILocation destination, bool preserveFlags)
        {
            using (var transferOperation = new TransferOperation(this, source))
            {
                transferOperation.PreserveFlagsOnPut = preserveFlags;
                return transferOperation.Transfer(destination);
            }
        }

        public ITransferResult Transfer(
            ILocation source,
            IList<ILocation> destinations,
            IGetObserver observer)
        {
            return Transfer(source, destinations, observer, true);
        }

        public ITransferResult Transfer(
            ILocation source,
            IList<ILocation> destinations,
            IGetObserver observer,
            bool preserveFlags)
        {
            using (var transferOperation = new TransferOperation(this, source))
            {
                transferOperation.PreserveFlagsOnPut = preserveFlags;
                return transferOperation.Transfer(destinations, observer);
            }
        }

        public ErrorCodes PushOut()
        {
            using (var outDiskOperation = new PushOutDiskOperation(this))
            {
                var errorCodes = outDiskOperation.Execute();
                if (errorCodes != ErrorCodes.Success)
                {
                    LogHelper.Instance.WithContext(LogEntryType.Error, "Push out disk returned error status {0}",
                        errorCodes.ToString().ToUpper());
                    ServiceLocator.Instance.GetService<IControlSystem>().LogPickerSensorState(LogEntryType.Error);
                }

                return errorCodes;
            }
        }

        public ErrorCodes ClearGripper()
        {
            using (var gripperOperation = new ClearGripperOperation())
            {
                var errorCodes = gripperOperation.Execute();
                if (errorCodes != ErrorCodes.Success)
                {
                    LogHelper.Instance.WithContext(LogEntryType.Error, "Clear gripper returned error status {0}",
                        errorCodes.ToString().ToUpper());
                    ServiceLocator.Instance.GetService<IControlSystem>().LogPickerSensorState(LogEntryType.Error);
                }

                return errorCodes;
            }
        }

        public IVendItemResult VendItemInPicker()
        {
            return VendItemInPicker(ControllerConfiguration.Instance.VendDiskPollCount);
        }

        public IVendItemResult VendItemInPicker(int attempts)
        {
            using (var vendItemOperation = new VendItemOperation(attempts))
            {
                var vendItemResult = vendItemOperation.Execute();
                if (!vendItemResult.Presented)
                    LogHelper.Instance.WithContext(LogEntryType.Error,
                        "VendItemInPicker: The disk was not presented to the user from the vend door.");
                else if (ErrorCodes.PickerFull == vendItemResult.Status ||
                         ErrorCodes.PickerEmpty == vendItemResult.Status)
                    LogHelper.Instance.WithContext(false, LogEntryType.Info, "Vend item in picker returned status {0}",
                        vendItemResult.Status.ToString().ToUpper());
                else
                    LogHelper.Instance.WithContext(LogEntryType.Error, "Vend item in picker returned error code {0}",
                        vendItemResult.Status.ToString().ToUpper());
                return vendItemResult;
            }
        }

        public ErrorCodes AcceptDiskAtDoor()
        {
            using (var acceptDiskOperation = new AcceptDiskOperation())
            {
                var errorCodes = acceptDiskOperation.Execute();
                if (ErrorCodes.PickerEmpty == errorCodes || ErrorCodes.PickerFull == errorCodes)
                    LogHelper.Instance.WithContext(false, LogEntryType.Info,
                        "Accept disk in picker returned status {0}", errorCodes);
                else
                    LogHelper.Instance.WithContext("Accept disk at door returned error {0}",
                        errorCodes.ToString().ToUpper());
                return errorCodes;
            }
        }

        public ErrorCodes RejectDiskInPicker()
        {
            return RejectDiskInPicker(ControllerConfiguration.Instance.RejectAtDoorAttempts);
        }

        public ErrorCodes RejectDiskInPicker(int attempts)
        {
            using (var rejectDiskOperation = new RejectDiskOperation(attempts))
            {
                var errorCodes = rejectDiskOperation.Execute();
                if (ErrorCodes.PickerEmpty == errorCodes || ErrorCodes.PickerFull == errorCodes)
                    LogHelper.Instance.WithContext(false, LogEntryType.Info, "Reject disk returned status {0}",
                        errorCodes.ToString().ToUpper());
                else
                    LogHelper.Instance.WithContext(LogEntryType.Error, "Reject disk returned error {0}",
                        errorCodes.ToString().ToUpper());
                return errorCodes;
            }
        }

        public ErrorCodes CanMove()
        {
            var service = ServiceLocator.Instance.GetService<IControlSystem>();
            if (VendDoorState.Closed != service.VendDoorState && !service.VendDoorClose().Success)
                return ErrorCodes.VendDoorNotClosed;
            if (ClearGripper() != ErrorCodes.Success)
                return ErrorCodes.ObstructionDetected;
            var readInputsResult = service.ReadPickerInputs();
            if (!readInputsResult.Success)
                return ErrorCodes.SensorReadError;
            if (!readInputsResult.IsInputActive(PickerInputs.Retract))
            {
                if (ControllerConfiguration.Instance.GripperRentOnMove &&
                    !readInputsResult.IsInputActive(PickerInputs.FingerRent) &&
                    !service.SetFinger(GripperFingerState.Rent).Success)
                    return ErrorCodes.GripperRentTimeout;
                if (!service.RetractArm().Success)
                    return ErrorCodes.GripperRetractTimeout;
            }

            if (!ControllerConfiguration.Instance.CheckGripperArmSensorsOnMove ||
                !readInputsResult.IsInputActive(PickerInputs.Extend) ||
                !readInputsResult.IsInputActive(PickerInputs.Retract))
                return ErrorCodes.Success;
            LogHelper.Instance.WithContext(false, LogEntryType.Error,
                "MOVE: can't move because extend/retract both triggered.");
            readInputsResult.Log(LogEntryType.Error);
            return ErrorCodes.SensorError;
        }

        private void OnCheckInventory(string expected, IFormattedLog log)
        {
            var service = ServiceLocator.Instance.GetService<IInventoryService>();
            var currentLocation = ServiceLocator.Instance.GetService<IMotionControlService>().CurrentLocation;
            var deck = currentLocation.Deck;
            var slot = currentLocation.Slot;
            var location = service.Get(deck, slot);
            if (!(location.ID != expected))
                return;
            LogHelper.Instance.WithContext(false, LogEntryType.Error,
                "** INVENTORY CHECK ERROR** - location shows ID {0} expected {1}", location.ID, expected);
        }

        private GetResult OnGet(IGetObserver observer, ILocation location)
        {
            if (location == null)
            {
                var getResult = new GetResult(location);
                getResult.Update(ErrorCodes.LocationOutOfRange);
                return getResult;
            }

            var contextLog = ServiceLocator.Instance.GetService<IExecutionService>().GetActiveContext().ContextLog;
            using (var getOperation = new GetOperation(location, observer, contextLog))
            {
                var getResult = getOperation.Execute();
                if (getResult.IsSlotEmpty)
                {
                    LogHelper.Instance.WithContext(string.Format("GET {0} returned SLOTEMPTY", location.ToString()));
                    return getResult;
                }

                if (!getResult.Success)
                {
                    LogHelper.Instance.WithContext(LogEntryType.Error,
                        string.Format("GET {0} returned error status {1}", location.ToString(),
                            getResult.ToString().ToUpper()));
                    return getResult;
                }

                var msg = string.Format("GET {0} ID={1}", location.ToString(), getResult.Previous);
                contextLog.WriteFormatted(msg);
                OnCheckInventory("EMPTY", contextLog);
                return getResult;
            }
        }

        private PutResult OnPut(IPutObserver observer, string id, ILocation location)
        {
            var service = ServiceLocator.Instance.GetService<IControlSystem>();
            var sensorReadResult = service.ReadPickerSensors();
            if (!sensorReadResult.Success)
                return new PutResult(id, location)
                {
                    Code = ErrorCodes.SensorReadError
                };
            if (!sensorReadResult.IsFull)
            {
                LogHelper.Instance.WithContext(LogEntryType.Error, "PUT: picker is empty.");
                service.LogPickerSensorState();
                service.LogInputs(ControlBoards.Picker, LogEntryType.Info);
                return new PutResult(id, location)
                {
                    Code = ErrorCodes.PickerEmpty
                };
            }

            var str = ServiceLocator.Instance.GetService<IDumpbinService>().IsBin(location)
                ? "DUMPBIN"
                : string.Format("Deck = {0} Slot = {1}", location.Deck, location.Slot);
            var contextLog = ServiceLocator.Instance.GetService<IExecutionService>().GetActiveContext().ContextLog;
            using (var putOperation = new PutOperation(id, observer, location, contextLog))
            {
                var putResult = putOperation.Execute();
                if (putResult.Success)
                {
                    contextLog.WriteFormatted(string.Format("PUT {0} ID={1}", str, id));
                    OnCheckInventory(id, contextLog);
                }
                else
                {
                    LogHelper.Instance.WithContext(LogEntryType.Error,
                        string.Format("PUT {0} ID={1} returned error status {2}", str, id,
                            putResult.ToString().ToUpper()));
                }

                return putResult;
            }
        }
    }
}