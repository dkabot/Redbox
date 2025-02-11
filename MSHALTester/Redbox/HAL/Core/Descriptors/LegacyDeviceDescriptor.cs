using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Core.Descriptors;

internal sealed class LegacyDeviceDescriptor : AbstractDeviceDescriptor
{
    private readonly IDriverDescriptor DriverDescriptor;

    internal LegacyDeviceDescriptor(
        string vendor,
        string product,
        string friendly,
        IDriverDescriptor driver,
        IUsbDeviceService s)
        : base(s, DeviceClass.Image)
    {
        Vendor = vendor.ToLower();
        Product = product.ToLower();
        Friendlyname = friendly;
        DriverDescriptor = driver;
    }

    protected override bool OnLocate()
    {
        return (GetStatus() & DeviceStatus.Found) != 0;
    }

    protected override bool OnMatchDriver()
    {
        return OnMatchDriver(this, DriverDescriptor);
    }
}