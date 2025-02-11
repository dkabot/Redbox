using Microsoft.Win32;
using System;
using System.Collections;
using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;

namespace Redbox.UpdateManager.Daemon
{
    [RunInstaller(true)]
    public class UpdateManagerServiceInstaller : Installer
    {
        private IContainer components;

        public UpdateManagerServiceInstaller()
        {
            this.InitializeComponent();
            ServiceInstaller serviceInstaller = new ServiceInstaller()
            {
                StartType = ServiceStartMode.Automatic,
                DisplayName = "Redbox Store Manager Service",
                ServiceName = "updatemgrd$default",
                Description = "Manage updates and run scheduled tasks.",
                ServicesDependedOn = new string[1] { "Tcpip" }
            };
            ServiceProcessInstaller processInstaller = new ServiceProcessInstaller()
            {
                Account = ServiceAccount.LocalSystem
            };
            this.Installers.Add((Installer)serviceInstaller);
            this.Installers.Add((Installer)processInstaller);
        }

        protected override void OnAfterInstall(IDictionary savedState)
        {
            try
            {
                string keyName = string.Format("HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Services\\{0}", (object)"updatemgrd$default");
                Registry.SetValue(keyName, "Type", (object)((int)Registry.GetValue(keyName, "Type", (object)0) | 256));
                if (Options.Instance.ForceStart)
                {
                    using (ServiceController serviceController = new ServiceController("updatemgrd$default"))
                        serviceController.Start();
                }
            }
            catch (Exception ex)
            {
                this.Context.LogMessage("An exception was raised in OnAfterInstall: " + (object)ex);
            }
            base.OnAfterInstall(savedState);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && this.components != null)
                this.components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent() => this.components = (IContainer)new System.ComponentModel.Container();
    }
}
