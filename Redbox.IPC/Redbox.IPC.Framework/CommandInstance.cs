using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Redbox.Command.Tokenizer;
using Redbox.Core;

namespace Redbox.IPC.Framework
{
    internal class CommandInstance
    {
        public readonly IDictionary<string, FormMethod> FormMethodCache = new Dictionary<string, FormMethod>();

        public Type CommandType { get; set; }

        public string CommandDescription { get; set; }

        public List<string> Filters { get; } = new List<string>();

        public static CommandInstance GetCommandInstance(Type commandType)
        {
            var commandInstance = new CommandInstance
            {
                CommandType = commandType
            };
            if (Attribute.GetCustomAttribute(commandType, typeof(DescriptionAttribute)) is DescriptionAttribute
                customAttribute1)
                commandInstance.CommandDescription = customAttribute1.Description;
            foreach (var method in commandType.GetMethods())
            {
                var customAttribute2 =
                    Attribute.GetCustomAttribute(method, typeof(NotLoggableAttribute)) as NotLoggableAttribute;
                if (Attribute.GetCustomAttribute(method, typeof(CommandFormAttribute)) is CommandFormAttribute
                    customAttribute6)
                {
                    var parameters = method.GetParameters();
                    if (parameters.Length != 0 && !(parameters[0].ParameterType != typeof(CommandContext)))
                    {
                        var customAttribute3 =
                            Attribute.GetCustomAttribute(method, typeof(UsageAttribute)) as UsageAttribute;
                        var customAttribute4 =
                            Attribute.GetCustomAttribute(method, typeof(DescriptionAttribute)) as DescriptionAttribute;
                        var formMethod = new FormMethod
                        {
                            Method = method,
                            Usage = customAttribute3 != null ? customAttribute3.Template : string.Empty,
                            Description = customAttribute4 != null ? customAttribute4.Description : string.Empty,
                            Loggable = customAttribute2 == null
                        };
                        if (!string.IsNullOrEmpty(customAttribute6.Filter))
                            customAttribute6.Filter.Trim().Split('|').ToList().ForEach(formMethod.Filters.Add);
                        var num = 0;
                        foreach (var element in parameters)
                            if (!(element.ParameterType == typeof(CommandContext)))
                            {
                                var formMethodParameter = new FormMethodParameter
                                {
                                    Parameter = element,
                                    Index = num++
                                };
                                var customAttribute5 =
                                    Attribute.GetCustomAttribute(element, typeof(CommandKeyValueAttribute)) as
                                        CommandKeyValueAttribute;
                                formMethodParameter.KeyName = element.Name.ToUpper();
                                if (customAttribute5 != null)
                                {
                                    formMethodParameter.MetaData = customAttribute5;
                                    if (customAttribute5.KeyName != null)
                                        formMethodParameter.KeyName = customAttribute5.KeyName.ToUpper();
                                }

                                formMethod.ParameterCache[formMethodParameter.KeyName] = formMethodParameter;
                            }

                        formMethod.Compile(commandInstance.CommandType);
                        commandInstance.FormMethodCache[customAttribute6.Name.ToUpper()] = formMethod;
                    }
                }
            }

            var method1 = commandType.GetMethod("Default", new Type[1]
            {
                typeof(CommandContext)
            });
            if (method1 != null)
            {
                var formMethod = new FormMethod
                {
                    Method = method1
                };
                formMethod.Compile(commandInstance.CommandType);
                commandInstance.FormMethodCache["Default"] = formMethod;
            }

            return commandInstance;
        }

        public object GetInstance()
        {
            return Activator.CreateInstance(CommandType);
        }

        public bool HasDefault()
        {
            return FormMethodCache.ContainsKey("Default");
        }

        public bool HasOnlyDefault()
        {
            return FormMethodCache.Count == 1 && HasDefault();
        }

        public void InvokeDefault(
            CommandResult result,
            CommandContext context,
            CommandTokenizer tokenizer,
            bool enable,
            List<string> sourceFilters)
        {
            if (!HasDefault())
                return;
            FormMethodCache["Default"].Invoke(result, context, tokenizer, GetInstance(), enable, sourceFilters);
        }

        public FormMethod GetMethod(string formName)
        {
            if (string.IsNullOrEmpty(formName))
                return null;
            var upper = formName.ToUpper();
            return FormMethodCache.ContainsKey(upper) ? FormMethodCache[upper] : null;
        }

        public bool IsInFilter(bool enable, List<string> sourceFilters)
        {
            if (!enable)
                return true;
            return sourceFilters != null && sourceFilters.Count != 0 && Filters.Count != 0 &&
                   sourceFilters.Intersect(Filters, StringComparer.CurrentCultureIgnoreCase).Count() > 0;
        }
    }
}