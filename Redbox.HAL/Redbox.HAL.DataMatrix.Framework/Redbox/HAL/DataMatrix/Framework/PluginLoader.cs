using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Core;

namespace Redbox.HAL.DataMatrix.Framework
{
    internal sealed class PluginLoader
    {
        private readonly List<CameraPluginDescriptor> PluginDescriptors;
        private readonly string PluginsDirectory;

        private PluginLoader()
        {
            PluginsDirectory = ServiceLocator.Instance.GetService<IRuntimeService>().RuntimePath("camera.plugins");
            var files = Directory.GetFiles(PluginsDirectory, "*.xml");
            PluginDescriptors = new List<CameraPluginDescriptor>();
            if (files.Length == 0)
                LogHelper.Instance.Log("[Camera Plugin Manager] There are no plugin descriptor files.",
                    LogEntryType.Error);
            else
                foreach (var xmlPath in files)
                {
                    var pluginDescriptor = CameraPluginDescriptor.FromXml(xmlPath);
                    if (pluginDescriptor != null)
                        PluginDescriptors.Add(pluginDescriptor);
                }
        }

        internal static PluginLoader Instance => Singleton<PluginLoader>.Instance;

        internal CameraPluginDescriptor LocatePlugin(string name, out ICameraPlugin plugin)
        {
            plugin = null;
            var descriptor = PluginDescriptors.Find(cameraPluginDescriptor_0 =>
                name.Equals(cameraPluginDescriptor_0.PluginName, StringComparison.CurrentCultureIgnoreCase));
            if (descriptor == null)
                return null;
            return !LocatePluginInner(descriptor, out plugin) ? null : descriptor;
        }

        private bool LocatePluginInner(CameraPluginDescriptor descriptor, out ICameraPlugin plugin)
        {
            plugin = null;
            foreach (var exportedType in Assembly.LoadFrom(Path.Combine(PluginsDirectory, descriptor.AssemblyName))
                         .GetExportedTypes())
                if (exportedType.FullName.Equals(descriptor.PluginNamespace))
                {
                    var method = exportedType.GetMethod(descriptor.PluginServiceProviderMethodName,
                        BindingFlags.Static | BindingFlags.Public);
                    if (method != null)
                    {
                        plugin = method.Invoke(null, null) as ICameraPlugin;
                        return plugin != null;
                    }
                }

            return false;
        }
    }
}