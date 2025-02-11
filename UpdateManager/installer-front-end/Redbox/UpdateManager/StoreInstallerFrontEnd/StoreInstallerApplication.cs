using Redbox.Core;
using Redbox.UpdateManager.BITS;
using Redbox.UpdateManager.ComponentModel;
using Redbox.UpdateManager.Environment;
using Redbox.UpdateManager.Kernel;
using Redbox.UpdateManager.Remoting;
using Redbox.UpdateManager.StoreInstallerFrontEnd.Properties;
using Redbox.UpdateManager.TaskScheduler;
using System;
using System.Collections;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace Redbox.UpdateManager.StoreInstallerFrontEnd
{
    public class StoreInstallerApplication : ApplicationContext, IEventNotifyService, IInputService
    {
        private Thread m_scriptThread;

        public static StoreInstallerApplication Instance
        {
            get => Singleton<StoreInstallerApplication>.Instance;
        }

        public DialogResult AskYesNoQuestion(string title, string message)
        {
            DialogResult result = DialogResult.None;
            this.ThreadSafeHostUpdate((MethodInvoker)(() => result = MessageBox.Show(message, title, MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button2)));
            return result;
        }

        public void NotifyInfo(string title, string message)
        {
            Font font = new Font(FontFamily.GenericSansSerif, 12f, FontStyle.Bold);
            int num;
            this.ThreadSafeHostUpdate((MethodInvoker)(() => num = (int)CustomMessageBox.Show(title, message, font)));
        }

        public FileInfo LocateFile()
        {
            FileInfo file = (FileInfo)null;
            this.ThreadSafeHostUpdate((MethodInvoker)(() => file = ((Host)this.MainForm).ShowFileDialog()));
            return file;
        }

        public ErrorList AddEvent(string name, string description)
        {
            try
            {
                this.ThreadSafeHostUpdate((MethodInvoker)(() => ((Host)this.MainForm).AddEvent(name, description)));
            }
            catch (Exception ex)
            {
                ErrorList errorList = new ErrorList();
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("E999", "Unhandled exception in AddEvent.", ex));
                return errorList;
            }
            return new ErrorList();
        }

        public ErrorList EventStart(string name)
        {
            try
            {
                this.ThreadSafeHostUpdate((MethodInvoker)(() => ((Host)this.MainForm).EventStart(name)));
            }
            catch (Exception ex)
            {
                ErrorList errorList = new ErrorList();
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("E999", "Unhandled exception in AddEvent.", ex));
                return errorList;
            }
            return new ErrorList();
        }

        public ErrorList EventErrored(string name, string code, string description, string details)
        {
            try
            {
                this.ThreadSafeHostUpdate((MethodInvoker)(() => ((Host)this.MainForm).EventError(name, Redbox.UpdateManager.ComponentModel.Error.NewError(code, description, details))));
            }
            catch (Exception ex)
            {
                ErrorList errorList = new ErrorList();
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("E999", "Unhandled exception in AddEvent.", ex));
                return errorList;
            }
            return new ErrorList();
        }

        public ErrorList EventComplete(string name)
        {
            try
            {
                this.ThreadSafeHostUpdate((MethodInvoker)(() => ((Host)this.MainForm).EventComplete(name)));
            }
            catch (Exception ex)
            {
                ErrorList errorList = new ErrorList();
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("E999", "Unhandled exception in AddEvent.", ex));
                return errorList;
            }
            return new ErrorList();
        }

        public ErrorList Exit()
        {
            try
            {
                this.ThreadSafeHostUpdate((MethodInvoker)(() => ((Host)this.MainForm).ShowFinish()));
            }
            catch (Exception ex)
            {
                ErrorList errorList = new ErrorList();
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("E999", "Unhandled exception in AddEvent.", ex));
                return errorList;
            }
            return new ErrorList();
        }

        public bool Run()
        {
            ErrorList errorList = new ErrorList();
            this.MainForm = (Form)new Host();
            this.m_scriptThread = new Thread((ParameterizedThreadStart)(o =>
            {
                try
                {
                    KernelService.Instance.ExecuteChunk(File.ReadAllText(this.Script));
                }
                catch (Exception ex)
                {
                    LogHelper.Instance.Log("Unhandled exception caught in execution of script.", ex);
                    this.ThreadSafeExceptionDisplay(ex);
                    this.ThreadSafeHostUpdate((MethodInvoker)(() => ((Host)this.MainForm).ErrorAll()));
                }
            }))
            {
                IsBackground = true
            };
            if (!errorList.ContainsError())
            {
                this.MainForm.Show();
                this.m_scriptThread.Start();
                return true;
            }
            int num = (int)new ErrorForm()
            {
                Errors = ((IEnumerable)errorList)
            }.ShowDialog();
            return false;
        }

        public bool ScriptCompleted => KernelService.Instance.ScriptCompleted;

        public ErrorList Initialize(string script, string storeNumber)
        {
            MacroService instance = new MacroService();
            ServiceLocator.Instance.AddService(typeof(IMacroService), (object)instance);
            string directoryName = Path.GetDirectoryName(typeof(StoreInstallerApplication).Assembly.Location);
            ErrorList errorList = new ErrorList();
            string str = instance.ExpandProperties(script);
            if (!Path.IsPathRooted(str))
                str = Path.Combine(directoryName, str);
            if (!File.Exists(str))
            {
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("F999", string.Format("Script file at {0} not found.", (object)script), "Supply a valid script file."));
                return errorList;
            }
            this.Script = str;
            instance["StoreNumber"] = storeNumber;
            instance["ProgramFiles"] = "C:\\Program Files";
            instance["REDS"] = "C:\\Program Files\\Redbox\\REDS";
            instance["data"] = "C:\\Program Files\\Redbox\\REDS\\data";
            instance["RedboxProgramFiles"] = "C:\\Program Files\\Redbox";
            instance["engine"] = "C:\\Program Files\\Redbox\\REDS\\Kiosk Engine";
            instance["Scripts"] = "C:\\Program Files\\Redbox\\REDS\\Update Manager\\scripts";
            instance["WorkingDirectory"] = Path.GetDirectoryName(typeof(StoreInstallerApplication).Assembly.Location);
            WindowsTaskScheduler.Instance.Initialize();
            Remoting.UpdateService.Instance.Initialize(Settings.Default.UpdateServiceUrl, TimeSpan.FromSeconds(120.0), storeNumber, Settings.Default.MinimumRetryDelay);
            KernelService.Instance.Initialize();
            DataStoreService.Instance.Initialize(Settings.Default.DataStoreRoot);
            Scheduler.Instance.Initialize();
            QueueService.Instance.Initialize(Settings.Default.DataStoreRoot);
            TransferService.Instance.Initialize();
            RepositoryService.Instance.Initialize(Settings.Default.ManifestRoot);
            ServiceLocator.Instance.AddService(typeof(IEventNotifyService), (object)this);
            ServiceLocator.Instance.AddService(typeof(IInputService), (object)this);
            return errorList;
        }

        public void ThreadSafeHostUpdate(MethodInvoker invoker)
        {
            if (this.MainForm.InvokeRequired)
                this.MainForm.Invoke((Delegate)invoker);
            else
                invoker();
        }

        public void AsyncButtonAction(Control button, Action action)
        {
            if (button != null)
                button.Enabled = false;
            ThreadPool.QueueUserWorkItem((WaitCallback)(o =>
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    this.ThreadSafeExceptionDisplay(ex);
                }
                finally
                {
                    if (button != null)
                        StoreInstallerApplication.Instance.ThreadSafeHostUpdate((MethodInvoker)(() => button.Enabled = true));
                }
            }));
        }

        public void ThreadSafeErrorDisplay(ErrorList errors)
        {
            int num;
            StoreInstallerApplication.Instance.ThreadSafeHostUpdate((MethodInvoker)(() => num = (int)new ErrorForm()
            {
                Errors = ((IEnumerable)errors)
            }.ShowDialog()));
        }

        public void ThreadSafeExceptionDisplay(Exception exception)
        {
            StoreInstallerApplication.Instance.ThreadSafeHostUpdate((MethodInvoker)(() =>
            {
                ErrorList errorList = new ErrorList()
              {
          Redbox.UpdateManager.ComponentModel.Error.NewError("E999", "An unhandled exception was thrown in Update Mananger.", exception)
              };
                int num = (int)new ErrorForm()
                {
                    Errors = ((IEnumerable)errorList)
                }.ShowDialog();
            }));
        }

        public string Script { get; set; }

        private StoreInstallerApplication()
        {
            Application.ThreadException += (ThreadExceptionEventHandler)((sender, e) =>
            {
                LogHelper.Instance.Log("An unhandled exception was rasied in the Application.ThreadException handler.", e.Exception);
                int num = (int)MessageBox.Show(e.Exception.ToString());
            });
        }
    }
}
