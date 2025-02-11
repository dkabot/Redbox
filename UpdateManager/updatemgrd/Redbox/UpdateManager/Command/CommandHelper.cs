using System;
using System.Linq;

namespace Redbox.UpdateManager.Command
{
    internal static class CommandHelper
    {
        public static Redbox.IPC.Framework.ErrorList ToIPCErrors(this Redbox.UpdateManager.ComponentModel.ErrorList errors)
        {
            Redbox.IPC.Framework.ErrorList ipcErrors = new Redbox.IPC.Framework.ErrorList();
            ipcErrors.AddRange(errors.Select<Redbox.UpdateManager.ComponentModel.Error, Redbox.IPC.Framework.Error>((Func<Redbox.UpdateManager.ComponentModel.Error, Redbox.IPC.Framework.Error>)(each => !each.IsWarning ? Redbox.IPC.Framework.Error.NewError(each.Code, each.Description, each.Details) : Redbox.IPC.Framework.Error.NewWarning(each.Code, each.Description, each.Details))));
            return ipcErrors;
        }
    }
}
