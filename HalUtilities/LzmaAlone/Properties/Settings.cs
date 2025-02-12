using System.Configuration;
using System.Runtime.InteropServices;
using System.Threading;

namespace LzmaAlone.Properties
{
  [ComVisible(true)]
  public class Settings : ApplicationSettingsBase
  {
    private static Settings m_Value;
    private static object m_SyncObject = new object();

    public static Settings Value
    {
      get
      {
        if (Settings.m_Value == null)
        {
          Monitor.Enter(Settings.m_SyncObject);
          if (Settings.m_Value == null)
          {
            try
            {
              Settings.m_Value = new Settings();
            }
            finally
            {
              Monitor.Exit(Settings.m_SyncObject);
            }
          }
        }
        return Settings.m_Value;
      }
    }
  }
}
