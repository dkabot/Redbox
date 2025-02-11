using System;
using System.Diagnostics;
using System.Threading;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Core.Descriptors;

namespace Redbox.HAL.Controller.Framework
{
    internal sealed class ArcusDeviceDescriptor : AbstractDeviceDescriptor
    {
        internal ArcusDeviceDescriptor(IUsbDeviceService usb)
            : base(usb, DeviceClass.None)
        {
            Vendor = "1589";
            Product = "a001";
            Friendlyname = "Proteus XES";
        }

        protected override bool OnResetDriver()
        {
            Thread.Sleep(3000);
            if (!UsbService.SetDeviceState(this, DeviceState.Disable) ||
                !UsbService.SetDeviceState(this, DeviceState.Enable))
                return false;
            if (UsbService.SetDeviceState(this, DeviceState.Enable))
                return true;
            try
            {
                var process = Process.Start("c:\\Program Files\\Redbox\\hal-tools\\halutilities.exe", "-resetProteus");
                process.WaitForExit();
                return process.ExitCode == 0;
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("Unable to reset Proteus driver.", ex);
                return false;
            }
        }

        protected override bool OnLocate()
        {
            return false;
        }

        protected override DeviceStatus OnGetStatus()
        {
            return DeviceStatus.Unknown;
        }

        protected override bool OnMatchDriver()
        {
            return false;
        }
    }
}