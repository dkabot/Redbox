using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Redbox.Core
{
    internal class LogHelper
    {
        private List<ILogger> m_loggers;

        public static LogHelper Instance => Singleton<LogHelper>.Instance;

        public void LogTo(string loggerName, string message)
        {
            ILogger logger = this.Loggers.Find((Predicate<ILogger>)(each => each.LoggerName == loggerName));
            if (logger == null)
                return;
            logger.Log(message);
            try
            {
                this.OnLogged(message);
            }
            catch (Exception ex)
            {
            }
        }

        public void LogTo(string loggerName, string message, Exception e)
        {
            ILogger logger = this.Loggers.Find((Predicate<ILogger>)(each => each.LoggerName == loggerName));
            if (logger == null)
                return;
            logger.Log(message, e);
            try
            {
                this.OnLogged(string.Format(message, (object)e));
            }
            catch (Exception ex)
            {
                this.OnLogged(message + "\n" + (object)e);
            }
        }

        public void Log(string message, Exception e)
        {
            if (this.Logger == null)
                return;
            this.Logger.Log(message, e);
            try
            {
                this.OnLogged(string.Format(message, (object)e));
            }
            catch (Exception ex)
            {
                this.OnLogged(message + "\n" + (object)e);
            }
        }

        public void Log(string message, LogEntryType type)
        {
            if (this.Logger == null)
                return;
            this.Logger.Log(message, type);
        }

        public void Log(string message, params object[] parms)
        {
            if (this.Logger == null)
                return;
            this.Logger.Log(message, parms);
            try
            {
                this.OnLogged(string.Format(message, parms));
            }
            catch (Exception ex)
            {
                this.OnLogged(message + "\n" + parms.ToJson());
            }
        }

        public void Log(string message, Exception e, params object[] parms)
        {
            if (this.Logger == null)
                return;
            this.Logger.Log(string.Format(message, parms), e);
            try
            {
                this.OnLogged(string.Format(string.Format(message, parms), (object)e));
            }
            catch (Exception ex)
            {
                this.OnLogged(message + "\n" + parms.ToJson() + "\n" + (object)e);
            }
        }

        public void Log(string message, Exception e, LogEntryType type)
        {
            if (this.Logger == null)
                return;
            this.Logger.Log(message, e, type);
        }

        public void LogStart() => this.Log("{0} - start...", (object)LogHelper.CallingFunction);

        public void LogFinish() => this.Log("{0} - finish...", (object)LogHelper.CallingFunction);

        public void LogFunctionStep(string mask, params object[] parms)
        {
            this.Log(string.Format("{0} - {1}", (object)LogHelper.CallingFunction, (object)string.Format(mask, parms)));
        }

        public void LogFailure(string operation, string reason)
        {
            this.Log(string.Format("{0}: Fail, Reason: {1}", (object)operation, (object)reason));
        }

        public void LogError(string code, string description)
        {
            this.Log(string.Format("{0} - error: {1}; '{2}'", (object)LogHelper.CallingFunction, (object)code, (object)description));
        }

        public void LogException(string message, Exception e)
        {
            this.Log(string.Format("{0} - {1}, {2}, '{3}'", (object)LogHelper.CallingFunction, (object)message, (object)e.GetType(), (object)e.Message));
        }

        internal void LogError(string callingfunction, string code, string description)
        {
            this.Log(string.Format("{0} - error: {1}; '{2}'", (object)callingfunction, (object)code, (object)description));
        }

        internal void LogException(string callingfunction, string message, Exception e)
        {
            this.Log(string.Format("{0} - {1}, {2}, '{3}'", (object)callingfunction, (object)message, (object)e.GetType(), (object)e.Message));
        }

        public List<ILogger> Loggers
        {
            get
            {
                if (this.m_loggers == null)
                    this.m_loggers = new List<ILogger>();
                return this.m_loggers;
            }
            set => this.m_loggers = value;
        }

        public ILogger Logger
        {
            get => this.Loggers == null || this.Loggers.Count <= 0 ? (ILogger)null : this.Loggers[0];
            set => this.Loggers.Add(value);
        }

        private LogHelper()
        {
        }

        internal static string CallingFunction
        {
            get
            {
                StackFrame frame = new StackTrace(2).GetFrame(0);
                return frame.GetMethod().DeclaringType.FullName + "." + frame.GetMethod().Name;
            }
        }

        public event LoggedEventHandler Logged;

        public void OnLogged(string message)
        {
            if (this.Logged == null)
                return;
            this.Logged(message);
        }
    }
}
