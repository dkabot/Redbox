using Redbox.Core;
using Redbox.Lua;
using Redbox.UpdateManager.ComponentModel;
using Redbox.UpdateManager.Environment;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security;
using System.Threading;

namespace Redbox.UpdateManager.Kernel
{
    internal static class SystemFunctions
    {
        [KernelFunction(Name = "kernel.systemreloadperusersystemparameters")]
        internal static bool ReloadPerUserSystemParamters()
        {
            if (SystemFunctions.UpdatePerUserSystemParameters())
                return true;
            LogHelper.Instance.Log("Failed to update per user system parameters.", (Exception)new Win32Exception(Marshal.GetLastWin32Error()));
            return false;
        }

        [KernelFunction(Name = "kernel.import_script")]
        internal static void Import(string path)
        {
            string path1 = ServiceLocator.Instance.GetService<IMacroService>().ExpandProperties(path);
            if (File.Exists(path1))
                KernelService.Instance.LuaRuntime.DoString(File.ReadAllText(path1));
            else
                LogHelper.Instance.Log("{0} does not exists can not import.", (object)path1);
        }

        [KernelFunction(Name = "kernel.log")]
        internal static void Log(string message)
        {
            LogHelper.Instance.Log(string.Format("Lua Runtime|{0}", (object)message));
        }

        [KernelFunction(Name = "kernel.logf")]
        internal static void Log(string message, object parms)
        {
            if (parms is LuaTable table)
            {
                List<object> list = new List<object>();
                SystemFunctions.ExpandTableList(table, (ICollection<object>)list);
                LogHelper.Instance.Log(string.Format("Lua Runtime|{0}", (object)message), list.ToArray());
            }
            else
                SystemFunctions.Log(message);
        }

        [KernelFunction(Name = "kernel.checkmutex")]
        internal static bool IsMutexActive(string name)
        {
            bool createdNew;
            using (new Mutex(false, name, out createdNew))
                ;
            return !createdNew;
        }

        [KernelFunction(Name = "kernel.getvirtualmemoryusage")]
        internal static double GetVirtualMemoryUsageInMB(string name)
        {
            Process[] processesByName = Process.GetProcessesByName(name);
            return processesByName.Length != 0 ? (double)processesByName[0].VirtualMemorySize64 / 1048576.0 : -1.0;
        }

        [KernelFunction(Name = "kernel.getphysicalmemoryusage")]
        internal static double GetPhysicalMemoryUsageInMB(string name)
        {
            Process[] processesByName = Process.GetProcessesByName(name);
            return processesByName.Length != 0 ? (double)processesByName[0].WorkingSet64 / 1048576.0 : -1.0;
        }

        [KernelFunction(Name = "kernel.movelockedfile")]
        internal static void MoveFileEx(string source, string destiaion)
        {
            SystemFunctions.MoveLockedFileSystemEntry(source, destiaion);
        }

        [KernelFunction(Name = "kernel.deletelockedfile")]
        internal static void MoveFileEx(string target)
        {
            SystemFunctions.DeleteLockedFileSystemEntry(target);
        }

        [KernelFunction(Name = "kernel.reboot")]
        internal static void Reboot() => KernelService.Instance.ShutdownType = ShutdownType.Reboot;

        [KernelFunction(Name = "kernel.shutdown")]
        internal static void Shutdown() => KernelService.Instance.ShutdownType = ShutdownType.Shutdown;

        [KernelFunction(Name = "kernel.wait")]
        internal static void Wait(int milliseconds)
        {
            AutoResetEvent autoResetEvent = new AutoResetEvent(false);
            autoResetEvent.WaitOne(milliseconds, false);
            autoResetEvent.Close();
        }

        [KernelFunction(Name = "kernel.shouldreboot")]
        public static bool ShouldReboot()
        {
            TimeSpan timeSpan1 = TimeSpan.FromMilliseconds((double)(System.Environment.TickCount & int.MaxValue));
            TimeSpan timeSpan2 = TimeSpan.FromHours(1.0);
            if (timeSpan1 < timeSpan2)
            {
                LogHelper.Instance.Log(string.Format("Not rebooting because this kiosk rebooted {0} minutes ago.", (object)timeSpan1.TotalMinutes), LogEntryType.Info);
                return false;
            }
            string key = (string)RegistryFunctions.GetKey("HKEY_LOCAL_MACHINE\\SOFTWARE\\Redbox\\REDS\\Kiosk Engine\\Store", "RebootTime");
            if (string.IsNullOrEmpty(key))
            {
                LogHelper.Instance.Log("WARNING: No reboot time was found in the registry.");
                return false;
            }
            DateTime result;
            if (!DateTime.TryParse(key, out result))
                return false;
            DateTime now = DateTime.Now;
            DateTime dateTime1 = result.AddMinutes(-1.0);
            DateTime dateTime2 = result.AddMinutes(1.0);
            return now >= dateTime1 && now <= dateTime2;
        }

        [KernelFunction(Name = "kernel.mutesound")]
        internal static void MuteSound()
        {
            uint dwVolume = 0;
            SystemFunctions.waveOutSetVolume(IntPtr.Zero, dwVolume);
            SystemFunctions.midiOutSetVolume(IntPtr.Zero, dwVolume);
        }

        [KernelFunction(Name = "kernel.systemrenamecomputer")]
        internal static void RenameComputer(string name)
        {
            foreach (ManagementObject instance in new ManagementClass("Win32_ComputerSystem").GetInstances())
            {
                ManagementBaseObject methodParameters = instance.GetMethodParameters("Rename");
                methodParameters.SetPropertyValue("Name", (object)name);
                instance.InvokeMethod("Rename", methodParameters, new InvokeMethodOptions());
            }
        }

        [KernelFunction(Name = "kernel.getSerialNumber")]
        internal static string GetSerialNumber()
        {
            string empty = string.Empty;
            try
            {
                using (ManagementObjectSearcher managementObjectSearcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_BIOS"))
                {
                    foreach (ManagementBaseObject managementBaseObject in managementObjectSearcher.Get())
                        empty = managementBaseObject["SerialNumber"] as string;
                }
            }
            catch
            {
            }
            return empty;
        }

        [KernelFunction(Name = "kernel.getManufacturer")]
        internal static string GetManufacturer()
        {
            string empty = string.Empty;
            try
            {
                using (ManagementObjectSearcher managementObjectSearcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_BIOS"))
                {
                    foreach (ManagementBaseObject managementBaseObject in managementObjectSearcher.Get())
                        empty = managementBaseObject["Manufacturer"] as string;
                }
            }
            catch
            {
            }
            return empty;
        }

        [KernelFunction(Name = "kernel.getUtcdatetime")]
        internal static string GetUtcDateTime() => DateTime.UtcNow.ToString();

        [KernelFunction(Name = "kernel.setdatetime")]
        internal static bool SetDateTime(string datetime)
        {
            try
            {
                return TimeZoneFunctions.SetTime(DateTime.Parse(datetime));
            }
            catch (FormatException ex)
            {
                LogHelper.Instance.Log("An unparsable DateTime string was passed to SetDateTime", (Exception)ex);
                return false;
            }
            catch (ArgumentNullException ex)
            {
                LogHelper.Instance.Log("A null DateTime string was passed to SetDateTime", (Exception)ex);
                return false;
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("Unhandled exception in kernel.setdatetime", ex);
                return false;
            }
        }

        [KernelFunction(Name = "kernel.settimezonefromserializedstring")]
        internal static bool SetTimeZoneFromSerializedString(string serializedTimeZoneInfo)
        {
            try
            {
                return TimeZoneFunctions.SetTimeZone(TimeZoneInfo.FromSerializedString(serializedTimeZoneInfo));
            }
            catch (SerializationException ex)
            {
                LogHelper.Instance.Log("The source parameter cannot be deserialized back into a TimeZoneInfo object.", (Exception)ex, LogEntryType.Error);
                return false;
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("Unhandled exception in SetTimeZoneFromSerializedString", ex, LogEntryType.Error);
                return false;
            }
        }

        [KernelFunction(Name = "kernel.settimezonefromid")]
        internal static bool SetTimeZoneFromid(string timeZoneInfoId)
        {
            try
            {
                return TimeZoneFunctions.SetTimeZone(TimeZoneInfo.FindSystemTimeZoneById(timeZoneInfoId));
            }
            catch (TimeZoneNotFoundException ex)
            {
                LogHelper.Instance.Log("The time zone identifier specified by id was not found. This means that a registry key whose name matches id does not exist, or that the key exists but does not contain any time zone data.", (Exception)ex, LogEntryType.Error);
                return false;
            }
            catch (ArgumentNullException ex)
            {
                LogHelper.Instance.Log("The id parameter is null.", (Exception)ex, LogEntryType.Error);
                return false;
            }
            catch (SecurityException ex)
            {
                LogHelper.Instance.Log("The process does not have the permissions required to read from the registry key that contains the time zone information.", (Exception)ex, LogEntryType.Error);
                return false;
            }
            catch (InvalidTimeZoneException ex)
            {
                LogHelper.Instance.Log("The time zone identifier was found, but the registry data is corrupted.", (Exception)ex, LogEntryType.Error);
                return false;
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("Unhandled exception in SetTimeZoneFromid", ex, LogEntryType.Error);
                return false;
            }
        }

        [KernelFunction(Name = "kernel.gettimezone")]
        internal static string GetTimeZone() => TimeZoneInfo.Local.StandardName;

        [KernelFunction(Name = "kernel.gettimezoneinfo")]
        internal static string GetTimeZoneInfo() => TimeZoneInfo.Local.ToSerializedString();

        [DllImport("Kernel32.dll")]
        private static extern bool MoveFileEx(
          string lpExistingFileName,
          string lpNewFileName,
          SystemFunctions.MoveFileFlags dwFlags);

        [DllImport("user32.dll")]
        private static extern bool UpdatePerUserSystemParameters();

        [DllImport("WinMM.dll")]
        private static extern int waveOutSetVolume(IntPtr hwo, uint dwVolume);

        [DllImport("WinMM.dll")]
        private static extern int midiOutSetVolume(IntPtr hmo, uint dwVolume);

        private static void ExpandTableList(LuaTable table, ICollection<object> list)
        {
            foreach (object table1 in (IEnumerable)table.Values)
            {
                if (table1 is LuaTable)
                    SystemFunctions.ExpandTableList((LuaTable)table1, list);
                list.Add((object)table1.ToString());
            }
        }

        private static void MoveLockedFileSystemEntry(string source, string destination)
        {
            IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
            string str1 = service.ExpandProperties(source);
            string str2 = service.ExpandProperties(destination);
            SystemFunctions.MoveFileFlags dwFlags = SystemFunctions.MoveFileFlags.MOVEFILE_DELAY_UNTIL_REBOOT;
            if (!Directory.Exists(str1) && !Directory.Exists(str2))
                dwFlags |= SystemFunctions.MoveFileFlags.MOVEFILE_REPLACE_EXISTING;
            SystemFunctions.MoveFileEx(str1, str2, dwFlags);
        }

        private static void DeleteLockedFileSystemEntry(string target)
        {
            SystemFunctions.MoveFileEx(ServiceLocator.Instance.GetService<IMacroService>().ExpandProperties(target), (string)null, SystemFunctions.MoveFileFlags.MOVEFILE_DELAY_UNTIL_REBOOT);
        }

        [Flags]
        private enum MoveFileFlags
        {
            MOVEFILE_REPLACE_EXISTING = 1,
            MOVEFILE_COPY_ALLOWED = 2,
            MOVEFILE_DELAY_UNTIL_REBOOT = 4,
            MOVEFILE_WRITE_THROUGH = 8,
        }
    }
}
