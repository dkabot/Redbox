using Redbox.HAL.Component.Model;
using System;

namespace HALUtilities
{
  internal sealed class ConsoleLogger : ILogger
  {
    private readonly bool EnableDebug;

    public void Log(string message, Exception e) => Console.WriteLine(message);

    public void Log(string message, LogEntryType type) => Console.WriteLine(message);

    public void Log(string message, Exception e, LogEntryType type) => Console.WriteLine(message);

    public bool IsLevelEnabled(LogEntryType entryLevel)
    {
      return LogEntryType.Debug != entryLevel || this.EnableDebug;
    }

    internal ConsoleLogger(bool enableDebug)
    {
      this.EnableDebug = enableDebug;
      ServiceLocator.Instance.AddService<ILogger>((object) this);
    }
  }
}
