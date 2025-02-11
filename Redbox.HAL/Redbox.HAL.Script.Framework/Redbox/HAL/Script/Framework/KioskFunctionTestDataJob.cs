using System;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework
{
    [NativeJob(ProgramName = "kiosk-function-test-data", Operand = "KIOSK-FUNCTION-TEST-DATA")]
    internal sealed class KioskFunctionTestDataJob : NativeJobAdapter
    {
        internal KioskFunctionTestDataJob(ExecutionResult result, ExecutionContext ctx)
            : base(result, ctx)
        {
        }

        protected override void ExecuteInner()
        {
            var verticalSlotTestResult = Context.PopTop<string>();
            var initResult = Context.PopTop<string>();
            var vendDoorTestResult = Context.PopTop<string>();
            var trackTestResult = Context.PopTop<string>();
            var snapDecodeTestResult = Context.PopTop<string>();
            var cameraDriverTestResult = Context.PopTop<string>();
            var touchscreenDriverTestResult = Context.PopTop<string>();
            var timestamp = DateTime.Parse(Context.PopTop<string>());
            var userIdentifier = Context.PopTop<string>();
            var data = ServiceLocator.Instance.GetService<ITableTypeFactory>().NewCheckData(verticalSlotTestResult,
                initResult, vendDoorTestResult, trackTestResult, snapDecodeTestResult, touchscreenDriverTestResult,
                cameraDriverTestResult, timestamp, userIdentifier);
            ServiceLocator.Instance.GetService<IKioskFunctionCheckService>().Add(data);
        }
    }
}