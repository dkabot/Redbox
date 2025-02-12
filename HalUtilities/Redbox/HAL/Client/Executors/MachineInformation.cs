using Redbox.HAL.Component.Model;
using System;
using System.Collections.Generic;

namespace Redbox.HAL.Client.Executors
{
  public sealed class MachineInformation : IDisposable, ICloneable<MachineInformation>
  {
    public void Dispose() => this.DecksConfiguration.Clear();

    public MachineInformation Clone(params object[] parms)
    {
      MachineInformation machineInformation = new MachineInformation();
      machineInformation.Configuration = this.Configuration;
      machineInformation.DecksConfiguration.AddRange((IEnumerable<IDeckConfig>) this.DecksConfiguration);
      return machineInformation;
    }

    public KioskConfiguration Configuration { get; internal set; }

    public List<IDeckConfig> DecksConfiguration { get; private set; }

    internal MachineInformation()
    {
      this.Configuration = KioskConfiguration.None;
      this.DecksConfiguration = new List<IDeckConfig>();
    }
  }
}
