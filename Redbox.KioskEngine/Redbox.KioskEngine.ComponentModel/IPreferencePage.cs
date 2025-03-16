using System;

namespace Redbox.KioskEngine.ComponentModel
{
    public interface IPreferencePage
    {
        string Name { get; }

        string SettingPath { get; }

        string DisplayPath { get; }

        IPreferencePageHost Host { get; }

        Func<IPreferencePageHost> GetPreferencePageHost { get; }

        PreferencePageTarget Target { get; }
        T GetValue<T>(string settingName);

        T GetValue<T>(string settingName, T defaultValue);

        void SetValue(string settingName, object value);

        void RaiseActivate();

        void RaiseDeactivate();

        event EventHandler Activate;

        event EventHandler Deactivate;
    }
}