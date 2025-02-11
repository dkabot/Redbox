using Redbox.Core;
using Redbox.UpdateManager.ComponentModel;
using System.IO;
using System.Linq;

namespace Redbox.UpdateManager.Kernel
{
    internal static class ScritpFunctions
    {
        [KernelFunction(Name = "kernel.markscriptcomplete")]
        internal static void SetScriptComplete() => KernelService.Instance.SetScriptComplete();

        [KernelFunction(Name = "kernel.runscript")]
        internal static object[] RunScript(string path)
        {
            return KernelService.Instance.LuaRuntime.DoString(File.ReadAllText(ServiceLocator.Instance.GetService<IMacroService>().ExpandProperties(path))).ToArray<object>();
        }
    }
}
