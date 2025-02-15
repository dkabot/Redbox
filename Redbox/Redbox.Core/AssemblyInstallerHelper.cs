using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration.Install;
using System.Reflection;

namespace Redbox.Core
{
    public class AssemblyInstallerHelper : MarshalByRefObject
    {
        public static bool ExecuteInAppDomain(
            string targetPath,
            string[] args,
            InstallMode mode,
            List<string> errorList)
        {
            var location = Assembly.GetExecutingAssembly().Location;
            if (string.IsNullOrEmpty(location))
                return false;
            var domain = (AppDomain)null;
            try
            {
                domain = AppDomain.CreateDomain("AssemblyInstallerDomain");
                return ((AssemblyInstallerHelper)domain.CreateInstanceFromAndUnwrap(location,
                    typeof(AssemblyInstallerHelper).FullName)).Execute(targetPath, args, mode.ToString(), errorList);
            }
            finally
            {
                if (domain != null)
                    AppDomain.Unload(domain);
            }
        }

        internal bool Execute(string targetPath, string[] args, string mode, List<string> errors)
        {
            try
            {
                AssemblyInstaller.CheckIfInstallable(targetPath);
                using (var assemblyInstaller = new AssemblyInstaller(targetPath, args))
                {
                    assemblyInstaller.UseNewContext = true;
                    var dictionary = (IDictionary)new Hashtable();
                    try
                    {
                        if (mode == "Install")
                            assemblyInstaller.Install(dictionary);
                        else
                            assemblyInstaller.Uninstall(dictionary);
                        assemblyInstaller.Commit(dictionary);
                        if (dictionary["RebootRequired"] != null)
                            return true;
                    }
                    catch (Exception ex)
                    {
                        errors.Add(ex.ToString());
                        assemblyInstaller.Rollback(dictionary);
                    }
                }
            }
            catch (Exception ex)
            {
                errors.Add(ex.ToString());
            }

            return false;
        }
    }
}