using Redbox.Core;
using Redbox.Lua;
using System.Collections.Generic;

namespace Redbox.UpdateManager.Kernel
{
    internal static class AssemblyFunctions
    {
        [KernelFunction(Name = "kernel.executeassemblyinstaller")]
        internal static LuaTable ExecuteAssemblyInstaller(
          string fileName,
          string mode,
          params object[] options)
        {
            List<string> stringList = new List<string>();
            if (options != null)
            {
                foreach (object option in options)
                {
                    if (option != null)
                        stringList.Add(option.ToString());
                }
            }
            List<string> errorList = new List<string>();
            bool flag = AssemblyInstallerHelper.ExecuteInAppDomain(fileName, stringList.ToArray(), string.Compare(mode, "install", true) == 0 ? InstallMode.Install : InstallMode.Uninstall, errorList);
            LuaTable luaTable1 = new LuaTable(KernelService.Instance.LuaRuntime);
            luaTable1[(object)"reboot_required"] = (object)flag;
            LuaTable luaTable2 = new LuaTable(KernelService.Instance.LuaRuntime);
            int num = 1;
            foreach (string str in errorList)
                luaTable2[(object)num++] = (object)str;
            luaTable1[(object)"errors"] = (object)luaTable2;
            return luaTable1;
        }
    }
}
