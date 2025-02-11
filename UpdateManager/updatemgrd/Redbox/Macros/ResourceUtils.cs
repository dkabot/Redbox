using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;

namespace Redbox.Macros
{
    internal static class ResourceUtils
    {
        private static ResourceManager _sharedResourceManager;
        private static readonly Hashtable _resourceManagerDictionary = new Hashtable();

        public static void RegisterSharedAssembly(Assembly assembly)
        {
            ResourceUtils._sharedResourceManager = new ResourceManager(assembly.GetName().Name, assembly);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static string GetString(string name)
        {
            Assembly callingAssembly = Assembly.GetCallingAssembly();
            return ResourceUtils.GetString(name, (CultureInfo)null, callingAssembly);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static string GetString(string name, CultureInfo culture)
        {
            Assembly callingAssembly = Assembly.GetCallingAssembly();
            return ResourceUtils.GetString(name, culture, callingAssembly);
        }

        public static string GetString(string name, CultureInfo culture, Assembly assembly)
        {
            string name1 = assembly.GetName().Name;
            if (!ResourceUtils._resourceManagerDictionary.Contains((object)name1))
                ResourceUtils.RegisterAssembly(assembly);
            string str = ((ResourceManager)ResourceUtils._resourceManagerDictionary[(object)name1]).GetString(name, culture);
            return str == null && ResourceUtils._sharedResourceManager != null ? ResourceUtils._sharedResourceManager.GetString(name, culture) : str;
        }

        private static void RegisterAssembly(Assembly assembly)
        {
            lock (ResourceUtils._resourceManagerDictionary)
            {
                string name = assembly.GetName().Name;
                ResourceUtils._resourceManagerDictionary.Add((object)name, (object)new ResourceManager(ResourceUtils.GetResourceName(name), assembly));
            }
        }

        private static string GetResourceName(string assemblyName)
        {
            return (assemblyName.EndsWith("Tasks") ? assemblyName.Substring(0, assemblyName.Length - 5) : assemblyName) + ".Strings";
        }
    }
}
