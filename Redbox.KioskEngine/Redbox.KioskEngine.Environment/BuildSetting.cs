using Redbox.KioskEngine.ComponentModel;
using System;
using System.Collections.Generic;

namespace Redbox.KioskEngine.Environment
{
  public class BuildSetting : IBuildSetting
  {
    public BuildSetting()
    {
      this.Properties = (IDictionary<string, string>) new Dictionary<string, string>();
    }

    public bool IsActive { get; set; }

    public DateTime? BuiltOn { get; set; }

    public string OutputName { get; set; }

    public IDictionary<string, string> Properties { get; set; }
  }
}
