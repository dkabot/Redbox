using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Redbox.UpdateManager.KioskUtil
{
    public static class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Redbox KioskUtil");
            Console.WriteLine("");
            var dictionary = ArgsToDictionary(args);
            if (dictionary.IsOptionSet("shell"))
                ShellHelper.StartProcessAsShellUser(Assembly.GetExecutingAssembly().Location,
                    string.Format("-{0}:\"{1}\"", "setbackground", dictionary.GetOption("setbackground")));
            if (dictionary.IsOptionSet("getbackground"))
                Console.WriteLine("Current background image set to " + BackgroundImage.GetWallpaper());
            if (!dictionary.IsOptionSet("setbackground"))
                return;
            var option = dictionary.GetOption("setbackground");
            Console.WriteLine("Setting background to Path: " + option);
            if (!File.Exists(option))
                return;
            BackgroundImage.SetWallpaper(option);
        }

        private static bool IsOptionSet(this string[] stringList, string option)
        {
            return stringList.ToList().Exists(s => s.Equals(option, StringComparison.CurrentCultureIgnoreCase));
        }

        private static bool IsOptionSet(this Dictionary<string, string> dictList, string option)
        {
            return dictList.ContainsKey(option);
        }

        private static string GetOption(this Dictionary<string, string> dictList, string option)
        {
            return !dictList.IsOptionSet(option) ? string.Empty : dictList[option];
        }

        private static Dictionary<string, string> ArgsToDictionary(string[] args)
        {
            var result = new Dictionary<string, string>();
            var list = args.ToList();
            var option = string.Empty;
            var value = string.Empty;
            var hasValue = false;
            list.ForEach(item =>
            {
                if (hasValue)
                {
                    result[option] = item.Trim();
                    hasValue = false;
                }
                else
                {
                    option = GetParam(item, out hasValue, out value);
                    if (string.IsNullOrEmpty(item))
                    {
                        hasValue = false;
                    }
                    else if (!hasValue)
                    {
                        result[option] = "true";
                    }
                    else if (!string.IsNullOrEmpty(value))
                    {
                        result[option] = value;
                        hasValue = false;
                    }
                    else
                    {
                        string.IsNullOrEmpty(value);
                    }
                }
            });
            return result;
        }

        private static string GetParam(string item, out bool hasValue, out string value)
        {
            hasValue = false;
            value = string.Empty;
            var str = string.Empty;
            item = item.Trim();
            var length1 = item.Length;
            if (item.StartsWith("--"))
                str = length1 <= 2 ? string.Empty : item.Substring(2);
            else if (item.StartsWith("-") || item.StartsWith("/"))
                str = length1 <= 1 ? string.Empty : item.Substring(1);
            if (str.Length == 0)
                return str;
            var length2 = str.Length;
            var length3 = str.IndexOf(':');
            if (length3 > -1)
            {
                hasValue = true;
                if (length2 > length3 + 1)
                    value = str.Substring(length3 + 1);
                str = str.Substring(0, length3);
            }

            var length4 = str.Length;
            var length5 = str.IndexOf('=');
            if (length5 > -1)
            {
                hasValue = true;
                if (length4 > length5 + 1)
                    value = str.Substring(length5 + 1);
                str = str.Substring(0, length5);
            }

            return str;
        }
    }
}