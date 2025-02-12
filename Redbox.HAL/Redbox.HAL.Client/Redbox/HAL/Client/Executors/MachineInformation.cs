using System;
using System.Collections.Generic;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Client.Executors
{
    public sealed class MachineInformation : IDisposable, ICloneable<MachineInformation>
    {
        internal MachineInformation()
        {
            Configuration = KioskConfiguration.None;
            DecksConfiguration = new List<IDeckConfig>();
        }

        public KioskConfiguration Configuration { get; internal set; }

        public List<IDeckConfig> DecksConfiguration { get; }

        public MachineInformation Clone(params object[] parms)
        {
            var machineInformation = new MachineInformation();
            machineInformation.Configuration = Configuration;
            machineInformation.DecksConfiguration.AddRange(DecksConfiguration);
            return machineInformation;
        }

        public void Dispose()
        {
            DecksConfiguration.Clear();
        }
    }
}