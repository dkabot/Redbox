using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Redbox.KioskEngine.IDE
{
    public class FindKeyValueForm : Form
    {
        private IContainer components;
        private RadioButton m_searchByKeyRadioButton;
        private RadioButton m_searchByValueRadioButton;
        private TextBox m_valueTextBox;
        private Label label1;
        private ErrorProvider m_errorProvider;
        private Button m_cancelButton;
        private Button m_okButton;

        public FindKeyValueForm()
        {
            InitializeComponent();
        }

        public KeyValueSearchType SearchType =>
            !m_searchByKeyRadioButton.Checked ? KeyValueSearchType.Value : KeyValueSearchType.Key;

        public string SearchFor
        {
            get => m_valueTextBox.Text;
            set => m_valueTextBox.Text = value;
        }

        private void OnOK(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void OnCancel(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void OnCheckChanged(object sender, EventArgs e)
        {
            m_valueTextBox.Focus();
        }

        private void OnLoad(object sender, EventArgs e)
        {
            m_valueTextBox.Focus();
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
            m_searchByKeyRadioButton = new RadioButton();
            m_searchByValueRadioButton = new RadioButton();
            m_valueTextBox = new TextBox();
            label1 = new Label();
            m_errorProvider = new ErrorProvider(components);
            m_okButton = new Button();
            m_cancelButton = new Button();
            ((ISupportInitialize)m_errorProvider).BeginInit();
            SuspendLayout();
            m_searchByKeyRadioButton.AutoSize = true;
            m_searchByKeyRadioButton.Checked = true;
            m_searchByKeyRadioButton.Location = new Point(19, 22);
            m_searchByKeyRadioButton.Name = "m_searchByKeyRadioButton";
            m_searchByKeyRadioButton.Size = new Size(43, 17);
            m_searchByKeyRadioButton.TabIndex = 1;
            m_searchByKeyRadioButton.TabStop = true;
            m_searchByKeyRadioButton.Text = "Key";
            m_searchByKeyRadioButton.UseVisualStyleBackColor = true;
            m_searchByKeyRadioButton.CheckedChanged += new EventHandler(OnCheckChanged);
            m_searchByValueRadioButton.AutoSize = true;
            m_searchByValueRadioButton.Location = new Point(19, 45);
            m_searchByValueRadioButton.Name = "m_searchByValueRadioButton";
            m_searchByValueRadioButton.Size = new Size(52, 17);
            m_searchByValueRadioButton.TabIndex = 2;
            m_searchByValueRadioButton.Text = "Value";
            m_searchByValueRadioButton.UseVisualStyleBackColor = true;
            m_searchByValueRadioButton.CheckedChanged += new EventHandler(OnCheckChanged);
            m_valueTextBox.Location = new Point(86, 42);
            m_valueTextBox.Name = "m_valueTextBox";
            m_valueTextBox.Size = new Size(184, 20);
            m_valueTextBox.TabIndex = 0;
            label1.AutoSize = true;
            label1.Location = new Point(83, 24);
            label1.Name = "label1";
            label1.Size = new Size(62, 13);
            label1.TabIndex = 2;
            label1.Text = "Search For:";
            m_errorProvider.ContainerControl = (ContainerControl)this;
            m_okButton.Location = new Point(313, 12);
            m_okButton.Name = "m_okButton";
            m_okButton.Size = new Size(75, 23);
            m_okButton.TabIndex = 3;
            m_okButton.Text = "OK";
            m_okButton.UseVisualStyleBackColor = true;
            m_okButton.Click += new EventHandler(OnOK);
            m_cancelButton.DialogResult = DialogResult.Cancel;
            m_cancelButton.Location = new Point(313, 45);
            m_cancelButton.Name = "m_cancelButton";
            m_cancelButton.Size = new Size(75, 23);
            m_cancelButton.TabIndex = 4;
            m_cancelButton.Text = "Cancel";
            m_cancelButton.UseVisualStyleBackColor = true;
            m_cancelButton.Click += new EventHandler(OnCancel);
            AcceptButton = (IButtonControl)m_okButton;
            AutoScaleDimensions = new SizeF(6f, 13f);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = (IButtonControl)m_cancelButton;
            ClientSize = new Size(400, 87);
            ControlBox = false;
            Controls.Add((Control)m_cancelButton);
            Controls.Add((Control)m_okButton);
            Controls.Add((Control)label1);
            Controls.Add((Control)m_valueTextBox);
            Controls.Add((Control)m_searchByValueRadioButton);
            Controls.Add((Control)m_searchByKeyRadioButton);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Name = nameof(FindKeyValueForm);
            StartPosition = FormStartPosition.CenterParent;
            Text = "Find Key/Value";
            Load += new EventHandler(OnLoad);
            ((ISupportInitialize)m_errorProvider).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }
    }
}