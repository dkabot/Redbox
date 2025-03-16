using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Redbox.KioskEngine.IDE
{
    public class NewProjectForm : Form
    {
        private IContainer components;
        private Label label1;
        private Label label2;
        private TextBox m_projectNameTextBox;
        private TextBox m_locationTextBox;
        private ErrorProvider m_errorProvider;
        private Button m_browseButton;
        private Button m_okButton;
        private Button m_cancelButton;

        public NewProjectForm()
        {
            InitializeComponent();
        }

        public string ProjectName
        {
            get => m_projectNameTextBox.Text;
            set => m_projectNameTextBox.Text = value;
        }

        public string ProjectLocation
        {
            get => m_locationTextBox.Text;
            set => m_locationTextBox.Text = value;
        }

        private void OnLoad(object sender, EventArgs e)
        {
            var service = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
            if (service == null)
                return;
            m_locationTextBox.Text = service.GetValue<string>("Ide", "DefaultProjectLocation", "C:\\Projects");
        }

        private void OnOK(object sender, EventArgs e)
        {
            m_errorProvider.SetError((Control)m_projectNameTextBox, string.Empty);
            m_errorProvider.SetError((Control)m_locationTextBox, string.Empty);
            if (string.IsNullOrEmpty(m_projectNameTextBox.Text))
            {
                m_errorProvider.SetError((Control)m_projectNameTextBox, "Provide a valid project name.");
            }
            else if (string.IsNullOrEmpty(m_locationTextBox.Text))
            {
                m_errorProvider.SetError((Control)m_locationTextBox, "Specify a valid location for the project.");
            }
            else
            {
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private void OnCancel(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
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
            label2 = new Label();
            m_projectNameTextBox = new TextBox();
            m_locationTextBox = new TextBox();
            m_errorProvider = new ErrorProvider(components);
            m_browseButton = new Button();
            m_okButton = new Button();
            m_cancelButton = new Button();
            ((ISupportInitialize)m_errorProvider).BeginInit();
            SuspendLayout();
            label1.AutoSize = true;
            label1.Location = new Point(12, 16);
            label1.Name = "label1";
            label1.Size = new Size(74, 13);
            label1.TabIndex = 0;
            label1.Text = "Project Name:";
            label2.AutoSize = true;
            label2.Location = new Point(35, 44);
            label2.Name = "label2";
            label2.Size = new Size(51, 13);
            label2.TabIndex = 2;
            label2.Text = "Location:";
            m_projectNameTextBox.Location = new Point(92, 13);
            m_projectNameTextBox.MaxLength = (int)byte.MaxValue;
            m_projectNameTextBox.Name = "m_projectNameTextBox";
            m_projectNameTextBox.Size = new Size(239, 20);
            m_projectNameTextBox.TabIndex = 1;
            m_locationTextBox.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            m_locationTextBox.AutoCompleteSource = AutoCompleteSource.FileSystemDirectories;
            m_locationTextBox.Location = new Point(92, 41);
            m_locationTextBox.MaxLength = 1024;
            m_locationTextBox.Name = "m_locationTextBox";
            m_locationTextBox.Size = new Size(343, 20);
            m_locationTextBox.TabIndex = 3;
            m_errorProvider.ContainerControl = (ContainerControl)this;
            m_browseButton.Location = new Point(457, 39);
            m_browseButton.Name = "m_browseButton";
            m_browseButton.Size = new Size(27, 23);
            m_browseButton.TabIndex = 4;
            m_browseButton.Text = "...";
            m_browseButton.UseVisualStyleBackColor = true;
            m_browseButton.Click += new EventHandler(OnBrowse);
            m_okButton.Location = new Point(513, 11);
            m_okButton.Name = "m_okButton";
            m_okButton.Size = new Size(75, 23);
            m_okButton.TabIndex = 5;
            m_okButton.Text = "OK";
            m_okButton.UseVisualStyleBackColor = true;
            m_okButton.Click += new EventHandler(OnOK);
            m_cancelButton.DialogResult = DialogResult.Cancel;
            m_cancelButton.Location = new Point(513, 40);
            m_cancelButton.Name = "m_cancelButton";
            m_cancelButton.Size = new Size(75, 23);
            m_cancelButton.TabIndex = 6;
            m_cancelButton.Text = "Cancel";
            m_cancelButton.UseVisualStyleBackColor = true;
            m_cancelButton.Click += new EventHandler(OnCancel);
            AcceptButton = (IButtonControl)m_okButton;
            AutoScaleDimensions = new SizeF(6f, 13f);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = (IButtonControl)m_cancelButton;
            ClientSize = new Size(600, 86);
            Controls.Add((Control)m_cancelButton);
            Controls.Add((Control)m_okButton);
            Controls.Add((Control)m_browseButton);
            Controls.Add((Control)m_locationTextBox);
            Controls.Add((Control)m_projectNameTextBox);
            Controls.Add((Control)label2);
            Controls.Add((Control)label1);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = nameof(NewProjectForm);
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Create New Project";
            Load += new EventHandler(OnLoad);
            ((ISupportInitialize)m_errorProvider).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }
    }
}