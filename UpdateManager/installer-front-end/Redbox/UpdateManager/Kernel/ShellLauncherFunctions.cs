using Redbox.Core;
using Redbox.UpdateManager.ComponentModel;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace Redbox.UpdateManager.Kernel
{
    [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
    internal static class ShellLauncherFunctions
    {
        private static readonly uint S_OK = 0;
        private static readonly string DefaultShellCommandLine = "/namespace:\\\\root\\StandardCimv2\\Embedded CLASS WESL_UserSetting CALL SetDefaultShell Shell=\"{0}\" DefaultAction=0";
        private static readonly string AdminShellCommandLine = "/namespace:\\\\root\\StandardCimv2\\Embedded CLASS WESL_UserSetting CALL SetCustomShell Sid=\"S-1-5-32-544\" Shell=\"{0}\" DefaultAction=0";
        private static readonly string ShellEnabledCommandLine = "/namespace:\\\\root\\StandardCimv2\\Embedded CLASS WESL_UserSetting CALL SetEnabled Enabled=true";

        [DllImport("slc.dll", CharSet = CharSet.Unicode)]
        private static extern uint SLGetWindowsInformationDWORD([MarshalAs(UnmanagedType.LPWStr)] string valueName, out int value);

        [KernelFunction(Name = "kernel.shelllauncherfeatureinstalled")]
        internal static bool ShellLauncherWindowsFeatureInstalled()
        {
            try
            {
                int num;
                uint informationDword = ShellLauncherFunctions.SLGetWindowsInformationDWORD("EmbeddedFeature-ShellLauncher-Enabled", out num);
                LogHelper.Instance.Log("ShellLauncherWindowsFeatureInstalled() - SLGetWindowsInformationDWORD returned: " + (object)informationDword);
                if ((int)informationDword != (int)ShellLauncherFunctions.S_OK)
                    return false;
                LogHelper.Instance.Log("ShellLauncherWindowsFeatureInstalled() - enabled: " + (object)num);
                return num != 0;
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("Exception calling ShellLauncherWindowsFeatureInstalled()!", ex);
            }
            return false;
        }

        [KernelFunction(Name = "kernel.replaceshellwith")]
        internal static bool ReplaceShellWith(string shellExecutable)
        {
            try
            {
                string str = ServiceLocator.Instance.GetService<IMacroService>().ExpandProperties(shellExecutable);
                string arguments1 = string.Format(ShellLauncherFunctions.DefaultShellCommandLine, (object)str);
                Process process1 = new Process();
                process1.StartInfo = new ProcessStartInfo("wmic.exe", arguments1)
                {
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    Verb = "runas"
                };
                process1.Start();
                process1.WaitForExit(120000);
                if (process1.ExitCode == 0)
                {
                    string arguments2 = string.Format(ShellLauncherFunctions.AdminShellCommandLine, (object)str);
                    Process process2 = new Process();
                    process2.StartInfo = new ProcessStartInfo("wmic.exe", arguments2)
                    {
                        CreateNoWindow = true,
                        WindowStyle = ProcessWindowStyle.Hidden,
                        Verb = "runas"
                    };
                    process2.Start();
                    process2.WaitForExit(120000);
                    if (process2.ExitCode == 0)
                    {
                        Process process3 = new Process();
                        process3.StartInfo = new ProcessStartInfo("wmic.exe", ShellLauncherFunctions.ShellEnabledCommandLine)
                        {
                            CreateNoWindow = true,
                            WindowStyle = ProcessWindowStyle.Hidden,
                            Verb = "runas"
                        };
                        process3.Start();
                        process3.WaitForExit(120000);
                        if (process3.ExitCode == 0)
                            return true;
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("An unhandled exception was rasied in ShellLauncherFunctions.ReplaceShellWith.", ex);
            }
            return false;
        }
    }
}
