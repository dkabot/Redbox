namespace Redbox.HAL.Script.Framework.Natives
{
    [NativeJob(ProgramName = "refurbish-kiosk-test", HideFromList = true)]
    internal sealed class KioskTestJob : NativeJobAdapter
    {
        internal KioskTestJob(ExecutionResult r, ExecutionContext c)
            : base(r, c)
        {
        }

        protected override void ExecuteInner()
        {
            using (var kioskTestAdapter = new KioskTestAdapter(Result, Context))
            {
                kioskTestAdapter.Run();
            }
        }
    }
}