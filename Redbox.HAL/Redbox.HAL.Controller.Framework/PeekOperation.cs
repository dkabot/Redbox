using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework
{
    public sealed class PeekOperation : AbstractOperation<IPeekResult>
    {
        public override IPeekResult Execute()
        {
            var peekResult = OnPeek();
            var printableLocation = ServiceLocator.Instance.GetService<IMotionControlService>().GetPrintableLocation();
            if (!peekResult.TestOk)
                LogHelper.Instance.WithContext(true, LogEntryType.Error, "Peek {0} returned error status {1}",
                    printableLocation, peekResult.Error.ToString().ToUpper());
            else
                LogHelper.Instance.WithContext(false, LogEntryType.Info, "Peek {0} returned status {1}",
                    printableLocation, peekResult.IsFull ? "FULL" : (object)"EMPTY");
            return peekResult;
        }

        private IPeekResult OnPeek()
        {
            var currentLocation = ServiceLocator.Instance.GetService<IMotionControlService>().CurrentLocation;
            var locationTestResult = new LocationTestResult(currentLocation.Deck, currentLocation.Slot);
            if (currentLocation.IsWide)
            {
                var errorCodes = SettleDiskInSlot();
                if (errorCodes != ErrorCodes.Success)
                {
                    locationTestResult.Error = errorCodes;
                    return locationTestResult;
                }
            }

            if (!Controller.SetFinger(GripperFingerState.Closed).Success)
            {
                locationTestResult.Error = ErrorCodes.GripperCloseTimeout;
                return locationTestResult;
            }

            var controlResponse = Controller.ExtendArm(ControllerConfiguration.Instance.TestExtendTime);
            if (controlResponse.CommError)
            {
                locationTestResult.Error = ErrorCodes.CommunicationError;
                return locationTestResult;
            }

            locationTestResult.IsFull = controlResponse.TimedOut;
            locationTestResult.Error = ErrorCodes.Success;
            Controller.RetractArm();
            Controller.SetFinger(GripperFingerState.Rent);
            return locationTestResult;
        }
    }
}