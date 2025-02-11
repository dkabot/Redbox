using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Redbox.Core
{
    public class LogHelper
    {
        private List<ILogger> m_loggers;

        private LogHelper()
        {
        }

        public static LogHelper Instance => Singleton<LogHelper>.Instance;

        public List<ILogger> Loggers
        {
            get
            {
                if (m_loggers == null)
                    m_loggers = new List<ILogger>();
                return m_loggers;
            }
            set => m_loggers = value;
        }

        public ILogger Logger
        {
            get => Loggers == null || Loggers.Count <= 0 ? null : Loggers[0];
            set => Loggers.Add(value);
        }

        internal static string CallingFunction
        {
            get
            {
                var frame = new StackTrace(2).GetFrame(0);
                return frame.GetMethod().DeclaringType.FullName + "." + frame.GetMethod().Name;
            }
        }

        public void LogTo(string loggerName, string message)
        {
            var logger = Loggers.Find(each => each.LoggerName == loggerName);
            if (logger == null)
                return;
            logger.Log(message);
            try
            {
                OnLogged(message);
            }
            catch (Exception ex)
            {
            }
        }

        public void LogTo(string loggerName, string message, Exception e)
        {
            var logger = Loggers.Find(each => each.LoggerName == loggerName);
            if (logger == null)
                return;
            logger.Log(message, e);
            try
            {
                OnLogged(string.Format(message, e));
            }
            catch (Exception ex)
            {
                OnLogged(message + "\n" + e);
            }
        }

        public void Log(string message, Exception e)
        {
            if (Logger == null)
                return;
            Logger.Log(message, e);
            try
            {
                OnLogged(string.Format(message, e));
            }
            catch (Exception ex)
            {
                OnLogged(message + "\n" + e);
            }
        }

        public void Log(string message, LogEntryType type)
        {
            if (Logger == null)
                return;
            Logger.Log(message, type);
        }

        public void Log(string message, params object[] parms)
        {
            if (Logger == null)
                return;
            Logger.Log(message, parms);
            try
            {
                OnLogged(string.Format(message, parms));
            }
            catch (Exception ex)
            {
                OnLogged(message + "\n" + parms.ToJson());
            }
        }

        public void Log(string message, Exception e, params object[] parms)
        {
            if (Logger == null)
                return;
            Logger.Log(string.Format(message, parms), e);
            try
            {
                OnLogged(string.Format(string.Format(message, parms), e));
            }
            catch (Exception ex)
            {
                OnLogged(message + "\n" + parms.ToJson() + "\n" + e);
            }
        }

        public void Log(string message, Exception e, LogEntryType type)
        {
            if (Logger == null)
                return;
            Logger.Log(message, e, type);
        }

        public void LogStart()
        {
            Log("{0} - start...", CallingFunction);
        }

        public void LogFinish()
        {
            Log("{0} - finish...", CallingFunction);
        }

        public void LogFunctionStep(string mask, params object[] parms)
        {
            Log(string.Format("{0} - {1}", CallingFunction, string.Format(mask, parms)));
        }

        public void LogFailure(string operation, string reason)
        {
            Log(string.Format("{0}: Fail, Reason: {1}", operation, reason));
        }

        public void LogError(string code, string description)
        {
            Log(string.Format("{0} - error: {1}; '{2}'", CallingFunction, code, description));
        }

        public void LogException(string message, Exception e)
        {
            Log(string.Format("{0} - {1}, {2}, '{3}'", CallingFunction, message, e.GetType(), e.Message));
        }

        public void LogFullException(string message, Exception e)
        {
            var type = e.GetType();
            var stringBuilder = new StringBuilder();
            for (; e != null; e = e.InnerException)
                stringBuilder.Append(e.Message);
            Log(string.Format("{0} - {1}, {2}, '{3}'", CallingFunction, message, type, stringBuilder));
        }

        internal void LogError(string callingfunction, string code, string description)
        {
            Log(string.Format("{0} - error: {1}; '{2}'", callingfunction, code, description));
        }

        internal void LogException(string callingfunction, string message, Exception e)
        {
            Log(string.Format("{0} - {1}, {2}, '{3}'", callingfunction, message, e.GetType(), e.Message));
        }

        public event LoggedEventHandler Logged;

        public void OnLogged(string message)
        {
            if (Logged == null)
                return;
            Logged(message);
        }
    }
}