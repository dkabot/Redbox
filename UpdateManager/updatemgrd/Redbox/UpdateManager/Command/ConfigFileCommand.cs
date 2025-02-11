using Redbox.Core;
using Redbox.IPC.Framework;
using Redbox.UpdateManager.ComponentModel;
using System.Collections.Generic;

namespace Redbox.UpdateManager.Command
{
    [Redbox.IPC.Framework.Command("configfile")]
    internal class ConfigFileCommand
    {
        [CommandForm(Name = "activate")]
        [Usage("configfile activate configfileid:")]
        public void ActivateConfigFile(CommandContext context, [CommandKeyValue(IsRequired = true)] long configfileid)
        {
            Redbox.UpdateManager.ComponentModel.ErrorList errors = ServiceLocator.Instance.GetService<IConfigFileService>().ActivateConfigFile(configfileid);
            if (!errors.ContainsError())
                return;
            context.Errors.AddRange((IEnumerable<Redbox.IPC.Framework.Error>)errors.ToIPCErrors());
        }
    }
}
