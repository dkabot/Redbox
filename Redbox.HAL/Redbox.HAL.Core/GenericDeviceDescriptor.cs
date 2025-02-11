using System;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Core;

internal class GenericDeviceDescriptor : IDeviceDescriptor
{
    private static readonly char[] HwidSeparator = new char[1]
    {
        '&'
    };

    public bool ResetDriver()
    {
        throw new NotImplementedException();
    }

    public bool MatchDriver()
    {
        throw new NotImplementedException();
    }

    public bool Locate()
    {
        throw new NotImplementedException();
    }

    public DeviceStatus GetStatus()
    {
        throw new NotImplementedException();
    }

    public string Vendor { get; internal set; }

    public string Product { get; internal set; }

    public string Friendlyname => throw new NotImplementedException();

    public IDeviceSetupClass SetupClass { get; internal set; }

    public override string ToString()
    {
        return SetupClass != null
            ? string.Format("{0}\\{1}&{2}", SetupClass.Class.ToString(), Vendor, Product)
            : Vendor;
    }

    internal static GenericDeviceDescriptor Create(string hwid, DeviceClass clazz)
    {
        if (string.IsNullOrEmpty(hwid))
            return null;
        hwid = hwid.ToLower();
        var strArray1 = hwid.Split('\\');
        if (strArray1.Length != 2)
            return null;
        var str = strArray1[0];
        var p = strArray1[1];
        var startIndex = p.IndexOf("vid");
        if (-1 == startIndex)
            return Create(string.Empty, p, clazz);
        var strArray2 = p.Substring(startIndex).Split(HwidSeparator, StringSplitOptions.RemoveEmptyEntries);
        return strArray2.Length < 2
            ? null
            : Create(strArray2[0].Substring(4).ToLower(), strArray2[1].Substring(4).ToLower(), clazz);
    }

    internal static GenericDeviceDescriptor Create(string v, string p, DeviceClass clazz)
    {
        var deviceDescriptor = new GenericDeviceDescriptor
        {
            Vendor = v,
            Product = p
        };
        if (clazz != DeviceClass.None)
        {
            var service = ServiceLocator.Instance.GetService<IDeviceSetupClassFactory>();
            deviceDescriptor.SetupClass = service.Get(clazz);
        }

        return deviceDescriptor;
    }
}