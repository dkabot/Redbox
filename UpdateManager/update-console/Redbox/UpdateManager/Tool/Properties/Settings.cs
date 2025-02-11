using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Redbox.UpdateManager.Tool.Properties
{
    [CompilerGenerated]
    [GeneratedCode("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "14.0.0.0")]
    internal sealed class Settings : ApplicationSettingsBase
    {
        private static Settings defaultInstance = (Settings)SettingsBase.Synchronized((SettingsBase)new Settings());

        public static Settings Default => Settings.defaultInstance;

        [ApplicationScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("..\\.store\\.data")]
        public string DataStoreRoot => (string)this[nameof(DataStoreRoot)];

        [ApplicationScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("${RunningPath}\\..\\Kiosk Engine\\bin\\kioskengine.exe")]
        public string KioskEnginePath => (string)this[nameof(KioskEnginePath)];

        [ApplicationScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("..\\.store\\.repository")]
        public string ManifestRoot => (string)this[nameof(ManifestRoot)];

        [ApplicationScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("rcp://localhost:7002")]
        public string UpdateServiceUrl => (string)this[nameof(UpdateServiceUrl)];

        [ApplicationScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("rcp://localhost:7004")]
        public string UpdateManagerUrl => (string)this[nameof(UpdateManagerUrl)];

        [ApplicationScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("60")]
        public int MinimumRetryDelay => (int)this[nameof(MinimumRetryDelay)];

        [ApplicationScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("rcp://localhost:8004")]
        public string KioskEngineUrl => (string)this[nameof(KioskEngineUrl)];

        private void SettingChangingEventHandler(object sender, SettingChangingEventArgs e)
        {
        }

        private void SettingsSavingEventHandler(object sender, CancelEventArgs e)
        {
        }
    }
}
