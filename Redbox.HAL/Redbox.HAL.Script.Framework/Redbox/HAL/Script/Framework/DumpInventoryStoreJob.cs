using System.IO;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework
{
    [NativeJob(ProgramName = "dump-inventory-store-job")]
    internal sealed class DumpInventoryStoreJob : NativeJobAdapter
    {
        internal DumpInventoryStoreJob(ExecutionResult result, ExecutionContext ctx)
            : base(result, ctx)
        {
        }

        protected override void ExecuteInner()
        {
            using (var writer = new StreamWriter(Path.Combine("c:\\Program Files\\Redbox\\KioskLogs\\Service",
                       string.Format("StoreDump-{0}",
                           ServiceLocator.Instance.GetService<IRuntimeService>().GenerateUniqueFile("txt")))))
            {
                ServiceLocator.Instance.GetService<IInventoryService>().DumpStore(writer);
                ServiceLocator.Instance.GetService<IDumpbinService>().DumpContents(writer);
                writer.WriteLine();
            }
        }
    }
}