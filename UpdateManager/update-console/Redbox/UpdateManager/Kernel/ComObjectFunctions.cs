using Redbox.Core;
using Redbox.UpdateManager.ComponentModel;
using System.Diagnostics;

namespace Redbox.UpdateManager.Kernel
{
    internal static class ComObjectFunctions
    {
        [KernelFunction(Name = "kernel.ismoduleselfregistering")]
        internal static bool IsModuleSelfRegistering(string fileName)
        {
            IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
            return service != null && SelfRegistrationTool.IsModuleSelfRegistering(service.ExpandProperties(fileName));
        }

        [KernelFunction(Name = "kernel.registercomobject")]
        internal static bool RegisterComObject(string fileName)
        {
            string fileName1 = ServiceLocator.Instance.GetService<IMacroService>().ExpandProperties(fileName);
            if (SelfRegistrationTool.IsModuleSelfRegistering(fileName1))
            {
                SelfRegistrationTool.Register(fileName1);
                return true;
            }
            Process process = new Process();
            process.StartInfo = new ProcessStartInfo()
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                FileName = "regsvr32.exe",
                WindowStyle = ProcessWindowStyle.Hidden,
                Arguments = string.Format("/s {0}", (object)fileName1)
            };
            process.Start();
            process.WaitForExit();
            return process.ExitCode == 0;
        }

        [KernelFunction(Name = "kernel.unregistercomobject")]
        internal static bool UnregisterComObject(string fileName)
        {
            string fileName1 = ServiceLocator.Instance.GetService<IMacroService>().ExpandProperties(fileName);
            if (SelfRegistrationTool.IsModuleSelfRegistering(fileName1))
            {
                SelfRegistrationTool.Unregister(fileName1);
                return true;
            }
            Process process = new Process();
            process.StartInfo = new ProcessStartInfo()
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                FileName = "regsvr32.exe",
                WindowStyle = ProcessWindowStyle.Hidden,
                Arguments = string.Format("/s /u {0}", (object)fileName1)
            };
            process.Start();
            process.WaitForExit();
            return process.ExitCode == 0;
        }
    }
}
