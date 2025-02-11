using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework.Natives
{
    [NativeJob(ProgramName = "qlm-kiosk-test", HideFromList = true)]
    internal sealed class QlmKioskTestJob : NativeJobAdapter
    {
        internal QlmKioskTestJob(ExecutionResult r, ExecutionContext c)
            : base(r, c)
        {
        }

        protected override void ExecuteInner()
        {
            using (var qlmTestAdapter = new QlmTestAdapter(Result, Context, Context.PopTop<bool>()))
            {
                qlmTestAdapter.Run();
            }
        }

        private class QlmTestAdapter : KioskTestAdapter
        {
            private readonly bool RunOp;

            internal QlmTestAdapter(ExecutionResult r, ExecutionContext c, bool runOp)
                : base(r, c)
            {
                RunOp = runOp;
            }

            protected override ErrorCodes OnUnitComplete()
            {
                var errorCodes = ErrorCodes.Success;
                if (RunOp)
                    switch (ControlSystem.GetQlmStatus())
                    {
                        case QlmStatus.Engaged:
                            errorCodes = ControlSystem.DisengageQlm(Context.AppLog);
                            break;
                        case QlmStatus.Disengaged:
                            errorCodes = ControlSystem.EngageQlm(Context.AppLog);
                            break;
                    }

                return errorCodes;
            }

            protected override bool OnVendDisk(string matrix)
            {
                var service = ServiceLocator.Instance.GetService<IRuntimeService>();
                if (!ControlSystem.VendDoorRent().Success)
                {
                    Context.CreateInfoResult("VendDoorRentError", "Failed to open the vend door.");
                    return false;
                }

                service.SpinWait(500);
                var controlResponse = ControlSystem.VendDoorClose();
                if (!controlResponse.Success)
                    Context.CreateInfoResult("VendDoorCloseError", "Failed to close the vend door.");
                return controlResponse.Success;
            }
        }
    }
}