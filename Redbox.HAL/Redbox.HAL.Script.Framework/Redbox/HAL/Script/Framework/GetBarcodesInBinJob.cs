using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework
{
    [NativeJob(ProgramName = "get-barcodes-in-bin", Operand = "GET-BARCODES-IN-BIN")]
    internal sealed class GetBarcodesInBinJob : NativeJobAdapter
    {
        internal GetBarcodesInBinJob(ExecutionResult result, ExecutionContext context)
            : base(result, context)
        {
        }

        protected override void ExecuteInner()
        {
            var barcodesInBin = ServiceLocator.Instance.GetService<IDumpbinService>().GetBarcodesInBin();
            using (new DisposeableList<IDumpBinInventoryItem>(barcodesInBin))
            {
                foreach (var binInventoryItem in barcodesInBin)
                    Context.CreateInfoResult("DumpBinInventoryItem", "The item is in the dump bin.",
                        binInventoryItem.ID);
            }
        }
    }
}