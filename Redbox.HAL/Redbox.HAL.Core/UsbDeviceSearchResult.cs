using System;
using System.Collections.Generic;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Core;

internal sealed class UsbDeviceSearchResult : IUsbDeviceSearchResult, IDisposable
{
    private bool Disposed;

    internal UsbDeviceSearchResult()
    {
        Errors = new ErrorList();
        Matches = new List<IDeviceDescriptor>();
    }

    public void Dispose()
    {
        if (Disposed)
            return;
        Disposed = true;
        Errors.Clear();
        Matches.Clear();
    }

    public bool Found => Errors.Count == 0 && Matches.Count > 0;

    public ErrorList Errors { get; }

    public List<IDeviceDescriptor> Matches { get; }
}