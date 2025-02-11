using System;
using System.CodeDom.Compiler;
using System.Configuration;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Redbox.UpdateManager.Daemon.Properties
{
    [CompilerGenerated]
    [GeneratedCode("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "14.0.0.0")]
    internal sealed class Settings : ApplicationSettingsBase
    {
        private static Settings defaultInstance = (Settings)SettingsBase.Synchronized((SettingsBase)new Settings());

        public static Settings Default => Settings.defaultInstance;

        [ApplicationScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("rcp://localhost:7004")]
        public string Url => (string)this[nameof(Url)];

        [ApplicationScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("3")]
        public int MaximumWorkerThreads => (int)this[nameof(MaximumWorkerThreads)];

        [ApplicationScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("..\\scripts")]
        public string ScriptLocation => (string)this[nameof(ScriptLocation)];

        [ApplicationScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("${REDS}\\Kiosk Engine\\bin\\kioskengine.exe")]
        public string KioskEnginePath => (string)this[nameof(KioskEnginePath)];

        [ApplicationScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("False")]
        public bool DeveloperMode => (bool)this[nameof(DeveloperMode)];

        [ApplicationScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("True")]
        public bool InitialSubscriptionState => (bool)this[nameof(InitialSubscriptionState)];

        [ApplicationScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("")]
        public string StoreNumber => (string)this[nameof(StoreNumber)];

        [ApplicationScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("3503")]
        public int TrimWorkingSetInterval => (int)this[nameof(TrimWorkingSetInterval)];

        [ApplicationScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("00:00:30")]
        public TimeSpan UpdateServiceTimeout => (TimeSpan)this[nameof(UpdateServiceTimeout)];

        [ApplicationScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("${Scripts}\\shutdown.lua")]
        public string ShutdownScriptPath => (string)this[nameof(ShutdownScriptPath)];

        [ApplicationScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("60")]
        public int MinimumRetryDelay => (int)this[nameof(MinimumRetryDelay)];

        [ApplicationScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("http://bundle-dev-02:8002")]
        public string WcfUpdateServiceUrl => (string)this[nameof(WcfUpdateServiceUrl)];

        [ApplicationScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("rcp://qa-updatesvc01:7002")]
        public string UpdateServiceUrl => (string)this[nameof(UpdateServiceUrl)];

        [ApplicationScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("rcp://localhost:8004")]
        public string KioskEngineUrl => (string)this[nameof(KioskEngineUrl)];
    }
}
