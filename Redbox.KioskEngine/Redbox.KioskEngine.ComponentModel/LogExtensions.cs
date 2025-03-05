using Redbox.Core;
using System;
using System.Runtime.CompilerServices;
using System.Text;

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
      Type type = e.GetType();
      StringBuilder stringBuilder = new StringBuilder();
      stringBuilder.AppendLine(string.Format("Exception Details - Type: {0} Caller Member Name: {1}", (object) type, (object) memberName));
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
