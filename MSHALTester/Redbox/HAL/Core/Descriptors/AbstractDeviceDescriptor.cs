using System;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Core.Descriptors;

public abstract class AbstractDeviceDescriptor : IDeviceDescriptor
{
    protected readonly IRuntimeService RuntimeService;
    protected readonly IUsbDeviceService UsbService;

    protected AbstractDeviceDescriptor(IUsbDeviceService service, DeviceClass clazz)
    {
        UsbService = service;
        RuntimeService = ServiceLocator.Instance.GetService<IRuntimeService>();
        SetupClass = ServiceLocator.Instance.GetService<IDeviceSetupClassFactory>().Get(clazz);
    }

    public bool ResetDriver()
    {
        return OnResetDriver();
    }

    public bool MatchDriver()
    {
        return OnMatchDriver();
    }

    public bool Locate()
    {
        return OnLocate();
    }

    public DeviceStatus GetStatus()
    {
        return OnGetStatus();
    }

    public string Vendor { get; protected set; }

    public string Product { get; protected set; }

    public string Friendlyname { get; protected set; }

    public IDeviceSetupClass SetupClass { get; }

    public override string ToString()
    {
        return string.Format("vid_{0}&pid_{1}", Vendor, Product);
    }

    protected virtual bool OnResetDriver()
    {
        return OnResetDriver(1500);
    }

    protected virtual bool OnLocate()
    {
        return UsbService.FindDevice(this).Found;
    }

    protected virtual DeviceStatus OnGetStatus()
    {
        return UsbService.FindDeviceStatus(this);
    }

    protected virtual bool OnMatchDriver()
    {
        throw new NotImplementedException();
    }

    protected bool OnResetDriver(int pause)
    {
        if (!UsbService.ChangeByHWID(this, DeviceState.Disable))
            return false;
        RuntimeService.Wait(pause);
        var num = UsbService.ChangeByHWID(this, DeviceState.Enable) ? 1 : 0;
        RuntimeService.SpinWait(pause);
        return num != 0;
    }

    protected bool OnMatchDriver(IDeviceDescriptor desc, IDriverDescriptor driverDesc)
    {
        var flag = UsbService.MatchDriver(desc, driverDesc);
        if (!flag)
            LogHelper.Instance.Log("[{0}] Could not match driver.", GetType().Name);
        return flag;
    }
}