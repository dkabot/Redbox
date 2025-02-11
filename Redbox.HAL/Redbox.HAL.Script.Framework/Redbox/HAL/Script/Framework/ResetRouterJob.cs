using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework
{
    [NativeJob(ProgramName = "power-cycle-router")]
    internal sealed class ResetRouterJob : NativeJobAdapter
    {
        internal ResetRouterJob(ExecutionResult result, ExecutionContext ctx)
            : base(result, ctx)
        {
        }

        protected override void ExecuteInner()
        {
            var powerCycleDevice = ServiceLocator.Instance.GetService<IPowerCycleDeviceService>()
                .Get(PowerCycleDevices.Router);
            if (!powerCycleDevice.Configured)
            {
                AddError("Router relay is not configured.");
            }
            else
            {
                var errorCodes = powerCycleDevice.CyclePower();
                LogHelper.Instance.Log("[ResetRouterJob] CyclePower returned {0}", errorCodes);
                ServiceLocator.Instance.GetService<IHardwareCorrectionStatisticService>()
                    .Insert(HardwareCorrectionStatistic.RouterRecycle, Context, errorCodes == ErrorCodes.Success);
                if (errorCodes == ErrorCodes.Success)
                    return;
                AddError("Power cycle failed.");
                ServiceLocator.Instance.GetService<IControlSystemService>().Restart();
            }
        }
    }
}