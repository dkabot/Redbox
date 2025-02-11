using System;
using System.Drawing;
using System.Drawing.Imaging;
using Redbox.DirectShow.Interop;
using Redbox.HAL.Component.Model;

namespace Redbox.DirectShow;

internal class SampleGrabber : ISampleGrabberCB
{
    private readonly ProcessImageCallback ProcessImage;

    internal SampleGrabber(ProcessImageCallback callback)
    {
        ProcessImage = callback != null ? callback : throw new ArgumentException("Callback cannot be null.");
    }

    internal bool LogSnapReceived { get; set; }

    internal Size Size { get; set; }

    internal bool Grab { get; set; }

    public int SampleCB(double sampleTime, IntPtr sample)
    {
        return 0;
    }

    public unsafe int BufferCB(double sampleTime, IntPtr buffer, int bufferLen)
    {
        if (!Grab)
            return 0;
        if (LogSnapReceived)
            LogHelper.Instance.Log("Snap requested.");
        var b = new Bitmap(Size.Width, Size.Height, PixelFormat.Format24bppRgb);
        var bitmapdata = b.LockBits(new Rectangle(0, 0, Size.Width, Size.Height), ImageLockMode.ReadWrite,
            PixelFormat.Format24bppRgb);
        var stride1 = bitmapdata.Stride;
        var stride2 = bitmapdata.Stride;
        var dst = (byte*)bitmapdata.Scan0.ToPointer() + stride2 * (Size.Height - 1);
        var pointer = (byte*)buffer.ToPointer();
        for (var index = 0; index < Size.Height; ++index)
        {
            Win32.memcpy(dst, pointer, stride1);
            dst -= stride2;
            pointer += stride1;
        }

        b.UnlockBits(bitmapdata);
        ProcessImage(b);
        b.Dispose();
        return 0;
    }
}