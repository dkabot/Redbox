using Redbox.Core;
using Redbox.Log.Framework;
using Redbox.UpdateManager.ComponentModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace Redbox.UpdateManager.UI
{
    public class Program : IDisposable
    {
        private static readonly ILogger m_logger = (ILogger)LogHelper.Instance.CreateLog4NetLogger(typeof(Program));

        [STAThread]
        public static void Main()
        {
            using (Program program = new Program())
                program.Run();
        }

        public void Run()
        {
            ErrorList errorList = new ErrorList();
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Program.m_logger.Configure(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "updatemgr-ui-log4net.config"));
                LogHelper.Instance.Logger = Program.m_logger;
                errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)UpdateManagerApplication.Instance.Initialize());
            }
            catch (Exception ex)
            {
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("E989", "An unhandled exception was thrown in Program.Run.", ex));
            }
            if (errorList.ContainsCode("INSTANCE"))
                return;
            if (errorList.ContainsError())
            {
                int num = (int)new ErrorForm()
                {
                    Errors = ((IEnumerable)errorList)
                }.ShowDialog();
            }
            else
            {
                if (!UpdateManagerApplication.Instance.Run())
                    return;
                Application.Run((ApplicationContext)UpdateManagerApplication.Instance);
            }
        }

        public void Dispose()
        {
        }
    }
}
