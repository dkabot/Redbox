using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using System;
using System.IO;
using VistaDB.DDA;

namespace Redbox.KioskEngine.Environment
{
  public class DataCacheService : IDataCacheService
  {
    private static string _profilePath;

    public static DataCacheService Instance => Singleton<DataCacheService>.Instance;

    public void Shutdown()
    {
      LogHelper.Instance.Log("Shutdown DataCacheService.");
      DataStoreCache.CloseAll();
    }

    public byte[] GetContent(DataCacheType type, string name)
    {
      try
      {
        using (IVistaDBTable vistaDbTable = DataStoreCache.GetProfileStore(DataCacheService.GetPath()).OpenTable("Cache", false, true))
        {
          if (vistaDbTable.Find(string.Format("Name: '{0}'; Type: {1}", (object) name, (object) (int) type), "IX_Key", false, false))
            return (byte[]) vistaDbTable.CurrentRow["Data"].Value;
        }
      }
      catch (Exception ex)
      {
        LogHelper.Instance.Log("Unable to load cached data due to unhandled exception.", ex);
      }
      return (byte[]) null;
    }

    private static string GetPath()
    {
      if (string.IsNullOrWhiteSpace(DataCacheService._profilePath))
        DataCacheService._profilePath = Path.Combine(ServiceLocator.Instance.GetService<IEngineApplication>().DataPath, "profile.data");
      return DataCacheService._profilePath;
    }

    private DataCacheService()
    {
    }
  }
}
