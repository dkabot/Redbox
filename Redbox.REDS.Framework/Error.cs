using System;
using System.Text;
using Redbox.Core;

namespace Redbox.REDS.Framework
{
    public class Error
    {
        private Error(string code, string description, string details, bool isWarning)
        {
            Code = code;
            Details = details;
            IsWarning = isWarning;
            Description = description;
        }

        public string Code { get; }

        public string Details { get; }

        public bool IsWarning { get; }

        public string Description { get; }

        public static Error Parse(string error)
        {
            var codeFromBrackets = error.ExtractCodeFromBrackets("[", "]");
            var startIndex = error.IndexOf("]");
            var strArray = error.Substring(startIndex).Split("|".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            var str = (string)null;
            if (strArray.Length > 1)
                str = strArray[1];
            var description = strArray[0].Substring(strArray[0].IndexOf(":") + 1);
            var details = str;
            var num = strArray[0].StartsWith("WARNING") ? 1 : 0;
            return new Error(codeFromBrackets, description, details, num != 0);
        }

        public static Error NewError(string code, string description, Exception e)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append(e);
            if (e.InnerException != null)
                stringBuilder.AppendFormat("\r\n\r\nInner Exception:\r\n---------------------------\r\n{0}\r\n",
                    e.InnerException);
            stringBuilder.AppendFormat("\r\n\r\nStack Trace:\r\n----------------------------\r\n{0}\r\n", e.StackTrace);
            return new Error(code, description, stringBuilder.ToString(), false);
        }

        public static Error NewError(string code, string description, string details)
        {
            return new Error(code, description, details, false);
        }

        public static Error NewWarning(string code, string description, string details)
        {
            return new Error(code, description, details, true);
        }

        public override string ToString()
        {
            return !Description.Contains("ERROR") && !Description.Contains("WARNING")
                ? string.Format("[{0}] {1}: {2}", Code, IsWarning ? "WARNING" : (object)"ERROR", Description)
                : string.Format("[{0}] {1}", Code, Description);
        }
    }
}