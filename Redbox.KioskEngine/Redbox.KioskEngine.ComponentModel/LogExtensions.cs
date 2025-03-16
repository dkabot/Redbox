using System;
using System.Runtime.CompilerServices;
using System.Text;
using Redbox.Core;

namespace Redbox.KioskEngine.ComponentModel
{
    public static class LogExtensions
    {
        public static void LogEntireException(
            this LogHelper log,
            string message,
            Exception e,
            [CallerMemberName] string memberName = null)
        {
            var type = e.GetType();
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(string.Format("Exception Details - Type: {0} Caller Member Name: {1}", type,
                memberName));
            stringBuilder.AppendLine(" Message: " + message);
            for (; e != null; e = e.InnerException)
            {
                stringBuilder.AppendLine("Exception Message " + e.Message);
                if (e.StackTrace != null)
                {
                    stringBuilder.AppendLine("Stack Trace");
                    stringBuilder.AppendLine(e.StackTrace);
                }
            }

            log.Log(stringBuilder.ToString());
        }
    }
}