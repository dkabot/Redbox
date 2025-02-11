using Redbox.Core;
using Redbox.Lua;
using Redbox.UpdateManager.ComponentModel;
using System.IO;

namespace Redbox.UpdateManager.Kernel
{
    internal static class InputFunctions
    {
        [KernelFunction(Name = "kernel.inputyesno")]
        internal static string AskYesNoQuestion(string title, string message)
        {
            IInputService service = ServiceLocator.Instance.GetService<IInputService>();
            return service != null ? service.AskYesNoQuestion(title, message).ToString("G").ToLower() : (string)null;
        }

        [KernelFunction(Name = "kernel.inputfindfile")]
        internal static LuaTable FindFile()
        {
            IInputService service = ServiceLocator.Instance.GetService<IInputService>();
            if (service == null)
                return (LuaTable)null;
            FileInfo fileInfo = service.LocateFile();
            if (fileInfo == null)
                return (LuaTable)null;
            return new LuaTable(KernelService.Instance.LuaRuntime)
            {
                [(object)"name"] = (object)fileInfo.Name,
                [(object)"length"] = (object)fileInfo.Length,
                [(object)"directory"] = (object)fileInfo.DirectoryName
            };
        }

        [KernelFunction(Name = "kernel.notifyinfo")]
        internal static void NotifyInfo(string title, string message)
        {
            ServiceLocator.Instance.GetService<IInputService>().NotifyInfo(title, message);
        }
    }
}
