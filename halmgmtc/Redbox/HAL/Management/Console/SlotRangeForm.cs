using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Redbox.HAL.Client;
using Redbox.HAL.Common.GUI.Functions;
using Redbox.HAL.Core;

namespace Redbox.HAL.Management.Console
{
    public class SlotRangeForm : Form
    {
        private readonly bool skipSlotCheck;
        private IContainer components;
        private Label label1;
        private Label label2;
        private Label label3;
        private Label label4;
        private Button m_cancelButton;
        private TextBox m_endDeckTextBox;
        private TextBox m_endSlotTextBox;
        private ErrorProvider m_errorProvider;
        private Button m_okButton;
        private Button m_pickEndDeckButton;
        private Button m_pickEndSlotButton;
        private Button m_pickStartDeckButton;
        private Button m_pickStartSlotButton;
        private TextBox m_startDeckTextBox;
        private TextBox m_startSlotTextBox;

        public SlotRangeForm(bool skipSlotCheck)
        {
            this.skipSlotCheck = skipSlotCheck;
            InitializeComponent();
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public SyncRange Range => new SyncRange(int.Parse(m_startDeckTextBox.Text), int.Parse(m_endDeckTextBox.Text),
            new Range(int.Parse(m_startSlotTextBox.Text), int.Parse(m_endSlotTextBox.Text)));

        private void OnOK(object sender, EventArgs e)
        {
            var flag = false;
            m_errorProvider.SetError(m_pickStartDeckButton, string.Empty);
            m_errorProvider.SetError(m_pickStartSlotButton, string.Empty);
            m_errorProvider.SetError(m_pickEndDeckButton, string.Empty);
            m_errorProvider.SetError(m_pickEndSlotButton, string.Empty);
            if (string.IsNullOrEmpty(m_startDeckTextBox.Text))
            {
                m_errorProvider.SetError(m_pickStartDeckButton, "The Start Deck value is required.");
                flag = true;
            }

            if (string.IsNullOrEmpty(m_endDeckTextBox.Text))
            {
                m_errorProvider.SetError(m_pickEndDeckButton, "The End Deck value is required.");
                flag = true;
            }

            if (string.IsNullOrEmpty(m_startSlotTextBox.Text))
            {
                m_errorProvider.SetError(m_pickStartSlotButton, "The Start Slot value is required.");
                flag = true;
            }

            if (string.IsNullOrEmpty(m_endSlotTextBox.Text))
            {
                m_errorProvider.SetError(m_pickEndSlotButton, "The End Slot value is required.");
                flag = true;
            }

            if (!flag && Range.EndDeck < Range.StartDeck)
            {
                m_errorProvider.SetError(m_pickEndDeckButton,
                    "The End Deck value must be greater than or equal to the Start Deck value.");
                flag = true;
            }

            if (skipSlotCheck && !flag && Range.EndDeck == Range.StartDeck && Range.Slots.End < Range.Slots.Start)
            {
                m_errorProvider.SetError(m_pickEndSlotButton,
                    "The End Slot value must be greater than or equal to the Start Slot value.");
                flag = true;
            }
            else if (!skipSlotCheck && !flag && Range.Slots.End < Range.Slots.Start)
            {
                m_errorProvider.SetError(m_pickEndSlotButton,
                    "The End Slot value must be greater than or equal to the Start Slot value.");
                flag = true;
            }

            if (flag)
                return;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void OnCancel(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void OnKeyPress(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar >= '0' && e.KeyChar <= '9') || e.KeyChar == '\b')
                return;
            e.Handled = true;
        }

        private void OnPickStartDeck(object sender, EventArgs e)
        {
            var numberPadForm = new NumberPadForm();
            if (numberPadForm.ShowDialog() != DialogResult.OK)
                return;
            m_startDeckTextBox.Text = numberPadForm.Number;
        }

        private void OnPickStartSlot(object sender, EventArgs e)
        {
            var numberPadForm = new NumberPadForm();
            if (numberPadForm.ShowDialog() != DialogResult.OK)
                return;
            m_startSlotTextBox.Text = numberPadForm.Number;
        }

        private void OnPickEndDeck(object sender, EventArgs e)
        {
            var numberPadForm = new NumberPadForm();
            if (numberPadForm.ShowDialog() != DialogResult.OK)
                return;
            m_endDeckTextBox.Text = numberPadForm.Number;
        }

        private void OnPickEndSlot(object sender, EventArgs e)
        {
            var numberPadForm = new NumberPadForm();
            if (numberPadForm.ShowDialog() != DialogResult.OK)
                return;
            m_endSlotTextBox.Text = numberPadForm.Number;
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
            label4 = new Label();
            m_startDeckTextBox = new TextBox();
            m_startSlotTextBox = new TextBox();
            m_endDeckTextBox = new TextBox();
            m_endSlotTextBox = new TextBox();
            m_pickStartDeckButton = new Button();
            m_pickStartSlotButton = new Button();
            m_pickEndDeckButton = new Button();
            m_pickEndSlotButton = new Button();
            m_okButton = new Button();
            m_cancelButton = new Button();
            m_errorProvider = new ErrorProvider(components);
            ((ISupportInitialize)m_errorProvider).BeginInit();
            SuspendLayout();
            label1.AutoSize = true;
            label1.Location = new Point(12, 12);
            label1.Name = "label1";
            label1.Size = new Size(61, 13);
            label1.TabIndex = 0;
            label1.Text = "Start Deck:";
            label2.AutoSize = true;
            label2.Location = new Point(12, 41);
            label2.Name = "label2";
            label2.Size = new Size(53, 13);
            label2.TabIndex = 3;
            label2.Text = "Start Slot:";
            label3.AutoSize = true;
            label3.Location = new Point(12, 70);
            label3.Name = "label3";
            label3.Size = new Size(58, 13);
            label3.TabIndex = 6;
            label3.Text = "End Deck:";
            label4.AutoSize = true;
            label4.Location = new Point(12, 99);
            label4.Name = "label4";
            label4.Size = new Size(50, 13);
            label4.TabIndex = 9;
            label4.Text = "End Slot:";
            m_startDeckTextBox.Location = new Point(79, 12);
            m_startDeckTextBox.MaxLength = 2;
            m_startDeckTextBox.Name = "m_startDeckTextBox";
            m_startDeckTextBox.Size = new Size(100, 20);
            m_startDeckTextBox.TabIndex = 1;
            m_startDeckTextBox.TextAlign = HorizontalAlignment.Right;
            m_startDeckTextBox.KeyPress += OnKeyPress;
            m_startSlotTextBox.Location = new Point(79, 41);
            m_startSlotTextBox.MaxLength = 3;
            m_startSlotTextBox.Name = "m_startSlotTextBox";
            m_startSlotTextBox.Size = new Size(100, 20);
            m_startSlotTextBox.TabIndex = 4;
            m_startSlotTextBox.TextAlign = HorizontalAlignment.Right;
            m_startSlotTextBox.KeyPress += OnKeyPress;
            m_endDeckTextBox.Location = new Point(79, 70);
            m_endDeckTextBox.MaxLength = 2;
            m_endDeckTextBox.Name = "m_endDeckTextBox";
            m_endDeckTextBox.Size = new Size(100, 20);
            m_endDeckTextBox.TabIndex = 7;
            m_endDeckTextBox.TextAlign = HorizontalAlignment.Right;
            m_endDeckTextBox.KeyPress += OnKeyPress;
            m_endSlotTextBox.Location = new Point(79, 99);
            m_endSlotTextBox.MaxLength = 3;
            m_endSlotTextBox.Name = "m_endSlotTextBox";
            m_endSlotTextBox.Size = new Size(100, 20);
            m_endSlotTextBox.TabIndex = 10;
            m_endSlotTextBox.TextAlign = HorizontalAlignment.Right;
            m_endSlotTextBox.KeyPress += OnKeyPress;
            m_pickStartDeckButton.Location = new Point(185, 12);
            m_pickStartDeckButton.Name = "m_pickStartDeckButton";
            m_pickStartDeckButton.Size = new Size(31, 23);
            m_pickStartDeckButton.TabIndex = 2;
            m_pickStartDeckButton.TabStop = false;
            m_pickStartDeckButton.Text = "...";
            m_pickStartDeckButton.UseVisualStyleBackColor = true;
            m_pickStartDeckButton.Click += OnPickStartDeck;
            m_pickStartSlotButton.Location = new Point(185, 41);
            m_pickStartSlotButton.Name = "m_pickStartSlotButton";
            m_pickStartSlotButton.Size = new Size(31, 23);
            m_pickStartSlotButton.TabIndex = 5;
            m_pickStartSlotButton.TabStop = false;
            m_pickStartSlotButton.Text = "...";
            m_pickStartSlotButton.UseVisualStyleBackColor = true;
            m_pickStartSlotButton.Click += OnPickStartSlot;
            m_pickEndDeckButton.Location = new Point(185, 70);
            m_pickEndDeckButton.Name = "m_pickEndDeckButton";
            m_pickEndDeckButton.Size = new Size(31, 23);
            m_pickEndDeckButton.TabIndex = 8;
            m_pickEndDeckButton.TabStop = false;
            m_pickEndDeckButton.Text = "...";
            m_pickEndDeckButton.UseVisualStyleBackColor = true;
            m_pickEndDeckButton.Click += OnPickEndDeck;
            m_pickEndSlotButton.Location = new Point(185, 99);
            m_pickEndSlotButton.Name = "m_pickEndSlotButton";
            m_pickEndSlotButton.Size = new Size(31, 23);
            m_pickEndSlotButton.TabIndex = 11;
            m_pickEndSlotButton.TabStop = false;
            m_pickEndSlotButton.Text = "...";
            m_pickEndSlotButton.UseVisualStyleBackColor = true;
            m_pickEndSlotButton.Click += OnPickEndSlot;
            m_okButton.Location = new Point(245, 12);
            m_okButton.Name = "m_okButton";
            m_okButton.Size = new Size(75, 23);
            m_okButton.TabIndex = 12;
            m_okButton.Text = "OK";
            m_okButton.UseVisualStyleBackColor = true;
            m_okButton.Click += OnOK;
            m_cancelButton.DialogResult = DialogResult.Cancel;
            m_cancelButton.Location = new Point(245, 41);
            m_cancelButton.Name = "m_cancelButton";
            m_cancelButton.Size = new Size(75, 23);
            m_cancelButton.TabIndex = 13;
            m_cancelButton.Text = "Cancel";
            m_cancelButton.UseVisualStyleBackColor = true;
            m_cancelButton.Click += OnCancel;
            m_errorProvider.ContainerControl = this;
            AcceptButton = m_okButton;
            AutoScaleDimensions = new SizeF(6f, 13f);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = m_cancelButton;
            ClientSize = new Size(332, 138);
            ControlBox = false;
            Controls.Add(m_cancelButton);
            Controls.Add(m_okButton);
            Controls.Add(m_pickEndSlotButton);
            Controls.Add(m_pickEndDeckButton);
            Controls.Add(m_pickStartSlotButton);
            Controls.Add(m_pickStartDeckButton);
            Controls.Add(m_endSlotTextBox);
            Controls.Add(m_endDeckTextBox);
            Controls.Add(m_startSlotTextBox);
            Controls.Add(m_startDeckTextBox);
            Controls.Add(label4);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(label1);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Name = nameof(SlotRangeForm);
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Deck/Slot Range";
            ((ISupportInitialize)m_errorProvider).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }
    }
}