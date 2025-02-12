using System;
using System.Windows.Forms;
using log4net;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Core;

namespace Redbox.HAL.Management.Console
{
    public class Program : IDisposable, ILogger
    {
        private static readonly ILog m_log = LogManager.GetLogger(typeof(Program));
        private static Guid m_applicationGuid = new Guid("{E3720F11-5B72-4d86-A1A8-445518455D4C}");
        private Core.NamedLock m_instanceLock;

        public static string Version => typeof(Program).Assembly.GetName().Version.ToString();

        public void Dispose()
        {
            if (m_instanceLock == null)
                return;
            m_instanceLock.Dispose();
        }

        public bool IsLevelEnabled(LogEntryType entryLogLevel)
        {
            switch (entryLogLevel)
            {
                case LogEntryType.Info:
                    return m_log.IsInfoEnabled;
                case LogEntryType.Debug:
                    return m_log.IsDebugEnabled;
                case LogEntryType.Error:
                    return m_log.IsErrorEnabled;
                case LogEntryType.Fatal:
                    return m_log.IsFatalEnabled;
                default:
                    return false;
            }
        }

        public void Log(string message, Exception e)
        {
            m_log.Error(message, e);
        }

        public void Log(string message, LogEntryType type)
        {
            switch (type)
            {
                case LogEntryType.Info:
                    m_log.Info(message);
                    break;
                case LogEntryType.Debug:
                    m_log.Debug(message);
                    break;
                case LogEntryType.Error:
                    m_log.Error(message);
                    break;
                case LogEntryType.Fatal:
                    m_log.Fatal(message);
                    break;
            }
        }

        public void Log(string message, Exception e, LogEntryType type)
        {
            switch (type)
            {
                case LogEntryType.Info:
                    m_log.Info(message, e);
                    break;
                case LogEntryType.Debug:
                    m_log.Debug(message, e);
                    break;
                case LogEntryType.Error:
                    m_log.Error(message, e);
                    break;
                case LogEntryType.Fatal:
                    m_log.Fatal(message, e);
                    break;
            }
        }

        public void Configure()
        {
        }

        public void Configure(string path)
        {
        }

        public void ConfigureAndWatch(string path)
        {
        }

        public void Log(string message, params object[] parms)
        {
        }

        public void Run()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }

        [STAThread]
        public static void Main()
        {
            using (var instance = new Program())
            {
                instance.m_instanceLock = new Core.NamedLock(m_applicationGuid.ToString(), LockScope.Local);
                if (!instance.m_instanceLock.IsOwned)
                {
                    var num = (int)MessageBox.Show("Only one instance of the Console is allowed to run.");
                }
                else
                {
                    ServiceLocator.Instance.AddService(typeof(ILogger), instance);
                    instance.Run();
                }
            }
        }
    }
}