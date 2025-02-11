using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Redbox.Core;

namespace Redbox.IPC.Framework
{
    [Command("command-repository", Filter = "System|Command-Repository")]
    [Description(
        "The COMMAND-REPOSITORY command scans the execution directory of the cache service for .NET assembly files (.dll and .exe) that contain types marked with the CommandAttribute class.")]
    public class CommandRepository
    {
        private static readonly IDictionary<string, CommandInstance> m_commandInstanceCache =
            new Dictionary<string, CommandInstance>();

        public static string[] AssemblyPatterns { get; set; }

        internal static ICollection<string> AllCommands => m_commandInstanceCache.Keys;

        public void Default(CommandContext context)
        {
            foreach (var installedCommand in DiscoverInstalledCommands())
                context.Messages.Add("Installed Command '" + installedCommand + "'.");
        }

        public static void InstallPerformanceCounters(string groupName)
        {
            var commands = new List<string>();
            DiscoverInstalledCommands();
            foreach (var keyValuePair in m_commandInstanceCache)
            {
                LogHelper.Instance.Log("Creating performance counters for command {0}.", keyValuePair.Key);
                foreach (var key in keyValuePair.Value.FormMethodCache.Keys)
                    commands.Add(string.Format("{0} {1}", keyValuePair.Key.ToUpper(), key.ToLower()));
            }

            PerformanceCounterSetup.Initialize(groupName, commands);
        }

        public static void UninstallPerformanceCounters(string groupName)
        {
            if (!PerformanceCounterCategory.Exists(groupName))
                return;
            PerformanceCounterCategory.Delete(groupName);
        }

        internal static List<string> DiscoverInstalledCommands()
        {
            using (var executionTimer = new ExecutionTimer())
            {
                var stringList1 = new List<string>();
                var directoryName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                var stringList2 = new List<string>();
                if (AssemblyPatterns == null || AssemblyPatterns.Length == 0)
                {
                    stringList2.AddRange(Directory.GetFiles(directoryName, "*.dll"));
                    stringList2.AddRange(Directory.GetFiles(directoryName, "*.exe"));
                }
                else
                {
                    foreach (var assemblyPattern in AssemblyPatterns)
                        if (assemblyPattern.IndexOf("*") == -1 && assemblyPattern.IndexOf("?") == -1)
                            stringList2.Add(Path.Combine(directoryName, assemblyPattern));
                        else
                            stringList2.AddRange(Directory.GetFiles(directoryName, assemblyPattern));
                }

                foreach (var str in stringList2)
                    if (File.Exists(str))
                        try
                        {
                            var assembly = Assembly.LoadFrom(str);
                            if (assembly != null)
                            {
                                LogHelper.Instance.Log("Load commands from assembly: '{0}'", assembly);
                                stringList1.AddRange(FindCommands(assembly.GetTypes()));
                            }
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Instance.Log(
                                string.Format("Unable to load assembly '{0}' to scan for commands.", str), ex);
                        }

                LogHelper.Instance.Log(string.Format("Time to scan for commands: {0}", executionTimer.Elapsed),
                    LogEntryType.Info);
                return stringList1;
            }
        }

        internal static CommandInstance GetCommand(string name)
        {
            if (string.IsNullOrEmpty(name))
                return null;
            var upper = name.ToUpper();
            return !m_commandInstanceCache.ContainsKey(upper) ? null : m_commandInstanceCache[upper];
        }

        private static List<string> FindCommands(IEnumerable<Type> types)
        {
            var commands = new List<string>();
            foreach (var type in types)
            foreach (var customAttribute in (CommandAttribute[])type.GetCustomAttributes(typeof(CommandAttribute),
                         false))
                if (customAttribute.Name != null)
                {
                    var upper = customAttribute.Name.ToUpper();
                    commands.Add(upper);
                    var commandInstance = CommandInstance.GetCommandInstance(type);
                    m_commandInstanceCache[upper] = commandInstance;
                    if (!string.IsNullOrEmpty(customAttribute.Filter))
                        customAttribute.Filter.Trim().Split('|').ToList().ForEach(commandInstance.Filters.Add);
                }

            return commands;
        }
    }
}