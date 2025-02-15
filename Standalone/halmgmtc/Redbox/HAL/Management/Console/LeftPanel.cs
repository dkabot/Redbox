using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Redbox.HAL.Core;

namespace Redbox.HAL.Management.Console
{
    public class LeftPanel : UserControl
    {
        private IContainer components;
        private TabPage m_immediateTabPage;
        private TabControl m_leftTabControl;
        private TabPage m_propertyTabPage;
        private Panel m_sensorPanel;

        private LeftPanel()
        {
            InitializeComponent();
            Dock = DockStyle.Fill;
            Visible = true;
            Enabled = false;
            AddTab(LeftPanelTab.Properties);
            m_sensorPanel.Controls.Add(SensorView.Instance);
            m_sensorPanel.Visible = true;
            ProfileManager.Instance.Connected += (_param1, _param2) =>
            {
                Enabled = true;
                Refresh();
            };
            ProfileManager.Instance.Disconnected += (_param1, _param2) => Enabled = false;
        }

        public static LeftPanel Instance => Singleton<LeftPanel>.Instance;

        public void AddTab(LeftPanelTab tab)
        {
            switch (tab)
            {
                case LeftPanelTab.Properties:
                    if (m_propertyTabPage == null)
                    {
                        m_propertyTabPage = new TabPage();
                        m_propertyTabPage.BackColor = Color.White;
                        m_propertyTabPage.Text = "Properties";
                        m_propertyTabPage.Controls.Add(new ConfigPropertyList());
                    }

                    m_leftTabControl.TabPages.Add(m_propertyTabPage);
                    break;
                case LeftPanelTab.Immediate:
                    if (m_immediateTabPage == null)
                    {
                        m_immediateTabPage = new TabPage();
                        m_immediateTabPage.Text = "Immediate";
                        m_immediateTabPage.Controls.Add(ImmediateWindow.Instance);
                        m_immediateTabPage.Enter += (_param1, _param2) => m_immediateTabPage.Controls[0].Select();
                    }

                    m_leftTabControl.TabPages.Add(m_immediateTabPage);
                    break;
            }
        }

        public void RemoveTab(LeftPanelTab tab)
        {
            switch (tab)
            {
                case LeftPanelTab.Properties:
                    if (m_propertyTabPage == null)
                        break;
                    m_leftTabControl.TabPages.Remove(m_propertyTabPage);
                    break;
                case LeftPanelTab.Immediate:
                    if (m_immediateTabPage == null)
                        break;
                    m_leftTabControl.TabPages.Remove(m_immediateTabPage);
                    break;
            }
        }

        public void ReloadPropertiesConfiguration()
        {
            if (m_propertyTabPage == null || m_propertyTabPage.Controls.Count <= 0 ||
                !(m_propertyTabPage.Controls["ConfigPropertyList"] is ConfigPropertyList control))
                return;
            control.LoadConfiguration();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            m_leftTabControl = new TabControl();
            m_sensorPanel = new Panel();
            SuspendLayout();
            m_leftTabControl.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            m_leftTabControl.ItemSize = new Size(80, 30);
            m_leftTabControl.Location = new Point(5, 2);
            m_leftTabControl.Name = "m_leftTabControl";
            m_leftTabControl.SelectedIndex = 0;
            m_leftTabControl.Size = new Size(262, 425);
            m_leftTabControl.TabIndex = 2;
            m_sensorPanel.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            m_sensorPanel.Location = new Point(5, 433);
            m_sensorPanel.Name = "m_sensorPanel";
            m_sensorPanel.Size = new Size(262, 150);
            m_sensorPanel.TabIndex = 3;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            BackColor = Color.WhiteSmoke;
            Controls.Add(m_sensorPanel);
            Controls.Add(m_leftTabControl);
            Margin = new Padding(5);
            Name = nameof(LeftPanel);
            Padding = new Padding(5);
            Size = new Size(275, 600);
            ResumeLayout(false);
        }
    }
}