using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework.Natives
{
    [NativeJob(ProgramName = "change-camera-configuration")]
    internal sealed class ChangeCameraConfigurationJob : NativeJobAdapter
    {
        internal ChangeCameraConfigurationJob(ExecutionResult result, ExecutionContext ctx)
            : base(result, ctx)
        {
        }

        protected override void ExecuteInner()
        {
            var scannerServices = Context.PopTop<ScannerServices>();
            ServiceLocator.Instance.GetService<IConfigurationService>().SetPropertyByName("CAMERA", "ScannerService",
                new object[1]
                {
                    scannerServices
                });
            if (ServiceLocator.Instance.GetService<IScannerDeviceService>().GetConfiguredDevice().Service ==
                scannerServices)
                return;
            AddError("Failed to toggle service.");
        }
    }
}