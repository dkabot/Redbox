using Redbox.Core;
using Redbox.Lua;
using Redbox.UpdateManager.ComponentModel;
using System;
using System.Diagnostics;
using System.Reflection;

namespace Redbox.UpdateManager.Kernel
{
    internal static class FileVersionFunctions
    {
        [KernelFunction(Name = "kernel.getfileversion")]
        internal static string GetFileVersion(string fileName)
        {
            try
            {
                return FileVersionInfo.GetVersionInfo(ServiceLocator.Instance.GetService<IMacroService>().ExpandProperties(fileName)).FileVersion;
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("Error getting file version.", ex);
                return string.Empty;
            }
        }

        [KernelFunction(Name = "kernel.getfileversiontable")]
        internal static LuaTable GetFileVersionInfo(string fileName)
        {
            try
            {
                FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(ServiceLocator.Instance.GetService<IMacroService>().ExpandProperties(fileName));
                return new LuaTable(KernelService.Instance.LuaRuntime)
                {
                    [(object)"file_version"] = (object)versionInfo.FileVersion,
                    [(object)"company_name"] = (object)versionInfo.CompanyName,
                    [(object)"comments"] = (object)versionInfo.Comments,
                    [(object)"build_version"] = (object)versionInfo.FileBuildPart,
                    [(object)"major_version"] = (object)versionInfo.FileMajorPart,
                    [(object)"minor_version"] = (object)versionInfo.FileMinorPart,
                    [(object)"private_version"] = (object)versionInfo.FilePrivatePart,
                    [(object)"copyright"] = (object)versionInfo.LegalCopyright
                };
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("Error getting file version.", ex);
                return (LuaTable)null;
            }
        }

        [KernelFunction(Name = "kernel.getdotnetassemblyversion")]
        internal static string GetAssemblyVersion(string file)
        {
            string assemblyVersion = string.Empty;
            try
            {
                assemblyVersion = AssemblyInfoHelper.GetVersion(Assembly.LoadFile(ServiceLocator.Instance.GetService<IMacroService>().ExpandProperties(file)));
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("Error getting file version.", ex);
            }
            return assemblyVersion;
        }

        [KernelFunction(Name = "kernel.getfrontendversion")]
        internal static string GetFrontendVersion()
        {
            return Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }
    }
}
