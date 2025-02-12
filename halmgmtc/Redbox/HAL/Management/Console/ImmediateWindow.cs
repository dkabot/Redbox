using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Redbox.HAL.Common.GUI.Functions;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Management.Console
{
    public class ImmediateWindow : UserControl
    {
        private static ImmediateWindow m_instance;
        private IContainer components;
        private ShellControl m_shellControl;

        public ImmediateWindow()
        {
            InitializeComponent();
            Dock = DockStyle.Fill;
            ImmediateCommand.Service = ProfileManager.Instance.Service;
            Enabled = true;
            ProfileManager.Instance.Connected += (_param1, _param2) =>
                ImmediateCommand.Service = ProfileManager.Instance.Service;
            ProfileManager.Instance.Disconnected += (_param1, _param2) => m_shellControl.Clear();
        }

        public new bool Enabled
        {
            get => base.Enabled;
            set
            {
                base.Enabled = value;
                m_shellControl.Enabled = value;
            }
        }

        public static ImmediateWindow Instance
        {
            get
            {
                if (m_instance == null)
                    m_instance = new ImmediateWindow();
                return m_instance;
            }
        }

        private void OnClear(object sender, EventArgs e)
        {
            m_shellControl.Clear();
        }

        private void OnCommandEntered(object sender, CommandEnteredEventArgs e)
        {
            var command = ImmediateCommand.GetCommand(e.Command);
            if (command == null)
                return;
            if (ImmediateCommand.Service == null)
            {
                OutputWindow.Instance.Append("Immediate window unable to contact service.");
                LogHelper.Instance.Log("Immediate window unable to contact service.", LogEntryType.Error);
            }
            else
            {
                var immediateCommandResult = command.Execute();
                if (immediateCommandResult.CommandResult == null)
                    return;
                CommonFunctions.ProcessCommandResult(immediateCommandResult.CommandResult);
            }
        }

        private void OnSelect(object sender, EventArgs e)
        {
            m_shellControl.Select();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            m_shellControl = new ShellControl();
            SuspendLayout();
            m_shellControl.Dock = DockStyle.Fill;
            m_shellControl.Enabled = false;
            m_shellControl.Font = new Font("ProFontWindows", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 0);
            m_shellControl.Location = new Point(0, 0);
            m_shellControl.Name = "m_shellControl";
            m_shellControl.Prompt = "> ";
            m_shellControl.ShellTextBackColor = SystemColors.Window;
            m_shellControl.ShellTextFont = new Font("ProFontWindows", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 0);
            m_shellControl.ShellTextForeColor = SystemColors.WindowText;
            m_shellControl.Size = new Size(465, 289);
            m_shellControl.TabIndex = 0;
            m_shellControl.CommandEntered += OnCommandEntered;
            AutoScaleDimensions = new SizeF(6f, 13f);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(m_shellControl);
            Name = nameof(ImmediateWindow);
            Size = new Size(465, 289);
            ResumeLayout(false);
        }
    }
}