using System.CodeDom.Compiler;
using System.Configuration;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Redbox.HAL.CameraTuner.Properties
{
    [CompilerGenerated]
    [GeneratedCode("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "12.0.0.0")]
    internal sealed class Settings : ApplicationSettingsBase
    {
        public static Settings Default { get; } = (Settings)Synchronized(new Settings());

        [ApplicationScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("False")]
        public bool DetailedLog => (bool)this[nameof(DetailedLog)];

        [ApplicationScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("5000")]
        public int WakeupPause => (int)this[nameof(WakeupPause)];
    }
}