using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Redbox.HAL.Management.Console
{
    public class ExecuteScriptForm : Form
    {
        private IContainer components;
        private Label label1;
        private Label label2;
        private Button m_cancelButton;
        private bool m_isInitialized;
        private Button m_okButton;
        private ComboBox m_scriptDropDownList;

        public ExecuteScriptForm()
        {
            InitializeComponent();
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string SelectedScript
        {
            get => m_scriptDropDownList.SelectedIndex != -1 ? m_scriptDropDownList.SelectedItem.ToString() : null;
            set
            {
                if (value == null)
                    m_scriptDropDownList.SelectedIndex = -1;
                else
                    m_scriptDropDownList.SelectedIndex = m_scriptDropDownList.FindStringExact(value);
            }
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

        private void OnActivated(object sender, EventArgs e)
        {
            if (m_isInitialized)
                return;
            var programs = ProfileManager.Instance.Service.GetPrograms();
            if (!programs.Success)
                Close();
            m_scriptDropDownList.BeginUpdate();
            for (var index = 0; index < programs.CommandMessages.Count; ++index)
                m_scriptDropDownList.Items.Add(programs.CommandMessages[index]
                    .Split("|".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[0]);
            m_scriptDropDownList.EndUpdate();
            if (m_scriptDropDownList.Items.Count > 0)
                m_scriptDropDownList.SelectedIndex = 0;
            m_isInitialized = true;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            m_okButton = new Button();
            m_cancelButton = new Button();
            label1 = new Label();
            m_scriptDropDownList = new ComboBox();
            label2 = new Label();
            SuspendLayout();
            m_okButton.Location = new Point(279, 12);
            m_okButton.Name = "m_okButton";
            m_okButton.Size = new Size(75, 23);
            m_okButton.TabIndex = 0;
            m_okButton.Text = "OK";
            m_okButton.UseVisualStyleBackColor = true;
            m_okButton.Click += OnOK;
            m_cancelButton.DialogResult = DialogResult.Cancel;
            m_cancelButton.Location = new Point(279, 41);
            m_cancelButton.Name = "m_cancelButton";
            m_cancelButton.Size = new Size(75, 23);
            m_cancelButton.TabIndex = 1;
            m_cancelButton.Text = "Cancel";
            m_cancelButton.UseVisualStyleBackColor = true;
            m_cancelButton.Click += OnCancel;
            label1.AutoSize = true;
            label1.Location = new Point(12, 12);
            label1.Name = "label1";
            label1.Size = new Size(121, 13);
            label1.TabIndex = 2;
            label1.Text = "Select script to execute:";
            m_scriptDropDownList.Anchor =
                AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            m_scriptDropDownList.DropDownStyle = ComboBoxStyle.DropDownList;
            m_scriptDropDownList.FormattingEnabled = true;
            m_scriptDropDownList.Location = new Point(12, 28);
            m_scriptDropDownList.Name = "m_scriptDropDownList";
            m_scriptDropDownList.Size = new Size(239, 21);
            m_scriptDropDownList.TabIndex = 3;
            label2.Location = new Point(12, 52);
            label2.Name = "label2";
            label2.Size = new Size(261, 58);
            label2.TabIndex = 4;
            label2.Text =
                "NOTE:  If the script requires input parameters on the stack, select the scheduled job from the Job Control toolbar and then use the Stack View to populate the necessary values.";
            AcceptButton = m_okButton;
            AutoScaleDimensions = new SizeF(6f, 13f);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = m_cancelButton;
            ClientSize = new Size(366, 114);
            ControlBox = false;
            Controls.Add(label2);
            Controls.Add(m_scriptDropDownList);
            Controls.Add(label1);
            Controls.Add(m_cancelButton);
            Controls.Add(m_okButton);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = nameof(ExecuteScriptForm);
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Execute Script";
            Activated += OnActivated;
            ResumeLayout(false);
            PerformLayout();
        }
    }
}