using System;
using System.Drawing;

namespace Redbox.DirectShow
{
    public class NewFrameEventArgs : EventArgs
    {
        public NewFrameEventArgs(Bitmap frame)
        {
            Frame = frame;
        }

        public Bitmap Frame { get; }
    }
}