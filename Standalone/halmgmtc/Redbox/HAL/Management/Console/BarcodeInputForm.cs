using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Redbox.HAL.Common.GUI.Functions;

namespace Redbox.HAL.Management.Console
{
    public class BarcodeInputForm : Form
    {
        private IContainer components;
        private Button m_addButton;
        private Button m_cancelButton;
        private Button m_clearButton;
        private ListBox m_listBox;
        private NumberPad m_numberPad;
        private Button m_okButton;
        private Button m_removeButton;

        public BarcodeInputForm()
        {
            InitializeComponent();
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string[] Barcodes
        {
            get
            {
                var stringList = new List<string>();
                foreach (string str in m_listBox.Items)
                    stringList.Add(str);
                return stringList.ToArray();
            }
            set
            {
                m_listBox.Items.Clear();
                m_listBox.Items.AddRange(value);
            }
        }

        private void OnNumberChanged(object sender, EventArgs e)
        {
            m_addButton.Enabled = !string.IsNullOrEmpty(m_numberPad.Number);
        }

        private void OnAdd(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(m_numberPad.Number))
            {
                m_listBox.SelectedIndex = m_listBox.Items.Add(m_numberPad.Number);
                m_numberPad.Clear();
            }

            SetButtonState();
            m_numberPad.Focus();
        }

        private void OnRemove(object sender, EventArgs e)
        {
            m_listBox.Items.RemoveAt(m_listBox.SelectedIndex);
            if (m_listBox.Items.Count > 0)
                m_listBox.SelectedIndex = m_listBox.Items.Count - 1;
            SetButtonState();
            m_numberPad.Focus();
        }

        private void OnClear(object sender, EventArgs e)
        {
            m_listBox.Items.Clear();
            SetButtonState();
            m_numberPad.Focus();
        }

        private void OnSelectedIndexChanged(object sender, EventArgs e)
        {
            SetButtonState();
            m_numberPad.Focus();
        }

        private void SetButtonState()
        {
            m_removeButton.Enabled = m_listBox.SelectedIndex != -1;
            m_clearButton.Enabled = m_listBox.Items.Count > 0;
            m_okButton.Enabled = m_listBox.Items.Count > 0;
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

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            m_listBox = new ListBox();
            m_addButton = new Button();
            m_removeButton = new Button();
            m_clearButton = new Button();
            m_cancelButton = new Button();
            m_okButton = new Button();
            m_numberPad = new NumberPad();
            SuspendLayout();
            m_listBox.FormattingEnabled = true;
            m_listBox.Location = new Point(3, 3);
            m_listBox.Name = "m_listBox";
            m_listBox.Size = new Size(429, 173);
            m_listBox.TabIndex = 4;
            m_listBox.SelectedIndexChanged += OnSelectedIndexChanged;
            m_addButton.Enabled = false;
            m_addButton.Location = new Point(440, 182);
            m_addButton.Name = "m_addButton";
            m_addButton.Size = new Size(75, 44);
            m_addButton.TabIndex = 1;
            m_addButton.Text = "Add";
            m_addButton.UseVisualStyleBackColor = true;
            m_addButton.Click += OnAdd;
            m_removeButton.Enabled = false;
            m_removeButton.Location = new Point(440, 232);
            m_removeButton.Name = "m_removeButton";
            m_removeButton.Size = new Size(75, 44);
            m_removeButton.TabIndex = 2;
            m_removeButton.Text = "Remove";
            m_removeButton.UseVisualStyleBackColor = true;
            m_removeButton.Click += OnRemove;
            m_clearButton.Enabled = false;
            m_clearButton.Location = new Point(440, 282);
            m_clearButton.Name = "m_clearButton";
            m_clearButton.Size = new Size(75, 44);
            m_clearButton.TabIndex = 3;
            m_clearButton.Text = "Clear";
            m_clearButton.UseVisualStyleBackColor = true;
            m_clearButton.Click += OnClear;
            m_cancelButton.DialogResult = DialogResult.Cancel;
            m_cancelButton.Location = new Point(440, 53);
            m_cancelButton.Name = "m_cancelButton";
            m_cancelButton.Size = new Size(75, 44);
            m_cancelButton.TabIndex = 6;
            m_cancelButton.Text = "Cancel";
            m_cancelButton.UseVisualStyleBackColor = true;
            m_cancelButton.Click += OnCancel;
            m_okButton.Enabled = false;
            m_okButton.Location = new Point(440, 3);
            m_okButton.Name = "m_okButton";
            m_okButton.Size = new Size(75, 44);
            m_okButton.TabIndex = 5;
            m_okButton.Text = "OK";
            m_okButton.UseVisualStyleBackColor = true;
            m_okButton.Click += OnOK;
            m_numberPad.Location = new Point(3, 182);
            m_numberPad.Name = "m_numberPad";
            m_numberPad.Size = new Size(426, 178);
            m_numberPad.TabIndex = 0;
            m_numberPad.NumberChanged += OnNumberChanged;
            AcceptButton = m_okButton;
            AutoScaleDimensions = new SizeF(6f, 13f);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = m_cancelButton;
            ClientSize = new Size(521, 365);
            ControlBox = false;
            Controls.Add(m_numberPad);
            Controls.Add(m_okButton);
            Controls.Add(m_cancelButton);
            Controls.Add(m_clearButton);
            Controls.Add(m_removeButton);
            Controls.Add(m_addButton);
            Controls.Add(m_listBox);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Name = nameof(BarcodeInputForm);
            Padding = new Padding(3);
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Barcodes";
            ResumeLayout(false);
        }
    }
}