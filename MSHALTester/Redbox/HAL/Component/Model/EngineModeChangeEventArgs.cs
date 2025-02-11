using System;

namespace Redbox.HAL.Component.Model;

public sealed class EngineModeChangeEventArgs : EventArgs
{
    public EngineModeChangeEventArgs(EngineModes newMode)
    {
        NewMode = newMode;
    }

    public EngineModes NewMode { get; private set; }
}