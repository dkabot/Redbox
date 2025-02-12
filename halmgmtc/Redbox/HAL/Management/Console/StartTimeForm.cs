using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Redbox.HAL.Management.Console
{
    public class StartTimeForm : Form
    {
        private IContainer components;
        private Label label1;
        private bool m_activated;
        private Button m_addHourButton;
        private Button m_addMinute;
        private Button m_cancelButton;
        private ErrorProvider m_errorProvider;
        private MaskedTextBox m_maskedTextBox;
        private Button m_okButton;
        private Button m_subtractHourButton;
        private Button m_subtractMinute;
        private Button m_todayButton;
        private Button m_tomorrowButton;

        public StartTimeForm()
        {
            InitializeComponent();
        }

        public DateTime? StartTime
        {
            get
            {
                DateTime result;
                return DateTime.TryParse(m_maskedTextBox.Text, out result) ? result : new DateTime?();
            }
            set
            {
                if (!value.HasValue)
                    return;
                m_maskedTextBox.Text = value.Value.ToString("MM/dd/yyyy HH:mm");
            }
        }

        private void OnToday(object sender, EventArgs e)
        {
            m_maskedTextBox.Text = DateTime.Now.ToString("MM/dd/yyyy HH:mm");
        }

        private void OnTomorrow(object sender, EventArgs e)
        {
            var maskedTextBox = m_maskedTextBox;
            var dateTime = DateTime.Now;
            dateTime = dateTime.AddDays(1.0);
            var str = dateTime.ToString("MM/dd/yyyy HH:mm");
            maskedTextBox.Text = str;
        }

        private void OnAddHour(object sender, EventArgs e)
        {
            if (!StartTime.HasValue)
                return;
            var maskedTextBox = m_maskedTextBox;
            var dateTime = StartTime.Value;
            dateTime = dateTime.AddHours(1.0);
            var str = dateTime.ToString("MM/dd/yyyy HH:mm");
            maskedTextBox.Text = str;
        }

        private void OnSubtractHour(object sender, EventArgs e)
        {
            if (!StartTime.HasValue)
                return;
            var maskedTextBox = m_maskedTextBox;
            var dateTime = StartTime.Value;
            dateTime = dateTime.AddHours(-1.0);
            var str = dateTime.ToString("MM/dd/yyyy HH:mm");
            maskedTextBox.Text = str;
        }

        private void OnAddMinute(object sender, EventArgs e)
        {
            if (!StartTime.HasValue)
                return;
            var maskedTextBox = m_maskedTextBox;
            var dateTime = StartTime.Value;
            dateTime = dateTime.AddMinutes(1.0);
            var str = dateTime.ToString("MM/dd/yyyy HH:mm");
            maskedTextBox.Text = str;
        }

        private void OnSubtractMinute(object sender, EventArgs e)
        {
            if (!StartTime.HasValue)
                return;
            var maskedTextBox = m_maskedTextBox;
            var dateTime = StartTime.Value;
            dateTime = dateTime.AddMinutes(-1.0);
            var str = dateTime.ToString("MM/dd/yyyy HH:mm");
            maskedTextBox.Text = str;
        }

        private void OnOK(object sender, EventArgs e)
        {
            m_errorProvider.SetError(m_maskedTextBox, string.Empty);
            if (!StartTime.HasValue)
            {
                m_errorProvider.SetError(m_maskedTextBox, "A valid date and time is required.");
            }
            else
            {
                var startTime = StartTime;
                var today = DateTime.Today;
                if ((startTime.HasValue ? startTime.GetValueOrDefault() < today ? 1 : 0 : 0) != 0)
                {
                    m_errorProvider.SetError(m_maskedTextBox, "Start time must be in the future.");
                }
                else
                {
                    DialogResult = DialogResult.OK;
                    Close();
                }
            }
        }

        private void OnCancel(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void OnActivated(object sender, EventArgs e)
        {
            if (m_activated)
                return;
            m_activated = true;
            m_maskedTextBox.Text = DateTime.Now.ToString("MM/dd/yyyy HH:mm");
            m_maskedTextBox.Focus();
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
            m_maskedTextBox = new MaskedTextBox();
            m_errorProvider = new ErrorProvider(components);
            m_todayButton = new Button();
            m_addHourButton = new Button();
            m_subtractHourButton = new Button();
            m_tomorrowButton = new Button();
            m_okButton = new Button();
            m_cancelButton = new Button();
            m_addMinute = new Button();
            m_subtractMinute = new Button();
            ((ISupportInitialize)m_errorProvider).BeginInit();
            SuspendLayout();
            label1.AutoSize = true;
            label1.Location = new Point(12, 9);
            label1.Name = "label1";
            label1.Size = new Size(58, 13);
            label1.TabIndex = 0;
            label1.Text = "Start Time:";
            m_maskedTextBox.Location = new Point(76, 6);
            m_maskedTextBox.Mask = "00/00/0000 90:00";
            m_maskedTextBox.Name = "m_maskedTextBox";
            m_maskedTextBox.Size = new Size(135, 20);
            m_maskedTextBox.TabIndex = 1;
            m_errorProvider.ContainerControl = this;
            m_todayButton.Location = new Point(15, 32);
            m_todayButton.Name = "m_todayButton";
            m_todayButton.Size = new Size(95, 23);
            m_todayButton.TabIndex = 2;
            m_todayButton.Text = "Today";
            m_todayButton.UseVisualStyleBackColor = true;
            m_todayButton.Click += OnToday;
            m_addHourButton.Location = new Point(15, 61);
            m_addHourButton.Name = "m_addHourButton";
            m_addHourButton.Size = new Size(95, 23);
            m_addHourButton.TabIndex = 4;
            m_addHourButton.Text = "Add Hour";
            m_addHourButton.UseVisualStyleBackColor = true;
            m_addHourButton.Click += OnAddHour;
            m_subtractHourButton.Location = new Point(116, 61);
            m_subtractHourButton.Name = "m_subtractHourButton";
            m_subtractHourButton.Size = new Size(95, 23);
            m_subtractHourButton.TabIndex = 5;
            m_subtractHourButton.Text = "Subtract Hour";
            m_subtractHourButton.UseVisualStyleBackColor = true;
            m_subtractHourButton.Click += OnSubtractHour;
            m_tomorrowButton.Location = new Point(116, 32);
            m_tomorrowButton.Name = "m_tomorrowButton";
            m_tomorrowButton.Size = new Size(95, 23);
            m_tomorrowButton.TabIndex = 3;
            m_tomorrowButton.Text = "Tomorrow";
            m_tomorrowButton.UseVisualStyleBackColor = true;
            m_tomorrowButton.Click += OnTomorrow;
            m_okButton.Location = new Point(242, 6);
            m_okButton.Name = "m_okButton";
            m_okButton.Size = new Size(75, 23);
            m_okButton.TabIndex = 8;
            m_okButton.Text = "OK";
            m_okButton.UseVisualStyleBackColor = true;
            m_okButton.Click += OnOK;
            m_cancelButton.DialogResult = DialogResult.Cancel;
            m_cancelButton.Location = new Point(242, 35);
            m_cancelButton.Name = "m_cancelButton";
            m_cancelButton.Size = new Size(75, 23);
            m_cancelButton.TabIndex = 9;
            m_cancelButton.Text = "Cancel";
            m_cancelButton.UseVisualStyleBackColor = true;
            m_cancelButton.Click += OnCancel;
            m_addMinute.Location = new Point(15, 90);
            m_addMinute.Name = "m_addMinute";
            m_addMinute.Size = new Size(95, 23);
            m_addMinute.TabIndex = 6;
            m_addMinute.Text = "Add Minute";
            m_addMinute.UseVisualStyleBackColor = true;
            m_addMinute.Click += OnAddMinute;
            m_subtractMinute.Location = new Point(116, 90);
            m_subtractMinute.Name = "m_subtractMinute";
            m_subtractMinute.Size = new Size(95, 23);
            m_subtractMinute.TabIndex = 7;
            m_subtractMinute.Text = "Subtract Minute";
            m_subtractMinute.UseVisualStyleBackColor = true;
            m_subtractMinute.Click += OnSubtractMinute;
            AcceptButton = m_okButton;
            AutoScaleDimensions = new SizeF(6f, 13f);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = m_cancelButton;
            ClientSize = new Size(329, 124);
            ControlBox = false;
            Controls.Add(m_subtractMinute);
            Controls.Add(m_addMinute);
            Controls.Add(m_cancelButton);
            Controls.Add(m_okButton);
            Controls.Add(m_tomorrowButton);
            Controls.Add(m_subtractHourButton);
            Controls.Add(m_addHourButton);
            Controls.Add(m_todayButton);
            Controls.Add(m_maskedTextBox);
            Controls.Add(label1);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Name = nameof(StartTimeForm);
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Set Start Time";
            Activated += OnActivated;
            ((ISupportInitialize)m_errorProvider).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }
    }
}