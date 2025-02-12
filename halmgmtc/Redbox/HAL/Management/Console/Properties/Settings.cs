using System.CodeDom.Compiler;
using System.Configuration;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Redbox.HAL.Management.Console.Properties
{
    [CompilerGenerated]
    [GeneratedCode("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "12.0.0.0")]
    internal sealed class Settings : ApplicationSettingsBase
    {
        public static Settings Default { get; } = (Settings)Synchronized(new Settings());

        [ApplicationScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("120000")]
        public int ImmediateInstructionTimeout => (int)this[nameof(ImmediateInstructionTimeout)];

        [ApplicationScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("Red")]
        public string ButtonRunningColor => (string)this[nameof(ButtonRunningColor)];

        [ApplicationScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("")]
        public string ButtonCompleteColor => (string)this[nameof(ButtonCompleteColor)];

        [ApplicationScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("rcp://127.0.0.1:7001")]
        public string DefaultConnectionURL => (string)this[nameof(DefaultConnectionURL)];

        [ApplicationScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("thin,thin-vmz,qlm-unload,unload-thin,sync,sync-locations,quick-return")]
        public string SecureJob => (string)this[nameof(SecureJob)];

        [ApplicationScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("False")]
        public bool DeveloperMode => (bool)this[nameof(DeveloperMode)];
    }
}