namespace Redbox.HAL.Script.Framework
{
    [NativeJob(ProgramName = "get-and-center")]
    internal sealed class GetAndCenterJob : NativeJobAdapter
    {
        internal GetAndCenterJob(ExecutionResult result, ExecutionContext ctx)
            : base(result, ctx)
        {
        }

        protected override void ExecuteInner()
        {
            if (new GetAndCenterOperation(Context).Run())
                return;
            AddError("get-and-center failed.");
        }
    }
}