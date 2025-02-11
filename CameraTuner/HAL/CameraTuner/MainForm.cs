using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Redbox.DirectShow;
using Redbox.HAL.CameraTuner.Properties;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Threading;

namespace Redbox.HAL.CameraTuner
{
    public class MainForm : Form
    {
        private readonly bool Details;
        private readonly AutoResetEvent ImageGrabbedWaiter;
        private readonly string ImagesFolder;
        private readonly Size ImageSize;
        private readonly AtomicFlag SnapFlag = new AtomicFlag();
        private readonly TunerLog TunerLogInstance;
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
        private FilterInfoCollection videoDevices;
        private VideoSourcePlayer videoSourcePlayer;

        public MainForm(TunerLog log)
        {
            InitializeComponent();
            TunerLogInstance = log;
            ImageGrabbedWaiter = new AutoResetEvent(false);
            ImageSize = new Size(640, 480);
            ImagesFolder = ServiceLocator.Instance.GetService<IRuntimeService>().InstallPath("Video");
            try
            {
                if (!Directory.Exists(ImagesFolder))
                    Directory.CreateDirectory(ImagesFolder);
            }
            catch
            {
            }

            Details = Settings.Default.DetailedLog;
        }

        private void OnLoad(object sender, EventArgs e)
        {
            videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            if (videoDevices == null || videoDevices.Count == 0)
            {
                WriteErrorText("Found no devices.");
            }
            else
            {
                videoDevice = new PlayerDevice(videoDevices[0].MonikerString, 500, false);
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
                    Thread.Sleep(Settings.Default.WakeupPause);
                    ResetView();
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
            CurrentImage = Path.Combine(ImagesFolder,
                ServiceLocator.Instance.GetService<IRuntimeService>().GenerateUniqueFile("jpg"));
            if (Details)
                LogHelper.Instance.Log("Snapframe called captured image {0}", CurrentImage);
            image.Save(CurrentImage);
            ImageGrabbedWaiter.Set();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            OnShutdown();
            Application.Exit();
        }

        private void ResetView()
        {
            m_barcodeBox.Text = "None";
            m_decodeTimeBox.Text = "0.0 s";
            m_snapStatusBox.Text = string.Empty;
            m_secureReadTB.Text = m_numberOfBarcodesBox.Text = "0";
            Application.DoEvents();
        }

        private void OnShutdown()
        {
            ShutdownPlayer();
            TunerLogInstance.Dispose();
        }

        private void ShutdownPlayer()
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

        private void button4_Click(object sender, EventArgs e)
        {
            button1.BackColor = Color.Red;
            button1.Enabled = false;
            ResetView();
            SnapFlag.Set();
            backgroundWorker1.RunWorkerAsync();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (videoDevice == null)
                return;
            videoDevice.DisplayPropertyPage(Process.GetCurrentProcess().MainWindowHandle);
        }

        private void WriteErrorText(string msg)
        {
            if (m_detectedErrorsTB.InvokeRequired)
                Invoke(new SetTextCallback(WriteErrorText), msg);
            else
                m_detectedErrorsTB.Text = msg;
        }

        private void button4_Click_1(object sender, EventArgs e)
        {
            WriteErrorText(string.Empty);
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            var flag = false;
            using (new AtomicFlagHelper(SnapFlag))
            {
                flag = ImageGrabbedWaiter.WaitOne(5000);
            }

            e.Result = flag ? ScanResult.Scan(CurrentImage) : (object)ScanResult.ErrorResult();
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            button1.BackColor = Color.LightGray;
            button1.Enabled = true;
            var result = e.Result as ScanResult;
            m_snapStatusBox.Text = !result.SnapOk ? "CAPTURE ERROR" : "SUCCESS";
            m_numberOfBarcodesBox.Text = result.ReadCount.ToString();
            m_decodeTimeBox.Text =
                string.Format("{0}.{1} s", result.ExecutionTime.Seconds, result.ExecutionTime.Milliseconds);
            m_barcodeBox.Text = result.ScannedMatrix;
            m_secureReadTB.Text = result.SecureCount.ToString();
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
            m_numberOfBarcodesBox = new TextBox();
            m_decodeTimeBox = new TextBox();
            m_barcodeBox = new TextBox();
            m_snapStatusBox = new TextBox();
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            label4 = new Label();
            label6 = new Label();
            m_detectedErrorsTB = new TextBox();
            button4 = new Button();
            videoSourcePlayer = new VideoSourcePlayer();
            backgroundWorker1 = new BackgroundWorker();
            label5 = new Label();
            m_secureReadTB = new TextBox();
            SuspendLayout();
            button1.BackColor = Color.LightGray;
            button1.Location = new Point(31, 504);
            button1.Name = "button1";
            button1.Size = new Size(128, 61);
            button1.TabIndex = 1;
            button1.Text = "Snap And Decode";
            button1.UseVisualStyleBackColor = false;
            button1.Click += button4_Click;
            button2.BackColor = Color.LightGray;
            button2.Location = new Point(31, 591);
            button2.Name = "button2";
            button2.Size = new Size(128, 63);
            button2.TabIndex = 2;
            button2.Text = "Camera Properties";
            button2.UseVisualStyleBackColor = false;
            button2.Click += button2_Click;
            button3.BackColor = Color.GreenYellow;
            button3.Location = new Point(527, 603);
            button3.Name = "button3";
            button3.Size = new Size(144, 60);
            button3.TabIndex = 3;
            button3.Text = "Exit";
            button3.UseVisualStyleBackColor = false;
            button3.Click += button3_Click;
            m_numberOfBarcodesBox.BackColor = Color.White;
            m_numberOfBarcodesBox.Location = new Point(305, 504);
            m_numberOfBarcodesBox.Name = "m_numberOfBarcodesBox";
            m_numberOfBarcodesBox.ReadOnly = true;
            m_numberOfBarcodesBox.Size = new Size(100, 20);
            m_numberOfBarcodesBox.TabIndex = 4;
            m_decodeTimeBox.BackColor = Color.White;
            m_decodeTimeBox.Location = new Point(305, 534);
            m_decodeTimeBox.Name = "m_decodeTimeBox";
            m_decodeTimeBox.ReadOnly = true;
            m_decodeTimeBox.Size = new Size(100, 20);
            m_decodeTimeBox.TabIndex = 5;
            m_barcodeBox.BackColor = Color.White;
            m_barcodeBox.Location = new Point(305, 568);
            m_barcodeBox.Name = "m_barcodeBox";
            m_barcodeBox.ReadOnly = true;
            m_barcodeBox.Size = new Size(100, 20);
            m_barcodeBox.TabIndex = 6;
            m_snapStatusBox.BackColor = Color.White;
            m_snapStatusBox.Location = new Point(305, 603);
            m_snapStatusBox.Name = "m_snapStatusBox";
            m_snapStatusBox.ReadOnly = true;
            m_snapStatusBox.Size = new Size(100, 20);
            m_snapStatusBox.TabIndex = 7;
            label1.AutoSize = true;
            label1.Location = new Point(172, 507);
            label1.Name = "label1";
            label1.Size = new Size(89, 13);
            label1.TabIndex = 9;
            label1.Text = "Number of Codes";
            label2.AutoSize = true;
            label2.Location = new Point(172, 537);
            label2.Name = "label2";
            label2.Size = new Size(30, 13);
            label2.TabIndex = 10;
            label2.Text = "Time";
            label3.AutoSize = true;
            label3.Location = new Point(172, 575);
            label3.Name = "label3";
            label3.Size = new Size(47, 13);
            label3.TabIndex = 11;
            label3.Text = "Barcode";
            label4.AutoSize = true;
            label4.Location = new Point(172, 610);
            label4.Name = "label4";
            label4.Size = new Size(65, 13);
            label4.TabIndex = 12;
            label4.Text = "Snap Status";
            label6.AutoSize = true;
            label6.Location = new Point(509, 502);
            label6.Name = "label6";
            label6.Size = new Size(83, 13);
            label6.TabIndex = 14;
            label6.Text = "Detected errors:";
            m_detectedErrorsTB.Location = new Point(512, 518);
            m_detectedErrorsTB.Name = "m_detectedErrorsTB";
            m_detectedErrorsTB.ReadOnly = true;
            m_detectedErrorsTB.Size = new Size(159, 20);
            m_detectedErrorsTB.TabIndex = 15;
            button4.BackColor = Color.LightGray;
            button4.Location = new Point(512, 544);
            button4.Name = "button4";
            button4.Size = new Size(126, 44);
            button4.TabIndex = 16;
            button4.Text = "Clear errors box";
            button4.UseVisualStyleBackColor = false;
            button4.Click += button4_Click_1;
            videoSourcePlayer.Location = new Point(31, 12);
            videoSourcePlayer.Name = "videoSourcePlayer";
            videoSourcePlayer.Size = new Size(640, 480);
            videoSourcePlayer.TabIndex = 0;
            videoSourcePlayer.Text = "videoSourcePlayer1";
            videoSourcePlayer.VideoSource = null;
            backgroundWorker1.DoWork += backgroundWorker1_DoWork;
            backgroundWorker1.RunWorkerCompleted += backgroundWorker1_RunWorkerCompleted;
            label5.AutoSize = true;
            label5.Location = new Point(172, 641);
            label5.Name = "label5";
            label5.Size = new Size(108, 13);
            label5.TabIndex = 17;
            label5.Text = "Secure Marker Count";
            m_secureReadTB.Location = new Point(305, 634);
            m_secureReadTB.Name = "m_secureReadTB";
            m_secureReadTB.Size = new Size(100, 20);
            m_secureReadTB.TabIndex = 18;
            AutoScaleDimensions = new SizeF(6f, 13f);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.DimGray;
            ClientSize = new Size(723, 670);
            Controls.Add(m_secureReadTB);
            Controls.Add(label5);
            Controls.Add(button4);
            Controls.Add(m_detectedErrorsTB);
            Controls.Add(label6);
            Controls.Add(label4);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(m_snapStatusBox);
            Controls.Add(m_barcodeBox);
            Controls.Add(m_decodeTimeBox);
            Controls.Add(m_numberOfBarcodesBox);
            Controls.Add(button3);
            Controls.Add(button2);
            Controls.Add(button1);
            Controls.Add(videoSourcePlayer);
            Name = nameof(MainForm);
            Text = "Redbox Camera Tuner";
            Load += OnLoad;
            ResumeLayout(false);
            PerformLayout();
        }

        private delegate void SetTextCallback(string msg);
    }
}