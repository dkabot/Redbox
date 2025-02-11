using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Redbox.UpdateManager.StoreInstallerFrontEnd
{
    public sealed class CustomMessageBox : Form
    {
        private const int Timeout = 30;
        private int _count = 30;
        private readonly Timer _timer = new Timer();
        private readonly Timer _timer2 = new Timer();
        private IContainer components;
        private Label MessageLabel;
        private Button OKButton;

        public static DialogResult Show(string title, string message, Font font)
        {
            using (CustomMessageBox customMessageBox = new CustomMessageBox(title, message, font))
                return customMessageBox.ShowDialog();
        }

        private CustomMessageBox(string title, string message, Font font)
        {
            this.Font = font;
            this.ForeColor = SystemColors.WindowText;
            this.InitializeComponent();
            this.Text = title;
            this.MessageLabel.Text = message;
            this.OKButton.Font = new Font(FontFamily.GenericSansSerif, 8.25f, FontStyle.Regular);
            this.OKButton.Text = string.Format("OK ({0})", (object)30);
        }

        private void TimedClose(object sender, EventArgs e)
        {
            this._timer.Interval = 30000;
            this._timer2.Interval = 1000;
            this._timer.Tick += new EventHandler(this.OkButtonClick);
            this._timer2.Tick += new EventHandler(this.UpdateOkButton);
            this._timer.Start();
            this._timer2.Start();
        }

        private void UpdateOkButton(object sender, EventArgs e)
        {
            this.OKButton.Text = string.Format("OK ({0})", (object)--this._count);
        }

        private void OkButtonClick(object sender, EventArgs e)
        {
            this._timer.Stop();
            this._timer2.Stop();
            this.Close();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && this.components != null)
                this.components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof(CustomMessageBox));
            this.MessageLabel = new Label();
            this.OKButton = new Button();
            this.SuspendLayout();
            this.MessageLabel.AutoSize = true;
            this.MessageLabel.Location = new Point(12, 9);
            this.MessageLabel.Name = "MessageLabel";
            this.MessageLabel.Size = new Size(35, 13);
            this.MessageLabel.TabIndex = 0;
            this.MessageLabel.Text = "label1";
            this.OKButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            this.OKButton.Location = new Point(29, 61);
            this.OKButton.Name = "OKButton";
            this.OKButton.Size = new Size(75, 23);
            this.OKButton.TabIndex = 1;
            this.OKButton.Text = "OK";
            this.OKButton.UseVisualStyleBackColor = true;
            this.OKButton.Click += new EventHandler(this.OkButtonClick);
            this.AutoScaleDimensions = new SizeF(6f, 13f);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.ClientSize = new Size(116, 96);
            this.ControlBox = false;
            this.Controls.Add((Control)this.OKButton);
            this.Controls.Add((Control)this.MessageLabel);
            this.Icon = (Icon)componentResourceManager.GetObject("$this.Icon");
            this.Name = nameof(CustomMessageBox);
            this.ShowInTaskbar = false;
            this.SizeGripStyle = SizeGripStyle.Hide;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = nameof(CustomMessageBox);
            this.TopMost = true;
            this.Activated += new EventHandler(this.TimedClose);
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
