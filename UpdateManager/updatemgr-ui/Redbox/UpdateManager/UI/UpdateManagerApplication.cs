using Redbox.Core;
using Redbox.IPC.Framework;
using Redbox.UpdateManager.ServiceProxies;
using Redbox.UpdateManager.UI.Properties;
using System;
using System.Collections;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace Redbox.UpdateManager.UI
{
    public class UpdateManagerApplication : ApplicationContext
    {
        public static UpdateManagerApplication Instance => Singleton<UpdateManagerApplication>.Instance;

        public bool Run()
        {
            Redbox.UpdateManager.ComponentModel.ErrorList errorList = new Redbox.UpdateManager.ComponentModel.ErrorList();
            this.MainForm = (Form)new Redbox.UpdateManager.UI.MainForm();
            if (!errorList.ContainsError())
            {
                this.MainForm.Show();
                return true;
            }
            int num = (int)new ErrorForm()
            {
                Errors = (errorList)
            }.ShowDialog();
            return false;
        }

        public Redbox.UpdateManager.ComponentModel.ErrorList Initialize()
        {
            Redbox.UpdateManager.ComponentModel.ErrorList errorlist = new Redbox.UpdateManager.ComponentModel.ErrorList();
            Path.GetDirectoryName(typeof(UpdateManagerApplication).Assembly.Location);
            this.UpdateManagerServiceUrl = Settings.Default.UpdateManagerUrl;
            SchedulerProxy.Instance.Initialize(this.UpdateManagerServiceUrl);
            QueueServiceProxy.Instance.Initialize(this.UpdateManagerServiceUrl);
            TransferServiceProxy.Instance.Initialize(this.UpdateManagerServiceUrl);
            RepositoryServiceProxy.Instance.Initialize(this.UpdateManagerServiceUrl);
            DataStoreServiceProxy.Instance.Initialize(this.UpdateManagerServiceUrl);
            bool isDeveloperUI;
            bool initialSubscriptionState;
            using (UpdateManagerService service = UpdateManagerService.GetService(this.UpdateManagerServiceUrl))
            {
                ClientCommandResult storeNumber = service.GetStoreNumber(out string _);
                if (!storeNumber.Success)
                {
                    storeNumber.Errors.ForEach((Action<Redbox.IPC.Framework.Error>)(e => errorlist.Add(e.IsWarning ? Redbox.UpdateManager.ComponentModel.Error.NewWarning(e.Code, e.Description, e.Details) : Redbox.UpdateManager.ComponentModel.Error.NewError(e.Code, e.Description, e.Details))));
                    return errorlist;
                }
                ClientCommandResult clientCommandResult = service.IsDeveloperUI(out isDeveloperUI);
                if (!clientCommandResult.Success)
                {
                    clientCommandResult.Errors.ForEach((Action<Redbox.IPC.Framework.Error>)(e => errorlist.Add(e.IsWarning ? Redbox.UpdateManager.ComponentModel.Error.NewWarning(e.Code, e.Description, e.Details) : Redbox.UpdateManager.ComponentModel.Error.NewError(e.Code, e.Description, e.Details))));
                    return errorlist;
                }
                ClientCommandResult subscriptionState = service.GetInitialSubscriptionState(out initialSubscriptionState);
                if (!subscriptionState.Success)
                {
                    subscriptionState.Errors.ForEach((Action<Redbox.IPC.Framework.Error>)(e => errorlist.Add(e.IsWarning ? Redbox.UpdateManager.ComponentModel.Error.NewWarning(e.Code, e.Description, e.Details) : Redbox.UpdateManager.ComponentModel.Error.NewError(e.Code, e.Description, e.Details))));
                    return errorlist;
                }
            }
            this.DeveloperMode = isDeveloperUI;
            this.InitialSubscriptionState = initialSubscriptionState;
            UpdateServiceProxy.Instance.Initialize(this.UpdateManagerServiceUrl, TimeSpan.FromSeconds(120.0));
            return errorlist;
        }

        public bool DeveloperMode { get; private set; }

        public bool InitialSubscriptionState { get; private set; }

        public string UpdateManagerServiceUrl { get; private set; }

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
                Cursor cursor = this.MainForm.Cursor;
                try
                {
                    if (button != null)
                        this.ThreadSafeHostUpdate((() => this.MainForm.Cursor = Cursors.WaitCursor));
                    this.ThreadSafeHostUpdate((() => action()));
                }
                catch (Exception ex)
                {
                    this.ThreadSafeExceptionDisplay(ex);
                }
                finally
                {
                    if (button != null)
                        UpdateManagerApplication.Instance.ThreadSafeHostUpdate((() =>
                  {
                      button.Enabled = true;
                      this.MainForm.Cursor = cursor;
                  }));
                }
            }));
        }

        public void ThreadSafeErrorDisplay(Redbox.UpdateManager.ComponentModel.ErrorList errors)
        {
            int num;
            UpdateManagerApplication.Instance.ThreadSafeHostUpdate((() => num = (int)new ErrorForm()
            {
                Errors = (errors)
            }.ShowDialog()));
        }

        public void ThreadSafeExceptionDisplay(Exception exception)
        {
            UpdateManagerApplication.Instance.ThreadSafeHostUpdate((() =>
            {
                Redbox.UpdateManager.ComponentModel.ErrorList errorList = new Redbox.UpdateManager.ComponentModel.ErrorList()
              {
          Redbox.UpdateManager.ComponentModel.Error.NewError("E988", "An unhandled exception was thrown in Update Mananger.", exception)
              };
                int num = (int)new ErrorForm()
                {
                    Errors = (errorList)
                }.ShowDialog();
            }));
        }

        private UpdateManagerApplication()
        {
            Application.ThreadException += (ThreadExceptionEventHandler)((sender, e) =>
            {
                LogHelper.Instance.Log("An unhandled exception was rasied in the Application.ThreadException handler.", e.Exception);
                int num = (int)MessageBox.Show(e.Exception.ToString());
            });
        }
    }
}
