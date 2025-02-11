using System;
using System.Collections.Generic;
using System.Reflection;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Timers;

namespace Redbox.HAL.Controller.Framework.Services
{
    internal sealed class PowerCycleDeviceService : IPowerCycleDeviceService
    {
        private readonly Dictionary<PowerCycleDevices, IPowerCycleDevice> Devices =
            new Dictionary<PowerCycleDevices, IPowerCycleDevice>();

        internal PowerCycleDeviceService(IRuntimeService rts)
        {
            using (var executionTimer = new ExecutionTimer())
            {
                var assemblyFile = string.Empty;
                try
                {
                    assemblyFile = rts.RuntimePath(Assembly.GetExecutingAssembly().GetName().Name + ".dll");
                    var type1 = typeof(IPowerCycleDevice);
                    foreach (var type2 in Assembly.LoadFrom(assemblyFile).GetTypes())
                        if (type1.IsAssignableFrom(type2) && !type2.IsInterface)
                        {
                            var customAttributes =
                                (PowerCycleDeviceAttribute[])type2.GetCustomAttributes(
                                    typeof(PowerCycleDeviceAttribute), false);
                            if (customAttributes.Length != 0)
                            {
                                var instance = (IPowerCycleDevice)Activator.CreateInstance(type2, true);
                                if (!Devices.ContainsKey(customAttributes[0].Device))
                                    Devices[customAttributes[0].Device] = instance;
                            }
                        }
                }
                catch (Exception ex)
                {
                    LogHelper.Instance.Log(
                        string.Format(
                            "[PowerCycleDeviceService]Unable to load assembly '{0}' to scan for power cycle devices.",
                            assemblyFile), ex);
                }

                executionTimer.Stop();
                LogHelper.Instance.Log("[PowerCycleDeviceService] Time to scan for {0} power cycle devices: {1}",
                    Devices.Keys.Count, executionTimer.Elapsed);
            }
        }

        public IPowerCycleDevice Get(PowerCycleDevices device)
        {
            return !Devices.ContainsKey(device) ? null : Devices[device];
        }
    }
}