using Redbox.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Redbox.KioskEngine.Environment
{
  public class OutdoorElo : GenericTouchScreen
  {
    private static readonly string EloInstallDirectory = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ProgramFiles), "Elo TouchSystems\\PCapacitive");
    private static readonly string ControlFile = Path.Combine(OutdoorElo.EloInstallDirectory, "EloPCap.INI");
    private static readonly string ControlExe = Path.Combine(OutdoorElo.EloInstallDirectory, "EloPCap_Control.exe");
    private static readonly string DriverExe = Path.Combine(OutdoorElo.EloInstallDirectory, "EloPCap_Driver.exe");

    public override string Driver => OutdoorElo.ControlExe;

    public static Process[] GetDriverProcess() => OutdoorElo.GetDriverProcess("EloPCap_Driver");

    public static Process[] GetDriverProcess(string processName)
    {
      try
      {
        return Process.GetProcessesByName(processName);
      }
      catch (Exception ex)
      {
        LogHelper.Instance.Log(string.Format("Error getting {0} process", (object) processName), ex);
        return new Process[0];
      }
    }

    protected override void ToggleScreen()
    {
      try
      {
        List<Process> processList = new List<Process>();
        processList.AddRange((IEnumerable<Process>) OutdoorElo.GetDriverProcess());
        processList.AddRange((IEnumerable<Process>) OutdoorElo.GetDriverProcess("EloPCap_Control"));
        processList.ForEach((Action<Process>) (process => process.Kill()));
        bool flag = !this.Enabled();
        WindowsAPI.WritePrivateProfileString("Output", "EnabledBoo1", Convert.ToInt16(flag).ToString(), OutdoorElo.ControlFile);
        string lpString = "2";
        if (!flag)
          lpString = "0";
        WindowsAPI.WritePrivateProfileString("Output", "PressTypeInt1", lpString, OutdoorElo.ControlFile);
        Process.Start(OutdoorElo.DriverExe);
      }
      catch (Exception ex)
      {
        LogHelper.Instance.Log("Error toggling touchscreen", ex);
      }
    }

    protected override bool Enabled()
    {
      try
      {
        if (!File.Exists(OutdoorElo.ControlFile))
          return true;
        ((IEnumerable<Process>) OutdoorElo.GetDriverProcess("EloPCap_Control")).ForEach<Process>((Action<Process>) (process => process.Kill()));
        StringBuilder lpReturnedString = new StringBuilder();
        int privateProfileString = (int) WindowsAPI.GetPrivateProfileString("Output", "EnabledBoo1", "1", lpReturnedString, lpReturnedString.Capacity, OutdoorElo.ControlFile);
        return string.Compare(lpReturnedString.ToString(), "1") == 0;
      }
      catch (Exception ex)
      {
        LogHelper.Instance.Log("Error getting elo driver process", ex);
        return true;
      }
    }
  }
}
