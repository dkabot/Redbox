using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Redbox.KioskEngine.Environment
{
  public class IndoorElo : GenericTouchScreen
  {
    private static readonly string ControlApplication = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ProgramFiles), "EloTouchSystems\\elotouch.cpl");
    private static readonly string AlternateControlApplication = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ProgramFiles), "Elo TouchSystems\\Elo Universal Driver 470\\elotouch.cpl");

    public override string Driver
    {
      get
      {
        if (File.Exists(IndoorElo.ControlApplication))
          return IndoorElo.ControlApplication;
        return !File.Exists(IndoorElo.AlternateControlApplication) ? string.Empty : IndoorElo.AlternateControlApplication;
      }
    }

    public static bool Connected()
    {
      try
      {
        return IndoorElo.EloIF_EnumTouchScreenEx(new ulong[32]) > 0;
      }
      catch (Exception ex)
      {
        LogHelper.Instance.Log("Error checking for connected indoor ELO", ex);
        return false;
      }
    }

    protected override void ToggleScreen()
    {
      IndoorElo.EloIF_DisableTouchEx(this.State == TouchScreenState.Enabled, -1);
    }

    protected override bool Enabled()
    {
      bool bflag = false;
      IndoorElo.EloIF_GetTouchState(ref bflag, 0U);
      return !bflag;
    }

    [DllImport("EloIntf.dll")]
    private static extern int EloIF_EnumTouchScreenEx(ulong[] array);

    [DllImport("EloIntf.dll")]
    private static extern void EloIF_GetTouchState(ref bool bflag, uint number);

    [DllImport("EloIntf.dll")]
    private static extern bool EloIF_DisableTouchEx(bool status, int monitor_id);
  }
}
