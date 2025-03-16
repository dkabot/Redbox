using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Redbox.KioskEngine.IDE
{
    public class PictureBox : UserControl
    {
        private IContainer components;

        public PictureBox()
        {
            InitializeComponent();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            var componentResourceManager = new ComponentResourceManager(typeof(PictureBox));
            SuspendLayout();
            componentResourceManager.ApplyResources((object)this, "$this");
            AutoScaleMode = AutoScaleMode.Font;
            MaximumSize = new Size(580, 260);
            Name = nameof(PictureBox);
            ResumeLayout(false);
        }
    }
}