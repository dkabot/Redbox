using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Redbox.HAL.Common.GUI.Functions.Properties;

namespace Redbox.HAL.Common.GUI.Functions
{
    public class PictureDialog : Form
    {
        private readonly Bitmap DisplayImage;
        private Button button1;
        private IContainer components;
        private PictureBox m_pictureBox;

        public PictureDialog(bool isFraud)
        {
            InitializeComponent();
            DisplayImage = (Bitmap)Resources.ResourceManager.GetObject(isFraud ? "Pirate" : "george_costanza");
            m_pictureBox.Image = DisplayImage;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            DisplayImage.Dispose();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            button1 = new Button();
            m_pictureBox = new PictureBox();
            ((ISupportInitialize)m_pictureBox).BeginInit();
            SuspendLayout();
            button1.Location = new Point(311, 499);
            button1.Name = "button1";
            button1.Size = new Size(135, 86);
            button1.TabIndex = 0;
            button1.Text = "OK";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            m_pictureBox.Location = new Point(12, 12);
            m_pictureBox.Name = "m_pictureBox";
            m_pictureBox.Size = new Size(705, 466);
            m_pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
            m_pictureBox.TabIndex = 1;
            m_pictureBox.TabStop = false;
            AutoScaleDimensions = new SizeF(6f, 13f);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(770, 597);
            Controls.Add(m_pictureBox);
            Controls.Add(button1);
            Name = nameof(PictureDialog);
            Text = nameof(PictureDialog);
            ((ISupportInitialize)m_pictureBox).EndInit();
            ResumeLayout(false);
        }
    }
}