using Alsing.SourceCode;
using Alsing.Windows.Forms;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Redbox.KioskEngine.IDE
{
    public class NewKeyValueForm : Form
    {
        private IContainer components;
        private Label label1;
        private TextBox m_keyNameTextBox;
        private Panel m_panel;
        private Button m_cancelButton;
        private Button m_okButton;
        private ErrorProvider m_errorProvider;
        private Panel m_topPanel;
        private SyntaxBoxControl m_syntaxBoxControl;
        private SyntaxDocument m_syntaxDocument;

        public NewKeyValueForm()
        {
            InitializeComponent();
            m_syntaxDocument.SetSyntaxFromEmbeddedResource(typeof(QueuePreferencePage).Assembly,
                "Redbox.KioskEngine.IDE.Lua.syn");
        }

        public string KeyName
        {
            get => m_keyNameTextBox.Text;
            set => m_keyNameTextBox.Text = value;
        }

        public string Value
        {
            get => m_syntaxDocument.Text;
            set => m_syntaxDocument.Text = value;
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
            components = (IContainer)new Container();
            label1 = new Label();
            m_keyNameTextBox = new TextBox();
            m_panel = new Panel();
            m_cancelButton = new Button();
            m_okButton = new Button();
            m_errorProvider = new ErrorProvider(components);
            m_topPanel = new Panel();
            m_syntaxBoxControl = new SyntaxBoxControl();
            m_syntaxDocument = new SyntaxDocument(components);
            m_panel.SuspendLayout();
            ((ISupportInitialize)m_errorProvider).BeginInit();
            m_topPanel.SuspendLayout();
            SuspendLayout();
            label1.AutoSize = true;
            label1.Location = new Point(12, 10);
            label1.Name = "label1";
            label1.Size = new Size(28, 13);
            label1.TabIndex = 1;
            label1.Text = "Key:";
            m_keyNameTextBox.Location = new Point(46, 7);
            m_keyNameTextBox.MaxLength = 1024;
            m_keyNameTextBox.Name = "m_keyNameTextBox";
            m_keyNameTextBox.Size = new Size(231, 20);
            m_keyNameTextBox.TabIndex = 0;
            m_panel.Controls.Add((Control)m_cancelButton);
            m_panel.Controls.Add((Control)m_okButton);
            m_panel.Dock = DockStyle.Right;
            m_panel.Location = new Point(317, 0);
            m_panel.Name = "m_panel";
            m_panel.Size = new Size(103, 305);
            m_panel.TabIndex = 2;
            m_cancelButton.DialogResult = DialogResult.Cancel;
            m_cancelButton.Location = new Point(16, 41);
            m_cancelButton.Name = "m_cancelButton";
            m_cancelButton.Size = new Size(75, 23);
            m_cancelButton.TabIndex = 1;
            m_cancelButton.Text = "Cancel";
            m_cancelButton.UseVisualStyleBackColor = true;
            m_cancelButton.Click += new EventHandler(OnCancel);
            m_okButton.Location = new Point(16, 12);
            m_okButton.Name = "m_okButton";
            m_okButton.Size = new Size(75, 23);
            m_okButton.TabIndex = 0;
            m_okButton.Text = "OK";
            m_okButton.UseVisualStyleBackColor = true;
            m_okButton.Click += new EventHandler(OnOK);
            m_errorProvider.ContainerControl = (ContainerControl)this;
            m_topPanel.Controls.Add((Control)m_keyNameTextBox);
            m_topPanel.Controls.Add((Control)label1);
            m_topPanel.Dock = DockStyle.Top;
            m_topPanel.Location = new Point(0, 0);
            m_topPanel.Name = "m_topPanel";
            m_topPanel.Size = new Size(317, 35);
            m_topPanel.TabIndex = 0;
            m_syntaxBoxControl.ActiveView = ActiveView.BottomRight;
            m_syntaxBoxControl.AllowBreakPoints = false;
            m_syntaxBoxControl.AutoListPosition = (TextPoint)null;
            m_syntaxBoxControl.AutoListSelectedText = "a123";
            m_syntaxBoxControl.AutoListVisible = false;
            m_syntaxBoxControl.BackColor = Color.White;
            m_syntaxBoxControl.BorderStyle = Alsing.Windows.Forms.BorderStyle.None;
            m_syntaxBoxControl.CopyAsRTF = false;
            m_syntaxBoxControl.Dock = DockStyle.Fill;
            m_syntaxBoxControl.Document = m_syntaxDocument;
            m_syntaxBoxControl.FontName = "Courier new";
            m_syntaxBoxControl.HighLightActiveLine = true;
            m_syntaxBoxControl.ImeMode = ImeMode.NoControl;
            m_syntaxBoxControl.InfoTipCount = 1;
            m_syntaxBoxControl.InfoTipPosition = (TextPoint)null;
            m_syntaxBoxControl.InfoTipSelectedIndex = 1;
            m_syntaxBoxControl.InfoTipVisible = false;
            m_syntaxBoxControl.Location = new Point(0, 35);
            m_syntaxBoxControl.LockCursorUpdate = false;
            m_syntaxBoxControl.Name = "m_syntaxBoxControl";
            m_syntaxBoxControl.ShowEOLMarker = true;
            m_syntaxBoxControl.ShowGutterMargin = false;
            m_syntaxBoxControl.ShowScopeIndicator = false;
            m_syntaxBoxControl.ShowTabGuides = true;
            m_syntaxBoxControl.ShowWhitespace = true;
            m_syntaxBoxControl.Size = new Size(317, 270);
            m_syntaxBoxControl.SmoothScroll = false;
            m_syntaxBoxControl.SplitView = false;
            m_syntaxBoxControl.SplitviewH = -4;
            m_syntaxBoxControl.SplitviewV = -4;
            m_syntaxBoxControl.TabGuideColor = Color.FromArgb(222, 219, 214);
            m_syntaxBoxControl.TabIndex = 1;
            m_syntaxBoxControl.WhitespaceColor = SystemColors.ControlDark;
            m_syntaxDocument.Lines = new string[1] { "" };
            m_syntaxDocument.MaxUndoBufferSize = 1000;
            m_syntaxDocument.Modified = false;
            m_syntaxDocument.UndoStep = 0;
            AcceptButton = (IButtonControl)m_okButton;
            AutoScaleDimensions = new SizeF(6f, 13f);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = (IButtonControl)m_cancelButton;
            ClientSize = new Size(420, 305);
            ControlBox = false;
            Controls.Add((Control)m_syntaxBoxControl);
            Controls.Add((Control)m_topPanel);
            Controls.Add((Control)m_panel);
            Name = nameof(NewKeyValueForm);
            StartPosition = FormStartPosition.CenterParent;
            Text = "New Key/Value";
            m_panel.ResumeLayout(false);
            ((ISupportInitialize)m_errorProvider).EndInit();
            m_topPanel.ResumeLayout(false);
            m_topPanel.PerformLayout();
            ResumeLayout(false);
        }
    }
}