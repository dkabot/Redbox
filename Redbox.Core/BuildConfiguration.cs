using System;
using System.Reflection;

namespace Redbox.Core
{
    public static class BuildConfiguration
    {
        private static readonly string[] m_configParts =
            ((AssemblyConfigurationAttribute)Attribute.GetCustomAttribute(Assembly.GetEntryAssembly(),
                typeof(AssemblyConfigurationAttribute))).Configuration.Split('|');

        public static bool IsDevelopmentBuild()
        {
            return m_configParts[0].Length == 0 || m_configParts[0] == "Debug";
        }

        public static bool IsIntegrationBuild()
        {
            return m_configParts.Length == 2 &&
                   m_configParts[0].IndexOf("Integration", StringComparison.CurrentCultureIgnoreCase) != -1;
        }

        public static string GetBuildConfigurationLabel()
        {
            var configurationLabel = (string)null;
            if (IsIntegrationBuild())
            {
                configurationLabel = string.Format("[Integration Build: {0}] INTERNAL USE ONLY", m_configParts[1]);
            }
            else if (IsDevelopmentBuild())
            {
                var str = m_configParts[0];
                if (str.Length == 0)
                    str = "Not Specified";
                configurationLabel = string.Format("[Development Build: {0}] NOT FOR DEPLOYMENT", str);
            }

            return configurationLabel;
        }
    }
}