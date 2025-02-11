using System;

namespace Redbox.DirectShow
{
    public class VideoSourceErrorEventArgs : EventArgs
    {
        public VideoSourceErrorEventArgs(string description)
        {
            Description = description;
        }

        public string Description { get; }
    }
}