using Microsoft.Win32;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.KioskEngine.Environment
{
  public class UserSettingsStore : IUserSettingsStore
  {
    public static UserSettingsStore Instance => Singleton<UserSettingsStore>.Instance;

    public T GetValue<T>(string path, string key) => this.GetValue<T>(path, key, default (T));

    public T GetValue<T>(string path, string key, T defaultValue)
    {
      RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(this.FormatPath(path), false);
      if (registryKey == null)
        return defaultValue;
      object obj = registryKey.GetValue(key, (object) defaultValue);
      registryKey.Close();
      return obj == null || obj.Equals((object) default (T)) ? defaultValue : (T) ConversionHelper.ChangeType(obj, typeof (T));
    }

    public void SetValue<T>(string path, string key, T value)
    {
      RegistryKey subKey = Registry.CurrentUser.CreateSubKey(this.FormatPath(path));
      if (subKey == null)
        return;
      subKey.SetValue(key, (object) value);
      EnvironmentManager.Instance.NotifyConfigurationCallbacks("user", "set", path, key, value?.ToString());
    }

    public void RemoveKey(string path)
    {
      try
      {
        Registry.CurrentUser.DeleteSubKeyTree(this.FormatPath(path));
        EnvironmentManager.Instance.NotifyConfigurationCallbacks("user", "removekey", path, (string) null, (string) null);
      }
      catch
      {
      }
    }

    public void RemoveValue(string path, string key)
    {
      RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(this.FormatPath(path), true);
      if (registryKey == null)
        return;
      registryKey.DeleteValue(key);
      registryKey.Close();
      EnvironmentManager.Instance.NotifyConfigurationCallbacks("machine", "removevalue", path, key, (string) null);
    }

    public string RootPath { get; set; }

    private UserSettingsStore()
    {
    }

    private string FormatPath(string path)
    {
      return string.Format("{0}\\{1}", (object) this.RootPath, (object) path);
    }
  }
}
