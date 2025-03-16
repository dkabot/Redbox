using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Redbox.KioskEngine.IDE
{
    public class GeneralPreferencePage : UserControl, IPreferencePageHost
    {
        private bool m_isLoaded;
        private IContainer components;
        private CheckBox m_allowDebuggingCheckBox;
        private Label label1;
        private ComboBox m_environmentComboBox;
        private Label label12;
        private ComboBox m_applicationComboBox;
        private MaskedTextBox m_rebootTimeMaskedTextBox;
        private Label label11;
        private CheckBox m_enableMouseCursorCheckBox;
        private Label label2;
        private Panel m_colorPanel;
        private Button m_editColorButton;
        private CheckBox m_sendAnalyticsOffPeakCheckbox;
        private CheckBox m_verboseLoggingCheckBox;
        private Label m_headerLabel;
        private Button button1;

        public GeneralPreferencePage()
        {
            InitializeComponent();
        }

        public bool SaveValues(IPreferencePage preferencePage)
        {
            var service1 = ServiceLocator.Instance.GetService<IDebugService>();
            if (service1 != null)
                service1.IsEnabled = m_allowDebuggingCheckBox.Checked;
            preferencePage.SetValue("Environment", (object)m_environmentComboBox.Text);
            preferencePage.SetValue("Application", (object)m_applicationComboBox.Text);
            preferencePage.SetValue("RebootTime", (object)m_rebootTimeMaskedTextBox.Text);
            var service2 = ServiceLocator.Instance.GetService<IRenderingService>();
            if (service2 != null)
                service2.BackgroundColor = m_colorPanel.BackColor;
            preferencePage.SetValue("EnableMouseCursor", (object)m_enableMouseCursorCheckBox.Checked);
            preferencePage.SetValue("SendAnalyticsOffPeak", (object)m_sendAnalyticsOffPeakCheckbox.Checked);
            preferencePage.SetValue("VerboseLogging", (object)m_verboseLoggingCheckBox.Checked);
            return true;
        }

        public void LoadValues(IPreferencePage preferencePage)
        {
            m_isLoaded = false;
            m_environmentComboBox.Text = preferencePage.GetValue<string>("Environment");
            m_applicationComboBox.Text = preferencePage.GetValue<string>("Application");
            m_rebootTimeMaskedTextBox.Text = GetRebootTime(preferencePage);
            var service1 = ServiceLocator.Instance.GetService<IDebugService>();
            if (service1 == null)
                m_allowDebuggingCheckBox.Enabled = false;
            else
                m_allowDebuggingCheckBox.Checked = service1.IsEnabled;
            m_enableMouseCursorCheckBox.Checked = preferencePage.GetValue<bool>("EnableMouseCursor", true);
            m_sendAnalyticsOffPeakCheckbox.Checked = preferencePage.GetValue<bool>("SendAnalyticsOffPeak", true);
            m_verboseLoggingCheckBox.Checked = preferencePage.GetValue<bool>("VerboseLogging", false);
            var service2 = ServiceLocator.Instance.GetService<IRenderingService>();
            if (service2 != null)
                m_colorPanel.BackColor = service2.BackgroundColor;
            m_isLoaded = true;
        }

        private static string GetRebootTime(IPreferencePage preferencePage)
        {
            var rebootTime = preferencePage.GetValue<string>("RebootTime");
            if (string.IsNullOrEmpty(rebootTime))
            {
                var pseudoUniqueId1 = IdHelper.GeneratePseudoUniqueId(1, "12345");
                var pseudoUniqueId2 = IdHelper.GeneratePseudoUniqueId(2, "012345");
                rebootTime = string.Format("{0:00}:{1:00}", (object)int.Parse(pseudoUniqueId1),
                    (object)int.Parse(pseudoUniqueId2));
                preferencePage.SetValue("RebootTime", (object)rebootTime);
            }

            return rebootTime;
        }

        private void OnEditColor(object sender, EventArgs e)
        {
            var colorDialog = new ColorDialog();
            if (colorDialog.ShowDialog() != DialogResult.OK)
                return;
            m_colorPanel.BackColor = colorDialog.Color;
            var openForm = Application.OpenForms["HostForm"];
            if (openForm == null)
                return;
            openForm.BackColor = m_colorPanel.BackColor;
        }

        private void OnEnableMouseCursorCheckChanged(object sender, EventArgs e)
        {
            if (!m_isLoaded)
                return;
            if (m_enableMouseCursorCheckBox.Checked)
                Cursor.Show();
            else
                Cursor.Hide();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            throw new Exception("This is a test");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            m_allowDebuggingCheckBox = new CheckBox();
            label1 = new Label();
            m_environmentComboBox = new ComboBox();
            label12 = new Label();
            m_applicationComboBox = new ComboBox();
            m_rebootTimeMaskedTextBox = new MaskedTextBox();
            label11 = new Label();
            m_enableMouseCursorCheckBox = new CheckBox();
            label2 = new Label();
            m_colorPanel = new Panel();
            m_editColorButton = new Button();
            m_sendAnalyticsOffPeakCheckbox = new CheckBox();
            m_verboseLoggingCheckBox = new CheckBox();
            m_headerLabel = new Label();
            button1 = new Button();
            SuspendLayout();
            m_allowDebuggingCheckBox.AutoSize = true;
            m_allowDebuggingCheckBox.Location = new Point(89, 53);
            m_allowDebuggingCheckBox.Name = "m_allowDebuggingCheckBox";
            m_allowDebuggingCheckBox.Size = new Size(106, 17);
            m_allowDebuggingCheckBox.TabIndex = 0;
            m_allowDebuggingCheckBox.Text = "Allow Debugging";
            m_allowDebuggingCheckBox.UseVisualStyleBackColor = true;
            label1.AutoSize = true;
            label1.Location = new Point(14, 101);
            label1.Name = "label1";
            label1.Size = new Size(69, 13);
            label1.TabIndex = 5;
            label1.Text = "Environment:";
            m_environmentComboBox.FormattingEnabled = true;
            m_environmentComboBox.Items.AddRange(new object[4]
            {
                (object)"Development",
                (object)"QA",
                (object)"UAT",
                (object)"Production"
            });
            m_environmentComboBox.Location = new Point(89, 98);
            m_environmentComboBox.Name = "m_environmentComboBox";
            m_environmentComboBox.Size = new Size(240, 21);
            m_environmentComboBox.TabIndex = 6;
            label12.AutoSize = true;
            label12.Location = new Point(21, 131);
            label12.Name = "label12";
            label12.Size = new Size(62, 13);
            label12.TabIndex = 7;
            label12.Text = "Application:";
            m_applicationComboBox.FormattingEnabled = true;
            m_applicationComboBox.Items.AddRange(new object[2]
            {
                (object)"DVD",
                (object)"Games"
            });
            m_applicationComboBox.Location = new Point(89, 128);
            m_applicationComboBox.Name = "m_applicationComboBox";
            m_applicationComboBox.Size = new Size(240, 21);
            m_applicationComboBox.TabIndex = 8;
            m_rebootTimeMaskedTextBox.Location = new Point(89, 158);
            m_rebootTimeMaskedTextBox.Mask = "00:00";
            m_rebootTimeMaskedTextBox.Name = "m_rebootTimeMaskedTextBox";
            m_rebootTimeMaskedTextBox.Size = new Size(44, 20);
            m_rebootTimeMaskedTextBox.TabIndex = 10;
            m_rebootTimeMaskedTextBox.ValidatingType = typeof(DateTime);
            label11.AutoSize = true;
            label11.Location = new Point(12, 161);
            label11.Name = "label11";
            label11.Size = new Size(71, 13);
            label11.TabIndex = 9;
            label11.Text = "Reboot Time:";
            m_enableMouseCursorCheckBox.AutoSize = true;
            m_enableMouseCursorCheckBox.Location = new Point(89, 76);
            m_enableMouseCursorCheckBox.Name = "m_enableMouseCursorCheckBox";
            m_enableMouseCursorCheckBox.Size = new Size((int)sbyte.MaxValue, 17);
            m_enableMouseCursorCheckBox.TabIndex = 1;
            m_enableMouseCursorCheckBox.Text = "Enable Mouse Cursor";
            m_enableMouseCursorCheckBox.UseVisualStyleBackColor = true;
            m_enableMouseCursorCheckBox.CheckedChanged += new EventHandler(OnEnableMouseCursorCheckChanged);
            label2.AutoSize = true;
            label2.Location = new Point(259, 57);
            label2.Name = "label2";
            label2.Size = new Size(95, 13);
            label2.TabIndex = 2;
            label2.Text = "Background Color:";
            m_colorPanel.BorderStyle = BorderStyle.FixedSingle;
            m_colorPanel.Location = new Point(360, 53);
            m_colorPanel.Name = "m_colorPanel";
            m_colorPanel.Size = new Size(22, 24);
            m_colorPanel.TabIndex = 3;
            m_colorPanel.DoubleClick += new EventHandler(OnEditColor);
            m_editColorButton.Location = new Point(388, 53);
            m_editColorButton.Name = "m_editColorButton";
            m_editColorButton.Size = new Size(25, 23);
            m_editColorButton.TabIndex = 4;
            m_editColorButton.Text = "...";
            m_editColorButton.UseVisualStyleBackColor = true;
            m_editColorButton.Click += new EventHandler(OnEditColor);
            m_sendAnalyticsOffPeakCheckbox.AutoSize = true;
            m_sendAnalyticsOffPeakCheckbox.Location = new Point(89, 243);
            m_sendAnalyticsOffPeakCheckbox.Name = "m_sendAnalyticsOffPeakCheckbox";
            m_sendAnalyticsOffPeakCheckbox.Size = new Size(189, 17);
            m_sendAnalyticsOffPeakCheckbox.TabIndex = 12;
            m_sendAnalyticsOffPeakCheckbox.Text = "Send Analytics Data Off-Peak only";
            m_sendAnalyticsOffPeakCheckbox.UseVisualStyleBackColor = true;
            m_verboseLoggingCheckBox.AutoSize = true;
            m_verboseLoggingCheckBox.Location = new Point(89, 220);
            m_verboseLoggingCheckBox.Name = "m_verboseLoggingCheckBox";
            m_verboseLoggingCheckBox.Size = new Size(106, 17);
            m_verboseLoggingCheckBox.TabIndex = 13;
            m_verboseLoggingCheckBox.Text = "Verbose Logging";
            m_verboseLoggingCheckBox.UseVisualStyleBackColor = true;
            m_headerLabel.Anchor = AnchorStyles.Top;
            m_headerLabel.BackColor = Color.MediumBlue;
            m_headerLabel.Font = new Font("Microsoft Sans Serif", 14.25f, FontStyle.Bold, GraphicsUnit.Point, (byte)0);
            m_headerLabel.ForeColor = Color.White;
            m_headerLabel.Location = new Point(0, 0);
            m_headerLabel.Name = "m_headerLabel";
            m_headerLabel.Size = new Size(542, 38);
            m_headerLabel.TabIndex = 14;
            m_headerLabel.Text = "  Engine Core: General";
            m_headerLabel.TextAlign = ContentAlignment.MiddleLeft;
            button1.Location = new Point(89, 311);
            button1.Name = "button1";
            button1.Size = new Size(149, 23);
            button1.TabIndex = 15;
            button1.Text = "Throw Test Exception";
            button1.UseVisualStyleBackColor = true;
            button1.Click += new EventHandler(button1_Click);
            AutoScaleDimensions = new SizeF(6f, 13f);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add((Control)button1);
            Controls.Add((Control)m_headerLabel);
            Controls.Add((Control)m_verboseLoggingCheckBox);
            Controls.Add((Control)m_sendAnalyticsOffPeakCheckbox);
            Controls.Add((Control)m_editColorButton);
            Controls.Add((Control)m_colorPanel);
            Controls.Add((Control)label2);
            Controls.Add((Control)m_enableMouseCursorCheckBox);
            Controls.Add((Control)m_rebootTimeMaskedTextBox);
            Controls.Add((Control)label11);
            Controls.Add((Control)label12);
            Controls.Add((Control)m_applicationComboBox);
            Controls.Add((Control)m_environmentComboBox);
            Controls.Add((Control)label1);
            Controls.Add((Control)m_allowDebuggingCheckBox);
            Name = nameof(GeneralPreferencePage);
            Size = new Size(542, 506);
            ResumeLayout(false);
            PerformLayout();
        }
    }
}