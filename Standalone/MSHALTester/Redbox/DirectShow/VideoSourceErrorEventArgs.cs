using System;

namespace Redbox.DirectShow;

public class VideoSourceErrorEventArgs : EventArgs
{
    public VideoSourceErrorEventArgs(string description)
    {
        this.Description = description;
    }

    public string Description { get; }
}