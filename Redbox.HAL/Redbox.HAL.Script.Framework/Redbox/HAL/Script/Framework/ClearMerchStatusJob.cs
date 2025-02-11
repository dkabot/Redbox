using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework
{
    [NativeJob(ProgramName = "clear-merch-status", Operand = "CLEAR-MERCH-STATUS")]
    internal sealed class ClearMerchStatusJob : NativeJobAdapter
    {
        internal ClearMerchStatusJob(ExecutionResult result, ExecutionContext ctx)
            : base(result, ctx)
        {
        }

        protected override void ExecuteInner()
        {
            var num1 = 0;
            var service = ServiceLocator.Instance.GetService<IInventoryService>();
            try
            {
                if (!ControllerConfiguration.Instance.IsVMZMachine)
                    return;
                var num2 = Context.PopTop<int>();
                for (var index = 0; index < num2; ++index)
                {
                    var id = Context.PopTop<string>();
                    var location = service.Lookup(id);
                    if (location != null && SegmentManager.Instance.InCompressedZone(location) &&
                        !SegmentManager.Instance.IsNonThinType(location.Flags))
                    {
                        LogHelper.Instance.WithContext("The flags at {0} are changed from {1} -> None",
                            location.ToString(), location.Flags.ToString());
                        location.Flags = MerchFlags.None;
                        service.Save(location);
                        ++num1;
                    }
                }
            }
            finally
            {
                Context.CreateInfoResult("MerchFlagsResetCount", num1.ToString());
            }
        }
    }
}