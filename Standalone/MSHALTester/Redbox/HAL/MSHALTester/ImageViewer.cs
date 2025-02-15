using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Redbox.HAL.MSHALTester;

public class ImageViewer : Form
{
    private IContainer components;
    private PictureBox m_picturebox;

    public ImageViewer()
    {
        InitializeComponent();
    }

    public void DisplayFile(string file)
    {
        if (string.IsNullOrEmpty(file) || !File.Exists(file))
            return;
        m_picturebox.Image?.Dispose();
        m_picturebox.Image = Image.FromFile(file);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null)
            components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        m_picturebox = new PictureBox();
        ((ISupportInitialize)m_picturebox).BeginInit();
        SuspendLayout();
        m_picturebox.Location = new Point(29, 22);
        m_picturebox.Name = "m_picturebox";
        m_picturebox.Size = new Size(640, 480);
        m_picturebox.SizeMode = PictureBoxSizeMode.StretchImage;
        m_picturebox.TabIndex = 0;
        m_picturebox.TabStop = false;
        AutoScaleDimensions = new SizeF(6f, 13f);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(733, 552);
        Controls.Add(m_picturebox);
        Name = nameof(ImageViewer);
        Text = nameof(ImageViewer);
        ((ISupportInitialize)m_picturebox).EndInit();
        ResumeLayout(false);
    }
}