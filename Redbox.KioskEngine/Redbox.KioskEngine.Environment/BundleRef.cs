using Redbox.KioskEngine.ComponentModel;
using System.Collections.Generic;

namespace Redbox.KioskEngine.Environment
{
  public class BundleRef : IBundleRef
  {
    public BundleRef()
    {
      this.BuildSettings = (IDictionary<string, IBuildSetting>) new Dictionary<string, IBuildSetting>();
    }

    public string Name { get; set; }

    public string Path { get; set; }

    public IDictionary<string, IBuildSetting> BuildSettings { get; set; }
  }
}
