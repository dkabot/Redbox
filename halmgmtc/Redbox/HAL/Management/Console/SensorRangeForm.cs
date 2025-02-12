using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Redbox.HAL.Common.GUI.Functions;

namespace Redbox.HAL.Management.Console
{
    public class SensorRangeForm : Form
    {
        private IContainer components;
        private Label label1;
        private Label label2;
        private Button m_cancelButton;
        private TextBox m_endSensorTextBox;
        private ErrorProvider m_errorProvider;
        private Button m_okButton;
        private Button m_pickEndSensorButton;
        private Button m_pickStartSensorButton;
        private TextBox m_startSensorTextBox;

        public SensorRangeForm()
        {
            InitializeComponent();
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int MinimumValue { get; set; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int MaximumValue { get; set; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int? Start
        {
            get
            {
                int result;
                return m_startSensorTextBox.Text.Length > 0 && int.TryParse(m_startSensorTextBox.Text, out result)
                    ? result
                    : new int?();
            }
            set => m_startSensorTextBox.Text = value.ToString();
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int? End
        {
            get
            {
                int result;
                return m_endSensorTextBox.Text.Length > 0 && int.TryParse(m_endSensorTextBox.Text, out result)
                    ? result
                    : new int?();
            }
            set => m_endSensorTextBox.Text = value.ToString();
        }

        public void DisableEnd()
        {
            m_endSensorTextBox.Enabled = false;
        }

        private void OnKeyPress(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar >= '0' && e.KeyChar <= '9') || e.KeyChar == '\b')
                return;
            e.Handled = true;
        }

        private void OnOK(object sender, EventArgs e)
        {
            m_errorProvider.SetError(m_startSensorTextBox, string.Empty);
            m_errorProvider.SetError(m_endSensorTextBox, string.Empty);
            var flag = false;
            int? nullable;
            if (Start.HasValue)
            {
                nullable = Start;
                var minimumValue = MinimumValue;
                if ((nullable.GetValueOrDefault() < minimumValue ? nullable.HasValue ? 1 : 0 : 0) == 0)
                {
                    nullable = Start;
                    var maximumValue = MaximumValue;
                    if ((nullable.GetValueOrDefault() > maximumValue ? nullable.HasValue ? 1 : 0 : 0) == 0)
                        goto label_4;
                }

                m_errorProvider.SetError(m_startSensorTextBox,
                    string.Format(
                        "The Start Sensor value must be equal to or greater than {0} and less than or equal to {1}.",
                        MinimumValue, MaximumValue));
                flag = true;
            }

            label_4:
            nullable = End;
            if (nullable.HasValue)
            {
                nullable = End;
                var minimumValue = MinimumValue;
                if ((nullable.GetValueOrDefault() < minimumValue ? nullable.HasValue ? 1 : 0 : 0) == 0)
                {
                    nullable = End;
                    var maximumValue = MaximumValue;
                    if ((nullable.GetValueOrDefault() > maximumValue ? nullable.HasValue ? 1 : 0 : 0) == 0)
                        goto label_8;
                }

                m_errorProvider.SetError(m_endSensorTextBox,
                    string.Format(
                        "The End Sensor value must be equal to or greater than {0} and less than or equal to {1}.",
                        MinimumValue, MaximumValue));
                flag = true;
            }

            label_8:
            nullable = Start;
            if (nullable.HasValue)
            {
                nullable = End;
                if (nullable.HasValue)
                {
                    nullable = End;
                    var start = Start;
                    if ((nullable.GetValueOrDefault() < start.GetValueOrDefault()
                            ? nullable.HasValue & start.HasValue ? 1 : 0
                            : 0) != 0)
                    {
                        m_errorProvider.SetError(m_endSensorTextBox,
                            "The End Sensor value must be equal to or greater than Start Sensor value.");
                        flag = true;
                    }
                }
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

        private void OnPickStartSensor(object sender, EventArgs e)
        {
            var numberPadForm1 = new NumberPadForm();
            numberPadForm1.Text = "Enter Start Sensor";
            var numberPadForm2 = numberPadForm1;
            if (numberPadForm2.ShowDialog() != DialogResult.OK)
                return;
            m_startSensorTextBox.Text = numberPadForm2.Number;
            m_endSensorTextBox.Focus();
        }

        private void OnPickEndSensor(object sender, EventArgs e)
        {
            var numberPadForm1 = new NumberPadForm();
            numberPadForm1.Text = "Enter End Sensor";
            var numberPadForm2 = numberPadForm1;
            if (numberPadForm2.ShowDialog() != DialogResult.OK)
                return;
            m_endSensorTextBox.Text = numberPadForm2.Number;
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
            m_startSensorTextBox = new TextBox();
            m_endSensorTextBox = new TextBox();
            m_pickStartSensorButton = new Button();
            m_pickEndSensorButton = new Button();
            m_okButton = new Button();
            m_cancelButton = new Button();
            m_errorProvider = new ErrorProvider(components);
            ((ISupportInitialize)m_errorProvider).BeginInit();
            SuspendLayout();
            label1.AutoSize = true;
            label1.Location = new Point(12, 12);
            label1.Name = "label1";
            label1.Size = new Size(68, 13);
            label1.TabIndex = 0;
            label1.Text = "Start Sensor:";
            label2.AutoSize = true;
            label2.Location = new Point(12, 46);
            label2.Name = "label2";
            label2.Size = new Size(65, 13);
            label2.TabIndex = 2;
            label2.Text = "End Sensor:";
            m_startSensorTextBox.Location = new Point(86, 9);
            m_startSensorTextBox.MaxLength = 2;
            m_startSensorTextBox.Name = "m_startSensorTextBox";
            m_startSensorTextBox.Size = new Size(76, 20);
            m_startSensorTextBox.TabIndex = 1;
            m_startSensorTextBox.TextAlign = HorizontalAlignment.Right;
            m_startSensorTextBox.KeyPress += OnKeyPress;
            m_endSensorTextBox.Location = new Point(86, 43);
            m_endSensorTextBox.MaxLength = 2;
            m_endSensorTextBox.Name = "m_endSensorTextBox";
            m_endSensorTextBox.Size = new Size(76, 20);
            m_endSensorTextBox.TabIndex = 3;
            m_endSensorTextBox.TextAlign = HorizontalAlignment.Right;
            m_endSensorTextBox.KeyPress += OnKeyPress;
            m_pickStartSensorButton.Location = new Point(200, 9);
            m_pickStartSensorButton.Name = "m_pickStartSensorButton";
            m_pickStartSensorButton.Size = new Size(55, 23);
            m_pickStartSensorButton.TabIndex = 4;
            m_pickStartSensorButton.TabStop = false;
            m_pickStartSensorButton.Text = "...";
            m_pickStartSensorButton.UseVisualStyleBackColor = true;
            m_pickStartSensorButton.Click += OnPickStartSensor;
            m_pickEndSensorButton.Location = new Point(200, 43);
            m_pickEndSensorButton.Name = "m_pickEndSensorButton";
            m_pickEndSensorButton.Size = new Size(55, 23);
            m_pickEndSensorButton.TabIndex = 5;
            m_pickEndSensorButton.TabStop = false;
            m_pickEndSensorButton.Text = "...";
            m_pickEndSensorButton.UseVisualStyleBackColor = true;
            m_pickEndSensorButton.Click += OnPickEndSensor;
            m_okButton.Location = new Point(86, 81);
            m_okButton.Name = "m_okButton";
            m_okButton.Size = new Size(75, 23);
            m_okButton.TabIndex = 4;
            m_okButton.Text = "OK";
            m_okButton.UseVisualStyleBackColor = true;
            m_okButton.Click += OnOK;
            m_cancelButton.DialogResult = DialogResult.Cancel;
            m_cancelButton.Location = new Point(180, 81);
            m_cancelButton.Name = "m_cancelButton";
            m_cancelButton.Size = new Size(75, 23);
            m_cancelButton.TabIndex = 5;
            m_cancelButton.Text = "Cancel";
            m_cancelButton.UseVisualStyleBackColor = true;
            m_cancelButton.Click += OnCancel;
            m_errorProvider.ContainerControl = this;
            AcceptButton = m_okButton;
            AutoScaleDimensions = new SizeF(6f, 13f);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = m_cancelButton;
            ClientSize = new Size(267, 116);
            ControlBox = false;
            Controls.Add(m_cancelButton);
            Controls.Add(m_okButton);
            Controls.Add(m_pickEndSensorButton);
            Controls.Add(m_pickStartSensorButton);
            Controls.Add(m_endSensorTextBox);
            Controls.Add(m_startSensorTextBox);
            Controls.Add(label2);
            Controls.Add(label1);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Name = nameof(SensorRangeForm);
            StartPosition = FormStartPosition.CenterParent;
            Text = "Enter Sensor Range";
            ((ISupportInitialize)m_errorProvider).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }
    }
}