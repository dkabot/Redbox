using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Core.Descriptors;

internal sealed class IdTechRev1 : AbstractDeviceDescriptor
{
    internal IdTechRev1(IUsbDeviceService ds, IDeviceSetupClassFactory factory)
        : base(ds, DeviceClass.HIDClass)
    {
        Vendor = "0ACD";
        Product = "0200";
        Friendlyname = "ID Tech Single";
    }
}