using System;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework
{
    [NativeJob(ProgramName = "reset-touchscreen-controller")]
    internal sealed class ResetTouchscreenControllerJob : NativeJobAdapter
    {
        internal ResetTouchscreenControllerJob(ExecutionResult result, ExecutionContext ctx)
            : base(result, ctx)
        {
        }

        protected override void ExecuteInner()
        {
            var applicationLog = ApplicationLog.ConfigureLog(Context, true, "Service", false, null);
            var service = ServiceLocator.Instance.GetService<IUsbDeviceService>();
            applicationLog.WriteFormatted("Request to reset touchscreen controller");
            var touchScreen = service.FindTouchScreen(false);
            var correctionOk = false;
            if (touchScreen != null)
            {
                try
                {
                    correctionOk = touchScreen.HardReset();
                }
                catch (Exception ex)
                {
                    LogHelper.Instance.Log("Reset touchscreen caught an exception", ex);
                    correctionOk = false;
                }

                applicationLog.WriteFormatted(" Touchscreen found; reset returned {0}", correctionOk);
            }
            else
            {
                applicationLog.WriteFormatted("Unable to locate a touchscreen.");
            }

            ServiceLocator.Instance.GetService<IHardwareCorrectionStatisticService>()
                .Insert(HardwareCorrectionStatistic.Touchscreen, Context, correctionOk);
            if (correctionOk)
                return;
            AddError("Touchscreen rest failure.");
        }
    }
}