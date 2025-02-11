using System;

namespace Redbox.HAL.Component.Model
{
    public sealed class LogHelper
    {
        private LogHelper()
        {
        }

        public static LogHelper Instance { get; } = new LogHelper();

        public void Log(string msg)
        {
            Log(msg, LogEntryType.Info);
        }

        public void Log(string fmt, params object[] stuff)
        {
            if (ServiceLocator.Instance.GetService<ILogger>() == null)
                return;
            Log(string.Format(fmt, stuff), LogEntryType.Info);
        }

        public void Log(string message, Exception e)
        {
            ServiceLocator.Instance.GetService<ILogger>()?.Log(message, e);
        }

        public void Log(string message, LogEntryType type)
        {
            ServiceLocator.Instance.GetService<ILogger>()?.Log(message, type);
        }

        public void Log(string message, Exception e, LogEntryType type)
        {
            ServiceLocator.Instance.GetService<ILogger>()?.Log(message, e, type);
        }

        public void Log(LogEntryType type, string message, params object[] args)
        {
            var service = ServiceLocator.Instance.GetService<ILogger>();
            if (service == null || !IsLevelEnabled(type))
                return;
            var message1 = string.Format(message, args);
            service.Log(message1, type);
        }

        public void WithContext(string fmt, params object[] stuff)
        {
            WithContext(LogEntryType.Info, fmt, stuff);
        }

        public void WithContext(LogEntryType type, string msg)
        {
            WithContext(true, type, msg);
        }

        public void WithContext(LogEntryType type, string fmt, params object[] stuff)
        {
            WithContext(type, string.Format(fmt, stuff));
        }

        public void WithContext(
            bool toFormattedLog,
            LogEntryType type,
            string fmt,
            params object[] stuff)
        {
            WithContext(toFormattedLog, type, string.Format(fmt, stuff));
        }

        public void WithContext(bool toFormattedLog, LogEntryType type, string msg)
        {
            var service = ServiceLocator.Instance.GetService<IExecutionService>();
            if (service == null)
            {
                Log(type, msg);
            }
            else
            {
                var activeContext = service.GetActiveContext();
                if (activeContext == null)
                {
                    Log(type, msg);
                }
                else
                {
                    Log(type, "[{0}, {1}]: {2}", activeContext.ProgramName, activeContext.ID, msg);
                    if (!toFormattedLog)
                        return;
                    activeContext.ContextLog.WriteFormatted(msg);
                }
            }
        }

        public bool IsLevelEnabled(LogEntryType logEntryLevel)
        {
            var service = ServiceLocator.Instance.GetService<ILogger>();
            return service != null && service.IsLevelEnabled(logEntryLevel);
        }
    }
}