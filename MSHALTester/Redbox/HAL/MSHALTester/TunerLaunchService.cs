using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using Redbox.HAL.Client;
using Redbox.HAL.Common.GUI.Functions;
using Redbox.HAL.Component.Model;
using Redbox.HAL.MSHALTester.Properties;

namespace Redbox.HAL.MSHALTester;

internal static class TunerLaunchService
{
    private static readonly byte[] ShutdownCommand;
    private static readonly byte[] StartupCommand;

    static TunerLaunchService()
    {
        var stringBuilder1 = new StringBuilder();
        stringBuilder1.Append("CAMERA STOP FORCE=TRUE" + Environment.NewLine);
        stringBuilder1.Append("RINGLIGHT ON" + Environment.NewLine);
        ShutdownCommand = Encoding.ASCII.GetBytes(stringBuilder1.ToString());
        var stringBuilder2 = new StringBuilder();
        stringBuilder2.Append("RINGLIGHT OFF" + Environment.NewLine);
        stringBuilder2.Append("CAMERA START" + Environment.NewLine);
        StartupCommand = Encoding.ASCII.GetBytes(stringBuilder2.ToString());
    }

    internal static void LaunchTunerAndWait(HardwareService service, ButtonAspectsManager manager)
    {
        HardwareJob job;
        service.ExecuteImmediateProgram(ShutdownCommand, out job);
        ServiceLocator.Instance.GetService<IRuntimeService>().SpinWait(Settings.Default.TunerStartPause);
        var tunerApplication = Settings.Default.TunerApplication;
        if (File.Exists(tunerApplication))
            Process.Start(tunerApplication, string.Empty).WaitForExit();
        else
            using (var cameraPreviewForm = new CameraPreviewForm(service, manager, Settings.Default.ImageDirectory))
            {
                var num = (int)cameraPreviewForm.ShowDialog();
            }

        service.ExecuteImmediateProgram(StartupCommand, out job);
    }
}