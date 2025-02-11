using System;
using System.Reflection;

namespace Redbox.Core
{
    internal static class BuildConfiguration
    {
        private static readonly string[] m_configParts = ((AssemblyConfigurationAttribute)Attribute.GetCustomAttribute(Assembly.GetEntryAssembly(), typeof(AssemblyConfigurationAttribute))).Configuration.Split('|');

        public static bool IsDevelopmentBuild()
        {
            return BuildConfiguration.m_configParts[0].Length == 0 || BuildConfiguration.m_configParts[0] == "Debug";
        }

        public static bool IsIntegrationBuild()
        {
            return BuildConfiguration.m_configParts.Length == 2 && BuildConfiguration.m_configParts[0].IndexOf("Integration", StringComparison.CurrentCultureIgnoreCase) != -1;
        }

        public static string GetBuildConfigurationLabel()
        {
            string configurationLabel = (string)null;
            if (BuildConfiguration.IsIntegrationBuild())
                configurationLabel = string.Format("[Integration Build: {0}] INTERNAL USE ONLY", (object)BuildConfiguration.m_configParts[1]);
            else if (BuildConfiguration.IsDevelopmentBuild())
            {
                string str = BuildConfiguration.m_configParts[0];
                if (str.Length == 0)
                    str = "Not Specified";
                configurationLabel = string.Format("[Development Build: {0}] NOT FOR DEPLOYMENT", (object)str);
            }
            return configurationLabel;
        }
    }
}
