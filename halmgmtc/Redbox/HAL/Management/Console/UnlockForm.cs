using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Redbox.HAL.Management.Console
{
    public class UnlockForm : Form
    {
        private IContainer components;
        private Label label1;
        private Button m_cancelButton;
        private ErrorProvider m_errorProvider;
        private Button m_okButton;
        private TextBox m_textBox;

        public UnlockForm()
        {
            InitializeComponent();
        }

        [Browsable(false)] public string Password => m_textBox.Text;

        private void OnOK(object sender, EventArgs e)
        {
            m_errorProvider.SetError(m_textBox, string.Empty);
            if (string.IsNullOrEmpty(m_textBox.Text))
            {
                m_errorProvider.SetError(m_textBox, "Password is a required field.");
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

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            components = new Container();
            m_errorProvider = new ErrorProvider(components);
            m_okButton = new Button();
            m_cancelButton = new Button();
            label1 = new Label();
            m_textBox = new TextBox();
            ((ISupportInitialize)m_errorProvider).BeginInit();
            SuspendLayout();
            m_errorProvider.ContainerControl = this;
            m_okButton.Location = new Point(205, 12);
            m_okButton.Name = "m_okButton";
            m_okButton.Size = new Size(75, 23);
            m_okButton.TabIndex = 2;
            m_okButton.Text = "OK";
            m_okButton.UseVisualStyleBackColor = true;
            m_okButton.Click += OnOK;
            m_cancelButton.DialogResult = DialogResult.Cancel;
            m_cancelButton.Location = new Point(205, 41);
            m_cancelButton.Name = "m_cancelButton";
            m_cancelButton.Size = new Size(75, 23);
            m_cancelButton.TabIndex = 3;
            m_cancelButton.Text = "Cancel";
            m_cancelButton.UseVisualStyleBackColor = true;
            m_cancelButton.Click += OnCancel;
            label1.Location = new Point(12, 12);
            label1.Name = "label1";
            label1.Size = new Size(187, 35);
            label1.TabIndex = 0;
            label1.Text = "Enter the administrator password to unlock settings:";
            m_textBox.Location = new Point(15, 44);
            m_textBox.MaxLength = 16;
            m_textBox.Name = "m_textBox";
            m_textBox.Size = new Size(165, 20);
            m_textBox.TabIndex = 1;
            m_textBox.UseSystemPasswordChar = true;
            AcceptButton = m_okButton;
            AutoScaleDimensions = new SizeF(6f, 13f);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = m_cancelButton;
            ClientSize = new Size(292, 85);
            ControlBox = false;
            Controls.Add(m_textBox);
            Controls.Add(label1);
            Controls.Add(m_cancelButton);
            Controls.Add(m_okButton);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Name = nameof(UnlockForm);
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Unlock Settings";
            ((ISupportInitialize)m_errorProvider).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }
    }
}