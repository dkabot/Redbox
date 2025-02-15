using System;
using System.IO;
using log4net;
using log4net.Config;
using Redbox.Core;

namespace Redbox.Log.Framework
{
    public class SimpleLog4NetLogger : ILogger
    {
        private readonly Type m_logType;
        private ILog m_log;

        public SimpleLog4NetLogger(Type type)
        {
            m_logType = type;
            LoggerName = nameof(SimpleLog4NetLogger);
        }

        private ILog LoggerInstance
        {
            get
            {
                if (m_log == null)
                    m_log = LogManager.GetLogger(m_logType);
                return m_log;
            }
        }

        public string LoggerName { get; }

        public void Configure()
        {
            XmlConfigurator.Configure();
            LoggerInstance.Info("SimpleLog4NetLogger configured from: app.config.");
        }

        public void Configure(string path)
        {
            XmlConfigurator.Configure(new FileInfo(path));
            LoggerInstance.Info(string.Format("SimpleLog4NetLogger configured from: '{0}'.", path));
        }

        public void ConfigureAndWatch(string path)
        {
            XmlConfigurator.ConfigureAndWatch(new FileInfo(path));
            LoggerInstance.Info(string.Format("SimpleLog4NetLogger configured from: '{0}', file watcher is enabled.",
                path));
        }

        public void Log(string message, Exception e)
        {
            if (!LoggerInstance.IsErrorEnabled)
                return;
            LoggerInstance.Error(message, e);
        }

        public void Log(string message, LogEntryType type)
        {
            switch (type)
            {
                case LogEntryType.Info:
                    if (!LoggerInstance.IsInfoEnabled)
                        break;
                    LoggerInstance.Info(message);
                    break;
                case LogEntryType.Debug:
                    if (!LoggerInstance.IsDebugEnabled)
                        break;
                    LoggerInstance.Debug(message);
                    break;
                case LogEntryType.Error:
                    if (!LoggerInstance.IsErrorEnabled)
                        break;
                    LoggerInstance.Error(message);
                    break;
                case LogEntryType.Fatal:
                    if (!LoggerInstance.IsFatalEnabled)
                        break;
                    LoggerInstance.Fatal(message);
                    break;
            }
        }

        public void Log(string message, params object[] parms)
        {
            if (!LoggerInstance.IsInfoEnabled)
                return;
            if (parms.Length != 0)
                LoggerInstance.Info(string.Format(message, parms));
            else
                LoggerInstance.Info(message);
        }

        public void Log(string message, Exception e, LogEntryType type)
        {
            switch (type)
            {
                case LogEntryType.Info:
                    if (!LoggerInstance.IsInfoEnabled)
                        break;
                    LoggerInstance.Info(message, e);
                    break;
                case LogEntryType.Debug:
                    if (!LoggerInstance.IsDebugEnabled)
                        break;
                    LoggerInstance.Debug(message, e);
                    break;
                case LogEntryType.Error:
                    if (!LoggerInstance.IsErrorEnabled)
                        break;
                    LoggerInstance.Error(message, e);
                    break;
                case LogEntryType.Fatal:
                    if (!LoggerInstance.IsFatalEnabled)
                        break;
                    LoggerInstance.Fatal(message, e);
                    break;
            }
        }
    }
}