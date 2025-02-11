using Redbox.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Redbox.IPC.Framework
{
    [Command("command-repository", Filter = "System|Command-Repository")]
    [Description("The COMMAND-REPOSITORY command scans the execution directory of the cache service for .NET assembly files (.dll and .exe) that contain types marked with the CommandAttribute class.")]
    internal class CommandRepository
    {
        private static readonly IDictionary<string, CommandInstance> m_commandInstanceCache = (IDictionary<string, CommandInstance>)new Dictionary<string, CommandInstance>();

        public void Default(CommandContext context)
        {
            foreach (string installedCommand in CommandRepository.DiscoverInstalledCommands())
                context.Messages.Add("Installed Command '" + installedCommand + "'.");
        }

        public static string[] AssemblyPatterns { get; set; }

        public static void InstallPerformanceCounters(string groupName)
        {
            List<string> commands = new List<string>();
            CommandRepository.DiscoverInstalledCommands();
            foreach (KeyValuePair<string, CommandInstance> keyValuePair in (IEnumerable<KeyValuePair<string, CommandInstance>>)CommandRepository.m_commandInstanceCache)
            {
                LogHelper.Instance.Log("Creating performance counters for command {0}.", (object)keyValuePair.Key);
                foreach (string key in (IEnumerable<string>)keyValuePair.Value.FormMethodCache.Keys)
                    commands.Add(string.Format("{0} {1}", (object)keyValuePair.Key.ToUpper(), (object)key.ToLower()));
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
            using (ExecutionTimer executionTimer = new ExecutionTimer())
            {
                List<string> stringList1 = new List<string>();
                string directoryName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                List<string> stringList2 = new List<string>();
                if (CommandRepository.AssemblyPatterns == null || CommandRepository.AssemblyPatterns.Length == 0)
                {
                    stringList2.AddRange((IEnumerable<string>)Directory.GetFiles(directoryName, "*.dll"));
                    stringList2.AddRange((IEnumerable<string>)Directory.GetFiles(directoryName, "*.exe"));
                }
                else
                {
                    foreach (string assemblyPattern in CommandRepository.AssemblyPatterns)
                    {
                        if (assemblyPattern.IndexOf("*") == -1 && assemblyPattern.IndexOf("?") == -1)
                            stringList2.Add(Path.Combine(directoryName, assemblyPattern));
                        else
                            stringList2.AddRange((IEnumerable<string>)Directory.GetFiles(directoryName, assemblyPattern));
                    }
                }
                foreach (string str in stringList2)
                {
                    if (File.Exists(str))
                    {
                        try
                        {
                            Assembly assembly = Assembly.LoadFrom(str);
                            if (assembly != (Assembly)null)
                            {
                                LogHelper.Instance.Log("Load commands from assembly: '{0}'", (object)assembly);
                                stringList1.AddRange((IEnumerable<string>)CommandRepository.FindCommands((IEnumerable<Type>)assembly.GetTypes()));
                            }
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Instance.Log(string.Format("Unable to load assembly '{0}' to scan for commands.", (object)str), ex);
                        }
                    }
                }
                LogHelper.Instance.Log(string.Format("Time to scan for commands: {0}", (object)executionTimer.Elapsed), LogEntryType.Info);
                return stringList1;
            }
        }

        internal static CommandInstance GetCommand(string name)
        {
            if (string.IsNullOrEmpty(name))
                return (CommandInstance)null;
            string upper = name.ToUpper();
            return !CommandRepository.m_commandInstanceCache.ContainsKey(upper) ? (CommandInstance)null : CommandRepository.m_commandInstanceCache[upper];
        }

        internal static ICollection<string> AllCommands
        {
            get => CommandRepository.m_commandInstanceCache.Keys;
        }

        private static List<string> FindCommands(IEnumerable<Type> types)
        {
            List<string> commands = new List<string>();
            foreach (Type type in types)
            {
                foreach (CommandAttribute customAttribute in (CommandAttribute[])type.GetCustomAttributes(typeof(CommandAttribute), false))
                {
                    if (customAttribute.Name != null)
                    {
                        string upper = customAttribute.Name.ToUpper();
                        commands.Add(upper);
                        CommandInstance commandInstance = CommandInstance.GetCommandInstance(type);
                        CommandRepository.m_commandInstanceCache[upper] = commandInstance;
                        if (!string.IsNullOrEmpty(customAttribute.Filter))
                            ((IEnumerable<string>)customAttribute.Filter.Trim().Split('|')).ToList<string>().ForEach(new Action<string>(commandInstance.Filters.Add));
                    }
                }
            }
            return commands;
        }
    }
}
