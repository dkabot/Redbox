using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using System;
using System.Collections.Generic;
using System.IO;
using VistaDB;
using VistaDB.DDA;

namespace Redbox.KioskEngine.Environment
{
  public class DataStoreCache
  {
    private static readonly IDictionary<string, DataStoreCache.DatabaseCache> m_databases = (IDictionary<string, DataStoreCache.DatabaseCache>) new Dictionary<string, DataStoreCache.DatabaseCache>();

    public static void CloseAll()
    {
      LogHelper.Instance.Log("Close all data store caches.");
      foreach (string path in new List<string>((IEnumerable<string>) DataStoreCache.m_databases.Keys))
      {
        LogHelper.Instance.Log("...Close data store: {0}", (object) path);
        DataStoreCache.CloseStore(path);
      }
    }

    public static void CloseStore(string path)
    {
      if (!DataStoreCache.m_databases.ContainsKey(path))
        return;
      DataStoreCache.DatabaseCache database = DataStoreCache.m_databases[path];
      database.Instance.Dispose();
      database.DDA.Dispose();
      DataStoreCache.m_databases.Remove(path);
    }

    public static IVistaDBDatabase GetWorkStore(string path)
    {
      return DataStoreCache.GetStore(path, VistaDBDatabaseOpenMode.NonexclusiveReadWrite);
    }

    public static IVistaDBDatabase GetProfileStore(string path)
    {
      return DataStoreCache.GetStore(path, VistaDBDatabaseOpenMode.SharedReadOnly);
    }

    private static IVistaDBDatabase GetStore(string path, VistaDBDatabaseOpenMode mode)
    {
      try
      {
        if (DataStoreCache.m_databases.ContainsKey(path))
          return DataStoreCache.m_databases[path].Instance;
        DataStoreCache.DatabaseCache databaseCache = new DataStoreCache.DatabaseCache()
        {
          DDA = VistaDBEngine.Connections.OpenDDA()
        };
        if (!File.Exists(path))
        {
          if (mode == VistaDBDatabaseOpenMode.SharedReadOnly)
            return (IVistaDBDatabase) null;
          databaseCache.Instance = databaseCache.DDA.CreateDatabase(path, false, (string) null, 0, 0, false);
        }
        if (databaseCache.Instance == null)
          databaseCache.Instance = databaseCache.DDA.OpenDatabase(path, mode, (string) null);
        DataStoreCache.m_databases[path] = databaseCache;
        return databaseCache.Instance;
      }
      catch (Exception ex)
      {
        LogHelper.Instance.Log("Unhandled exception in DataStoreCache.GetStore; file :" + Path.GetFileName(path) + ".", ex);
        ServiceLocator.Instance.GetService<IEnvironmentNotificationService>()?.RaiseCorruptDb();
        throw;
      }
    }

    private sealed class DatabaseCache
    {
      public IVistaDBDDA DDA;
      public IVistaDBDatabase Instance;
    }
  }
}
