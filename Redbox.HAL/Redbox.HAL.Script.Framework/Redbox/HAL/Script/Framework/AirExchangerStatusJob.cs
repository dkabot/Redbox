using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework
{
    [NativeJob(ProgramName = "air-exchanger-status", Operand = "AIR-EXCHANGER-STATUS")]
    internal sealed class AirExchangerStatusJob : NativeJobAdapter
    {
        internal AirExchangerStatusJob(ExecutionResult result, ExecutionContext ctx)
            : base(result, ctx)
        {
        }

        protected override void ExecuteInner()
        {
            var service = ServiceLocator.Instance.GetService<IAirExchangerService>();
            if (!service.Configured)
            {
                Context.CreateInfoResult("AirExchangerNotConfigured", "There is no air exchanger configured.");
            }
            else
            {
                Context.CreateInfoResult("AirExchangerConfigured", "There is an air exchanger configured.");
                if (!service.ShouldRetry())
                    Context.CreateInfoResult("PersistentErrorStatus", "The board is in an persistent error state.");
                else
                    Context.CreateInfoResult("BoardStatusOk", "The board is not in a persistent error state.");
            }
        }
    }
}