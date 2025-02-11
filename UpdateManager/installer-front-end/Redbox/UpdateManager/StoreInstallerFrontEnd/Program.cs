using Redbox.Core;
using Redbox.GetOpts;
using Redbox.Log.Framework;
using Redbox.UpdateManager.ComponentModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace Redbox.UpdateManager.StoreInstallerFrontEnd
{
    public class Program : IDisposable
    {
        private static readonly ILogger m_logger = (ILogger)LogHelper.Instance.CreateLog4NetLogger(typeof(Program));
        private static CommandLine m_commandLine;

        [STAThread]
        public static int Main()
        {
            using (Program program = new Program())
                return program.Run();
        }

        public int Run()
        {
            ErrorList errorList = new ErrorList();
            if (Program.CommandLine.Errors.Count > 0 && !Program.CommandLine.HelpRequested)
            {
                Program.CommandLine.WriteUsage(false);
                Console.Error.WriteLine("Command Line errors forcing exit.");
                return 1;
            }
            if (Program.CommandLine.HelpRequested)
            {
                Program.CommandLine.WriteUsage(true);
                return 0;
            }
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Program.m_logger.Configure(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "installer-front-end-log4net.config"));
                LogHelper.Instance.Logger = Program.m_logger;
                errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)StoreInstallerApplication.Instance.Initialize(Program.Opts.Script, Program.Opts.StoreNumber));
            }
            catch (Exception ex)
            {
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("E999", "An unhandled exception was thrown in Program.Run.", ex));
            }
            if (errorList.ContainsCode("INSTANCE"))
                return -1;
            if (errorList.ContainsError())
            {
                int num = (int)new ErrorForm()
                {
                    Errors = ((IEnumerable)errorList)
                }.ShowDialog();
                return -2;
            }
            if (!StoreInstallerApplication.Instance.Run())
                return -3;
            Application.Run((ApplicationContext)StoreInstallerApplication.Instance);
            return !StoreInstallerApplication.Instance.ScriptCompleted ? -1 : 0;
        }

        public void Dispose()
        {
        }

        private static CommandLine CommandLine
        {
            get
            {
                if (Program.m_commandLine == null)
                    Program.m_commandLine = CommandLine.ParseTo((object)Program.Opts);
                return Program.m_commandLine;
            }
        }

        private static Options Opts => Singleton<Options>.Instance;
    }
}
