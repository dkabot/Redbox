using Redbox.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

namespace Redbox.GetOpts
{
    internal class CommandLine : IDisposable
    {
        internal const char SingleDash = '-';
        internal const string DoubleDash = "--";
        internal const char ColonSeparator = ':';
        internal const char EqualSeparator = '=';
        internal const char FlagEnabled = '+';
        internal const char FlagDisabled = '-';
        private UsageAttribute m_usage;
        private Stack<string> m_errors;
        private OptionValueList m_options;
        private readonly object m_container;
        private readonly IConsole m_console;
        private List<string> m_bareArguments;
        private List<string> m_providedOptions;

        public static CommandLine ParseTo(object container)
        {
            return CommandLine.ParseTo(container, (IConsole)new DefaultConsole(), (string[])null);
        }

        public static CommandLine ParseTo(object container, IConsole console, string[] arguments)
        {
            CommandLine to = new CommandLine(container, console);
            to.GetUsageAttribute();
            string[] strArray = arguments ?? CommandLine.GetCommandLineArguments();
            ReadOnlyCollection<string> requiredOptions = to.Options.GetRequiredOptions();
            if (strArray.Length - 1 <= 0)
            {
                if (requiredOptions.Count > 0)
                    to.Errors.Push("Required parameters are missing.");
                return to;
            }
            int index = 0;
            OptionValue optionValue = (OptionValue)null;
            do
            {
                string str1 = strArray[index];
                if (str1 == "--help" || str1 == "-?")
                {
                    to.HelpRequested = true;
                    to.WriteUsage(true);
                    break;
                }
                string str2 = str1;
                char ch = '-';
                string str3 = ch.ToString();
                if (str2.StartsWith(str3))
                {
                    string str4 = str1;
                    ch = '-';
                    string str5 = ch.ToString();
                    if (str4 != str5 && str1 != "--")
                    {
                        if (to.ProcessStandardForm(ref optionValue, str1))
                            goto label_14;
                        else
                            break;
                    }
                }
                if (optionValue == null)
                {
                    to.BareArguments.Add(str1);
                }
                else
                {
                    string key = str1;
                    object parameterValue = (object)str1;
                    if ((!optionValue.IsDictionary() || to.ProcessDictionaryValues(str1, (string)parameterValue, out key, out parameterValue)) && to.SetOptionValue(optionValue, parameterValue, key))
                    {
                        to.ProvidedOptions.Add(optionValue.GetName());
                        optionValue = (OptionValue)null;
                    }
                    else
                        break;
                }
            label_14:
                ++index;
            }
            while (index != strArray.Length);
            if (new List<string>(to.ProvidedOptions.Intersect<string>((IEnumerable<string>)requiredOptions)).Count != requiredOptions.Count)
                to.Errors.Push("Required parameters are missing.");
            return to;
        }

        public void WriteUsage(bool includeHelp)
        {
            Assembly entryAssembly = Assembly.GetEntryAssembly();
            AssemblyTitleAttribute customAttribute1 = (AssemblyTitleAttribute)Attribute.GetCustomAttribute(entryAssembly, typeof(AssemblyTitleAttribute));
            AssemblyCopyrightAttribute customAttribute2 = (AssemblyCopyrightAttribute)Attribute.GetCustomAttribute(entryAssembly, typeof(AssemblyCopyrightAttribute));
            AssemblyTrademarkAttribute customAttribute3 = (AssemblyTrademarkAttribute)Attribute.GetCustomAttribute(entryAssembly, typeof(AssemblyTrademarkAttribute));
            if (customAttribute1 != null)
                this.m_console.Write(customAttribute1.Title);
            this.m_console.Write(" " + (object)entryAssembly.GetName().Version);
            this.m_console.Write(Environment.NewLine);
            if (customAttribute2 != null && customAttribute2.Copyright.Length > 0)
                this.m_console.Write(customAttribute2.Copyright + Environment.NewLine);
            if (customAttribute3 != null && customAttribute3.Trademark.Length > 0)
                this.m_console.Write(customAttribute3.Trademark + Environment.NewLine);
            if (this.m_usage != null)
                this.m_console.WriteLine("\nUsage: {0}\n", (object)this.m_usage.Template);
            foreach (string error in this.Errors)
                this.m_console.WriteLine(error);
            if (this.Errors.Count > 0)
                this.m_console.WriteLine("\nTry --help or -? for more information.\n");
            if (!includeHelp)
                return;
            this.m_console.WriteLine("Options:\n");
            this.Options.WriteOptions(this.m_console);
        }

        public void Dispose()
        {
            if (this.m_console == null)
                return;
            this.m_console.Dispose();
        }

        public IList<string> BareArguments
        {
            get
            {
                if (this.m_bareArguments == null)
                    this.m_bareArguments = new List<string>();
                return (IList<string>)this.m_bareArguments;
            }
        }

        public Stack<string> Errors
        {
            get
            {
                if (this.m_errors == null)
                    this.m_errors = new Stack<string>();
                return this.m_errors;
            }
        }

        public static string[] GetCommandLineArguments() => Environment.GetCommandLineArgs();

        public bool HelpRequested { get; internal set; }

        internal CommandLine(object container, IConsole console)
        {
            this.m_container = container;
            this.m_console = console;
        }

        internal void GetUsageAttribute()
        {
            this.m_usage = (UsageAttribute)Attribute.GetCustomAttribute((MemberInfo)this.m_container.GetType(), typeof(UsageAttribute));
        }

        internal static bool GetFlagNameAndValue(string parameter, out string name)
        {
            int dashCount = CommandLine.GetDashCount((IEnumerable<char>)parameter);
            if (parameter.Length == dashCount)
            {
                name = (string)null;
                return false;
            }
            name = parameter.Substring(dashCount, parameter.Length - (dashCount + 1));
            string str1 = parameter;
            if (str1[str1.Length - 1] == '+')
                return true;
            string str2 = parameter;
            if (str2[str2.Length - 1] == '-')
                return false;
            name = (string)null;
            return false;
        }

        internal static object GetValues(string parameter, out string name, char[] separators)
        {
            if (parameter == null)
            {
                name = (string)null;
                return (object)null;
            }
            object values = (object)null;
            int dashCount = CommandLine.GetDashCount((IEnumerable<char>)parameter);
            string str = parameter.Substring(dashCount);
            int length = -1;
            foreach (char separator in separators)
            {
                length = str.IndexOf(separator);
                if (length != -1)
                    break;
            }
            if (length != -1)
            {
                name = str.Substring(0, length);
                values = (object)str.Substring(length + 1, str.Length - (length + 1));
            }
            else
                name = str;
            return values;
        }

        internal bool SetOptionValue(OptionValue optionValue, object value, string argument)
        {
            try
            {
                optionValue.SetValue((object)argument, value);
                return true;
            }
            catch (Exception ex)
            {
                this.Errors.Push("Parameter '" + argument + "': " + ex.Message);
                return false;
            }
        }

        internal static int GetDashCount(IEnumerable<char> parameter)
        {
            int dashCount = 0;
            foreach (char ch in parameter)
            {
                if (ch == '-')
                    ++dashCount;
                else
                    break;
            }
            return dashCount;
        }

        internal bool ProcessStandardForm(ref OptionValue optionValue, string argument)
        {
            int dashCount = CommandLine.GetDashCount((IEnumerable<char>)argument);
            if (optionValue != null)
            {
                this.Errors.Push("The parameter '" + optionValue.GetName() + "' was not properly set.");
                return false;
            }
            string name;
            bool flagNameAndValue = CommandLine.GetFlagNameAndValue(argument, out name);
            object obj;
            if (name == null)
                obj = CommandLine.GetValues(argument, out name, new char[2]
                {
          ':',
          '='
                });
            else
                obj = (object)flagNameAndValue;
            object parameterValue = obj;
            if (name.Length == 1 && dashCount > 1)
            {
                this.Errors.Push("The parameter '" + argument + "' is short form and does not support the double dash prefix.");
                return false;
            }
            optionValue = this.Options.GetOptionValue(name);
            if (optionValue == null)
            {
                this.Errors.Push("The parameter '" + argument + "' is not valid.");
                return false;
            }
            if (optionValue.IsDictionary() && !this.ProcessDictionaryValues(argument, (string)parameterValue, out name, out parameterValue))
                return false;
            if (parameterValue != null)
            {
                if (!this.SetOptionValue(optionValue, parameterValue, name))
                    return false;
                this.ProvidedOptions.Add(optionValue.GetName());
                optionValue = (OptionValue)null;
            }
            else if (optionValue.IsFlag())
            {
                optionValue.SetValue((object)null, (object)true);
                this.ProvidedOptions.Add(optionValue.GetName());
                optionValue = (OptionValue)null;
            }
            return true;
        }

        internal bool ProcessDictionaryValues(
          string argument,
          string parameterValue,
          out string key,
          out object value)
        {
            value = CommandLine.GetValues(parameterValue, out key, new char[1]
            {
        '='
            });
            if (value != null && key != null || key == null)
                return true;
            this.Errors.Push("The syntax of parameter '" + argument + "' is incorrect.");
            return false;
        }

        internal List<string> ProvidedOptions
        {
            get
            {
                if (this.m_providedOptions == null)
                    this.m_providedOptions = new List<string>();
                return this.m_providedOptions;
            }
        }

        internal OptionValueList Options
        {
            get
            {
                if (this.m_options == null)
                    this.m_options = OptionValue.GetOptionValues(this.m_container);
                return this.m_options;
            }
        }
    }
}
