using System.Threading;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework
{
    [NativeJob(ProgramName = "hardware-healer", Operand = "HARDWARE-HEALER")]
    internal class HardwareHealerJob : NativeJobAdapter
    {
        internal HardwareHealerJob(ExecutionResult result, ExecutionContext ctx)
            : base(result, ctx)
        {
        }

        protected override void ExecuteInner()
        {
            ApplicationLog.ConfigureLog(Context, true, "Service", false, null);
            if (ControllerConfiguration.Instance.EnableLeiweHealing && !ControlSystem.GetRevision().Success)
            {
                ControlSystem.Shutdown();
                Thread.Sleep(500);
                ControlSystem.Initialize();
                if (!ControlSystem.GetRevision().Success)
                    AddError("Failed to reset boards.");
            }

            if (ControllerConfiguration.Instance.EnableArcusHealing)
            {
                if (!MotionService.TestAndReset(false))
                {
                    AddError("Can't reset controller.");
                }
                else
                {
                    var num = (int)MotionService.MoveVend(MoveMode.Get, Context.AppLog);
                }
            }

            if (!ControllerConfiguration.Instance.EnableCameraHealing)
                return;
            var configuredDevice = ServiceLocator.Instance.GetService<IScannerDeviceService>().GetConfiguredDevice();
            using (var snapResult = configuredDevice.Snap())
            {
                if (snapResult.SnapOk)
                    return;
                LogHelper.Instance.WithContext("Camera snap returned failure; restart device returned {0}",
                    configuredDevice.Restart().ToString());
            }
        }
    }
}