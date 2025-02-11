using Microsoft.Win32;
using Redbox.Core;
using Redbox.Lua;
using Redbox.UpdateManager.ComponentModel;
using System;
using System.Diagnostics;
using System.Security.Principal;

namespace Redbox.UpdateManager.Kernel
{
    internal static class RegistryFunctions
    {
        [KernelFunction(Name = "kernel.getuserregistryprefix")]
        internal static string GetUserRegistryPrefix(string user)
        {
            try
            {
                SecurityIdentifier securityIdentifier = (SecurityIdentifier)new NTAccount(user).Translate(typeof(SecurityIdentifier));
                if (securityIdentifier != (SecurityIdentifier)null)
                    return string.Format("HKEY_USERS\\{0}", (object)securityIdentifier.Value);
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("Failed to get sid for user.", ex);
            }
            return string.Empty;
        }

        [KernelFunction(Name = "kernel.getuserregistryprefixwithdomain")]
        internal static string GetUserRegistryPrefixWithDomain(string domain, string user)
        {
            try
            {
                SecurityIdentifier securityIdentifier = (SecurityIdentifier)new NTAccount(domain, user).Translate(typeof(SecurityIdentifier));
                if (securityIdentifier != (SecurityIdentifier)null)
                    return string.Format("HKEY_USERS\\{0}", (object)securityIdentifier.Value);
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("Failed to get sid for user.", ex);
            }
            return string.Empty;
        }

        [KernelFunction(Name = "kernel.deleteregistrysubkey")]
        internal static bool DeleteSubKey(string path, string k)
        {
            try
            {
                RegistryKey registryKey = (RegistryKey)null;
                if (path.StartsWith("HKEY_CURRENT_USER"))
                    registryKey = Registry.CurrentUser.OpenSubKey(path.Replace("HKEY_CURRENT_USER\\", ""), true);
                else if (path.StartsWith("HKEY_LOCAL_MACHINE"))
                    registryKey = Registry.LocalMachine.OpenSubKey(path.Replace("HKEY_LOCAL_MACHINE\\", ""), true);
                else if (path.StartsWith("HKEY_CURRENT_CONFIG"))
                    registryKey = Registry.CurrentConfig.OpenSubKey(path.Replace("HKEY_CURRENT_CONFIG\\", ""), true);
                else if (path.StartsWith("HKEY_USERS"))
                    registryKey = Registry.Users.OpenSubKey(path.Replace("HKEY_USERS\\", ""), true);
                else if (path.StartsWith("HKEY_CLASSES_ROOT"))
                    registryKey = Registry.ClassesRoot.OpenSubKey(path.Replace("HKEY_CLASSES_ROOT\\", ""), true);
                if (registryKey != null)
                {
                    registryKey.DeleteSubKeyTree(k);
                    registryKey.Close();
                    return true;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
            return false;
        }

        [KernelFunction(Name = "kernel.deleteregistryvalue")]
        internal static bool DeleteValue(string path, string value)
        {
            try
            {
                RegistryKey registryKey = (RegistryKey)null;
                if (path.StartsWith("HKEY_CURRENT_USER"))
                    registryKey = Registry.CurrentUser.OpenSubKey(path.Replace("HKEY_CURRENT_USER\\", ""), true);
                else if (path.StartsWith("HKEY_LOCAL_MACHINE"))
                    registryKey = Registry.LocalMachine.OpenSubKey(path.Replace("HKEY_LOCAL_MACHINE\\", ""), true);
                else if (path.StartsWith("HKEY_CURRENT_CONFIG"))
                    registryKey = Registry.CurrentConfig.OpenSubKey(path.Replace("HKEY_CURRENT_CONFIG\\", ""), true);
                else if (path.StartsWith("HKEY_USERS"))
                    registryKey = Registry.Users.OpenSubKey(path.Replace("HKEY_USERS\\", ""), true);
                else if (path.StartsWith("HKEY_CLASSES_ROOT"))
                    registryKey = Registry.ClassesRoot.OpenSubKey(path.Replace("HKEY_CLASSES_ROOT\\", ""), true);
                if (registryKey != null)
                {
                    registryKey.DeleteValue(value);
                    registryKey.Close();
                    return true;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
            return false;
        }

        [KernelFunction(Name = "kernel.getregistrykey")]
        internal static object GetKey(string path, string name)
        {
            IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
            return Registry.GetValue(service.ExpandProperties(path), service.ExpandProperties(name), (object)string.Empty);
        }

        [KernelFunction(Name = "kernel.setregistrykey")]
        internal static void SetKey(string path, string name, object value)
        {
            IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
            object obj = value;
            if (value is string)
                obj = (object)service.ExpandProperties((string)value);
            Registry.SetValue(service.ExpandProperties(path), service.ExpandProperties(name), obj);
        }

        [KernelFunction(Name = "kernel.setdwordregistrykey")]
        internal static void SetDWORDKey(string path, string name, int value)
        {
            IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
            Registry.SetValue(service.ExpandProperties(path), service.ExpandProperties(name), (object)value, RegistryValueKind.DWord);
        }

        [KernelFunction(Name = "kernel.getdotnetversions")]
        internal static LuaTable GetDotNetVersions()
        {
            LuaTable dotNetVersions = new LuaTable(KernelService.Instance.LuaRuntime);
            LuaTable luaTable = new LuaTable(KernelService.Instance.LuaRuntime);
            dotNetVersions[(object)"success"] = (object)false;
            dotNetVersions[(object)"versions"] = (object)luaTable;
            RegistryKey registryKey1 = (RegistryKey)null;
            try
            {
                registryKey1 = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\NET Framework Setup\\NDP");
                if (registryKey1 != null)
                {
                    int num = 1;
                    foreach (string subKeyName in registryKey1.GetSubKeyNames())
                    {
                        RegistryKey registryKey2 = registryKey1.OpenSubKey(subKeyName);
                        if (registryKey2 != null)
                        {
                            luaTable[(object)num++] = (object)new LuaTable(KernelService.Instance.LuaRuntime)
                            {
                                [(object)"version"] = registryKey2.GetValue("Version"),
                                [(object)"service_pack"] = (object)(int)(registryKey2.GetValue("SP") ?? (object)0),
                                [(object)"install_path"] = registryKey2.GetValue("InstallPath")
                            };
                            registryKey2.Close();
                        }
                    }
                    dotNetVersions[(object)"success"] = (object)true;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("An unhandled exception was raised in RegistryFunctions.GetDotNetVersions.", ex);
            }
            finally
            {
                registryKey1?.Close();
            }
            return dotNetVersions;
        }

        [KernelFunction(Name = "kernel.exportregistryfile")]
        internal static void ExportRegistryKey(string fileName, string key)
        {
            IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
            string str1 = service.ExpandProperties(key);
            string str2 = service.ExpandProperties(fileName);
            if (string.IsNullOrEmpty(str1) || string.IsNullOrEmpty(str2))
                return;
            ProcessStartInfo processStartInfo = new ProcessStartInfo()
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                FileName = "regedit.exe",
                WindowStyle = ProcessWindowStyle.Hidden,
                Arguments = string.Format("/E \"{0}\" {1}", (object)str2, (object)str1)
            };
            Process process = new Process();
            process.StartInfo = processStartInfo;
            process.Start();
            process.WaitForExit();
        }

        [KernelFunction(Name = "kernel.importregistryfile")]
        internal static void ImportRegistryFile(string fileName)
        {
            string str = ServiceLocator.Instance.GetService<IMacroService>().ExpandProperties(fileName);
            if (string.IsNullOrEmpty(str))
                return;
            ProcessStartInfo processStartInfo = new ProcessStartInfo()
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                FileName = "regedit.exe",
                WindowStyle = ProcessWindowStyle.Hidden,
                Arguments = string.Format("/S \"{0}\"", (object)str)
            };
            Process process = new Process();
            process.StartInfo = processStartInfo;
            process.Start();
            process.WaitForExit();
        }
    }
}
