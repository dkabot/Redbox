using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Lua;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace Redbox.KioskEngine.IDE
{
    public class ImmediateToolWindow : ToolWindow
    {
        private IContainer components;
        private ToolStrip m_toolStrip;
        private ToolStripButton m_executeScriptToolStripButton;
        private ShellControl m_shellControl;

        public ImmediateToolWindow()
        {
            InitializeComponent();
        }

        private void OnCommandEntered(object sender, CommandEnteredEventArgs e)
        {
            try
            {
                var readOnlyCollection =
                    ((LuaDebugger)ServiceLocator.Instance.GetService<IDebugService>().DebuggerInstance).Lua.DoString(
                        e.Command);
                if (readOnlyCollection == null || readOnlyCollection.Count == 0)
                    m_shellControl.WriteText("nil");
                else
                    foreach (var obj in readOnlyCollection)
                        m_shellControl.WriteText(LuaHelper.FormatLuaValue(obj));
            }
            catch (Exception ex)
            {
                m_shellControl.WriteText(ex.Message);
            }
        }

        private void OnExecuteScript(object sender, EventArgs e)
        {
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            var componentResourceManager = new ComponentResourceManager(typeof(ImmediateToolWindow));
            m_toolStrip = new ToolStrip();
            m_executeScriptToolStripButton = new ToolStripButton();
            m_shellControl = new ShellControl();
            m_toolStrip.SuspendLayout();
            SuspendLayout();
            m_toolStrip.Items.AddRange(new ToolStripItem[1]
            {
                (ToolStripItem)m_executeScriptToolStripButton
            });
            m_toolStrip.LayoutStyle = ToolStripLayoutStyle.Flow;
            m_toolStrip.Location = new Point(0, 0);
            m_toolStrip.Name = "m_toolStrip";
            m_toolStrip.Size = new Size(567, 23);
            m_toolStrip.TabIndex = 0;
            m_executeScriptToolStripButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            m_executeScriptToolStripButton.Image = (Image)Properties.Resources.Execute;
            m_executeScriptToolStripButton.ImageTransparentColor = Color.Magenta;
            m_executeScriptToolStripButton.Name = "m_executeScriptToolStripButton";
            m_executeScriptToolStripButton.Size = new Size(23, 20);
            m_executeScriptToolStripButton.Click += new EventHandler(OnExecuteScript);
            m_shellControl.Dock = DockStyle.Fill;
            m_shellControl.Location = new Point(0, 23);
            m_shellControl.Name = "m_shellControl";
            m_shellControl.Prompt = "> ";
            m_shellControl.ShellTextBackColor = SystemColors.Window;
            m_shellControl.ShellTextFont =
                new Font("Courier New", 8.25f, FontStyle.Regular, GraphicsUnit.Point, (byte)0);
            m_shellControl.ShellTextForeColor = SystemColors.WindowText;
            m_shellControl.Size = new Size(567, 235);
            m_shellControl.TabIndex = 1;
            m_shellControl.CommandEntered += new EventCommandEntered(OnCommandEntered);
            AutoScaleDimensions = new SizeF(6f, 13f);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(567, 258);
            Controls.Add((Control)m_shellControl);
            Controls.Add((Control)m_toolStrip);
            Font = new Font("Microsoft Sans Serif", 8.25f, FontStyle.Regular, GraphicsUnit.Point, (byte)0);
            HideOnClose = true;
            Icon = (Icon)componentResourceManager.GetObject("$this.Icon");
            Name = nameof(ImmediateToolWindow);
            ShowHint = DockState.DockBottom;
            Text = "Immediate";
            m_toolStrip.ResumeLayout(false);
            m_toolStrip.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }
    }
}