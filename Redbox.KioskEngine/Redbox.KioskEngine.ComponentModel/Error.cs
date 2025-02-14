using System;
using System.Text;

namespace Redbox.KioskEngine.ComponentModel
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