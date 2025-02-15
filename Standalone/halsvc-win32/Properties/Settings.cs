using System.CodeDom.Compiler;
using System.Configuration;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Redbox.HAL.Service.Win32.Properties
{
    [CompilerGenerated]
    [GeneratedCode("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "12.0.0.0")]
    internal sealed class Settings : ApplicationSettingsBase
    {
        public static Settings Default { get; } = (Settings)Synchronized(new Settings());

        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("Modern")]
        public string MotionControlServiceVersion
        {
            get => (string)this[nameof(MotionControlServiceVersion)];
            set => this[nameof(MotionControlServiceVersion)] = value;
        }

        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("True")]
        public bool CountersDBExclusiveMode
        {
            get => (bool)this[nameof(CountersDBExclusiveMode)];
            set => this[nameof(CountersDBExclusiveMode)] = value;
        }

        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("True")]
        public bool UsbServiceDebug
        {
            get => (bool)this[nameof(UsbServiceDebug)];
            set => this[nameof(UsbServiceDebug)] = value;
        }

        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("rcp://127.0.0.1:7001")]
        public string ServiceProtocol
        {
            get => (string)this[nameof(ServiceProtocol)];
            set => this[nameof(ServiceProtocol)] = value;
        }

        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("Legacy")]
        public string ServiceHostGeneration
        {
            get => (string)this[nameof(ServiceHostGeneration)];
            set => this[nameof(ServiceHostGeneration)] = value;
        }
    }
}