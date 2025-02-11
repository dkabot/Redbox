using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Redbox.HAL.MSHALTester.Properties;

[CompilerGenerated]
[GeneratedCode("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "12.0.0.0")]
public sealed class Settings : ApplicationSettingsBase
{
    public static Settings Default { get; } = (Settings)Synchronized(new Settings());

    [ApplicationScopedSetting]
    [DebuggerNonUserCode]
    [DefaultSettingValue("rcp://127.0.0.1:7001")]
    public string CommunicationURL => (string)this[nameof(CommunicationURL)];

    [ApplicationScopedSetting]
    [DebuggerNonUserCode]
    [DefaultSettingValue("70000")]
    public int ImmediateMoveTimeout => (int)this[nameof(ImmediateMoveTimeout)];

    [ApplicationScopedSetting]
    [DebuggerNonUserCode]
    [DefaultSettingValue("True")]
    public bool CheckForRunningHAL => (bool)this[nameof(CheckForRunningHAL)];

    [ApplicationScopedSetting]
    [DebuggerNonUserCode]
    [DefaultSettingValue("False")]
    public bool AllowSessionDisplay => (bool)this[nameof(AllowSessionDisplay)];

    [ApplicationScopedSetting]
    [DebuggerNonUserCode]
    [DefaultSettingValue("True")]
    public bool UsbServiceDebug => (bool)this[nameof(UsbServiceDebug)];

    [ApplicationScopedSetting]
    [DebuggerNonUserCode]
    [DefaultSettingValue("True")]
    public bool TestCommOnStartup => (bool)this[nameof(TestCommOnStartup)];

    [UserScopedSetting]
    [DebuggerNonUserCode]
    [DefaultSettingValue("c:\\Program Files\\Redbox\\KioskLogs\\ErrorLogs\\Timeouts.log")]
    public string CountersFile
    {
        get => (string)this[nameof(CountersFile)];
        set => this[nameof(CountersFile)] = value;
    }

    [ApplicationScopedSetting]
    [DebuggerNonUserCode]
    [DefaultSettingValue("5000")]
    public int RouterRelayPause => (int)this[nameof(RouterRelayPause)];

    [ApplicationScopedSetting]
    [DebuggerNonUserCode]
    [DefaultSettingValue("False")]
    public bool DisplayEngineeringTab => (bool)this[nameof(DisplayEngineeringTab)];

    [ApplicationScopedSetting]
    [DebuggerNonUserCode]
    [DefaultSettingValue("c:\\Program Files\\Redbox\\HALService\\video")]
    public string ImageDirectory => (string)this[nameof(ImageDirectory)];

    [ApplicationScopedSetting]
    [DebuggerNonUserCode]
    [DefaultSettingValue("c:\\Program Files\\Redbox\\halservice\\bin\\CameraTuner.exe")]
    public string TunerApplication => (string)this[nameof(TunerApplication)];

    [ApplicationScopedSetting]
    [DebuggerNonUserCode]
    [DefaultSettingValue("2500")]
    public int TunerStartPause => (int)this[nameof(TunerStartPause)];

    private void SettingChangingEventHandler(object sender, SettingChangingEventArgs e)
    {
    }

    private void SettingsSavingEventHandler(object sender, CancelEventArgs e)
    {
    }
}