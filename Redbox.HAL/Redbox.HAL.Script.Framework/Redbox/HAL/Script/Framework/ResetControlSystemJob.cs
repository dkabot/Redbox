using System.Threading;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework
{
    [NativeJob(ProgramName = "reset-controlsystem-job")]
    internal class ResetControlSystemJob : NativeJobAdapter
    {
        internal ResetControlSystemJob(ExecutionResult result, ExecutionContext ctx)
            : base(result, ctx)
        {
        }

        protected override void ExecuteInner()
        {
            var service = ServiceLocator.Instance.GetService<IControlSystem>();
            service.Shutdown();
            Thread.Sleep(500);
            var controlResponse = service.Initialize();
            Context.CreateInfoResult("ResetStatus", string.Format("RESET returned {0}", controlResponse));
            if (controlResponse.Success)
                return;
            AddError("RESET failure.");
        }
    }
}