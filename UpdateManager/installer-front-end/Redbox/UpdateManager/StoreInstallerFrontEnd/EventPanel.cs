using Redbox.Core;
using Redbox.UpdateManager.ComponentModel;
using Redbox.UpdateManager.StoreInstallerFrontEnd.Properties;
using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Redbox.UpdateManager.StoreInstallerFrontEnd
{
    public class EventPanel : UserControl
    {
        private readonly ErrorList m_errors = new ErrorList();
        private IContainer components;
        private Label m_name;
        private TextBox m_description;
        private PictureBox m_status;

        public EventPanel() => this.InitializeComponent();

        public EventPanel(string name, string descripion)
        {
            this.InitializeComponent();
            this.m_name.Text = name;
            this.m_description.Text = descripion;
        }

        public void Start() => this.m_status.Image = (Image)Resources.Spinner32x32;

        public void Error(Redbox.UpdateManager.ComponentModel.Error error)
        {
            this.m_errors.Add(error);
            this.m_status.Image = (Image)Resources.error2;
        }

        public void Complete() => this.m_status.Image = (Image)Resources.sucess;

        public void Pending() => this.m_status.Image = (Image)Resources.pending;

        private void m_status_Click(object sender, EventArgs e)
        {
            if (!this.m_errors.ContainsError())
                return;
            int num = (int)new ErrorForm()
            {
                Errors = ((IEnumerable)this.m_errors)
            }.ShowDialog();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && this.components != null)
                this.components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof(EventPanel));
            this.m_name = new Label();
            this.m_description = new TextBox();
            this.m_status = new PictureBox();
            ((ISupportInitialize)this.m_status).BeginInit();
            this.SuspendLayout();
            this.m_name.AutoSize = true;
            this.m_name.Font = new Font("Consolas", 9.75f, FontStyle.Bold, GraphicsUnit.Point, (byte)0);
            this.m_name.Location = new Point(4, 4);
            this.m_name.Name = "m_name";
            this.m_name.Size = new Size(77, 15);
            this.m_name.TabIndex = 0;
            this.m_name.Text = "Event Name";
            this.m_description.BackColor = SystemColors.Control;
            this.m_description.BorderStyle = BorderStyle.None;
            this.m_description.CausesValidation = false;
            this.m_description.Font = new Font("Consolas", 9f, FontStyle.Regular, GraphicsUnit.Point, (byte)0);
            this.m_description.Location = new Point(7, 22);
            this.m_description.Multiline = true;
            this.m_description.Name = "m_description";
            this.m_description.ReadOnly = true;
            this.m_description.Size = new Size(439, 67);
            this.m_description.TabIndex = 1;
            this.m_description.TabStop = false;
            this.m_description.Text = componentResourceManager.GetString("m_description.Text");
            this.m_status.BackgroundImageLayout = ImageLayout.Stretch;
            this.m_status.Image = (Image)Resources.pending;
            this.m_status.Location = new Point(464, 33);
            this.m_status.Name = "m_status";
            this.m_status.Size = new Size(32, 32);
            this.m_status.TabIndex = 2;
            this.m_status.TabStop = false;
            this.m_status.Click += new EventHandler(this.m_status_Click);
            this.AutoScaleMode = AutoScaleMode.None;
            this.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.BorderStyle = BorderStyle.FixedSingle;
            this.Controls.Add((Control)this.m_status);
            this.Controls.Add((Control)this.m_description);
            this.Controls.Add((Control)this.m_name);
            this.Font = new Font("Consolas", 8.25f, FontStyle.Regular, GraphicsUnit.Point, (byte)0);
            this.Name = nameof(EventPanel);
            this.Size = new Size(515, 92);
            ((ISupportInitialize)this.m_status).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
