using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Redbox.Core;

namespace Redbox.GetOpts
{
    public class CommandLine : IDisposable
    {
        internal const char SingleDash = '-';
        internal const string DoubleDash = "--";
        internal const char ColonSeparator = ':';
        internal const char EqualSeparator = '=';
        internal const char FlagEnabled = '+';
        internal const char FlagDisabled = '-';
        private readonly IConsole m_console;
        private readonly object m_container;
        private List<string> m_bareArguments;
        private Stack<string> m_errors;
        private OptionValueList m_options;
        private List<string> m_providedOptions;
        private UsageAttribute m_usage;

        internal CommandLine(object container, IConsole console)
        {
            m_container = container;
            m_console = console;
        }

        public IList<string> BareArguments
        {
            get
            {
                if (m_bareArguments == null)
                    m_bareArguments = new List<string>();
                return m_bareArguments;
            }
        }

        public Stack<string> Errors
        {
            get
            {
                if (m_errors == null)
                    m_errors = new Stack<string>();
                return m_errors;
            }
        }

        public bool HelpRequested { get; internal set; }

        internal List<string> ProvidedOptions
        {
            get
            {
                if (m_providedOptions == null)
                    m_providedOptions = new List<string>();
                return m_providedOptions;
            }
        }

        internal OptionValueList Options
        {
            get
            {
                if (m_options == null)
                    m_options = OptionValue.GetOptionValues(m_container);
                return m_options;
            }
        }

        public void Dispose()
        {
            if (m_console == null)
                return;
            m_console.Dispose();
        }

        public static CommandLine ParseTo(object container)
        {
            return ParseTo(container, new DefaultConsole(), null);
        }

        public static CommandLine ParseTo(object container, IConsole console, string[] arguments)
        {
            var to = new CommandLine(container, console);
            to.GetUsageAttribute();
            var strArray = arguments ?? GetCommandLineArguments();
            var requiredOptions = to.Options.GetRequiredOptions();
            if (strArray.Length - 1 <= 0)
            {
                if (requiredOptions.Count > 0)
                    to.Errors.Push("Required parameters are missing.");
                return to;
            }

            var index = 0;
            var optionValue = (OptionValue)null;
            do
            {
                var str1 = strArray[index];
                if (str1 == "--help" || str1 == "-?")
                {
                    to.HelpRequested = true;
                    to.WriteUsage(true);
                    break;
                }

                var str2 = str1;
                var ch = '-';
                var str3 = ch.ToString();
                if (str2.StartsWith(str3))
                {
                    var str4 = str1;
                    ch = '-';
                    var str5 = ch.ToString();
                    if (str4 != str5 && str1 != "--")
                    {
                        if (to.ProcessStandardForm(ref optionValue, str1))
                            goto label_14;
                        break;
                    }
                }

                if (optionValue == null)
                {
                    to.BareArguments.Add(str1);
                }
                else
                {
                    var key = str1;
                    var parameterValue = (object)str1;
                    if ((!optionValue.IsDictionary() ||
                         to.ProcessDictionaryValues(str1, (string)parameterValue, out key, out parameterValue)) &&
                        to.SetOptionValue(optionValue, parameterValue, key))
                    {
                        to.ProvidedOptions.Add(optionValue.GetName());
                        optionValue = null;
                    }
                    else
                    {
                        break;
                    }
                }

                label_14:
                ++index;
            } while (index != strArray.Length);

            if (new List<string>(to.ProvidedOptions.Intersect(requiredOptions)).Count != requiredOptions.Count)
                to.Errors.Push("Required parameters are missing.");
            return to;
        }

        public void WriteUsage(bool includeHelp)
        {
            var entryAssembly = Assembly.GetEntryAssembly();
            var customAttribute1 =
                (AssemblyTitleAttribute)Attribute.GetCustomAttribute(entryAssembly, typeof(AssemblyTitleAttribute));
            var customAttribute2 =
                (AssemblyCopyrightAttribute)Attribute.GetCustomAttribute(entryAssembly,
                    typeof(AssemblyCopyrightAttribute));
            var customAttribute3 =
                (AssemblyTrademarkAttribute)Attribute.GetCustomAttribute(entryAssembly,
                    typeof(AssemblyTrademarkAttribute));
            if (customAttribute1 != null)
                m_console.Write(customAttribute1.Title);
            m_console.Write(" " + entryAssembly.GetName().Version);
            m_console.Write(Environment.NewLine);
            if (customAttribute2 != null && customAttribute2.Copyright.Length > 0)
                m_console.Write(customAttribute2.Copyright + Environment.NewLine);
            if (customAttribute3 != null && customAttribute3.Trademark.Length > 0)
                m_console.Write(customAttribute3.Trademark + Environment.NewLine);
            if (m_usage != null)
                m_console.WriteLine("\nUsage: {0}\n", m_usage.Template);
            foreach (var error in Errors)
                m_console.WriteLine(error);
            if (Errors.Count > 0)
                m_console.WriteLine("\nTry --help or -? for more information.\n");
            if (!includeHelp)
                return;
            m_console.WriteLine("Options:\n");
            Options.WriteOptions(m_console);
        }

        public static string[] GetCommandLineArguments()
        {
            return Environment.GetCommandLineArgs();
        }

        internal void GetUsageAttribute()
        {
            m_usage = (UsageAttribute)Attribute.GetCustomAttribute(m_container.GetType(), typeof(UsageAttribute));
        }

        internal static bool GetFlagNameAndValue(string parameter, out string name)
        {
            var dashCount = GetDashCount(parameter);
            if (parameter.Length == dashCount)
            {
                name = null;
                return false;
            }

            name = parameter.Substring(dashCount, parameter.Length - (dashCount + 1));
            if (parameter[parameter.Length - 1] == '+')
                return true;
            if (parameter[parameter.Length - 1] == '-')
                return false;
            name = null;
            return false;
        }

        internal static object GetValues(string parameter, out string name, char[] separators)
        {
            if (parameter == null)
            {
                name = null;
                return null;
            }

            var values = (object)null;
            var dashCount = GetDashCount(parameter);
            var str = parameter.Substring(dashCount);
            var length = -1;
            foreach (var separator in separators)
            {
                length = str.IndexOf(separator);
                if (length != -1)
                    break;
            }

            if (length != -1)
            {
                name = str.Substring(0, length);
                values = str.Substring(length + 1, str.Length - (length + 1));
            }
            else
            {
                name = str;
            }

            return values;
        }

        internal bool SetOptionValue(OptionValue optionValue, object value, string argument)
        {
            try
            {
                optionValue.SetValue(argument, value);
                return true;
            }
            catch (Exception ex)
            {
                Errors.Push("Parameter '" + argument + "': " + ex.Message);
                return false;
            }
        }

        internal static int GetDashCount(IEnumerable<char> parameter)
        {
            var dashCount = 0;
            foreach (var ch in parameter)
                if (ch == '-')
                    ++dashCount;
                else
                    break;
            return dashCount;
        }

        internal bool ProcessStandardForm(ref OptionValue optionValue, string argument)
        {
            var dashCount = GetDashCount(argument);
            if (optionValue != null)
            {
                Errors.Push("The parameter '" + optionValue.GetName() + "' was not properly set.");
                return false;
            }

            string name;
            var flagNameAndValue = GetFlagNameAndValue(argument, out name);
            object obj;
            if (name == null)
                obj = GetValues(argument, out name, new char[2]
                {
                    ':',
                    '='
                });
            else
                obj = flagNameAndValue;
            var parameterValue = obj;
            if (name.Length == 1 && dashCount > 1)
            {
                Errors.Push("The parameter '" + argument +
                            "' is short form and does not support the double dash prefix.");
                return false;
            }

            optionValue = Options.GetOptionValue(name);
            if (optionValue == null)
            {
                Errors.Push("The parameter '" + argument + "' is not valid.");
                return false;
            }

            if (optionValue.IsDictionary() &&
                !ProcessDictionaryValues(argument, (string)parameterValue, out name, out parameterValue))
                return false;
            if (parameterValue != null)
            {
                if (!SetOptionValue(optionValue, parameterValue, name))
                    return false;
                ProvidedOptions.Add(optionValue.GetName());
                optionValue = null;
            }
            else if (optionValue.IsFlag())
            {
                optionValue.SetValue(null, true);
                ProvidedOptions.Add(optionValue.GetName());
                optionValue = null;
            }

            return true;
        }

        internal bool ProcessDictionaryValues(
            string argument,
            string parameterValue,
            out string key,
            out object value)
        {
            value = GetValues(parameterValue, out key, new char[1]
            {
                '='
            });
            if ((value != null && key != null) || key == null)
                return true;
            Errors.Push("The syntax of parameter '" + argument + "' is incorrect.");
            return false;
        }
    }
}