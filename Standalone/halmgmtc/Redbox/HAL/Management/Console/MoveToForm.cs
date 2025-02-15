using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Redbox.HAL.Common.GUI.Functions;

namespace Redbox.HAL.Management.Console
{
    public class MoveToForm : Form
    {
        private IContainer components;
        private Label label1;
        private Label label2;
        private bool m_activated;
        private Button m_cancelButton;
        private TextBox m_deckTextBox;
        private RadioButton m_getModeRadioButton;
        private RadioButton m_normalModeRadioButton;
        private Button m_okButton;
        private Button m_pickDeckButton;
        private Button m_pickSlotButton;
        private RadioButton m_putModeRadioButton;
        private TextBox m_slotTextBox;

        public MoveToForm()
        {
            InitializeComponent();
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int? Deck
        {
            get
            {
                int result;
                return !string.IsNullOrEmpty(m_deckTextBox.Text) && int.TryParse(m_deckTextBox.Text, out result)
                    ? result
                    : new int?();
            }
            set => m_deckTextBox.Text = value.ToString();
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int? Slot
        {
            get
            {
                int result;
                return !string.IsNullOrEmpty(m_slotTextBox.Text) && int.TryParse(m_slotTextBox.Text, out result)
                    ? result
                    : new int?();
            }
            set => m_slotTextBox.Text = value.ToString();
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Mode
        {
            get
            {
                if (m_normalModeRadioButton.Checked)
                    return string.Empty;
                if (m_getModeRadioButton.Checked)
                    return "GET";
                return !m_putModeRadioButton.Checked ? string.Empty : "PUT";
            }
        }

        public void ClearFields()
        {
            m_deckTextBox.Clear();
            m_slotTextBox.Clear();
        }

        public void DisableModes()
        {
            m_normalModeRadioButton.Enabled = false;
            m_getModeRadioButton.Enabled = false;
            m_putModeRadioButton.Enabled = false;
        }

        private void OnKeyPress(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar >= '0' && e.KeyChar <= '9') || e.KeyChar == '\b')
                return;
            e.Handled = true;
        }

        private void OnPickDeck(object sender, EventArgs e)
        {
            var numberPadForm1 = new NumberPadForm();
            numberPadForm1.Text = "Enter Deck Number";
            var numberPadForm2 = numberPadForm1;
            if (numberPadForm2.ShowDialog() != DialogResult.OK)
                return;
            m_deckTextBox.Text = numberPadForm2.Number;
            m_slotTextBox.Focus();
        }

        private void OnPickSlot(object sender, EventArgs e)
        {
            var numberPadForm1 = new NumberPadForm();
            numberPadForm1.Text = "Enter Slot Number";
            var numberPadForm2 = numberPadForm1;
            if (numberPadForm2.ShowDialog() != DialogResult.OK)
                return;
            m_slotTextBox.Text = numberPadForm2.Number;
            m_okButton.Focus();
        }

        private void OnOK(object sender, EventArgs e)
        {
            m_activated = false;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void OnCancel(object sender, EventArgs e)
        {
            m_activated = false;
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void OnActivated(object sender, EventArgs e)
        {
            if (m_activated)
                return;
            m_deckTextBox.Focus();
            m_activated = true;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            label1 = new Label();
            label2 = new Label();
            m_deckTextBox = new TextBox();
            m_slotTextBox = new TextBox();
            m_pickDeckButton = new Button();
            m_pickSlotButton = new Button();
            m_okButton = new Button();
            m_cancelButton = new Button();
            m_normalModeRadioButton = new RadioButton();
            m_getModeRadioButton = new RadioButton();
            m_putModeRadioButton = new RadioButton();
            SuspendLayout();
            label1.AutoSize = true;
            label1.Location = new Point(12, 9);
            label1.Name = "label1";
            label1.Size = new Size(36, 13);
            label1.TabIndex = 0;
            label1.Text = "Deck:";
            label2.AutoSize = true;
            label2.Location = new Point(12, 41);
            label2.Name = "label2";
            label2.Size = new Size(28, 13);
            label2.TabIndex = 2;
            label2.Text = "Slot:";
            m_deckTextBox.Location = new Point(54, 9);
            m_deckTextBox.Name = "m_deckTextBox";
            m_deckTextBox.Size = new Size(90, 20);
            m_deckTextBox.TabIndex = 1;
            m_deckTextBox.TextAlign = HorizontalAlignment.Right;
            m_deckTextBox.KeyPress += OnKeyPress;
            m_slotTextBox.Location = new Point(54, 41);
            m_slotTextBox.Name = "m_slotTextBox";
            m_slotTextBox.Size = new Size(90, 20);
            m_slotTextBox.TabIndex = 3;
            m_slotTextBox.TextAlign = HorizontalAlignment.Right;
            m_pickDeckButton.Location = new Point(150, 7);
            m_pickDeckButton.Name = "m_pickDeckButton";
            m_pickDeckButton.Size = new Size(54, 23);
            m_pickDeckButton.TabIndex = 4;
            m_pickDeckButton.TabStop = false;
            m_pickDeckButton.Text = "...";
            m_pickDeckButton.UseVisualStyleBackColor = true;
            m_pickDeckButton.Click += OnPickDeck;
            m_pickSlotButton.Location = new Point(150, 41);
            m_pickSlotButton.Name = "m_pickSlotButton";
            m_pickSlotButton.Size = new Size(54, 23);
            m_pickSlotButton.TabIndex = 5;
            m_pickSlotButton.TabStop = false;
            m_pickSlotButton.Text = "...";
            m_pickSlotButton.UseVisualStyleBackColor = true;
            m_pickSlotButton.Click += OnPickSlot;
            m_okButton.Location = new Point(15, 158);
            m_okButton.Name = "m_okButton";
            m_okButton.Size = new Size(75, 23);
            m_okButton.TabIndex = 7;
            m_okButton.Text = "OK";
            m_okButton.UseVisualStyleBackColor = true;
            m_okButton.Click += OnOK;
            m_cancelButton.DialogResult = DialogResult.Cancel;
            m_cancelButton.Location = new Point(129, 158);
            m_cancelButton.Name = "m_cancelButton";
            m_cancelButton.Size = new Size(75, 23);
            m_cancelButton.TabIndex = 8;
            m_cancelButton.Text = "Cancel";
            m_cancelButton.UseVisualStyleBackColor = true;
            m_cancelButton.Click += OnCancel;
            m_normalModeRadioButton.AutoSize = true;
            m_normalModeRadioButton.Checked = true;
            m_normalModeRadioButton.Location = new Point(15, 76);
            m_normalModeRadioButton.Name = "m_normalModeRadioButton";
            m_normalModeRadioButton.Size = new Size(88, 17);
            m_normalModeRadioButton.TabIndex = 4;
            m_normalModeRadioButton.TabStop = true;
            m_normalModeRadioButton.Text = "Normal Mode";
            m_normalModeRadioButton.UseVisualStyleBackColor = true;
            m_getModeRadioButton.AutoSize = true;
            m_getModeRadioButton.Location = new Point(15, 99);
            m_getModeRadioButton.Name = "m_getModeRadioButton";
            m_getModeRadioButton.Size = new Size(72, 17);
            m_getModeRadioButton.TabIndex = 5;
            m_getModeRadioButton.TabStop = true;
            m_getModeRadioButton.Text = "Get Mode";
            m_getModeRadioButton.UseVisualStyleBackColor = true;
            m_putModeRadioButton.AutoSize = true;
            m_putModeRadioButton.Location = new Point(15, 122);
            m_putModeRadioButton.Name = "m_putModeRadioButton";
            m_putModeRadioButton.Size = new Size(71, 17);
            m_putModeRadioButton.TabIndex = 6;
            m_putModeRadioButton.TabStop = true;
            m_putModeRadioButton.Text = "Put Mode";
            m_putModeRadioButton.UseVisualStyleBackColor = true;
            AcceptButton = m_okButton;
            AutoScaleDimensions = new SizeF(6f, 13f);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = m_cancelButton;
            ClientSize = new Size(219, 193);
            ControlBox = false;
            Controls.Add(m_putModeRadioButton);
            Controls.Add(m_getModeRadioButton);
            Controls.Add(m_normalModeRadioButton);
            Controls.Add(m_cancelButton);
            Controls.Add(m_okButton);
            Controls.Add(m_pickSlotButton);
            Controls.Add(m_pickDeckButton);
            Controls.Add(m_slotTextBox);
            Controls.Add(m_deckTextBox);
            Controls.Add(label2);
            Controls.Add(label1);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Name = nameof(MoveToForm);
            StartPosition = FormStartPosition.CenterParent;
            Text = "Move To Location";
            Activated += OnActivated;
            ResumeLayout(false);
            PerformLayout();
        }
    }
}