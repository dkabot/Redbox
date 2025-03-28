using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using System;

namespace Redbox.KioskEngine.Environment
{
  internal sealed class ConfigurationCallbackEntry : ICallbackEntry
  {
    public string Name => nameof (ConfigurationCallbackEntry);

    public void Invoke()
    {
      if (this.Function == null)
        return;
      try
      {
        this.Function(this.Store, this.Action, this.Path, this.Key, this.NewValue);
      }
      catch (Exception ex)
      {
        LogHelper.Instance.Log("An unhandled exception was raised in ConfigurationCallbackEntry handler.", ex);
      }
    }

    public string Store { get; set; }

    public string Path { get; set; }

    public string Key { get; set; }

    public string Action { get; set; }

    public string NewValue { get; set; }

    public ConfigurationCallback Function { get; set; }
  }
}
