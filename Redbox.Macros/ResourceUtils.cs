using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;

namespace Redbox.Macros
{
    public static class ResourceUtils
    {
        private static ResourceManager _sharedResourceManager;
        private static readonly Hashtable _resourceManagerDictionary = new Hashtable();

        public static void RegisterSharedAssembly(Assembly assembly)
        {
            _sharedResourceManager = new ResourceManager(assembly.GetName().Name, assembly);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static string GetString(string name)
        {
            var callingAssembly = Assembly.GetCallingAssembly();
            return GetString(name, null, callingAssembly);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static string GetString(string name, CultureInfo culture)
        {
            var callingAssembly = Assembly.GetCallingAssembly();
            return GetString(name, culture, callingAssembly);
        }

        public static string GetString(string name, CultureInfo culture, Assembly assembly)
        {
            var name1 = assembly.GetName().Name;
            if (!_resourceManagerDictionary.Contains(name1))
                RegisterAssembly(assembly);
            var str = ((ResourceManager)_resourceManagerDictionary[name1]).GetString(name, culture);
            return str == null && _sharedResourceManager != null
                ? _sharedResourceManager.GetString(name, culture)
                : str;
        }

        private static void RegisterAssembly(Assembly assembly)
        {
            lock (_resourceManagerDictionary)
            {
                var name = assembly.GetName().Name;
                _resourceManagerDictionary.Add(name, new ResourceManager(GetResourceName(name), assembly));
            }
        }

        private static string GetResourceName(string assemblyName)
        {
            return (assemblyName.EndsWith("Tasks")
                ? assemblyName.Substring(0, assemblyName.Length - 5)
                : assemblyName) + ".Strings";
        }
    }
}