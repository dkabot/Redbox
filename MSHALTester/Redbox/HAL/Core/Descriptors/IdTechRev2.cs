using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Core.Descriptors;

internal sealed class IdTechRev2 : AbstractDeviceDescriptor
{
    internal IdTechRev2(IUsbDeviceService ds, IDeviceSetupClassFactory factory)
        : base(ds, DeviceClass.HIDClass)
    {
        Vendor = "0ACD";
        Product = "0520";
        Friendlyname = "ID Tech Dual";
    }
}