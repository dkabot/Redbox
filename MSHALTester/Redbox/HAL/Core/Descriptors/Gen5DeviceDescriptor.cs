using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Core.Descriptors;

internal sealed class Gen5DeviceDescriptor : AbstractDeviceDescriptor
{
    private readonly IDriverDescriptor DriverInfo = new DriverDescriptor("2.0.0.2", "Scanner Manufacturer");

    internal Gen5DeviceDescriptor(IUsbDeviceService s)
        : base(s, DeviceClass.USB)
    {
        Vendor = "11FA";
        Product = "0204";
        Friendlyname = "Cortex Scanner";
    }

    protected override bool OnResetDriver()
    {
        return false;
    }

    protected override bool OnLocate()
    {
        var status = GetStatus();
        return (status & DeviceStatus.Found) != DeviceStatus.None && (status & DeviceStatus.Enabled) != 0;
    }

    protected override bool OnMatchDriver()
    {
        return OnMatchDriver(this, DriverInfo);
    }
}