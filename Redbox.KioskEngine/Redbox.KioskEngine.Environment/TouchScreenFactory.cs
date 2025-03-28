using Redbox.Core;
using System;
using System.IO;

namespace Redbox.KioskEngine.Environment
{
  public class TouchScreenFactory
  {
    private GenericTouchScreen m_touchScreen;

    public static TouchScreenFactory Instance => Singleton<TouchScreenFactory>.Instance;

    public void Initialize()
    {
      try
      {
        LogHelper.Instance.Log("Detecting touch screen model");
        if (OutdoorElo.GetDriverProcess().Length != 0)
        {
          LogHelper.Instance.Log("Detected outdoor ELO");
          this.m_touchScreen = (GenericTouchScreen) new OutdoorElo();
        }
        else if (File.Exists(Path.Combine(System.Environment.SystemDirectory, "EloIntf.dll")))
        {
          if (IndoorElo.Connected())
          {
            LogHelper.Instance.Log("Detected indoor ELO");
            this.m_touchScreen = (GenericTouchScreen) new IndoorElo();
          }
          else
            LogHelper.Instance.Log("EloIntf.dll exists but cannot connect to it as an indoor ELO.");
        }
        else
          LogHelper.Instance.Log("No known touch screen detected.");
      }
      catch (Exception ex)
      {
        LogHelper.Instance.Log("Error determining touchscreen model, using generic", ex);
      }
      if (this.m_touchScreen != null)
        return;
      LogHelper.Instance.Log("Using generic touch screen driver.");
      this.m_touchScreen = new GenericTouchScreen();
    }

    public GenericTouchScreen GetTouchScreen() => this.m_touchScreen;

    private TouchScreenFactory()
    {
    }
  }
}
