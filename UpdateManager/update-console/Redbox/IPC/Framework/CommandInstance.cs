using Redbox.Command.Tokenizer;
using Redbox.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace Redbox.IPC.Framework
{
    internal class CommandInstance
    {
        private List<string> _filters = new List<string>();
        public readonly IDictionary<string, FormMethod> FormMethodCache = (IDictionary<string, FormMethod>)new Dictionary<string, FormMethod>();

        public static CommandInstance GetCommandInstance(Type commandType)
        {
            CommandInstance commandInstance = new CommandInstance()
            {
                CommandType = commandType
            };
            if (Attribute.GetCustomAttribute((MemberInfo)commandType, typeof(DescriptionAttribute)) is DescriptionAttribute customAttribute1)
                commandInstance.CommandDescription = customAttribute1.Description;
            foreach (MethodInfo method in commandType.GetMethods())
            {
                NotLoggableAttribute customAttribute2 = Attribute.GetCustomAttribute((MemberInfo)method, typeof(NotLoggableAttribute)) as NotLoggableAttribute;
                if (Attribute.GetCustomAttribute((MemberInfo)method, typeof(CommandFormAttribute)) is CommandFormAttribute customAttribute6)
                {
                    ParameterInfo[] parameters = method.GetParameters();
                    if (parameters.Length != 0 && !(parameters[0].ParameterType != typeof(CommandContext)))
                    {
                        UsageAttribute customAttribute3 = Attribute.GetCustomAttribute((MemberInfo)method, typeof(UsageAttribute)) as UsageAttribute;
                        DescriptionAttribute customAttribute4 = Attribute.GetCustomAttribute((MemberInfo)method, typeof(DescriptionAttribute)) as DescriptionAttribute;
                        FormMethod formMethod = new FormMethod()
                        {
                            Method = method,
                            Usage = customAttribute3 != null ? customAttribute3.Template : string.Empty,
                            Description = customAttribute4 != null ? customAttribute4.Description : string.Empty,
                            Loggable = customAttribute2 == null
                        };
                        if (!string.IsNullOrEmpty(customAttribute6.Filter))
                            ((IEnumerable<string>)customAttribute6.Filter.Trim().Split('|')).ToList<string>().ForEach(new Action<string>(formMethod.Filters.Add));
                        int num = 0;
                        foreach (ParameterInfo element in parameters)
                        {
                            if (!(element.ParameterType == typeof(CommandContext)))
                            {
                                FormMethodParameter formMethodParameter = new FormMethodParameter()
                                {
                                    Parameter = element,
                                    Index = num++
                                };
                                CommandKeyValueAttribute customAttribute5 = Attribute.GetCustomAttribute(element, typeof(CommandKeyValueAttribute)) as CommandKeyValueAttribute;
                                formMethodParameter.KeyName = element.Name.ToUpper();
                                if (customAttribute5 != null)
                                {
                                    formMethodParameter.MetaData = customAttribute5;
                                    if (customAttribute5.KeyName != null)
                                        formMethodParameter.KeyName = customAttribute5.KeyName.ToUpper();
                                }
                                formMethod.ParameterCache[formMethodParameter.KeyName] = formMethodParameter;
                            }
                        }
                        formMethod.Compile(commandInstance.CommandType);
                        commandInstance.FormMethodCache[customAttribute6.Name.ToUpper()] = formMethod;
                    }
                }
            }
            MethodInfo method1 = commandType.GetMethod("Default", new Type[1]
            {
        typeof (CommandContext)
            });
            if (method1 != (MethodInfo)null)
            {
                FormMethod formMethod = new FormMethod()
                {
                    Method = method1
                };
                formMethod.Compile(commandInstance.CommandType);
                commandInstance.FormMethodCache["Default"] = formMethod;
            }
            return commandInstance;
        }

        public object GetInstance() => Activator.CreateInstance(this.CommandType);

        public bool HasDefault() => this.FormMethodCache.ContainsKey("Default");

        public bool HasOnlyDefault() => this.FormMethodCache.Count == 1 && this.HasDefault();

        public void InvokeDefault(
          CommandResult result,
          CommandContext context,
          CommandTokenizer tokenizer,
          bool enable,
          List<string> sourceFilters)
        {
            if (!this.HasDefault())
                return;
            this.FormMethodCache["Default"].Invoke(result, context, tokenizer, this.GetInstance(), enable, sourceFilters);
        }

        public FormMethod GetMethod(string formName)
        {
            if (string.IsNullOrEmpty(formName))
                return (FormMethod)null;
            string upper = formName.ToUpper();
            return this.FormMethodCache.ContainsKey(upper) ? this.FormMethodCache[upper] : (FormMethod)null;
        }

        public Type CommandType { get; set; }

        public string CommandDescription { get; set; }

        public bool IsInFilter(bool enable, List<string> sourceFilters)
        {
            if (!enable)
                return true;
            return sourceFilters != null && sourceFilters.Count != 0 && this._filters.Count != 0 && sourceFilters.Intersect<string>((IEnumerable<string>)this._filters, (IEqualityComparer<string>)StringComparer.CurrentCultureIgnoreCase).Count<string>() > 0;
        }

        public List<string> Filters => this._filters;
    }
}
