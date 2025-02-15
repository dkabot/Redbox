using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Redbox.HAL.Management.Console
{
    public class InputBoxForm : Form
    {
        private IContainer components;
        private Button m_cancelButton;
        private ErrorProvider m_errorProvider;
        private Label m_label;
        private Button m_okButton;
        private MaskedTextBox m_textBox;

        public InputBoxForm()
        {
            InitializeComponent();
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Label
        {
            get => m_label.Text;
            set => m_label.Text = value;
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Value
        {
            get => m_textBox.Text;
            set => m_textBox.Text = value;
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsValueRequired { get; set; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string ErrorMessage { get; set; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string InputMask
        {
            get => m_textBox.Mask;
            set => m_textBox.Mask = value;
        }

        private void OnOK(object sender, EventArgs e)
        {
            m_errorProvider.SetError(m_textBox, string.Empty);
            if (IsValueRequired && string.IsNullOrEmpty(m_textBox.Text))
            {
                m_errorProvider.SetError(m_textBox, ErrorMessage ?? "A value is required.");
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
            m_label = new Label();
            m_okButton = new Button();
            m_cancelButton = new Button();
            m_errorProvider = new ErrorProvider(components);
            m_textBox = new MaskedTextBox();
            ((ISupportInitialize)m_errorProvider).BeginInit();
            SuspendLayout();
            m_label.AutoSize = true;
            m_label.Location = new Point(9, 9);
            m_label.Name = "m_label";
            m_label.Size = new Size(104, 13);
            m_label.TabIndex = 0;
            m_label.Text = "Place your text here:";
            m_okButton.Location = new Point(291, 12);
            m_okButton.Name = "m_okButton";
            m_okButton.Size = new Size(75, 23);
            m_okButton.TabIndex = 2;
            m_okButton.Text = "OK";
            m_okButton.UseVisualStyleBackColor = true;
            m_okButton.Click += OnOK;
            m_cancelButton.DialogResult = DialogResult.Cancel;
            m_cancelButton.Location = new Point(291, 41);
            m_cancelButton.Name = "m_cancelButton";
            m_cancelButton.Size = new Size(75, 23);
            m_cancelButton.TabIndex = 3;
            m_cancelButton.Text = "Cancel";
            m_cancelButton.UseVisualStyleBackColor = true;
            m_cancelButton.Click += OnCancel;
            m_errorProvider.ContainerControl = this;
            m_textBox.Location = new Point(13, 25);
            m_textBox.Name = "m_textBox";
            m_textBox.Size = new Size(258, 20);
            m_textBox.TabIndex = 1;
            AcceptButton = m_okButton;
            AutoScaleDimensions = new SizeF(6f, 13f);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = m_cancelButton;
            ClientSize = new Size(378, 78);
            ControlBox = false;
            Controls.Add(m_textBox);
            Controls.Add(m_cancelButton);
            Controls.Add(m_okButton);
            Controls.Add(m_label);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Name = nameof(InputBoxForm);
            StartPosition = FormStartPosition.CenterParent;
            Text = "Input Box";
            ((ISupportInitialize)m_errorProvider).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }
    }
}