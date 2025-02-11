using Redbox.Core;
using Redbox.GetOpts;
using Redbox.Log.Framework;
using System;
using System.Collections;
using System.Configuration.Install;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.ServiceProcess;
using System.Text;

namespace Redbox.UpdateManager.Daemon
{
    public class Program
    {
        private static readonly ILogger m_logger = (ILogger)LogHelper.Instance.CreateLog4NetLogger(typeof(UpdateManagerService));

        public static void Main(string[] args)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            try
            {
                AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(Program.CurrentDomain_UnhandledException);
                Program.m_logger.Configure(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "updatemgrd-log4net.config"));
                LogHelper.Instance.Logger = Program.m_logger;
                LogHelper.Instance.Log("Configured log4net");
            }
            catch (Exception ex)
            {
                string str = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "..\\logs");
                Directory.CreateDirectory(str);
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.AppendLine(DateTime.Now.ToString());
                stringBuilder.AppendLine("An unhandled exception occurred registering log4net.");
                stringBuilder.AppendLine(ex.Message);
                stringBuilder.AppendLine(ex.StackTrace);
                if (ex.InnerException != null)
                {
                    stringBuilder.AppendLine(ex.InnerException.Message);
                    stringBuilder.AppendLine(ex.InnerException.StackTrace);
                }
                System.IO.File.AppendAllText(Path.Combine(str, "errors.log"), stringBuilder.ToString());
                return;
            }
            try
            {
                LogHelper.Instance.Log("Detecting console mode");
                bool console;
                using (CommandLine to = CommandLine.ParseTo((object)Options.Instance))
                {
                    if (to.HelpRequested || to.Errors.Count > 0)
                        return;
                    using (DefaultConsole defaultConsole = new DefaultConsole())
                    {
                        if (Options.Instance.Register)
                        {
                            if (Program.ExecuteInstaller(true, (IConsole)defaultConsole))
                            {
                                defaultConsole.WriteLine("Service successfully registered.");
                                return;
                            }
                            defaultConsole.WriteLine("Service was not registered.");
                            return;
                        }
                        if (Options.Instance.Unregister)
                        {
                            if (Program.ExecuteInstaller(false, (IConsole)defaultConsole))
                            {
                                defaultConsole.WriteLine("Service successfully unregistered.");
                                return;
                            }
                            defaultConsole.WriteLine("Service was not unregistered.");
                            return;
                        }
                        console = Options.Instance.Console;
                    }
                }
                UpdateManagerService updateManagerService = new UpdateManagerService();
                if (Debugger.IsAttached | console)
                {
                    LogHelper.Instance.Log("Debugger.IsAttached({0} || ConsoleMode({1})", (object)Debugger.IsAttached, (object)console);
                    LogHelper.Instance.Log("Starting service manually");
                    updateManagerService.StartService();
                }
                else
                {
                    LogHelper.Instance.Log("Starting service with ServiceBase.Run");
                    ServiceBase.Run((ServiceBase[])new UpdateManagerService[1]
                    {
            updateManagerService
                    });
                }
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("Unhandled exception occurred in Program.Main", ex);
            }
        }

        private static void CurrentDomain_UnhandledException(
          object sender,
          UnhandledExceptionEventArgs e)
        {
            LogHelper.Instance.Log("Unhandled CurrentDomain Exception. Exiting Application", (Exception)e.ExceptionObject);
        }

        private static bool ExecuteInstaller(bool install, IConsole console)
        {
            try
            {
                using (AssemblyInstaller assemblyInstaller = new AssemblyInstaller(typeof(Program).Assembly, new string[0]))
                {
                    assemblyInstaller.UseNewContext = true;
                    IDictionary dictionary = (IDictionary)new Hashtable();
                    try
                    {
                        if (install)
                            assemblyInstaller.Install(dictionary);
                        else
                            assemblyInstaller.Uninstall(dictionary);
                        assemblyInstaller.Commit(dictionary);
                        return true;
                    }
                    catch (Exception ex)
                    {
                        assemblyInstaller.Rollback(dictionary);
                        console.WriteLine((object)ex);
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                console.WriteLine((object)ex);
                LogHelper.Instance.Log("Unhandled exception occurred in Program.ExecuteInstaller", ex);
                return false;
            }
        }
    }
}
