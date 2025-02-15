using System;
using System.Diagnostics;
using System.IO;
using System.Security.Permissions;
using Redbox.Core;

namespace Redbox.Shell.Remoting
{
    [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
    public class ShellHelper
    {
        public static int StartProcessAsAdministrator(string processName, string args)
        {
            try
            {
                var directoryName = Path.GetDirectoryName(processName);
                var fileName = Path.GetFileName(processName);
                var str = string.IsNullOrEmpty(args) ? null : string.Format("{0} {1}", processName, args);
                LogHelper.Instance.Log(
                    "starting from path: '{0}' with file: '{1}' in directory: '{2}' with command line: '{3}'",
                    processName, fileName, directoryName, str);
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = processName,
                        Arguments = args,
                        UseShellExecute = true,
                        WindowStyle = ProcessWindowStyle.Maximized,
                        Verb = "runas"
                    }
                };
                if (process.Start())
                    return process.Id;
                LogHelper.Instance.Log("Failed to start Kiosk Engine!");
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("Error in Function StartProcessAsAdministrator.", ex);
            }

            return -1;
        }
    }
}