using System.IO;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework.Natives
{
    [NativeJob(ProgramName = "decode-image")]
    internal sealed class DecodeImageJob : NativeJobAdapter
    {
        internal DecodeImageJob(ExecutionResult result, ExecutionContext ctx)
            : base(result, ctx)
        {
        }

        protected override void ExecuteInner()
        {
            Context.ShouldCleanup = false;
            var path2 = Context.PopTop<string>();
            var service = ServiceLocator.Instance.GetService<IBarcodeReaderFactory>();
            var str = Path.Combine(service.ImagePath, path2);
            if (!File.Exists(str))
            {
                LogHelper.Instance.WithContext("The image '{0}' was not found", str);
                AddError("Image not found.");
            }
            else
            {
                new ScanResult(service.GetConfiguredReader().Scan(str)).PushTo(Context);
            }
        }
    }
}