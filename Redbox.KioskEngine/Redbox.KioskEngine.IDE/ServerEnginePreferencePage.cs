using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Redbox.KioskEngine.IDE
{
    public class ServerEnginePreferencePage : UserControl, IPreferencePageHost
    {
        private IContainer components;
        private Label label1;
        private TextBox m_serverEngineUrlTextBox;
        private Label label2;
        private TextBox m_serverEngineTmeoutTextBox;
        private ErrorProvider m_errorProvider;

        public ServerEnginePreferencePage()
        {
            InitializeComponent();
        }

        public bool SaveValues(IPreferencePage preferencePage)
        {
            m_errorProvider.SetError((Control)m_serverEngineUrlTextBox, string.Empty);
            m_errorProvider.SetError((Control)m_serverEngineTmeoutTextBox, string.Empty);
            var service = ServiceLocator.Instance.GetService<IServerEngineService>();
            if (service == null)
                return true;
            if (string.IsNullOrEmpty(m_serverEngineUrlTextBox.Text))
            {
                m_errorProvider.SetError((Control)m_serverEngineTmeoutTextBox,
                    "A valid URL is required to the Server Engine service, e.g. rcp://localhost:7008.");
                return false;
            }

            service.Url = m_serverEngineUrlTextBox.Text;
            int result;
            if (!int.TryParse(m_serverEngineTmeoutTextBox.Text, out result))
            {
                m_errorProvider.SetError((Control)m_serverEngineTmeoutTextBox,
                    "A valid integer value is required for the timeout.");
                return false;
            }

            service.Timeout = result;
            return true;
        }

        public void LoadValues(IPreferencePage preferencePage)
        {
            var service = ServiceLocator.Instance.GetService<IServerEngineService>();
            if (service == null)
                return;
            m_serverEngineUrlTextBox.Text = service.Url;
            m_serverEngineTmeoutTextBox.Text = service.Timeout.ToString();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            components = (IContainer)new Container();
            label1 = new Label();
            m_serverEngineUrlTextBox = new TextBox();
            label2 = new Label();
            m_serverEngineTmeoutTextBox = new TextBox();
            m_errorProvider = new ErrorProvider(components);
            ((ISupportInitialize)m_errorProvider).BeginInit();
            SuspendLayout();
            label1.AutoSize = true;
            label1.Location = new Point(30, 54);
            label1.Name = "label1";
            label1.Size = new Size(71, 13);
            label1.TabIndex = 0;
            label1.Text = "Service URL:";
            m_serverEngineUrlTextBox.Location = new Point(107, 51);
            m_serverEngineUrlTextBox.Name = "m_serverEngineUrlTextBox";
            m_serverEngineUrlTextBox.Size = new Size(399, 20);
            m_serverEngineUrlTextBox.TabIndex = 1;
            label2.AutoSize = true;
            label2.Location = new Point(15, 81);
            label2.Name = "label2";
            label2.Size = new Size(87, 13);
            label2.TabIndex = 2;
            label2.Text = "Service Timeout:";
            m_serverEngineTmeoutTextBox.Location = new Point(107, 78);
            m_serverEngineTmeoutTextBox.Name = "m_serverEngineTmeoutTextBox";
            m_serverEngineTmeoutTextBox.Size = new Size(100, 20);
            m_serverEngineTmeoutTextBox.TabIndex = 3;
            m_errorProvider.ContainerControl = (ContainerControl)this;
            AutoScaleDimensions = new SizeF(6f, 13f);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add((Control)m_serverEngineTmeoutTextBox);
            Controls.Add((Control)label2);
            Controls.Add((Control)m_serverEngineUrlTextBox);
            Controls.Add((Control)label1);
            Name = nameof(ServerEnginePreferencePage);
            Size = new Size(542, 506);
            ((ISupportInitialize)m_errorProvider).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }
    }
}