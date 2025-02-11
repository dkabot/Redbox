using Redbox.Core;
using Redbox.UpdateManager.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;

namespace Redbox.UpdateManager.Kernel
{
    [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
    internal static class ProcessFunctions
    {
        private const int TOKEN_DUPLICATE = 2;
        private const int TOKEN_QUERY = 8;
        private const int TOKEN_IMPERSONATE = 4;
        private const uint GRANTED_ALL = 268435456;
        private const uint CREATE_NEW_CONSOLE = 16;

        [KernelFunction(Name = "kernel.startprocessasshelluser")]
        internal static int LaunchProcessAsShellUser(string processName, string args)
        {
            return ProcessFunctions.StartProcessAsShellUser(processName, args);
        }

        [KernelFunction(Name = "kernel.processtrimworkingset")]
        internal static void TrimWorkingSet(string name)
        {
            Process process = ((IEnumerable<Process>)Process.GetProcesses()).FirstOrDefault<Process>((Func<Process, bool>)(p => string.Compare(p.ProcessName, name, true) == 0));
            if (process == null)
                return;
            ProcessFunctions.SetProcessWorkingSetSize(process.Handle, -1, -1);
        }

        [KernelFunction(Name = "kernel.killprocessbyname")]
        public static void KillProcessByName(string name)
        {
            ((IEnumerable<Process>)Process.GetProcesses()).FirstOrDefault<Process>((Func<Process, bool>)(p => string.Compare(p.ProcessName, name, true) == 0))?.Kill();
        }

        [KernelFunction(Name = "kernel.isprocessrunning")]
        internal static bool IsProcessRunning(string name)
        {
            return Process.GetProcessesByName(name).Length != 0;
        }

        [KernelFunction(Name = "kernel.executestring")]
        internal static int Execute(
          string process,
          string arguments,
          string workingDirectory,
          object timeout)
        {
            IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
            ProcessStartInfo processStartInfo = new ProcessStartInfo()
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden,
                FileName = service.ExpandProperties(process),
                Arguments = service.ExpandProperties(arguments),
                WorkingDirectory = service.ExpandProperties(workingDirectory)
            };
            Process process1 = new Process()
            {
                StartInfo = processStartInfo
            };
            process1.Start();
            int? nullable = new int?();
            if (timeout != null)
            {
                try
                {
                    nullable = new int?(Convert.ToInt32(timeout));
                }
                catch
                {
                }
            }
            if (nullable.HasValue)
            {
                LogHelper.Instance.Log("Entering bounded WaitForExit in ProcessFunctions.Execute with a timeout of {0}.", (object)nullable.Value);
                process1.WaitForExit(nullable.Value);
            }
            else
            {
                LogHelper.Instance.Log("Entering unbounded WaitForExit in ProcessFunctions.Execute.");
                process1.WaitForExit();
            }
            return process1.HasExited ? process1.ExitCode : -1;
        }

        [DllImport("Kernel32.dll")]
        private static extern bool SetProcessWorkingSetSize(
          IntPtr hProcess,
          int dwMinimumWorkingSetSize,
          int dwMaximumWorkingSetSize);

        public static int StartProcessAsShellUser(string processName, string args)
        {
            Process[] processesByName = Process.GetProcessesByName("KioskShell");
            if (processesByName.Length == 0 || processesByName.Length > 1)
                return -1;
            IntPtr zero = IntPtr.Zero;
            if (ProcessFunctions.OpenProcessToken(processesByName[0].Handle, 14, ref zero) != 0)
            {
                try
                {
                    IntPtr num = ProcessFunctions.DuplicateToken(zero);
                    if (num == IntPtr.Zero)
                    {
                        LogHelper.Instance.Log("Unable to to duplicate process token with DuplicateToken.", (Exception)new Win32Exception(Marshal.GetLastWin32Error()));
                        return -1;
                    }
                    try
                    {
                        string directoryName = Path.GetDirectoryName(processName);
                        string fileName = Path.GetFileName(processName);
                        string lpCommandLine = string.IsNullOrEmpty(args) ? (string)null : string.Format("{0} {1}", (object)processName, (object)args);
                        LogHelper.Instance.Log("starting from path: '{0}' with file: '{1}' in directory: '{2}' with command line: '{3}'", (object)processName, (object)fileName, (object)directoryName, (object)lpCommandLine);
                        ProcessFunctions.STARTUPINFO lpStartupInfo = new ProcessFunctions.STARTUPINFO();
                        lpStartupInfo.cb = Marshal.SizeOf<ProcessFunctions.STARTUPINFO>(lpStartupInfo);
                        lpStartupInfo.lpDesktop = "winsta0\\default";
                        ProcessFunctions.PROCESS_INFORMATION lpProcessInformation;
                        if (ProcessFunctions.CreateProcessWithTokenW(num, 0U, processName, lpCommandLine, 16U, IntPtr.Zero, directoryName, ref lpStartupInfo, out lpProcessInformation))
                            return lpProcessInformation.dwProcessId;
                        LogHelper.Instance.Log("Error Calling CreateProcessWithTokenW", (Exception)new Win32Exception(Marshal.GetLastWin32Error()));
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Instance.Log("Error in Function StartProcessAsShellUser.", ex);
                    }
                    finally
                    {
                        ProcessFunctions.CloseHandle(num);
                    }
                }
                finally
                {
                    ProcessFunctions.CloseHandle(zero);
                }
            }
            else
                LogHelper.Instance.Log("Unable to acquire process token via OpenProcessToken.", (Exception)new Win32Exception(Marshal.GetLastWin32Error()));
            return -1;
        }

        [SuppressUnmanagedCodeSecurity]
        [DllImport("advapi32", SetLastError = true)]
        private static extern int OpenProcessToken(
          IntPtr processHandle,
          int desiredAccess,
          ref IntPtr tokenHandle);

        [SuppressUnmanagedCodeSecurity]
        [DllImport("kernel32", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr handle);

        [DllImport("advapi32", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool CreateProcessWithTokenW(
          IntPtr hToken,
          uint dwLogonFlags,
          string lpApplicationName,
          string lpCommandLine,
          uint dwCreationFlags,
          IntPtr lpEnvironment,
          string lpCurrentDirectory,
          [In] ref ProcessFunctions.STARTUPINFO lpStartupInfo,
          out ProcessFunctions.PROCESS_INFORMATION lpProcessInformation);

        private static IntPtr DuplicateToken(IntPtr sourceToken)
        {
            IntPtr phNewToken;
            return !ProcessFunctions.DuplicateTokenEx(sourceToken, 268435456U, IntPtr.Zero, ProcessFunctions.SECURITY_IMPERSONATION_LEVEL.Impersonation, ProcessFunctions.TOKEN_TYPE.Primary, out phNewToken) ? IntPtr.Zero : phNewToken;
        }

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool DuplicateTokenEx(
          IntPtr hExistingToken,
          uint dwDesiredAccess,
          IntPtr lpTokenAttributes,
          ProcessFunctions.SECURITY_IMPERSONATION_LEVEL ImpersonationLevel,
          ProcessFunctions.TOKEN_TYPE TokenType,
          out IntPtr phNewToken);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct STARTUPINFO
        {
            public int cb;
            public string lpReserved;
            public string lpDesktop;
            public string lpTitle;
            public int dwX;
            public int dwY;
            public int dwXSize;
            public int dwYSize;
            public int dwXCountChars;
            public int dwYCountChars;
            public int dwFillAttribute;
            public int dwFlags;
            public short wShowWindow;
            public short cbReserved2;
            public IntPtr lpReserved2;
            public IntPtr hStdInput;
            public IntPtr hStdOutput;
            public IntPtr hStdError;
        }

        private enum SECURITY_IMPERSONATION_LEVEL
        {
            Anonymous,
            Identifcation,
            Impersonation,
            Delegation,
        }

        private enum TOKEN_TYPE
        {
            Primary = 1,
            Impersonation = 2,
        }

        private struct PROCESS_INFORMATION
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public int dwProcessId;
            public int dwThreadId;
        }
    }
}
