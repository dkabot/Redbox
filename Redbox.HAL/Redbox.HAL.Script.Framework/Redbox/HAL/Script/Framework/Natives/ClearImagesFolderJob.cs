using System;
using System.IO;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework.Natives
{
    [NativeJob(ProgramName = "clear-images-folder")]
    internal sealed class ClearImagesFolderJob : NativeJobAdapter
    {
        internal ClearImagesFolderJob(ExecutionResult result, ExecutionContext ctx)
            : base(result, ctx)
        {
        }

        protected override void ExecuteInner()
        {
            var files = Directory.GetFiles(ServiceLocator.Instance.GetService<IBarcodeReaderFactory>().ImagePath);
            var errors = 0;
            var action = (Action<string>)(file =>
            {
                try
                {
                    File.Delete(file);
                }
                catch (Exception ex)
                {
                    LogHelper.Instance.WithContext("Failed to delete file '{0}'", file);
                    LogHelper.Instance.Log(ex.Message);
                    ++errors;
                }
            });
            Array.ForEach(files, action);
            if (errors <= 0)
                return;
            AddError("Failed to delete some images.");
        }
    }
}