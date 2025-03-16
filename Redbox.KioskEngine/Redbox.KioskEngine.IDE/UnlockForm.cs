using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Redbox.KioskEngine.IDE
{
    public class UnlockForm : Form
    {
        private IContainer components;
        private ErrorProvider m_errorProvider;
        private Button m_okButton;
        private Button m_cancelButton;
        private TextBox m_textBox;
        private Label label1;

        public UnlockForm()
        {
            InitializeComponent();
        }

        [Browsable(false)] public string Password => m_textBox.Text;

        private void OnOK(object sender, EventArgs e)
        {
            m_errorProvider.SetError((Control)m_textBox, string.Empty);
            if (string.IsNullOrEmpty(m_textBox.Text))
            {
                m_errorProvider.SetError((Control)m_textBox, "Password is a required field.");
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
            m_errorProvider = new ErrorProvider(components);
            m_okButton = new Button();
            m_cancelButton = new Button();
            label1 = new Label();
            m_textBox = new TextBox();
            ((ISupportInitialize)m_errorProvider).BeginInit();
            SuspendLayout();
            m_errorProvider.ContainerControl = (ContainerControl)this;
            m_okButton.Location = new Point(205, 12);
            m_okButton.Name = "m_okButton";
            m_okButton.Size = new Size(75, 23);
            m_okButton.TabIndex = 2;
            m_okButton.Text = "OK";
            m_okButton.UseVisualStyleBackColor = true;
            m_okButton.Click += new EventHandler(OnOK);
            m_cancelButton.DialogResult = DialogResult.Cancel;
            m_cancelButton.Location = new Point(205, 41);
            m_cancelButton.Name = "m_cancelButton";
            m_cancelButton.Size = new Size(75, 23);
            m_cancelButton.TabIndex = 3;
            m_cancelButton.Text = "Cancel";
            m_cancelButton.UseVisualStyleBackColor = true;
            m_cancelButton.Click += new EventHandler(OnCancel);
            label1.Location = new Point(12, 12);
            label1.Name = "label1";
            label1.Size = new Size(187, 35);
            label1.TabIndex = 0;
            label1.Text = "Enter the administrator password to unlock the management console:";
            m_textBox.Location = new Point(15, 44);
            m_textBox.MaxLength = 16;
            m_textBox.Name = "m_textBox";
            m_textBox.Size = new Size(165, 20);
            m_textBox.TabIndex = 1;
            m_textBox.UseSystemPasswordChar = true;
            AcceptButton = (IButtonControl)m_okButton;
            AutoScaleDimensions = new SizeF(6f, 13f);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = (IButtonControl)m_cancelButton;
            ClientSize = new Size(292, 85);
            ControlBox = false;
            Controls.Add((Control)m_textBox);
            Controls.Add((Control)label1);
            Controls.Add((Control)m_cancelButton);
            Controls.Add((Control)m_okButton);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Name = nameof(UnlockForm);
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Unlock Management Console";
            Load += new EventHandler(OnLoad);
            FormClosed += new FormClosedEventHandler(OnFormClosed);
            ((ISupportInitialize)m_errorProvider).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }
    }
}