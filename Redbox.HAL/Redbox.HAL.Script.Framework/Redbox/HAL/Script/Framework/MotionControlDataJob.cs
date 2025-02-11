using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework
{
    [NativeJob(ProgramName = "motion-control-get-data")]
    internal sealed class MotionControlDataJob : NativeJobAdapter
    {
        internal MotionControlDataJob(ExecutionResult result, ExecutionContext ctx)
            : base(result, ctx)
        {
        }

        protected override void ExecuteInner()
        {
            var service = ServiceLocator.Instance.GetService<IMotionControlService>();
            var controllerPosition = service.ReadPositions();
            ErrorCodes errorCodes;
            int index;
            if (!controllerPosition.ReadOk)
            {
                var context = Context;
                errorCodes = ErrorCodes.ArcusNotResponsive;
                var message = errorCodes.ToString();
                context.CreateInfoResult("PositionReadError", message);
            }
            else
            {
                if (controllerPosition.XCoordinate.HasValue)
                {
                    var context = Context;
                    index = controllerPosition.XCoordinate.Value;
                    var message = index.ToString();
                    context.CreateInfoResult("PositionX", message);
                }

                if (controllerPosition.YCoordinate.HasValue)
                {
                    var context = Context;
                    index = controllerPosition.YCoordinate.Value;
                    var message = index.ToString();
                    context.CreateInfoResult("PositionY", message);
                }
            }

            var controlLimitResponse = service.ReadLimits();
            if (controlLimitResponse.ReadOk)
            {
                var limits = controlLimitResponse.Limits;
                for (index = 0; index < limits.Length; ++index)
                {
                    var motionControlLimit = limits[index];
                    Context.CreateInfoResult(motionControlLimit.Limit.ToString(),
                        motionControlLimit.Blocked ? "BLOCKED" : "CLEAR");
                }
            }
            else
            {
                var context = Context;
                errorCodes = ErrorCodes.ArcusNotResponsive;
                var message = errorCodes.ToString();
                context.CreateInfoResult("LimitReadError", message);
            }

            if (service.AtVendDoor)
                Context.CreateInfoResult("CurrentLocation", "VENDDOOR");
            else if (service.CurrentLocation == null)
                Context.CreateInfoResult("CurrentLocation", "UNKNOWN");
            else
                Context.CreateInfoResult("CurrentLocation",
                    string.Format("Deck = {0}, Slot = {1}", service.CurrentLocation.Deck,
                        service.CurrentLocation.Slot));
        }
    }
}