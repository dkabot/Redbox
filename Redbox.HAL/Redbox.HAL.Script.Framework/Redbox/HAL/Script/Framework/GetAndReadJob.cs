namespace Redbox.HAL.Script.Framework
{
    [NativeJob(ProgramName = "get-and-read")]
    internal sealed class GetAndReadJob : NativeJobAdapter
    {
        internal GetAndReadJob(ExecutionResult result, ExecutionContext ctx)
            : base(result, ctx)
        {
        }

        protected override void ExecuteInner()
        {
            if (new GetAndCenterOperation(Context).Run())
                using (var scanner = new Scanner(ControlSystem))
                {
                    using (var scanResult = scanner.Read())
                    {
                        scanResult.PushTo(Context);
                    }
                }
            else
                AddError("get-and-read failed.");
        }
    }
}