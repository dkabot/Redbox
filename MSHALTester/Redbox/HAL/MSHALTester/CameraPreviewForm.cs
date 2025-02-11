using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Redbox.DirectShow;
using Redbox.HAL.Client;
using Redbox.HAL.Common.GUI.Functions;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Threading;

namespace Redbox.HAL.MSHALTester;

public class CameraPreviewForm : Form
{
    private readonly AutoResetEvent ImageGrabbedWaiter = new(false);
    private readonly string ImagesFolder;
    private readonly Size ImageSize;
    private readonly ButtonAspectsManager Manager;
    private readonly HardwareService Service;
    private readonly AtomicFlag SnapFlag = new();
    private BackgroundWorker backgroundWorker1;
    private Button button1;
    private Button button2;
    private Button button3;
    private Button button4;
    private IContainer components;
    private string CurrentImage;
    private Label label1;
    private Label label2;
    private Label label3;
    private Label label4;
    private Label label5;
    private Label label6;
    private TextBox m_barcodeBox;
    private TextBox m_decodeTimeBox;
    private TextBox m_detectedErrorsTB;
    private TextBox m_numberOfBarcodesBox;
    private TextBox m_secureReadTB;
    private TextBox m_snapStatusBox;
    private PlayerDevice videoDevice;
    private VideoSourcePlayer videoSourcePlayer;

    public CameraPreviewForm(
        HardwareService service,
        ButtonAspectsManager manager,
        string imageFolder)
    {
        InitializeComponent();
        Service = service;
        ImagesFolder = imageFolder;
        ImageSize = new Size(640, 480);
        Manager = manager;
    }

    private void OnLoad(object sender, EventArgs e)
    {
        var filterInfoCollection = new FilterInfoCollection(FilterCategory.VideoInputDevice);
        if (filterInfoCollection == null || filterInfoCollection.Count == 0)
        {
            WriteErrorText("Found no devices.");
        }
        else
        {
            videoDevice = new PlayerDevice(filterInfoCollection[0].MonikerString, 500, false);
            if (videoDevice == null)
            {
                WriteErrorText("Found no devices.");
            }
            else
            {
                var flag = false;
                foreach (var videoCapability in videoDevice.VideoCapabilities)
                    if (videoCapability.FrameSize == ImageSize)
                    {
                        flag = true;
                        videoDevice.VideoResolution = videoCapability;
                        break;
                    }

                if (!flag)
                    LogHelper.Instance.Log("Unable to find video capability with size {0}w X {1}h", Size.Width,
                        Size.Height);
                videoDevice.PlayingFinished += videoSource_PlayingFinished;
                videoSourcePlayer.NewFrame += SnapFrame;
                videoSourcePlayer.VideoSource = videoDevice;
                videoSourcePlayer.Start();
                Thread.Sleep(5000);
                ResetTextBoxes();
            }
        }
    }

    private void videoSource_PlayingFinished(object sender, ReasonToFinishPlaying reason)
    {
        if (ReasonToFinishPlaying.StoppedByUser == reason)
            return;
        var msg = reason.ToString();
        LogHelper.Instance.Log(msg);
        WriteErrorText(msg);
    }

    private void SnapFrame(object sender, ref Bitmap image)
    {
        if (!SnapFlag.Clear())
            return;
        var now = DateTime.Now;
        CurrentImage = Path.Combine(ImagesFolder,
            string.Format("m{0}d{1}y{2}_h{3}m{4}s{5}t{6}.jpg", now.Month, now.Day, now.Year, now.Hour, now.Minute,
                now.Second, now.Millisecond));
        LogHelper.Instance.Log("Snapframe called captured image {0}", CurrentImage);
        image.Save(CurrentImage);
        ImageGrabbedWaiter.Set();
    }

    private void button1_Click(object sender, EventArgs e)
    {
        button1.BackColor = Color.Red;
        button1.Enabled = false;
        ResetTextBoxes();
        SnapFlag.Set();
        backgroundWorker1.RunWorkerAsync();
    }

    private void button2_Click(object sender, EventArgs e)
    {
        if (videoDevice == null)
            return;
        videoDevice.DisplayPropertyPage(Process.GetCurrentProcess().MainWindowHandle);
    }

    private void button3_Click(object sender, EventArgs e)
    {
        WriteErrorText(string.Empty);
    }

    private void button4_Click(object sender, EventArgs e)
    {
        ShutdownPlayer();
        DialogResult = DialogResult.OK;
        Close();
    }

    private void ResetTextBoxes()
    {
        m_barcodeBox.Text = string.Empty;
        m_decodeTimeBox.Text = string.Empty;
        m_numberOfBarcodesBox.Text = string.Empty;
        m_snapStatusBox.Text = string.Empty;
        m_secureReadTB.Text = string.Empty;
        Application.DoEvents();
    }

    private void ShutdownPlayer()
    {
        try
        {
            if (videoSourcePlayer == null)
                return;
            videoSourcePlayer.SignalToStop();
            videoSourcePlayer.WaitForStop();
            if (videoDevice != null)
            {
                videoSourcePlayer.NewFrame -= SnapFrame;
                videoDevice.PlayingFinished -= videoSource_PlayingFinished;
            }

            videoSourcePlayer.VideoSource = videoDevice = null;
        }
        catch (Exception ex)
        {
            LogHelper.Instance.Log("[CameraPreview] OnShutdown: caught an exception", ex);
        }
    }

    private void WriteErrorText(string msg)
    {
        if (m_detectedErrorsTB.InvokeRequired)
            Invoke(new SetTextCallback(WriteErrorText), msg);
        else
            m_detectedErrorsTB.Text = msg;
    }

    private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
    {
        var flag = false;
        using (new AtomicFlagHelper(SnapFlag))
        {
            flag = ImageGrabbedWaiter.WaitOne(5000);
        }

        if (!flag || !File.Exists(CurrentImage))
            e.Result = ScanResult.New();
        else
            using (var decodeExecutor = new DecodeExecutor(Service, Path.GetFileName(CurrentImage)))
            {
                decodeExecutor.Run();
                e.Result = decodeExecutor.ScanResult;
            }
    }

    private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
    {
        button1.BackColor = Color.LightGray;
        button1.Enabled = true;
        var result = e.Result as ScanResult;
        m_snapStatusBox.Text = !result.SnapOk ? "CAPTURE ERROR" : "SUCCESS";
        m_numberOfBarcodesBox.Text = result.ReadCount.ToString();
        m_decodeTimeBox.Text = result.ExecutionTime;
        m_barcodeBox.Text = result.ScannedMatrix;
        m_secureReadTB.Text = result.SecureCount.ToString();
        if (File.Exists(CurrentImage))
            try
            {
                File.Delete(CurrentImage);
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log(string.Format("Unable to delete image '{0}'", CurrentImage), ex);
            }

        CurrentImage = null;
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
        button2 = new Button();
        button3 = new Button();
        button4 = new Button();
        label1 = new Label();
        label2 = new Label();
        label3 = new Label();
        label4 = new Label();
        label5 = new Label();
        label6 = new Label();
        m_numberOfBarcodesBox = new TextBox();
        m_decodeTimeBox = new TextBox();
        m_barcodeBox = new TextBox();
        m_snapStatusBox = new TextBox();
        m_secureReadTB = new TextBox();
        m_detectedErrorsTB = new TextBox();
        videoSourcePlayer = new VideoSourcePlayer();
        backgroundWorker1 = new BackgroundWorker();
        SuspendLayout();
        button1.BackColor = Color.LightGray;
        button1.Location = new Point(24, 498);
        button1.Name = "button1";
        button1.Size = new Size(125, 60);
        button1.TabIndex = 1;
        button1.Text = "Snap and Decode";
        button1.UseVisualStyleBackColor = false;
        button1.Click += button1_Click;
        button2.BackColor = Color.LightGray;
        button2.Location = new Point(24, 574);
        button2.Name = "button2";
        button2.Size = new Size(125, 60);
        button2.TabIndex = 2;
        button2.Text = "Camera Properties";
        button2.UseVisualStyleBackColor = false;
        button2.Click += button2_Click;
        button3.BackColor = Color.LightGray;
        button3.Location = new Point(542, 553);
        button3.Name = "button3";
        button3.Size = new Size(125, 45);
        button3.TabIndex = 3;
        button3.Text = "Clear errors box";
        button3.UseVisualStyleBackColor = false;
        button3.Click += button3_Click;
        button4.BackColor = Color.GreenYellow;
        button4.Location = new Point(542, 604);
        button4.Name = "button4";
        button4.Size = new Size(125, 60);
        button4.TabIndex = 4;
        button4.Text = "Exit";
        button4.UseVisualStyleBackColor = false;
        button4.Click += button4_Click;
        label1.AutoSize = true;
        label1.Location = new Point(181, 508);
        label1.Name = "label1";
        label1.Size = new Size(89, 13);
        label1.TabIndex = 5;
        label1.Text = "Number of Codes";
        label2.AutoSize = true;
        label2.Location = new Point(181, 537);
        label2.Name = "label2";
        label2.Size = new Size(71, 13);
        label2.TabIndex = 6;
        label2.Text = "Decode Time";
        label3.AutoSize = true;
        label3.Location = new Point(181, 566);
        label3.Name = "label3";
        label3.Size = new Size(47, 13);
        label3.TabIndex = 7;
        label3.Text = "Barcode";
        label4.AutoSize = true;
        label4.Location = new Point(181, 600);
        label4.Name = "label4";
        label4.Size = new Size(62, 13);
        label4.TabIndex = 8;
        label4.Text = "SnapStatus";
        label5.AutoSize = true;
        label5.Location = new Point(181, 635);
        label5.Name = "label5";
        label5.Size = new Size(108, 13);
        label5.TabIndex = 9;
        label5.Text = "Found Secure Code?";
        label6.AutoSize = true;
        label6.Location = new Point(531, 503);
        label6.Name = "label6";
        label6.Size = new Size(81, 13);
        label6.TabIndex = 10;
        label6.Text = "Detected Errors";
        m_numberOfBarcodesBox.Location = new Point(306, 505);
        m_numberOfBarcodesBox.Name = "m_numberOfBarcodesBox";
        m_numberOfBarcodesBox.Size = new Size(100, 20);
        m_numberOfBarcodesBox.TabIndex = 11;
        m_decodeTimeBox.Location = new Point(306, 534);
        m_decodeTimeBox.Name = "m_decodeTimeBox";
        m_decodeTimeBox.Size = new Size(100, 20);
        m_decodeTimeBox.TabIndex = 12;
        m_barcodeBox.Location = new Point(306, 566);
        m_barcodeBox.Name = "m_barcodeBox";
        m_barcodeBox.Size = new Size(100, 20);
        m_barcodeBox.TabIndex = 13;
        m_snapStatusBox.Location = new Point(306, 600);
        m_snapStatusBox.Name = "m_snapStatusBox";
        m_snapStatusBox.Size = new Size(100, 20);
        m_snapStatusBox.TabIndex = 14;
        m_secureReadTB.Location = new Point(306, 628);
        m_secureReadTB.Name = "m_secureReadTB";
        m_secureReadTB.Size = new Size(100, 20);
        m_secureReadTB.TabIndex = 15;
        m_detectedErrorsTB.Location = new Point(542, 527);
        m_detectedErrorsTB.Name = "m_detectedErrorsTB";
        m_detectedErrorsTB.Size = new Size(122, 20);
        m_detectedErrorsTB.TabIndex = 16;
        videoSourcePlayer.Location = new Point(24, 12);
        videoSourcePlayer.Name = "videoSourcePlayer";
        videoSourcePlayer.Size = new Size(640, 480);
        videoSourcePlayer.TabIndex = 0;
        videoSourcePlayer.Text = "videoSourcePlayer";
        videoSourcePlayer.VideoSource = null;
        backgroundWorker1.DoWork += backgroundWorker1_DoWork;
        backgroundWorker1.RunWorkerCompleted += backgroundWorker1_RunWorkerCompleted;
        AutoScaleDimensions = new SizeF(6f, 13f);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.DimGray;
        ClientSize = new Size(724, 694);
        Controls.Add(m_detectedErrorsTB);
        Controls.Add(m_secureReadTB);
        Controls.Add(m_snapStatusBox);
        Controls.Add(m_barcodeBox);
        Controls.Add(m_decodeTimeBox);
        Controls.Add(m_numberOfBarcodesBox);
        Controls.Add(label6);
        Controls.Add(label5);
        Controls.Add(label4);
        Controls.Add(label3);
        Controls.Add(label2);
        Controls.Add(label1);
        Controls.Add(button4);
        Controls.Add(button3);
        Controls.Add(button2);
        Controls.Add(button1);
        Controls.Add(videoSourcePlayer);
        Name = nameof(CameraPreviewForm);
        Text = nameof(CameraPreviewForm);
        Load += OnLoad;
        ResumeLayout(false);
        PerformLayout();
    }

    private delegate void SetTextCallback(string msg);
}