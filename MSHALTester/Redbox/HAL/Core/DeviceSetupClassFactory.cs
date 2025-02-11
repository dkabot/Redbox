using System;
using System.Collections.Generic;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Core;

public sealed class DeviceSetupClassFactory : IDeviceSetupClassFactory
{
    private readonly Dictionary<DeviceClass, IDeviceSetupClass> ClassMap = new();

    public DeviceSetupClassFactory()
    {
        ClassMap[DeviceClass.Ports] = new PortsSetupClass();
        ClassMap[DeviceClass.USB] = new UsbSetupClass();
        ClassMap[DeviceClass.Image] = new ImageSetupClass();
        ClassMap[DeviceClass.Monitor] = new MonitorSetupClass();
        ClassMap[DeviceClass.HIDClass] = new HidSetupClass();
        ClassMap[DeviceClass.Mouse] = new MouseSetupClass();
        ClassMap[DeviceClass.Battery] = new BatterySetupClass();
        ClassMap[DeviceClass.None] = new NoSetupClass();
    }

    public IDeviceSetupClass Get(DeviceClass clazz)
    {
        return !ClassMap.ContainsKey(clazz) ? null : ClassMap[clazz];
    }

    private class NoSetupClass : IDeviceSetupClass
    {
        public DeviceClass Class => DeviceClass.None;

        public Guid Guid => Guid.Empty;
    }

    private class PortsSetupClass : IDeviceSetupClass
    {
        public DeviceClass Class => DeviceClass.Ports;

        public Guid Guid { get; } = new("4D36E978-E325-11CE-BFC1-08002BE10318");
    }

    private class UsbSetupClass : IDeviceSetupClass
    {
        public DeviceClass Class => DeviceClass.USB;

        public Guid Guid { get; } = new("36FC9E60-C465-11CF-8056-444553540000");
    }

    private class ImageSetupClass : IDeviceSetupClass
    {
        public DeviceClass Class => DeviceClass.Image;

        public Guid Guid { get; } = new("6BDD1FC6-810F-11D0-BEC7-08002BE2092F");
    }

    private class MonitorSetupClass : IDeviceSetupClass
    {
        public DeviceClass Class => DeviceClass.Monitor;

        public Guid Guid { get; } = new("4d36e96e-e325-11ce-bfc1-08002be10318");
    }

    private class HidSetupClass : IDeviceSetupClass
    {
        public DeviceClass Class => DeviceClass.HIDClass;

        public Guid Guid { get; } = new("745a17a0-74d3-11d0-b6fe-00a0c90f57da");
    }

    private class MouseSetupClass : IDeviceSetupClass
    {
        public DeviceClass Class => DeviceClass.Mouse;

        public Guid Guid { get; } = new("4D36E96F-E325-11CE-BFC1-08002BE10318");
    }

    private class BatterySetupClass : IDeviceSetupClass
    {
        public DeviceClass Class => DeviceClass.Battery;

        public Guid Guid { get; } = new("72631e54-78a4-11d0-bcf7-00aa00b7b32a");
    }
}