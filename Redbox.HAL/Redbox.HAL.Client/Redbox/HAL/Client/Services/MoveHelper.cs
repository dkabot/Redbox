using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Extensions;

namespace Redbox.HAL.Client.Services
{
    public sealed class MoveHelper
    {
        private readonly HardwareService Service;
        private readonly int Timeout;

        public MoveHelper(HardwareService service, int timeout)
        {
            Service = service;
            Timeout = timeout;
        }

        public MoveHelper(HardwareService service)
            : this(service, 64000)
        {
        }

        public ErrorCodes MoveTo(int deck, int slot, string mode)
        {
            HardwareJob job;
            return !Service.ExecuteImmediate(string.Format("MOVE DECK={0} SLOT={1} MODE={2}", deck, slot, mode),
                Timeout, out job).Success
                ? ErrorCodes.ServiceChannelError
                : Enum<ErrorCodes>.ParseIgnoringCase(job.GetTopOfStack(), ErrorCodes.ServiceChannelError);
        }

        public ErrorCodes MoveTo(int deck, int slot)
        {
            return MoveTo(deck, slot, "NORMAL");
        }

        public IControllerPosition GetPosition()
        {
            using (var controlDataExecutor = new MotionControlDataExecutor(Service))
            {
                controlDataExecutor.Run();
                return controlDataExecutor.Position;
            }
        }

        public ErrorCodes MoveAbs(Axis axis, int units)
        {
            return MoveAbs(axis, units, true);
        }

        public ErrorCodes MoveAbs(Axis axis, int units, bool checkSensors)
        {
            HardwareJob job;
            return !Service
                .ExecuteImmediate(
                    string.Format("MOVEABS AXIS={0} UNITS={1} SENSOR-CHECK={2}", axis.ToString().ToUpper(),
                        units.ToString(), checkSensors.ToString().ToUpper()), Timeout, out job).Success
                ? ErrorCodes.ServiceChannelError
                : Enum<ErrorCodes>.ParseIgnoringCase(job.GetTopOfStack(), ErrorCodes.ServiceChannelError);
        }

        public IMotionControlLimitResponse ReadLimits()
        {
            using (var controlDataExecutor = new MotionControlDataExecutor(Service))
            {
                controlDataExecutor.Run();
                return controlDataExecutor.Limits;
            }
        }
    }
}