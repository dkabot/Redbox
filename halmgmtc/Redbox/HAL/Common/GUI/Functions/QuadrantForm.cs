using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Redbox.HAL.Common.GUI.Functions
{
    public class QuadrantForm : Form
    {
        private IContainer components;
        private Label label1;
        private Label label2;
        private Label label3;
        private Button m_cancelButton;
        private TextBox m_endSlotTextBox;
        private ErrorProvider m_errorProvider;
        private TextBox m_offsetTextBox;
        private Button m_okButton;
        private TextBox m_startSlotTextBox;

        public QuadrantForm()
        {
            InitializeComponent();
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int? Offset
        {
            get
            {
                int result;
                return int.TryParse(m_offsetTextBox.Text, out result) ? result : new int?();
            }
            set
            {
                m_offsetTextBox.Clear();
                if (!value.HasValue)
                    return;
                m_offsetTextBox.Text = value.ToString();
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int? StartSlot
        {
            get
            {
                int result;
                return int.TryParse(m_startSlotTextBox.Text, out result) ? result : new int?();
            }
            set
            {
                m_startSlotTextBox.Clear();
                if (!value.HasValue)
                    return;
                m_startSlotTextBox.Text = value.ToString();
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int? EndSlot
        {
            get
            {
                int result;
                return int.TryParse(m_endSlotTextBox.Text, out result) ? result : new int?();
            }
            set
            {
                m_endSlotTextBox.Clear();
                if (!value.HasValue)
                    return;
                m_endSlotTextBox.Text = value.ToString();
            }
        }

        private void OnOK(object sender, EventArgs e)
        {
            m_errorProvider.SetError(m_offsetTextBox, string.Empty);
            m_errorProvider.SetError(m_startSlotTextBox, string.Empty);
            m_errorProvider.SetError(m_endSlotTextBox, string.Empty);
            if (Offset.HasValue)
            {
                var offset = Offset;
                var num = 0;
                if ((offset.GetValueOrDefault() < num ? offset.HasValue ? 1 : 0 : 0) == 0)
                {
                    DialogResult = DialogResult.OK;
                    Close();
                    return;
                }
            }

            m_errorProvider.SetError(m_offsetTextBox,
                "Offset is a required value and must be a postive integer value.");
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
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            m_offsetTextBox = new TextBox();
            m_startSlotTextBox = new TextBox();
            m_endSlotTextBox = new TextBox();
            m_errorProvider = new ErrorProvider(components);
            m_okButton = new Button();
            m_cancelButton = new Button();
            ((ISupportInitialize)m_errorProvider).BeginInit();
            SuspendLayout();
            label1.AutoSize = true;
            label1.Location = new Point(12, 15);
            label1.Name = "label1";
            label1.Size = new Size(38, 13);
            label1.TabIndex = 0;
            label1.Text = "Offset:";
            label2.AutoSize = true;
            label2.Location = new Point(12, 43);
            label2.Name = "label2";
            label2.Size = new Size(53, 13);
            label2.TabIndex = 2;
            label2.Text = "Start Slot:";
            label3.AutoSize = true;
            label3.Location = new Point(12, 75);
            label3.Name = "label3";
            label3.Size = new Size(50, 13);
            label3.TabIndex = 4;
            label3.Text = "End Slot:";
            m_offsetTextBox.Location = new Point(75, 12);
            m_offsetTextBox.Name = "m_offsetTextBox";
            m_offsetTextBox.Size = new Size(100, 20);
            m_offsetTextBox.TabIndex = 1;
            m_startSlotTextBox.Location = new Point(75, 40);
            m_startSlotTextBox.Name = "m_startSlotTextBox";
            m_startSlotTextBox.Size = new Size(100, 20);
            m_startSlotTextBox.TabIndex = 3;
            m_endSlotTextBox.Location = new Point(75, 72);
            m_endSlotTextBox.Name = "m_endSlotTextBox";
            m_endSlotTextBox.Size = new Size(100, 20);
            m_endSlotTextBox.TabIndex = 5;
            m_errorProvider.ContainerControl = this;
            m_okButton.Location = new Point(15, 107);
            m_okButton.Name = "m_okButton";
            m_okButton.Size = new Size(75, 23);
            m_okButton.TabIndex = 6;
            m_okButton.Text = "OK";
            m_okButton.UseVisualStyleBackColor = true;
            m_okButton.Click += OnOK;
            m_cancelButton.DialogResult = DialogResult.Cancel;
            m_cancelButton.Location = new Point(133, 108);
            m_cancelButton.Name = "m_cancelButton";
            m_cancelButton.Size = new Size(75, 23);
            m_cancelButton.TabIndex = 7;
            m_cancelButton.Text = "Cancel";
            m_cancelButton.UseVisualStyleBackColor = true;
            m_cancelButton.Click += OnCancel;
            AcceptButton = m_okButton;
            AutoScaleDimensions = new SizeF(6f, 13f);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = m_cancelButton;
            ClientSize = new Size(220, 142);
            ControlBox = false;
            Controls.Add(m_cancelButton);
            Controls.Add(m_okButton);
            Controls.Add(m_endSlotTextBox);
            Controls.Add(m_startSlotTextBox);
            Controls.Add(m_offsetTextBox);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(label1);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Name = nameof(QuadrantForm);
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Edit Deck Quadrant";
            ((ISupportInitialize)m_errorProvider).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }
    }
}