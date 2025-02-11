using Redbox.Core;
using Redbox.Lua;
using Redbox.UpdateManager.ComponentModel;
using System.Collections.Generic;

namespace Redbox.UpdateManager.Kernel
{
    internal static class EventNotificationFunctions
    {
        [KernelFunction(Name = "kernel.notifyaddevent")]
        internal static LuaTable AddEvent(string name, string description)
        {
            IEventNotifyService service = ServiceLocator.Instance.GetService<IEventNotifyService>();
            return service != null ? EventNotificationFunctions.CreateResultTable(service.AddEvent(name, description)) : (LuaTable)null;
        }

        [KernelFunction(Name = "kernel.notifyeventstart")]
        internal static LuaTable EventStart(string name)
        {
            IEventNotifyService service = ServiceLocator.Instance.GetService<IEventNotifyService>();
            return service != null ? EventNotificationFunctions.CreateResultTable(service.EventStart(name)) : (LuaTable)null;
        }

        [KernelFunction(Name = "kernel.notifyeventerror")]
        internal static LuaTable EventError(
          string name,
          string code,
          string description,
          string details)
        {
            IEventNotifyService service = ServiceLocator.Instance.GetService<IEventNotifyService>();
            return service != null ? EventNotificationFunctions.CreateResultTable(service.EventErrored(name, code, description, details)) : (LuaTable)null;
        }

        [KernelFunction(Name = "kernel.notifyeventcomplete")]
        internal static LuaTable EventComplete(string name)
        {
            IEventNotifyService service = ServiceLocator.Instance.GetService<IEventNotifyService>();
            return service != null ? EventNotificationFunctions.CreateResultTable(service.EventComplete(name)) : (LuaTable)null;
        }

        [KernelFunction(Name = "kernel.notifyexit")]
        internal static LuaTable Exit()
        {
            IEventNotifyService service = ServiceLocator.Instance.GetService<IEventNotifyService>();
            return service != null ? EventNotificationFunctions.CreateResultTable(service.Exit()) : (LuaTable)null;
        }

        private static LuaTable CreateResultTable(ErrorList errors)
        {
            LuaTable resultTable = new LuaTable(KernelService.Instance.LuaRuntime);
            resultTable[(object)"success"] = (object)!errors.ContainsError();
            LuaTable luaTable = new LuaTable(KernelService.Instance.LuaRuntime);
            foreach (Redbox.UpdateManager.ComponentModel.Error error in (List<Redbox.UpdateManager.ComponentModel.Error>)errors)
                luaTable[(object)error.Code] = (object)error.Description;
            resultTable[(object)nameof(errors)] = (object)luaTable;
            return resultTable;
        }
    }
}
