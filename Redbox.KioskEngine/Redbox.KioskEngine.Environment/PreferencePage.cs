using Redbox.KioskEngine.ComponentModel;
using System;

namespace Redbox.KioskEngine.Environment
{
  internal class PreferencePage : IPreferencePage
  {
    private IPreferencePageHost _preferencePageHost;

    public T GetValue<T>(string settingName) => this.GetValue<T>(settingName, default (T));

    public T GetValue<T>(string settingName, T defaultValue)
    {
      return this.Target != PreferencePageTarget.LocalSystem ? UserSettingsStore.Instance.GetValue<T>(this.SettingPath, settingName, defaultValue) : MachineSettingsStore.Instance.GetValue<T>(this.SettingPath, settingName, defaultValue);
    }

    public void SetValue(string settingName, object value)
    {
      if (this.Target == PreferencePageTarget.LocalSystem)
        MachineSettingsStore.Instance.SetValue<object>(this.SettingPath, settingName, value);
      else
        UserSettingsStore.Instance.SetValue<object>(this.SettingPath, settingName, value);
    }

    public void RaiseActivate()
    {
      if (this.Activate == null)
        return;
      this.Activate((object) this, EventArgs.Empty);
    }

    public void RaiseDeactivate()
    {
      if (this.Deactivate == null)
        return;
      this.Deactivate((object) this, EventArgs.Empty);
    }

    public string Name { get; internal set; }

    public string SettingPath { get; internal set; }

    public string DisplayPath { get; internal set; }

    public IPreferencePageHost Host
    {
      get
      {
        if (this._preferencePageHost == null)
        {
          Func<IPreferencePageHost> preferencePageHost = this.GetPreferencePageHost;
          this._preferencePageHost = preferencePageHost != null ? preferencePageHost() : (IPreferencePageHost) null;
        }
        return this._preferencePageHost;
      }
      internal set => this._preferencePageHost = value;
    }

    public Func<IPreferencePageHost> GetPreferencePageHost { get; set; }

    public PreferencePageTarget Target { get; internal set; }

    public event EventHandler Activate;

    public event EventHandler Deactivate;
  }
}
