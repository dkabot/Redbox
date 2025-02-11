using System;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework
{
    [AttributeUsage(AttributeTargets.Class)]
    public class PowerCycleDeviceAttribute : Attribute
    {
        public PowerCycleDevices Device { get; set; }
    }
}