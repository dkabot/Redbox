using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Redbox.KioskEngine.IDE
{
    public class TaskRunnerForm : Form
    {
        private IContainer components;
        private Button m_runButton;
        private Button m_closeButton;
        private Label label1;
        private ComboBox m_processComboBox;
        private ErrorProvider m_errorProvider;

        public TaskRunnerForm()
        {
            InitializeComponent();
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string ProcessToRun { get; set; }

        public event EventHandler Run;

        private void OnRun(object sender, EventArgs e)
        {
            m_errorProvider.SetError((Control)m_processComboBox, string.Empty);
            if (string.IsNullOrEmpty(m_processComboBox.Text))
            {
                m_errorProvider.SetError((Control)m_processComboBox, "A process is required.");
            }
            else
            {
                ProcessToRun = m_processComboBox.Text;
                if (Run != null)
                    Run((object)this, EventArgs.Empty);
                Close();
            }
        }

        private void OnClose(object sender, EventArgs e)
        {
            Close();
        }

        private void OnLoad(object sender, EventArgs e)
        {
            Cursor.Show();
        }

        private void OnFormClosed(object sender, FormClosedEventArgs e)
        {
            Cursor.Hide();
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
            m_runButton = new Button();
            m_closeButton = new Button();
            label1 = new Label();
            m_processComboBox = new ComboBox();
            m_errorProvider = new ErrorProvider(components);
            ((ISupportInitialize)m_errorProvider).BeginInit();
            SuspendLayout();
            m_runButton.Location = new Point(305, 12);
            m_runButton.Name = "m_runButton";
            m_runButton.Size = new Size(75, 23);
            m_runButton.TabIndex = 2;
            m_runButton.Text = "Run";
            m_runButton.UseVisualStyleBackColor = true;
            m_runButton.Click += new EventHandler(OnRun);
            m_closeButton.DialogResult = DialogResult.Cancel;
            m_closeButton.Location = new Point(305, 41);
            m_closeButton.Name = "m_closeButton";
            m_closeButton.Size = new Size(75, 23);
            m_closeButton.TabIndex = 3;
            m_closeButton.Text = "Close";
            m_closeButton.UseVisualStyleBackColor = true;
            m_closeButton.Click += new EventHandler(OnClose);
            label1.AutoSize = true;
            label1.Location = new Point(12, 17);
            label1.Name = "label1";
            label1.Size = new Size(78, 13);
            label1.TabIndex = 0;
            label1.Text = "Process to run:";
            m_processComboBox.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            m_processComboBox.AutoCompleteSource = AutoCompleteSource.AllUrl;
            m_processComboBox.FormattingEnabled = true;
            m_processComboBox.Location = new Point(15, 33);
            m_processComboBox.Name = "m_processComboBox";
            m_processComboBox.Size = new Size(251, 21);
            m_processComboBox.TabIndex = 1;
            m_errorProvider.ContainerControl = (ContainerControl)this;
            AcceptButton = (IButtonControl)m_runButton;
            AutoScaleDimensions = new SizeF(6f, 13f);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = (IButtonControl)m_closeButton;
            ClientSize = new Size(392, 82);
            Controls.Add((Control)m_processComboBox);
            Controls.Add((Control)label1);
            Controls.Add((Control)m_closeButton);
            Controls.Add((Control)m_runButton);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = nameof(TaskRunnerForm);
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Run Task";
            TopMost = true;
            Load += new EventHandler(OnLoad);
            FormClosed += new FormClosedEventHandler(OnFormClosed);
            ((ISupportInitialize)m_errorProvider).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }
    }
}