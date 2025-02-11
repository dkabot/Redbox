using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework
{
    [NativeJob(ProgramName = "loopy")]
    internal sealed class LoopyJob : NativeJobAdapter
    {
        internal LoopyJob(ExecutionResult result, ExecutionContext context)
            : base(result, context)
        {
        }

        protected override void ExecuteInner()
        {
            var service = ServiceLocator.Instance.GetService<IRuntimeService>();
            var num = Context.PopTop<int>();
            for (var index = 0; index < num; ++index)
            {
                Context.Send(string.Format("Message-{0}", index));
                service.Wait(1500);
            }
        }
    }
}