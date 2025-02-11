using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework
{
    [NativeJob(ProgramName = "load-kiosk-function-check-data", Operand = "LOAD-KIOSK-FUNCTION-CHECK-DATA")]
    internal sealed class KioskFunctionTestDataLoadJob : NativeJobAdapter
    {
        internal KioskFunctionTestDataLoadJob(ExecutionResult result, ExecutionContext ctx)
            : base(result, ctx)
        {
        }

        protected override void ExecuteInner()
        {
            var functionCheckDataList = ServiceLocator.Instance.GetService<IKioskFunctionCheckService>().Load();
            foreach (var functionCheckData in functionCheckDataList)
            {
                Context.PushTop(functionCheckData.UserIdentifier);
                Context.PushTop(functionCheckData.Timestamp.ToString());
                Context.PushTop(functionCheckData.TouchscreenDriverTestResult);
                Context.PushTop(functionCheckData.CameraDriverTestResult);
                Context.PushTop(functionCheckData.SnapDecodeTestResult);
                Context.PushTop(functionCheckData.TrackTestResult);
                Context.PushTop(functionCheckData.VendDoorTestResult);
                Context.PushTop(functionCheckData.InitTestResult);
                Context.PushTop(functionCheckData.VerticalSlotTestResult);
            }

            Context.PushTop(functionCheckDataList.Count);
        }
    }
}