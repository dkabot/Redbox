using System;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Core.Descriptors;

public sealed class QuickReturnDescriptor : AbstractDeviceDescriptor
{
    private readonly IDriverDescriptor Driver = new DriverDescriptor("2.4.6.0", "FTDI");

    public QuickReturnDescriptor(IUsbDeviceService service)
        : base(service, DeviceClass.Ports)
    {
        Vendor = "0403";
        Product = "6001";
        Friendlyname = "Quick Return";
    }

    protected override bool OnResetDriver()
    {
        throw new NotImplementedException();
    }

    protected override DeviceStatus OnGetStatus()
    {
        throw new NotImplementedException();
    }

    protected override bool OnMatchDriver()
    {
        return OnMatchDriver(this, Driver);
    }
}