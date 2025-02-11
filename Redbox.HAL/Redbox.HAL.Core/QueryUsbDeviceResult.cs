using System;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Core;

internal sealed class QueryUsbDeviceResult : IQueryUsbDeviceResult
{
    internal QueryUsbDeviceResult(IDeviceDescriptor descriptor)
    {
        Status = DeviceStatus.None;
        Descriptor = descriptor;
    }

    public IDeviceDescriptor Descriptor { get; }

    public bool Found => (Status & DeviceStatus.Found) != 0;

    public DeviceStatus Status { get; internal set; }

    public bool IsDisabled
    {
        get
        {
            ThrowIfNotFound();
            return (DeviceStatus.Disabled & Status) != 0;
        }
    }

    public bool IsNotStarted
    {
        get
        {
            ThrowIfNotFound();
            return (Status & DeviceStatus.NotStarted) != 0;
        }
    }

    public bool Running => (Status & DeviceStatus.Found) != DeviceStatus.None && (Status & DeviceStatus.Enabled) != 0;

    public override string ToString()
    {
        return !Found
            ? "UNKNOWN device"
            : string.Format("{0} ( {1} ) status = {2}", Descriptor, Descriptor.Friendlyname, Status);
    }

    private void ThrowIfNotFound()
    {
        if (!Found)
            throw new InvalidOperationException("Device not found");
    }
}