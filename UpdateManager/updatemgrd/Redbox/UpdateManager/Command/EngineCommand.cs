using Redbox.Core;
using Redbox.IPC.Framework;
using Redbox.UpdateManager.ComponentModel;
using Redbox.UpdateManager.Domain;
using System.Collections.Generic;
using System.IO;

namespace Redbox.UpdateManager.Command
{
    [Redbox.IPC.Framework.Command("engine")]
    internal class EngineCommand
    {
        [CommandForm(Name = "export-queue")]
        [Usage("engine export-queue path:")]
        public void ExportQueue(CommandContext context, [CommandKeyValue(IsRequired = true)] string path)
        {
            if (!Directory.Exists(Path.GetDirectoryName(path)))
            {
                context.Errors.Add(Redbox.IPC.Framework.Error.NewError("D999", string.Format("{0} does not exists.", (object)Path.GetDirectoryName(path)), "Supply a valid directory and try again."));
            }
            else
            {
                IKioskEngineService service = ServiceLocator.Instance.GetService<IKioskEngineService>();
                context.Errors.AddRange((IEnumerable<Redbox.IPC.Framework.Error>)service.ExportQueue(path).ToIPCErrors());
            }
        }

        [CommandForm(Name = "export-sessions")]
        [Usage("engine export-sessions path:")]
        public void ExportSession(CommandContext context, [CommandKeyValue(IsRequired = true)] string path)
        {
            if (!Directory.Exists(Path.GetDirectoryName(path)))
            {
                context.Errors.Add(Redbox.IPC.Framework.Error.NewError("D999", string.Format("{0} does not exists.", (object)Path.GetDirectoryName(path)), "Supply a valid directory and try again."));
            }
            else
            {
                IKioskEngineService service = ServiceLocator.Instance.GetService<IKioskEngineService>();
                context.Errors.AddRange((IEnumerable<Redbox.IPC.Framework.Error>)service.ExportSessions(path).ToIPCErrors());
            }
        }

        [CommandForm(Name = "load-bundle")]
        [Usage("engine load-bundle bundle:")]
        public void LoadBundle(CommandContext context, [CommandKeyValue(IsRequired = true)] string bundle)
        {
            IKioskEngineService service = ServiceLocator.Instance.GetService<IKioskEngineService>();
            context.Errors.AddRange((IEnumerable<Redbox.IPC.Framework.Error>)service.ChangeBundle(bundle).ToIPCErrors());
        }

        [CommandForm(Name = "show-offline-screen")]
        [Usage("engine show-offline-screen")]
        public void ShowOfflineScreen(CommandContext context)
        {
            IKioskEngineService service = ServiceLocator.Instance.GetService<IKioskEngineService>();
            context.Errors.AddRange((IEnumerable<Redbox.IPC.Framework.Error>)service.ShowOfflineScreen().ToIPCErrors());
        }

        [CommandForm(Name = "reset-bundle")]
        [Usage("engine reset-bundle")]
        public void ResetBundle(CommandContext context)
        {
            IKioskEngineService service = ServiceLocator.Instance.GetService<IKioskEngineService>();
            context.Errors.AddRange((IEnumerable<Redbox.IPC.Framework.Error>)service.ReloadBundle().ToIPCErrors());
        }

        [CommandForm(Name = "bring-to-front")]
        [Usage("engine bring-to-front")]
        public void BringToFront(CommandContext context)
        {
            IKioskEngineService service = ServiceLocator.Instance.GetService<IKioskEngineService>();
            context.Errors.AddRange((IEnumerable<Redbox.IPC.Framework.Error>)service.BringToFront().ToIPCErrors());
        }

        [CommandForm(Name = "shutdown")]
        [Usage("engine shutdown")]
        public void Shutdown(CommandContext context)
        {
            IKioskEngineService service = ServiceLocator.Instance.GetService<IKioskEngineService>();
            bool isRunning;
            service.IsEngineRunning(out isRunning);
            if (!isRunning)
                return;
            context.Errors.AddRange((IEnumerable<Redbox.IPC.Framework.Error>)service.Shutdown().ToIPCErrors());
        }

        [CommandForm(Name = "start")]
        [Usage("engine start")]
        public void Start(CommandContext context, string bundle)
        {
            LogHelper.Instance.Log("Starting engine at location: {0}", (object)UpdateManagerService.Instance.KioskEnginePath);
            IKioskEngineService service = ServiceLocator.Instance.GetService<IKioskEngineService>();
            if (string.IsNullOrEmpty(bundle))
                context.Errors.AddRange((IEnumerable<Redbox.IPC.Framework.Error>)service.StartEngine().ToIPCErrors());
            else
                context.Errors.AddRange((IEnumerable<Redbox.IPC.Framework.Error>)service.StartEngine(bundle).ToIPCErrors());
        }

        [CommandForm(Name = "execute-script")]
        [Usage("engine execute-script path:")]
        public void ExecuteScript(CommandContext context, [CommandKeyValue(IsRequired = true)] string path)
        {
            IKioskEngineService service = ServiceLocator.Instance.GetService<IKioskEngineService>();
            context.Errors.AddRange((IEnumerable<Redbox.IPC.Framework.Error>)service.ExecuteScript(path).ToIPCErrors());
        }
    }
}
