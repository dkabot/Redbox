using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Attributes;
using Redbox.HAL.Component.Model.Timers;
using Redbox.IPC.Framework;

namespace Redbox.HAL.IPC.Framework
{
    [Command("command-repository")]
    [Description(
        "The COMMAND-REPOSITORY command scans the execution directory of the cache service for .NET assembly files (.dll and .exe) that contain types marked with the CommandAttribute class.")]
    public class CommandRepository
    {
        private static readonly IDictionary<string, Type> m_commands = new Dictionary<string, Type>();

        private static readonly IDictionary<Type, CommandInstance> m_instanceCache =
            new Dictionary<Type, CommandInstance>();

        internal static ICollection<string> AllCommands => m_commands.Keys;

        public void Default(CommandContext context)
        {
            foreach (var installedCommand in DiscoverInstalledCommands())
                context.Messages.Add("Installed Command '" + installedCommand + "'.");
        }

        internal static List<string> DiscoverInstalledCommands()
        {
            using (var executionTimer = new ExecutionTimer())
            {
                var stringList1 = new List<string>();
                var directoryName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                var stringList2 = new List<string>();
                stringList2.AddRange(Directory.GetFiles(directoryName, "*.dll"));
                foreach (var assemblyFile in stringList2)
                    try
                    {
                        var assembly = Assembly.LoadFrom(assemblyFile);
                        stringList1.AddRange(FindCommands(assembly.GetTypes()));
                    }
                    catch (BadImageFormatException ex)
                    {
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Instance.Log(
                            string.Format("Unable to load assembly '{0}' to scan for commands.", assemblyFile), ex);
                    }

                LogHelper.Instance.Log(string.Format("Time to scan for commands: {0}", executionTimer.Elapsed));
                return stringList1;
            }
        }

        internal static CommandInstance GetCommand(string name)
        {
            if (string.IsNullOrEmpty(name))
                return null;
            Type type;
            if (!m_commands.TryGetValue(name.ToUpper(), out type))
                return null;
            CommandInstance command;
            if (!m_instanceCache.TryGetValue(type, out command))
            {
                command = new CommandInstance
                {
                    CommandType = type
                };
                if (Attribute.GetCustomAttribute(type, typeof(DescriptionAttribute)) is DescriptionAttribute
                    customAttribute1)
                    command.CommandDescription = customAttribute1.Description;
                foreach (var method in type.GetMethods())
                    if (Attribute.GetCustomAttribute(method, typeof(CommandFormAttribute)) is CommandFormAttribute
                        customAttribute5)
                    {
                        var parameters = method.GetParameters();
                        if (parameters.Length != 0 && parameters[0].ParameterType == typeof(CommandContext))
                        {
                            var customAttribute2 =
                                Attribute.GetCustomAttribute(method, typeof(UsageAttribute)) as UsageAttribute;
                            var customAttribute3 =
                                Attribute.GetCustomAttribute(method, typeof(DescriptionAttribute)) as
                                    DescriptionAttribute;
                            var formMethod = new FormMethod
                            {
                                Method = method,
                                Usage = customAttribute2 != null ? customAttribute2.Template : string.Empty,
                                Description = customAttribute3 != null ? customAttribute3.Description : string.Empty
                            };
                            var num = 0;
                            foreach (var element in parameters)
                                if (element.ParameterType != typeof(CommandContext))
                                {
                                    var formMethodParameter = new FormMethodParameter
                                    {
                                        Parameter = element,
                                        Index = num++
                                    };
                                    var customAttribute4 =
                                        Attribute.GetCustomAttribute(element, typeof(CommandKeyValueAttribute)) as
                                            CommandKeyValueAttribute;
                                    formMethodParameter.KeyName = element.Name.ToUpper();
                                    if (customAttribute4 != null)
                                    {
                                        formMethodParameter.MetaData = customAttribute4;
                                        if (customAttribute4.KeyName != null)
                                            formMethodParameter.KeyName = customAttribute4.KeyName.ToUpper();
                                        formMethod.ParameterCache[formMethodParameter.KeyName] = formMethodParameter;
                                    }
                                    else
                                    {
                                        formMethod.ParameterCache[formMethodParameter.KeyName] = formMethodParameter;
                                    }
                                }

                            command.FormMethodCache[customAttribute5.Name.ToUpper()] = formMethod;
                        }
                    }

                var method1 = type.GetMethod("Default", new Type[1]
                {
                    typeof(CommandContext)
                });
                if (method1 != null)
                    command.FormMethodCache["Default"] = new FormMethod
                    {
                        Method = method1
                    };
                m_instanceCache[type] = command;
            }

            return command;
        }

        internal static void Register(IEnumerable<Type> types)
        {
            FindCommands(types);
            LogHelper.Instance.Log("Installed commands:");
            foreach (var allCommand in AllCommands)
                LogHelper.Instance.Log(" Command '{0}'.", allCommand);
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
                    m_commands[upper] = type;
                }

            return commands;
        }
    }
}