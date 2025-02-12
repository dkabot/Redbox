using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Redbox.HAL.Common.GUI.Functions
{
    public class ComputeQuadrantsForm : Form
    {
        private IContainer components;
        private Label label1;
        private Label label2;
        private Label label3;
        private Button m_cancelButton;
        private ErrorProvider m_errorProvider;
        private TextBox m_numberOfQuadrantsTextBox;
        private Button m_okButton;
        private TextBox m_slotsPerQuadrantTextBox;
        private TextBox m_startOffsetTextBox;

        public ComputeQuadrantsForm()
        {
            InitializeComponent();
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public decimal? StartOffset
        {
            get
            {
                decimal result;
                return decimal.TryParse(m_startOffsetTextBox.Text, out result) ? result : new decimal?();
            }
            set
            {
                m_startOffsetTextBox.Clear();
                if (!value.HasValue)
                    return;
                m_startOffsetTextBox.Text = value.ToString();
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int? NumberOfQuadrants
        {
            get
            {
                int result;
                return int.TryParse(m_numberOfQuadrantsTextBox.Text, out result) ? result : new int?();
            }
            set
            {
                m_numberOfQuadrantsTextBox.Clear();
                if (!value.HasValue)
                    return;
                m_numberOfQuadrantsTextBox.Text = value.ToString();
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int? SlotsPerQuadrant
        {
            get
            {
                int result;
                return int.TryParse(m_slotsPerQuadrantTextBox.Text, out result) ? result : new int?();
            }
            set
            {
                m_slotsPerQuadrantTextBox.Clear();
                if (!value.HasValue)
                    return;
                m_slotsPerQuadrantTextBox.Text = value.ToString();
            }
        }

        private void OnOK(object sender, EventArgs e)
        {
            m_errorProvider.SetError(m_startOffsetTextBox, string.Empty);
            m_errorProvider.SetError(m_numberOfQuadrantsTextBox, string.Empty);
            if (!StartOffset.HasValue)
            {
                m_errorProvider.SetError(m_startOffsetTextBox, "Start Offset is a required value.");
            }
            else if (!NumberOfQuadrants.HasValue)
            {
                m_errorProvider.SetError(m_numberOfQuadrantsTextBox, "Number of Quadrants is a required value.");
            }
            else if (!SlotsPerQuadrant.HasValue)
            {
                m_errorProvider.SetError(m_slotsPerQuadrantTextBox, "Slots per Quadrant is a required value.");
            }
            else
            {
                var startOffset = StartOffset;
                var num1 = 0M;
                if ((startOffset.GetValueOrDefault() < num1 ? startOffset.HasValue ? 1 : 0 : 0) != 0)
                {
                    m_errorProvider.SetError(m_startOffsetTextBox, "Start Offset must be a positive value.");
                }
                else
                {
                    var nullable = NumberOfQuadrants;
                    var num2 = 0;
                    if ((nullable.GetValueOrDefault() < num2 ? nullable.HasValue ? 1 : 0 : 0) != 0)
                    {
                        m_errorProvider.SetError(m_numberOfQuadrantsTextBox,
                            "Number of Quadrants must be a positive value.");
                    }
                    else
                    {
                        nullable = SlotsPerQuadrant;
                        var num3 = 0;
                        if ((nullable.GetValueOrDefault() < num3 ? nullable.HasValue ? 1 : 0 : 0) != 0)
                        {
                            m_errorProvider.SetError(m_slotsPerQuadrantTextBox,
                                "Slots per Quadrant must be a positive value.");
                        }
                        else
                        {
                            DialogResult = DialogResult.OK;
                            Close();
                        }
                    }
                }
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
            label1 = new Label();
            m_startOffsetTextBox = new TextBox();
            label2 = new Label();
            m_numberOfQuadrantsTextBox = new TextBox();
            m_errorProvider = new ErrorProvider(components);
            m_okButton = new Button();
            m_cancelButton = new Button();
            label3 = new Label();
            m_slotsPerQuadrantTextBox = new TextBox();
            ((ISupportInitialize)m_errorProvider).BeginInit();
            SuspendLayout();
            label1.AutoSize = true;
            label1.Location = new Point(12, 9);
            label1.Name = "label1";
            label1.Size = new Size(63, 13);
            label1.TabIndex = 0;
            label1.Text = "Start Offset:";
            m_startOffsetTextBox.Location = new Point(129, 9);
            m_startOffsetTextBox.Name = "m_startOffsetTextBox";
            m_startOffsetTextBox.Size = new Size(100, 20);
            m_startOffsetTextBox.TabIndex = 1;
            label2.AutoSize = true;
            label2.Location = new Point(12, 37);
            label2.Name = "label2";
            label2.Size = new Size(111, 13);
            label2.TabIndex = 2;
            label2.Text = "Number of Quadrants:";
            m_numberOfQuadrantsTextBox.Enabled = false;
            m_numberOfQuadrantsTextBox.Location = new Point(129, 34);
            m_numberOfQuadrantsTextBox.Name = "m_numberOfQuadrantsTextBox";
            m_numberOfQuadrantsTextBox.Size = new Size(100, 20);
            m_numberOfQuadrantsTextBox.TabIndex = 3;
            m_errorProvider.ContainerControl = this;
            m_okButton.Location = new Point(85, 100);
            m_okButton.Name = "m_okButton";
            m_okButton.Size = new Size(75, 23);
            m_okButton.TabIndex = 6;
            m_okButton.Text = "OK";
            m_okButton.UseVisualStyleBackColor = true;
            m_okButton.Click += OnOK;
            m_cancelButton.DialogResult = DialogResult.Cancel;
            m_cancelButton.Location = new Point(166, 100);
            m_cancelButton.Name = "m_cancelButton";
            m_cancelButton.Size = new Size(75, 23);
            m_cancelButton.TabIndex = 7;
            m_cancelButton.Text = "Cancel";
            m_cancelButton.UseVisualStyleBackColor = true;
            m_cancelButton.Click += OnCancel;
            label3.AutoSize = true;
            label3.Location = new Point(12, 63);
            label3.Name = "label3";
            label3.Size = new Size(98, 13);
            label3.TabIndex = 4;
            label3.Text = "Slots per Quadrant:";
            m_slotsPerQuadrantTextBox.Enabled = false;
            m_slotsPerQuadrantTextBox.Location = new Point(129, 60);
            m_slotsPerQuadrantTextBox.Name = "m_slotsPerQuadrantTextBox";
            m_slotsPerQuadrantTextBox.Size = new Size(100, 20);
            m_slotsPerQuadrantTextBox.TabIndex = 5;
            AcceptButton = m_okButton;
            AutoScaleDimensions = new SizeF(6f, 13f);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = m_cancelButton;
            ClientSize = new Size(253, 135);
            ControlBox = false;
            Controls.Add(m_slotsPerQuadrantTextBox);
            Controls.Add(label3);
            Controls.Add(m_cancelButton);
            Controls.Add(m_okButton);
            Controls.Add(m_numberOfQuadrantsTextBox);
            Controls.Add(label2);
            Controls.Add(m_startOffsetTextBox);
            Controls.Add(label1);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Name = nameof(ComputeQuadrantsForm);
            StartPosition = FormStartPosition.CenterParent;
            Text = "Compute Quadrants";
            ((ISupportInitialize)m_errorProvider).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }
    }
}