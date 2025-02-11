using Redbox.Core;
using Redbox.IPC.Framework;
using Redbox.UpdateManager.ComponentModel;
using Redbox.UpdateManager.Environment;
using Redbox.UpdateService.Model;
using System;
using System.Collections.Generic;

namespace Redbox.UpdateManager.Command
{
    [Redbox.IPC.Framework.Command("client")]
    internal class ClientCommands
    {
        [CommandForm(Name = "poll")]
        [Usage("client poll")]
        public void Poll(CommandContext context)
        {
            IUpdateService service = ServiceLocator.Instance.GetService<IUpdateService>();
            if (service == null)
                return;
            context.Errors.AddRange((IEnumerable<Redbox.IPC.Framework.Error>)service.Poll().ToIPCErrors());
        }

        [CommandForm(Name = "serverpoll")]
        [Usage("client serverpoll")]
        public void ServerPoll(CommandContext context)
        {
            IUpdateService service = ServiceLocator.Instance.GetService<IUpdateService>();
            if (service == null)
                return;
            context.Errors.AddRange((IEnumerable<Redbox.IPC.Framework.Error>)service.ServerPoll().ToIPCErrors());
        }

        [CommandForm(Name = "execute-script")]
        [Usage("client execute-script script: [reset:]")]
        public void ExecuteScript(CommandContext context, [CommandKeyValue(IsRequired = true)] string script, [CommandKeyValue(IsRequired = true)] bool? reset)
        {
            IUpdateService service = ServiceLocator.Instance.GetService<IUpdateService>();
            if (service == null)
                return;
            if (string.IsNullOrEmpty(script))
            {
                context.Errors.Add(Redbox.IPC.Framework.Error.NewError("CC999", "Client Command execute-script requires the script parameter.", "Add a script parameter and retry."));
            }
            else
            {
                string result;
                Redbox.UpdateManager.ComponentModel.ErrorList errors = service.ExecuteScript(script, out result, ((reset ?? false) ? 1 : 0) != 0);
                if (errors.ContainsError())
                    context.Errors.AddRange((IEnumerable<Redbox.IPC.Framework.Error>)errors.ToIPCErrors());
                else
                    context.Messages.Add(result);
            }
        }

        [CommandForm(Name = "execute-scriptfile")]
        [Usage("client execute-scriptfile file: [reset:]")]
        public void ExecuteScriptFile(CommandContext context, [CommandKeyValue(IsRequired = true)] string file, [CommandKeyValue(IsRequired = true)] bool? reset)
        {
            IUpdateService service = ServiceLocator.Instance.GetService<IUpdateService>();
            if (service == null)
                return;
            if (string.IsNullOrEmpty(file))
            {
                context.Errors.Add(Redbox.IPC.Framework.Error.NewError("CC998", "Client Command execute-script requires the file path parameter.", "Add a script path parameter and retry."));
            }
            else
            {
                string result;
                Redbox.UpdateManager.ComponentModel.ErrorList errors = service.ExecuteScriptFile(file, out result, ((reset ?? false) ? 1 : 0) != 0);
                if (errors.ContainsError())
                    context.Errors.AddRange((IEnumerable<Redbox.IPC.Framework.Error>)errors.ToIPCErrors());
                else
                    context.Messages.Add(result);
            }
        }

        [CommandForm(Name = "force-data-files")]
        [Usage("client force-data-files")]
        public void ForceDataFiles(CommandContext context)
        {
            IUpdateService service = ServiceLocator.Instance.GetService<IUpdateService>();
            if (service == null)
                return;
            context.Errors.AddRange((IEnumerable<Redbox.IPC.Framework.Error>)service.ForcePublish("dvd-data").ToIPCErrors());
        }

        [CommandForm(Name = "get-dvd-data-state")]
        [Usage("client get-dvd-data-state")]
        public void GetDvdDataState(CommandContext context)
        {
            IUpdateService service = ServiceLocator.Instance.GetService<IUpdateService>();
            if (service == null)
                return;
            DateTime lastRun;
            SubscriptionState state;
            context.Errors.AddRange((IEnumerable<Redbox.IPC.Framework.Error>)service.GetSubscriptionState("dvd-data", out lastRun, out state).ToIPCErrors());
            if (context.Errors.ContainsError())
                return;
            context.Messages.Add(lastRun.ToJson());
            context.Messages.Add(state.ToJson());
        }

        [CommandForm(Name = "list-current-downloads")]
        [Usage("client list-current-downloads")]
        public void ListCurrentDownloads(CommandContext context)
        {
            ITransferService service = ServiceLocator.Instance.GetService<ITransferService>();
            if (service == null)
                return;
            List<ITransferJob> jobs;
            context.Errors.AddRange((IEnumerable<Redbox.IPC.Framework.Error>)service.GetJobs(out jobs, false).ToIPCErrors());
            if (context.Errors.ContainsError())
                return;
            context.Messages.Add(jobs.ToJson());
        }

        [CommandForm(Name = "send-status-message")]
        [Usage("client send-status-message type:(info, warning, error) key: description: data:")]
        public void SendStatusMessage(
          CommandContext context,
          [CommandKeyValue(IsRequired = true)] string type,
          [CommandKeyValue(IsRequired = true)] string key,
          [CommandKeyValue(IsRequired = true)] string description,
          [CommandKeyValue(IsRequired = true)] string data)
        {
            StatusMessage.StatusMessageType result;
            if (!Extensions.TryParse<StatusMessage.StatusMessageType>(type, out result, true))
            {
                result = StatusMessage.StatusMessageType.Info;
                context.Messages.Add("The type " + type + " is unparsable, defaulting to 'info'");
            }
            StatusMessageService.Instance.EnqueueMessage(result, key, description, data);
        }
    }
}
