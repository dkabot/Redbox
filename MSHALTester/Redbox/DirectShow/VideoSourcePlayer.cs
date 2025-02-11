using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace Redbox.DirectShow;

public class VideoSourcePlayer : Control
{
    public delegate void NewFrameHandler(object sender, ref Bitmap image);

    private readonly object sync = new();
    private bool autosize;
    private Color borderColor = Color.Black;
    private IContainer components;
    private Bitmap convertedFrame;
    private Bitmap currentFrame;
    private bool firstFrameNotProcessed = true;
    private Size frameSize = new(320, 240);
    private bool keepRatio;
    private string lastMessage;
    private bool needSizeUpdate;
    private Control parent;
    private volatile bool requestedToStop;
    private IVideoSource videoSource;

    public VideoSourcePlayer()
    {
        InitializeComponent();
        SetStyle(
            ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.AllPaintingInWmPaint |
            ControlStyles.DoubleBuffer, true);
    }

    [DefaultValue(false)]
    public bool AutoSizeControl
    {
        get => autosize;
        set
        {
            autosize = value;
            UpdatePosition();
        }
    }

    [DefaultValue(false)]
    public bool KeepAspectRatio
    {
        get => keepRatio;
        set
        {
            keepRatio = value;
            Invalidate();
        }
    }

    [DefaultValue(typeof(Color), "Black")]
    public Color BorderColor
    {
        get => borderColor;
        set
        {
            borderColor = value;
            Invalidate();
        }
    }

    [Browsable(false)]
    public IVideoSource VideoSource
    {
        get => videoSource;
        set
        {
            CheckForCrossThreadAccess();
            if (videoSource != null)
            {
                videoSource.NewFrame -= videoSource_NewFrame;
                videoSource.VideoSourceError -= videoSource_VideoSourceError;
                videoSource.PlayingFinished -= videoSource_PlayingFinished;
            }

            lock (sync)
            {
                if (currentFrame != null)
                {
                    currentFrame.Dispose();
                    currentFrame = null;
                }
            }

            videoSource = value;
            if (videoSource != null)
            {
                videoSource.NewFrame += videoSource_NewFrame;
                videoSource.VideoSourceError += videoSource_VideoSourceError;
                videoSource.PlayingFinished += videoSource_PlayingFinished;
            }
            else
            {
                frameSize = new Size(320, 240);
            }

            lastMessage = null;
            needSizeUpdate = true;
            firstFrameNotProcessed = true;
            Invalidate();
        }
    }

    [Browsable(false)]
    public bool IsRunning
    {
        get
        {
            CheckForCrossThreadAccess();
            return videoSource != null && videoSource.IsRunning;
        }
    }

    public event NewFrameHandler NewFrame;

    public event PlayingFinishedEventHandler PlayingFinished;

    private void CheckForCrossThreadAccess()
    {
        if (!IsHandleCreated)
        {
            CreateControl();
            if (!IsHandleCreated)
                CreateHandle();
        }

        if (InvokeRequired)
            throw new InvalidOperationException("Cross thread access to the control is not allowed.");
    }

    public void Start()
    {
        CheckForCrossThreadAccess();
        requestedToStop = false;
        if (videoSource == null)
            return;
        firstFrameNotProcessed = true;
        videoSource.Start();
        Invalidate();
    }

    public void Stop()
    {
        CheckForCrossThreadAccess();
        requestedToStop = true;
        if (videoSource == null)
            return;
        videoSource.Stop();
        if (currentFrame != null)
        {
            currentFrame.Dispose();
            currentFrame = null;
        }

        Invalidate();
    }

    public void SignalToStop()
    {
        CheckForCrossThreadAccess();
        requestedToStop = true;
        if (videoSource == null)
            return;
        videoSource.SignalToStop();
    }

    public void WaitForStop()
    {
        CheckForCrossThreadAccess();
        if (!requestedToStop)
            SignalToStop();
        if (videoSource == null)
            return;
        videoSource.WaitForStop();
        if (currentFrame != null)
        {
            currentFrame.Dispose();
            currentFrame = null;
        }

        Invalidate();
    }

    public Bitmap GetCurrentVideoFrame()
    {
        lock (sync)
        {
            return currentFrame == null ? null : Image.Clone(currentFrame);
        }
    }

    private void VideoSourcePlayer_Paint(object sender, PaintEventArgs e)
    {
        if (!Visible)
            return;
        if (needSizeUpdate || firstFrameNotProcessed)
        {
            UpdatePosition();
            needSizeUpdate = false;
        }

        lock (sync)
        {
            var graphics = e.Graphics;
            var clientRectangle = ClientRectangle;
            var pen = new Pen(borderColor, 1f);
            graphics.DrawRectangle(pen, clientRectangle.X, clientRectangle.Y, clientRectangle.Width - 1,
                clientRectangle.Height - 1);
            if (videoSource != null)
            {
                if (currentFrame != null && lastMessage == null)
                {
                    var bitmap = convertedFrame != null ? convertedFrame : currentFrame;
                    if (keepRatio)
                    {
                        var num = bitmap.Width / (double)bitmap.Height;
                        var rectangle = clientRectangle;
                        if (clientRectangle.Width < clientRectangle.Height * num)
                            rectangle.Height = (int)(clientRectangle.Width / num);
                        else
                            rectangle.Width = (int)(clientRectangle.Height * num);
                        rectangle.X = (clientRectangle.Width - rectangle.Width) / 2;
                        rectangle.Y = (clientRectangle.Height - rectangle.Height) / 2;
                        graphics.DrawImage(bitmap, rectangle.X + 1, rectangle.Y + 1, rectangle.Width - 2,
                            rectangle.Height - 2);
                    }
                    else
                    {
                        graphics.DrawImage(bitmap, clientRectangle.X + 1, clientRectangle.Y + 1,
                            clientRectangle.Width - 2, clientRectangle.Height - 2);
                    }

                    firstFrameNotProcessed = false;
                }
                else
                {
                    var solidBrush = new SolidBrush(ForeColor);
                    graphics.DrawString(lastMessage == null ? "Connecting ..." : lastMessage, Font, solidBrush,
                        new PointF(5f, 5f));
                    solidBrush.Dispose();
                }
            }

            pen.Dispose();
        }
    }

    private void UpdatePosition()
    {
        if (!autosize || Dock == DockStyle.Fill || Parent == null)
            return;
        var clientRectangle = Parent.ClientRectangle;
        var width = frameSize.Width;
        var height = frameSize.Height;
        SuspendLayout();
        Location = new Point((clientRectangle.Width - width - 2) / 2, (clientRectangle.Height - height - 2) / 2);
        Size = new Size(width + 2, height + 2);
        ResumeLayout();
    }

    private void videoSource_NewFrame(object sender, NewFrameEventArgs eventArgs)
    {
        if (requestedToStop)
            return;
        var image = (Bitmap)eventArgs.Frame.Clone();
        if (NewFrame != null)
            NewFrame(this, ref image);
        lock (sync)
        {
            if (currentFrame != null)
            {
                if (currentFrame.Size != eventArgs.Frame.Size)
                    needSizeUpdate = true;
                currentFrame.Dispose();
                currentFrame = null;
            }

            if (convertedFrame != null)
            {
                convertedFrame.Dispose();
                convertedFrame = null;
            }

            currentFrame = image;
            frameSize = currentFrame.Size;
            lastMessage = null;
            if (currentFrame.PixelFormat != PixelFormat.Format16bppGrayScale &&
                currentFrame.PixelFormat != PixelFormat.Format48bppRgb)
                if (currentFrame.PixelFormat != PixelFormat.Format64bppArgb)
                    goto label_15;
            convertedFrame = Image.Convert16bppTo8bpp(currentFrame);
        }

        label_15:
        Invalidate();
    }

    private void videoSource_VideoSourceError(object sender, VideoSourceErrorEventArgs eventArgs)
    {
        lastMessage = eventArgs.Description;
        Invalidate();
    }

    private void videoSource_PlayingFinished(object sender, ReasonToFinishPlaying reason)
    {
        switch (reason)
        {
            case ReasonToFinishPlaying.EndOfStreamReached:
                lastMessage = "Video has finished";
                break;
            case ReasonToFinishPlaying.StoppedByUser:
                lastMessage = "Video was stopped";
                break;
            case ReasonToFinishPlaying.DeviceLost:
                lastMessage = "Video device was unplugged";
                break;
            case ReasonToFinishPlaying.VideoSourceError:
                lastMessage = "Video has finished because of error in video source";
                break;
            default:
                lastMessage = "Video has finished for unknown reason";
                break;
        }

        Invalidate();
        if (PlayingFinished == null)
            return;
        PlayingFinished(this, reason);
    }

    private void VideoSourcePlayer_ParentChanged(object sender, EventArgs e)
    {
        if (parent != null)
            parent.SizeChanged -= parent_SizeChanged;
        parent = Parent;
        if (parent == null)
            return;
        parent.SizeChanged += parent_SizeChanged;
    }

    private void parent_SizeChanged(object sender, EventArgs e)
    {
        UpdatePosition();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null)
            components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        SuspendLayout();
        Paint += VideoSourcePlayer_Paint;
        ParentChanged += VideoSourcePlayer_ParentChanged;
        ResumeLayout(false);
    }
}