using System;
using System.Drawing;

namespace Redbox.DirectShow;

public class NewFrameEventArgs : EventArgs
{
    public NewFrameEventArgs(Bitmap frame)
    {
        this.Frame = frame;
    }

    public Bitmap Frame { get; }
}