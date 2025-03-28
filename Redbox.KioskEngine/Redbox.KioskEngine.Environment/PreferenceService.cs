using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Redbox.KioskEngine.Environment
{
  public class PreferenceService : IPreferenceService
  {
    private IDictionary<string, IPreferencePage> m_pages;

    public static IPreferenceService Instance
    {
      get => (IPreferenceService) Singleton<PreferenceService>.Instance;
    }

    public T GetValue<T>(string pageName, string settingName)
    {
      IPreferencePage preferencePage = this.GetPreferencePage(pageName);
      if (preferencePage == null)
        return default (T);
      return preferencePage.Target != PreferencePageTarget.LocalSystem ? UserSettingsStore.Instance.GetValue<T>(preferencePage.SettingPath, settingName) : MachineSettingsStore.Instance.GetValue<T>(preferencePage.SettingPath, settingName);
    }

    public void SetValue(string pageName, string settingName, object value)
    {
      IPreferencePage preferencePage = this.GetPreferencePage(pageName);
      if (preferencePage == null)
        return;
      if (preferencePage.Target == PreferencePageTarget.LocalSystem)
        MachineSettingsStore.Instance.SetValue<object>(preferencePage.SettingPath, settingName, value);
      else
        UserSettingsStore.Instance.SetValue<object>(preferencePage.SettingPath, settingName, value);
    }

    public void RemoveAll() => this.Pages.Clear();

    public IPreferencePage AddPreferencePage(
      string pageName,
      string settingPath,
      string displayPath,
      PreferencePageTarget target,
      IPreferencePageHost host,
      Func<IPreferencePageHost> getPreferencePageHost = null)
    {
      IPreferencePage preferencePage1 = this.GetPreferencePage(pageName);
      if (preferencePage1 != null)
        return preferencePage1;
      IPreferencePage preferencePage2 = (IPreferencePage) new PreferencePage()
      {
        Host = host,
        Name = pageName,
        Target = target,
        DisplayPath = displayPath,
        SettingPath = settingPath,
        GetPreferencePageHost = getPreferencePageHost
      };
      this.Pages[pageName] = preferencePage2;
      return preferencePage2;
    }

    public bool RemovePreferencePage(string pageName) => this.Pages.Remove(pageName);

    public IPreferencePage GetPreferencePage(string pageName)
    {
      return !this.Pages.ContainsKey(pageName) ? (IPreferencePage) null : this.Pages[pageName];
    }

    public ReadOnlyCollection<IPreferencePage> PreferencePages
    {
      get
      {
        return new List<IPreferencePage>((IEnumerable<IPreferencePage>) this.Pages.Values).AsReadOnly();
      }
    }

    internal IDictionary<string, IPreferencePage> Pages
    {
      get
      {
        if (this.m_pages == null)
          this.m_pages = (IDictionary<string, IPreferencePage>) new Dictionary<string, IPreferencePage>();
        return this.m_pages;
      }
    }

    private PreferenceService()
    {
    }
  }
}
