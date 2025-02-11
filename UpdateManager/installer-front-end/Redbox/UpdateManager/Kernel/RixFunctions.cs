using Microsoft.Win32;
using Redbox.Core;
using Redbox.Lua;
using Redbox.UpdateManager.ComponentModel;
using System.Collections;
using System.Diagnostics;
using System.IO;

namespace Redbox.UpdateManager.Kernel
{
    internal static class RixFunctions
    {
        [KernelFunction(Name = "kernel.rixinstall")]
        internal static int InstallPackage(string path, LuaTable parameters)
        {
            string arguments = "--force --silent";
            foreach (object key in (IEnumerable)parameters.Keys)
                arguments += string.Format(" -D:{0}=\"{1}\"", key, parameters[key]);
            IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
            Process process = new Process();
            process.StartInfo = new ProcessStartInfo(service.ExpandProperties(path), arguments)
            {
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                WorkingDirectory = path
            };
            process.Start();
            process.WaitForExit();
            return process.ExitCode;
        }

        [KernelFunction(Name = "kernel.rixuninstall")]
        internal static int UninstallPackage(string path, LuaTable parameters)
        {
            string arguments = "--force --silent";
            foreach (object key in (IEnumerable)parameters.Keys)
                arguments += string.Format(" -D:{0}=\"{1}\"", key, parameters[key]);
            IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
            Process process = new Process();
            process.StartInfo = new ProcessStartInfo(service.ExpandProperties(path), arguments)
            {
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                WorkingDirectory = path
            };
            process.Start();
            process.WaitForExit();
            return process.ExitCode;
        }

        [KernelFunction(Name = "kernel.rixisproductinstalled")]
        internal static bool IsRixPackageInstalled(string productGuid)
        {
            return !string.IsNullOrEmpty(productGuid) && !string.IsNullOrEmpty((string)Registry.GetValue(string.Format("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\{0}", (object)productGuid), "SilentUninstallString", (object)null));
        }

        [KernelFunction(Name = "kernel.rixfindinstalldirectory")]
        internal static string FindInstalledDirectory(string productGuid)
        {
            if (string.IsNullOrEmpty(productGuid))
                return (string)null;
            string str = (string)Registry.GetValue(string.Format("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\{0}", (object)productGuid), "InstallPath", (object)null);
            return !string.IsNullOrEmpty(str) ? str : (string)null;
        }

        [KernelFunction(Name = "kernel.rixuninstallproduct")]
        internal static int UninstallPackage(string productGuid)
        {
            if (string.IsNullOrEmpty(productGuid))
                return -1;
            string str1 = (string)Registry.GetValue(string.Format("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\{0}", (object)productGuid), "SilentUninstallString", (object)null);
            if (string.IsNullOrEmpty(str1))
                return -1;
            int num = str1.IndexOf(".exe");
            string path = str1.Substring(0, num + 5);
            string str2 = str1.Substring(num + 5);
            if (!File.Exists(path))
            {
                LogHelper.Instance.Log("In rixuninstallproduct, {0} does not exists from product {1}.", (object)path, (object)productGuid);
                return -1;
            }
            Process process = new Process();
            process.StartInfo = new ProcessStartInfo()
            {
                FileName = path,
                Arguments = str2,
                CreateNoWindow = true,
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden
            };
            process.Start();
            process.WaitForExit();
            return process.ExitCode;
        }
    }
}
