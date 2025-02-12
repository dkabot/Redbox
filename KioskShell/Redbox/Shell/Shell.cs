using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Outerwall.Shell;
using Outerwall.Shell.HotKeys;
using Redbox.Core;
using Redbox.Shell.HotKeys;
using Redbox.Shell.Properties;

namespace Redbox.Shell
{
    public class Shell : Form
    {
        private const int WS_EX_TRANSPARENT = 32;
        private const int WS_EX_TOOLWINDOW = 128;
        private readonly IHotKeyCommand _attentionSequenceCommand;
        private readonly HotKeyManager _hotKeyManager;
        private readonly LowLevelMouseHook _mouseHook;
        private IContainer components;

        public Shell()
        {
            InitializeComponent();
            _mouseHook = new LowLevelMouseHook();
            _hotKeyManager = new HotKeyManager();
            try
            {
                _attentionSequenceCommand = new AttentionSequenceCommand();
                _hotKeyManager.AddHotKey(new HotKey("Redbox Desktop", Settings.Default.AttentionSequene,
                    _attentionSequenceCommand.Execute));
                _hotKeyManager.Start();
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log(
                    string.Format("The redbox desktop hot key \"{0}\" could not be registered.",
                        Settings.Default.AttentionSequene), ex);
                _hotKeyManager.Dispose();
            }
        }

        protected override CreateParams CreateParams
        {
            get
            {
                var createParams = base.CreateParams;
                createParams.ExStyle |= 32;
                createParams.ExStyle |= 128;
                return createParams;
            }
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 32770)
            {
                LogHelper.Instance.Log("Installing mouse hook");
                _mouseHook.SetHook();
            }
            else if (m.Msg == 32771)
            {
                LogHelper.Instance.Log("Removing mouse hook");
                _mouseHook.UnHook();
            }
            else if (m.Msg == 32772)
            {
                var num = _mouseHook.IsHooked ? new IntPtr(1) : IntPtr.Zero;
                LogHelper.Instance.Log("Mouse hook status = {0}", num);
                m.Result = num;
            }
            else
            {
                base.WndProc(ref m);
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (!File.Exists(Settings.Default.Wallpaper))
                return;
            BackgroundImage = Image.FromFile(Settings.Default.Wallpaper);
            BackgroundImageLayout = ImageLayout.Center;
        }

        private void Shell_Load(object sender, EventArgs e)
        {
            try
            {
                SetProcessWorkingSetSize(Process.GetCurrentProcess().Handle, -1, -1);
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("Error trimming memory usauge", ex);
            }
        }

        [DllImport("kernel32.dll")]
        private static extern bool SetProcessWorkingSetSize(
            IntPtr hProcess,
            int dwMinimumWorkingSetSize,
            int dwMaximumWorkingSetSize);

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            SuspendLayout();
            AutoScaleBaseSize = new Size(5, 13);
            BackColor = Color.FromArgb(0, 0, 0);
            TransparencyKey = Color.FromArgb(0, 0, 0);
            WindowState = FormWindowState.Maximized;
            ControlBox = false;
            FormBorderStyle = FormBorderStyle.None;
            Location = new Point(0, 0);
            Name = nameof(Shell);
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.Manual;
            Load += Shell_Load;
            ResumeLayout(false);
        }
    }
}