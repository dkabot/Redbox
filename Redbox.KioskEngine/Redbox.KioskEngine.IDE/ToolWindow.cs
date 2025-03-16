using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using System.ComponentModel;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace Redbox.KioskEngine.IDE
{
    public class ToolWindow : DockContent
    {
        private IContainer components;

        public ToolWindow()
        {
            InitializeComponent();
        }

        internal IUserSettingsStore UserSettings => ServiceLocator.Instance.GetService<IUserSettingsStore>();

        internal IResourceBundleService BundleService => ServiceLocator.Instance.GetService<IResourceBundleService>();

        internal IMachineSettingsStore MachineSettings => ServiceLocator.Instance.GetService<IMachineSettingsStore>();

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            components = (IContainer)new Container();
            AutoScaleMode = AutoScaleMode.Font;
            Text = nameof(ToolWindow);
        }
    }
}