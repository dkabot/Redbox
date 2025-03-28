using Microsoft.Win32;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using System;

namespace Redbox.KioskEngine.Environment
{
  public class MachineSettingsStore : IMachineSettingsStore
  {
    public static MachineSettingsStore Instance => Singleton<MachineSettingsStore>.Instance;

    public void Initialize(string rootPath)
    {
      LogHelper.Instance.Log("Initialize machine settings store, root path: {0}", (object) rootPath);
      this.RootPath = rootPath;
    }

    public T GetValue<T>(string path, string key) => this.GetValue<T>(path, key, default (T));

    public T GetValue<T>(string path, string key, T defaultValue)
    {
      RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(this.FormatPath(path), false);
      if (registryKey == null)
        return defaultValue;
      object obj = registryKey.GetValue(key, (object) defaultValue);
      registryKey.Close();
      return obj == null || obj.Equals((object) default (T)) ? defaultValue : (T) ConversionHelper.ChangeType(obj, typeof (T));
    }

    public void SetValue<T>(string path, string key, T value)
    {
      RegistryKey subKey = Registry.LocalMachine.CreateSubKey(this.FormatPath(path));
      if (subKey == null)
        return;
      subKey.SetValue(key, (object) value);
      EnvironmentManager.Instance.NotifyConfigurationCallbacks("machine", "set", path, key, value?.ToString());
    }

    public void RemoveKey(string path)
    {
      Registry.LocalMachine.DeleteSubKeyTree(this.FormatPath(path));
      EnvironmentManager.Instance.NotifyConfigurationCallbacks("machine", "removekey", path, (string) null, (string) null);
    }

    public void RemoveValue(string path, string key)
    {
      RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(this.FormatPath(path), true);
      if (registryKey == null)
        return;
      try
      {
        registryKey.DeleteValue(key);
        registryKey.Close();
        EnvironmentManager.Instance.NotifyConfigurationCallbacks("machine", "removevalue", path, key, (string) null);
      }
      catch (Exception ex)
      {
        LogHelper.Instance.LogException("Failed to remove value " + key + " at path " + path + ".", ex);
      }
    }

    public string RootPath { get; set; }

    private MachineSettingsStore()
    {
    }

    private string FormatPath(string path)
    {
      return string.Format("{0}\\{1}", (object) this.RootPath, (object) path);
    }
  }
}
