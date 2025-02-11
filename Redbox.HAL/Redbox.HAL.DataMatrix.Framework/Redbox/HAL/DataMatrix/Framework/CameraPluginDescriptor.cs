using System;
using System.Xml;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.DataMatrix.Framework
{
    internal sealed class CameraPluginDescriptor
    {
        internal string PluginName { get; set; }

        internal string AssemblyName { get; set; }

        internal string PluginNamespace { get; set; }

        internal string PluginServiceProviderMethodName { get; set; }

        internal string Version { get; set; }

        internal string DescriptorPath { get; private set; }

        internal static CameraPluginDescriptor FromXml(string xmlPath)
        {
            try
            {
                var xmlDocument = new XmlDocument();
                xmlDocument.Load(xmlPath);
                if (xmlDocument.DocumentElement == null)
                {
                    LogHelper.Instance.Log(LogEntryType.Error,
                        "[Camera Plugin Manager] The descriptor file '{0}' doesn't have a root element.", xmlPath);
                    return null;
                }

                var xmlNode = xmlDocument.SelectSingleNode("Plugin");
                if (xmlNode == null)
                {
                    LogHelper.Instance.Log(LogEntryType.Error,
                        "[Camera Plugin Manager] The descriptor file '{0}' is missing the required Plugin node.",
                        xmlPath);
                    return null;
                }

                var pluginDescriptor = new CameraPluginDescriptor();
                pluginDescriptor.DescriptorPath = xmlPath;
                foreach (XmlNode childNode in xmlNode.ChildNodes)
                    if (childNode.Name.Equals("PluginType"))
                    {
                        pluginDescriptor.PluginName = childNode.Attributes.GetNamedItem("Name").Value;
                        pluginDescriptor.Version = childNode.Attributes.GetNamedItem("Version").Value;
                    }
                    else if (childNode.Name.Equals("Assembly"))
                    {
                        pluginDescriptor.AssemblyName = childNode.Attributes.GetNamedItem("Name").Value;
                    }
                    else if (childNode.Name.Equals("PluginProvider"))
                    {
                        pluginDescriptor.PluginServiceProviderMethodName =
                            childNode.Attributes.GetNamedItem("Method").Value;
                        pluginDescriptor.PluginNamespace = childNode.Attributes.GetNamedItem("Namespace").Value;
                    }

                return pluginDescriptor;
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("[Camera Plugin Manager] Unable to read plugin descriptor.", LogEntryType.Error);
                LogHelper.Instance.Log("[Camera Plugin Manager] Detailed exception: ", ex);
                return null;
            }
        }
    }
}