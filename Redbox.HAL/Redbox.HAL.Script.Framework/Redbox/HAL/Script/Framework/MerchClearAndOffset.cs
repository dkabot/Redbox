using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework
{
    [NativeJob(ProgramName = "merch-clear-and-offset")]
    internal sealed class MerchClearAndOffset : NativeJobAdapter
    {
        internal MerchClearAndOffset(ExecutionResult r, ExecutionContext c)
            : base(r, c)
        {
        }

        protected override void ExecuteInner()
        {
            var deck = Context.PopTop<int>();
            var slot = Context.PopTop<int>();
            var service = ServiceLocator.Instance.GetService<IInventoryService>();
            var location = service.Get(deck, slot);
            var flag = service.Reset(location);
            Context.CreateInfoResult(flag ? "ResetSuccess" : "ResetFailure", "The location was {0} reset.",
                flag ? "successfully" : "unsuccessfully");
            ShowEmptyStuck(location);
        }
    }
}