using Redbox.KioskEngine.ComponentModel;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Redbox.KioskEngine.IDE
{
    public class IdePreferencePage : UserControl, IPreferencePageHost
    {
        private IContainer components;
        private Label m_defaultProjectLocationLabel;
        private TextBox m_locationTextBox;
        private Button m_browseButton;
        private ErrorProvider m_errorProvider;
        private CheckBox m_showIdeInTaskBar;
        private CheckBox m_showKioskEngineInTaskBarCheckBox;
        private Label m_headerLabel;

        public IdePreferencePage()
        {
            InitializeComponent();
        }

        public bool SaveValues(IPreferencePage preferencePage)
        {
            m_errorProvider.SetError((Control)m_locationTextBox, string.Empty);
            if (string.IsNullOrEmpty(m_locationTextBox.Text))
            {
                m_errorProvider.SetError((Control)m_locationTextBox, "Provide a valid default project location.");
                return false;
            }

            preferencePage.SetValue("DefaultProjectLocation", (object)m_locationTextBox.Text);
            preferencePage.SetValue("ShowKioskEngineInTaskBar", (object)m_showKioskEngineInTaskBarCheckBox.Checked);
            preferencePage.SetValue("ShowIDEInTaskBar", (object)m_showIdeInTaskBar.Checked);
            return true;
        }

        public void LoadValues(IPreferencePage preferencePage)
        {
            m_locationTextBox.Text = preferencePage.GetValue<string>("DefaultProjectLocation", "C:\\Projects");
            m_showKioskEngineInTaskBarCheckBox.Checked =
                preferencePage.GetValue<bool>("ShowKioskEngineInTaskBar", false);
            m_showIdeInTaskBar.Checked = preferencePage.GetValue<bool>("ShowIDEInTaskBar", true);
        }

        private void OnBrowse(object sender, EventArgs e)
        {
            var folderBrowserDialog = new FolderBrowserDialog()
            {
                SelectedPath = m_locationTextBox.Text
            };
            if (folderBrowserDialog.ShowDialog() != DialogResult.OK)
                return;
            m_locationTextBox.Text = folderBrowserDialog.SelectedPath;
        }

        private void OnShowIdeCheckChanged(object sender, EventArgs e)
        {
            var openForm = Application.OpenForms["IdeMainForm"];
            if (openForm == null)
                return;
            openForm.ShowInTaskbar = m_showIdeInTaskBar.Checked;
        }

        private void OnShowKioskEngineCheckChanged(object sender, EventArgs e)
        {
            var openForm = Application.OpenForms["HostForm"];
            if (openForm == null)
                return;
            openForm.ShowInTaskbar = m_showKioskEngineInTaskBarCheckBox.Checked;
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
            m_defaultProjectLocationLabel = new Label();
            m_locationTextBox = new TextBox();
            m_browseButton = new Button();
            m_errorProvider = new ErrorProvider(components);
            m_showKioskEngineInTaskBarCheckBox = new CheckBox();
            m_showIdeInTaskBar = new CheckBox();
            m_headerLabel = new Label();
            ((ISupportInitialize)m_errorProvider).BeginInit();
            SuspendLayout();
            m_defaultProjectLocationLabel.AutoSize = true;
            m_defaultProjectLocationLabel.Location = new Point(31, 60);
            m_defaultProjectLocationLabel.Name = "m_defaultProjectLocationLabel";
            m_defaultProjectLocationLabel.Size = new Size(124, 13);
            m_defaultProjectLocationLabel.TabIndex = 0;
            m_defaultProjectLocationLabel.Text = "Default Project Location:";
            m_locationTextBox.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            m_locationTextBox.AutoCompleteSource = AutoCompleteSource.FileSystemDirectories;
            m_locationTextBox.Location = new Point(34, 76);
            m_locationTextBox.MaxLength = 1024;
            m_locationTextBox.Name = "m_locationTextBox";
            m_locationTextBox.Size = new Size(415, 20);
            m_locationTextBox.TabIndex = 1;
            m_browseButton.Location = new Point(473, 74);
            m_browseButton.Name = "m_browseButton";
            m_browseButton.Size = new Size(24, 23);
            m_browseButton.TabIndex = 2;
            m_browseButton.Text = "...";
            m_browseButton.UseVisualStyleBackColor = true;
            m_browseButton.Click += new EventHandler(OnBrowse);
            m_errorProvider.ContainerControl = (ContainerControl)this;
            m_showKioskEngineInTaskBarCheckBox.AutoSize = true;
            m_showKioskEngineInTaskBarCheckBox.Location = new Point(34, 111);
            m_showKioskEngineInTaskBarCheckBox.Name = "m_showKioskEngineInTaskBarCheckBox";
            m_showKioskEngineInTaskBarCheckBox.Size = new Size(175, 17);
            m_showKioskEngineInTaskBarCheckBox.TabIndex = 3;
            m_showKioskEngineInTaskBarCheckBox.Text = "Show Kiosk Engine in Task Bar";
            m_showKioskEngineInTaskBarCheckBox.UseVisualStyleBackColor = true;
            m_showKioskEngineInTaskBarCheckBox.CheckedChanged += new EventHandler(OnShowKioskEngineCheckChanged);
            m_showIdeInTaskBar.AutoSize = true;
            m_showIdeInTaskBar.Location = new Point(34, 134);
            m_showIdeInTaskBar.Name = "m_showIdeInTaskBar";
            m_showIdeInTaskBar.Size = new Size(131, 17);
            m_showIdeInTaskBar.TabIndex = 4;
            m_showIdeInTaskBar.Text = "Show IDE in Task Bar";
            m_showIdeInTaskBar.UseVisualStyleBackColor = true;
            m_showIdeInTaskBar.CheckedChanged += new EventHandler(OnShowIdeCheckChanged);
            m_headerLabel.Anchor = AnchorStyles.Top;
            m_headerLabel.BackColor = Color.MediumBlue;
            m_headerLabel.Font = new Font("Microsoft Sans Serif", 14.25f, FontStyle.Bold, GraphicsUnit.Point, (byte)0);
            m_headerLabel.ForeColor = Color.White;
            m_headerLabel.Location = new Point(0, 0);
            m_headerLabel.Name = "m_headerLabel";
            m_headerLabel.Size = new Size(542, 38);
            m_headerLabel.TabIndex = 15;
            m_headerLabel.Text = "  Engine Core: IDE Settings";
            m_headerLabel.TextAlign = ContentAlignment.MiddleLeft;
            AutoScaleDimensions = new SizeF(6f, 13f);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add((Control)m_headerLabel);
            Controls.Add((Control)m_showIdeInTaskBar);
            Controls.Add((Control)m_showKioskEngineInTaskBarCheckBox);
            Controls.Add((Control)m_browseButton);
            Controls.Add((Control)m_locationTextBox);
            Controls.Add((Control)m_defaultProjectLocationLabel);
            Name = nameof(IdePreferencePage);
            Size = new Size(542, 506);
            ((ISupportInitialize)m_errorProvider).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }
    }
}