using Redbox.Core;
using System;
using System.Diagnostics;

namespace Redbox.KioskEngine.Environment
{
  internal static class Windows10TouchScreen
  {
    private const string EXE = "wmic.exe";
    private const string ARGS_MASK = "path Win32_PnPEntity where \"Name='HID-Compliant touch screen'\" call {0}";
    private const string ENABLE = "Enable";
    private const string DISABLE = "Disable";

    public static void Enable() => Windows10TouchScreen.FireProcess(nameof (Enable));

    public static void Disable() => Windows10TouchScreen.FireProcess(nameof (Disable));

    private static void FireProcess(string wmiMethod)
    {
      Process process = (Process) null;
      try
      {
        LogHelper.Instance.Log("Windows10TouchScreen - Attempting to " + wmiMethod + " HID-compliant touch screen");
        string arguments = string.Format("path Win32_PnPEntity where \"Name='HID-Compliant touch screen'\" call {0}", (object) wmiMethod);
        process = new Process()
        {
          StartInfo = new ProcessStartInfo("wmic.exe", arguments)
          {
            CreateNoWindow = true,
            WindowStyle = ProcessWindowStyle.Hidden,
            UseShellExecute = false,
            RedirectStandardOutput = true
          }
        };
        process.Start();
        process.WaitForExit();
        LogHelper.Instance.Log(string.Format("Windows10TouchScreen - operation result: {0}, output: {1}", (object) process.ExitCode, (object) process.StandardOutput.ReadToEnd()));
      }
      catch (Exception ex)
      {
        LogHelper.Instance.LogException("Exception trying to toggle HID touch screen device!", ex);
      }
      finally
      {
        process.Close();
      }
    }
  }
}
