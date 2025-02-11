using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework.Natives
{
    [NativeJob(ProgramName = "test-and-reset-ccr")]
    internal sealed class TestAndResetCCRJob : NativeJobAdapter
    {
        private const string CCRCode = "CCRStatus";

        internal TestAndResetCCRJob(ExecutionResult result, ExecutionContext ctx)
            : base(result, ctx)
        {
        }

        protected override void ExecuteInner()
        {
            var applicationLog = ApplicationLog.ConfigureLog(Context, true, "Service", true, null);
            var service = ServiceLocator.Instance.GetService<IUsbDeviceService>();
            var ccr = service.FindCCR();
            if (!ccr.Found)
            {
                applicationLog.WriteFormatted("Unable to find CCR device.");
                Context.CreateInfoResult("CCRStatus", "Unable to locate CCR device");
                AddError("No CCR");
            }
            else if (ccr.Running)
            {
                Context.CreateInfoResult("CCRStatus", "The CCR appears to be working.");
            }
            else
            {
                applicationLog.WriteFormatted("Found device {0}", ccr);
                if (ccr.IsNotStarted)
                    applicationLog.WriteFormatted(" Device not started; RESET returned {0}",
                        ccr.Descriptor.ResetDriver());
                else if (ccr.IsDisabled)
                    applicationLog.WriteFormatted(" Device not enabled; change to enable returned {0}",
                        service.ChangeByHWID(ccr.Descriptor, DeviceState.Enable));
                var deviceStatus = service.FindDeviceStatus(ccr.Descriptor);
                applicationLog.WriteFormatted(" Device status AFTER fix = {0}", deviceStatus);
                var args = new HardwareCorrectionEventArgs(HardwareCorrectionStatistic.CreditCardReaderReset);
                if ((deviceStatus & DeviceStatus.Found) != DeviceStatus.None &&
                    (deviceStatus & DeviceStatus.Enabled) != DeviceStatus.None)
                {
                    applicationLog.WriteFormatted("  ** Successfully reset the device **");
                    Context.CreateInfoResult("CCRStatus", "Successfully reset the CCR");
                    args.CorrectionOk = true;
                }
                else
                {
                    applicationLog.WriteFormatted("  !!Failed to reset the device!!");
                    Context.CreateInfoResult("CCRStatus", "Failed to reset the CCR.");
                    AddError("Failed to restart CCR");
                    args.CorrectionOk = false;
                }

                ServiceLocator.Instance.GetService<IHardwareCorrectionStatisticService>().Insert(args, Context);
            }
        }
    }
}