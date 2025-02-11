using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework
{
    internal sealed class GetOperation : AbstractOperation<GetResult>
    {
        private readonly IDeck Deck;
        private readonly ILocation Location;
        private readonly IGetObserver Observer;
        private readonly GetResult Result;
        private readonly int Slot;
        private IFormattedLog Log;

        internal GetOperation(ILocation location, IGetObserver observer, IFormattedLog log)
        {
            Observer = observer;
            Log = log;
            Deck = ServiceLocator.Instance.GetService<IDecksService>().GetFrom(location);
            Slot = location.Slot;
            Location = location;
            Result = new GetResult(Location);
        }

        public override GetResult Execute()
        {
            if (Validate())
                FetchDisk();
            return Result;
        }

        protected override void OnDispose()
        {
            Log = null;
        }

        private void FetchDisk()
        {
            if (Location.IsWide)
            {
                var num1 = (int)SettleDiskInSlot();
            }

            if (!Controller.TrackOpen().Success)
            {
                Result.Update(ErrorCodes.TrackOpenTimeout);
            }
            else
            {
                Controller.StartRollerOut();
                var newError = PullFrom(Location);
                if (newError != ErrorCodes.Success)
                {
                    Controller.StopRoller();
                    Controller.TrackClose();
                    Result.Update(newError);
                }
                else if (!Controller.TrackClose().Success)
                {
                    Controller.StopRoller();
                    var num2 = (int)PushIntoSlot();
                    Result.Update(ErrorCodes.TrackCloseTimeout);
                }
                else
                {
                    var sensorReadResult1 = Controller.ReadPickerSensors();
                    if (!sensorReadResult1.Success)
                    {
                        Result.Update(sensorReadResult1.Error);
                    }
                    else
                    {
                        if (sensorReadResult1.IsInputActive(PickerInputs.Sensor1) &&
                            !sensorReadResult1.IsInputActive(PickerInputs.Sensor2))
                            PullFrom(Location, 1);
                        RuntimeService.SpinWait(300);
                        var service = ServiceLocator.Instance.GetService<IInventoryService>();
                        if (Controller.RollerToPosition(RollerPosition.Position4, 6000, false).Success)
                        {
                            service.Reset(Location);
                        }
                        else
                        {
                            var sensorReadResult2 = Controller.ReadPickerSensors();
                            if (!sensorReadResult2.Success)
                            {
                                Result.Update(ErrorCodes.SensorReadError);
                            }
                            else if (sensorReadResult2.IsFull)
                            {
                                LogHelper.Instance.WithContext(false, LogEntryType.Info,
                                    "[GET] Disk did not make it to sensor 4.");
                                sensorReadResult2.Log(LogEntryType.Error);
                                OnStuck();
                            }
                            else
                            {
                                LogHelper.Instance.WithContext(false, LogEntryType.Info,
                                    "[GET] no disk in picker after pull.");
                                sensorReadResult2.Log();
                                Result.Update(ErrorCodes.SlotEmpty);
                                if (!Observer.OnEmpty(Result))
                                    return;
                                service.Reset(Location);
                            }
                        }
                    }
                }
            }
        }

        private void OnStuck()
        {
            Controller.StartRollerIn();
            try
            {
                var flag = ClearDiskFromPicker();
                var num = (int)PushIntoSlot();
                LogHelper.Instance.WithContext(false, LogEntryType.Error,
                    "[GET] couldn't get disc, ClearDiscFromPicker returned {0}", flag ? "TIMEOUT" : (object)"SUCCESS");
                var sensorReadResult = Controller.ReadPickerSensors();
                sensorReadResult.Log(LogEntryType.Error);
                if (!sensorReadResult.Success)
                    Result.Update(ErrorCodes.SensorReadError);
                else
                    Result.Update(sensorReadResult.IsFull ? ErrorCodes.PickerObstructed : ErrorCodes.ItemStuck);
            }
            finally
            {
                Controller.StopRoller();
            }

            Observer.OnStuck(Result);
        }

        private bool Validate()
        {
            if (Controller.TrackState != TrackState.Closed && !Controller.TrackClose().Success)
            {
                Result.Update(ErrorCodes.TrackCloseTimeout);
                return false;
            }

            if (!Controller.ReadPickerSensors().IsFull)
                return true;
            Result.Update(ErrorCodes.PickerFull);
            return false;
        }
    }
}