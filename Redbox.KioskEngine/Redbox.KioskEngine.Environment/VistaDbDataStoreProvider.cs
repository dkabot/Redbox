using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using VistaDB;
using VistaDB.DDA;

namespace Redbox.KioskEngine.Environment
{
  internal class VistaDbDataStoreProvider : DataStoreProvider
  {
    protected override void OnClear(string dataStore, string tableName)
    {
      try
      {
        IVistaDBDatabase workStore = DataStoreCache.GetWorkStore(DataStoreProvider.GetPath(dataStore));
        if (workStore == null || !workStore.ContainsTable(tableName))
          return;
        using (IVistaDBTable vistaDbTable = workStore.OpenTable(tableName, false, false))
        {
          vistaDbTable.First();
          while (!vistaDbTable.EndOfTable)
          {
            vistaDbTable.Delete();
            vistaDbTable.Next();
          }
        }
      }
      catch (Exception ex)
      {
        LogHelper.Instance.Log(string.Format("Exception in VistaDbDataStoreProvider.OnClear({0},{1})", (object) dataStore, (object) tableName), ex);
        throw;
      }
    }

    protected override void OnCloseAllStores() => DataStoreCache.CloseAll();

    protected override void OnCloseStore(string dataStore)
    {
      DataStoreCache.CloseStore(DataStoreProvider.GetPath(dataStore));
    }

    protected override long OnCount(string dataStore, string tableName)
    {
      try
      {
        IVistaDBDatabase workStore = DataStoreCache.GetWorkStore(DataStoreProvider.GetPath(dataStore));
        if (workStore == null || !workStore.ContainsTable(tableName))
          return 0;
        using (IVistaDBTable vistaDbTable = workStore.OpenTable(tableName, false, true))
          return vistaDbTable.RowCount;
      }
      catch (Exception ex)
      {
        LogHelper.Instance.Log(string.Format("Exception in VistaDbDataStoreProvider.OnCount({0},{1})", (object) dataStore, (object) tableName), ex);
        throw;
      }
    }

    protected override object OnGet(string dataStore, string tableName, string keyName)
    {
      try
      {
        IVistaDBDatabase workStore = DataStoreCache.GetWorkStore(DataStoreProvider.GetPath(dataStore));
        if (workStore == null || !workStore.ContainsTable(tableName))
          return (object) null;
        using (IVistaDBTable table = workStore.OpenTable(tableName, false, true))
          return VistaDbDataStoreProvider.InternalGet(table, keyName);
      }
      catch (Exception ex)
      {
        LogHelper.Instance.Log(string.Format("Exception in VistaDbDataStoreProvider.OnGet({0},{1},{2})", (object) dataStore, (object) tableName, (object) keyName), ex);
        throw;
      }
    }

    protected override Dictionary<string, object> OnGetAll(string dataStore, string tableName)
    {
      Dictionary<string, object> all = new Dictionary<string, object>();
      try
      {
        IVistaDBDatabase workStore = DataStoreCache.GetWorkStore(DataStoreProvider.GetPath(dataStore));
        if (workStore == null || !workStore.ContainsTable(tableName))
          return all;
        using (IVistaDBTable vistaDbTable = workStore.OpenTable(tableName, false, true))
        {
          vistaDbTable.Next();
          while (!vistaDbTable.EndOfTable)
          {
            string key = vistaDbTable.CurrentRow["Key"].Value as string;
            string str = vistaDbTable.CurrentRow["Value"].Value as string;
            if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(str))
            {
              all[key] = (object) str;
              vistaDbTable.Next();
            }
          }
        }
      }
      catch (Exception ex)
      {
        LogHelper.Instance.Log(string.Format("Exception in VistaDbDataStoreProvider.OnGetAll({0},{1})", (object) dataStore, (object) tableName), ex);
        throw;
      }
      return all;
    }

    protected override void OnDrop(string dataStore, string tableName)
    {
      try
      {
        IVistaDBDatabase workStore = DataStoreCache.GetWorkStore(DataStoreProvider.GetPath(dataStore));
        if (workStore == null || !workStore.ContainsTable(tableName))
          return;
        workStore.DropTable(tableName);
      }
      catch (Exception ex)
      {
        LogHelper.Instance.Log(string.Format("Exception in VistaDbDataStoreProvider.OnDrop({0},{1})", (object) dataStore, (object) tableName), ex);
        throw;
      }
    }

    protected override ReadOnlyCollection<string> OnGetKeys(string dataStore, string tableName)
    {
      List<string> stringList = new List<string>();
      try
      {
        IVistaDBDatabase workStore = DataStoreCache.GetWorkStore(DataStoreProvider.GetPath(dataStore));
        if (workStore == null || !workStore.ContainsTable(tableName))
          return stringList.AsReadOnly();
        using (IVistaDBTable vistaDbTable = workStore.OpenTable(tableName, false, true))
        {
          vistaDbTable.Next();
          while (!vistaDbTable.EndOfTable)
          {
            string str = vistaDbTable.CurrentRow["Key"].Value as string;
            if (!string.IsNullOrEmpty(str))
            {
              stringList.Add(str);
              vistaDbTable.Next();
            }
          }
        }
      }
      catch (Exception ex)
      {
        LogHelper.Instance.Log(string.Format("Exception in VistaDbDataStoreProvider.OnGetKeys({0},{1})", (object) dataStore, (object) tableName), ex);
        throw;
      }
      return stringList.AsReadOnly();
    }

    protected override void OnRemove(string dataStore, string tableName, string keyName)
    {
      try
      {
        IVistaDBDatabase workStore = DataStoreCache.GetWorkStore(DataStoreProvider.GetPath(dataStore));
        if (workStore == null || !workStore.ContainsTable(tableName))
          return;
        using (IVistaDBTable vistaDbTable = workStore.OpenTable(tableName, false, false))
        {
          if (!vistaDbTable.Find(string.Format("Key: '{0}'", (object) keyName), "IX_Key", false, false))
            return;
          vistaDbTable.Delete();
        }
      }
      catch (Exception ex)
      {
        LogHelper.Instance.Log(string.Format("Exception in VistaDbDataStoreProvider.OnRemove({0},{1},{2})", (object) dataStore, (object) tableName, (object) keyName), ex);
        throw;
      }
    }

    protected override void OnSet(string dataStore, string tableName, string keyName, object data)
    {
      IVistaDBDatabase workStore = DataStoreCache.GetWorkStore(DataStoreProvider.GetPath(dataStore));
      if (workStore == null)
        return;
      IVistaDBTable table = (IVistaDBTable) null;
      try
      {
        if (!workStore.ContainsTable(tableName))
        {
          IVistaDBTableSchema schema = workStore.NewTable(tableName);
          schema.AddColumn("Key", VistaDBType.VarChar, 250);
          schema.AddColumn("Value", VistaDBType.NText);
          table = workStore.CreateTable(schema, false, false);
          table.CreateIndex("IX_Key", "Key", true, true);
        }
        else
          table = workStore.OpenTable(tableName, false, false);
        IKernelService service = ServiceLocator.Instance.GetService<IKernelService>();
        if (VistaDbDataStoreProvider.InternalGet(table, keyName) == null)
        {
          table.Insert();
          table.PutString("Key", keyName);
        }
        table.PutString("Value", service.FormatLuaValue(data));
        table.Post();
      }
      catch (Exception ex)
      {
        LogHelper.Instance.Log(string.Format("Exception in VistaDbDataStoreProvider.OnSet({0},{1},{2})", (object) dataStore, (object) tableName, (object) keyName), ex);
        throw;
      }
      finally
      {
        table?.Dispose();
      }
    }

    protected override void OnBulkSet(
      string dataStore,
      string tableName,
      IDictionary<string, object> values)
    {
      IVistaDBDatabase workStore = DataStoreCache.GetWorkStore(DataStoreProvider.GetPath(dataStore));
      if (workStore == null)
        return;
      IKernelService service = ServiceLocator.Instance.GetService<IKernelService>();
      IVistaDBTable table = (IVistaDBTable) null;
      try
      {
        if (!workStore.ContainsTable(tableName))
        {
          IVistaDBTableSchema schema = workStore.NewTable(tableName);
          schema.AddColumn("Key", VistaDBType.VarChar, 250);
          schema.AddColumn("Value", VistaDBType.NText);
          table = workStore.CreateTable(schema, false, false);
          table.CreateIndex("IX_Key", "Key", true, true);
        }
        else
          table = workStore.OpenTable(tableName, false, false);
        foreach (KeyValuePair<string, object> keyValuePair in (IEnumerable<KeyValuePair<string, object>>) values)
        {
          if (VistaDbDataStoreProvider.InternalGet(table, keyValuePair.Key) == null)
          {
            table.Insert();
            table.PutString("Key", keyValuePair.Key);
          }
          table.PutString("Value", service.FormatLuaValue(keyValuePair.Value));
          table.Post();
        }
      }
      catch (Exception ex)
      {
        LogHelper.Instance.Log(string.Format("Exception in VistaDbDataStoreProvider.OnBulkSet({0},{1})", (object) dataStore, (object) tableName), ex);
        throw;
      }
      finally
      {
        table?.Dispose();
      }
    }

    protected override ReadOnlyCollection<string> OnGetTables(string dataStore)
    {
      List<string> stringList = new List<string>();
      IVistaDBDatabase workStore = DataStoreCache.GetWorkStore(DataStoreProvider.GetPath(dataStore));
      if (workStore == null)
        return stringList.AsReadOnly();
      try
      {
        foreach (string enumTable in workStore.EnumTables())
          stringList.Add(enumTable);
      }
      catch (Exception ex)
      {
        LogHelper.Instance.Log(string.Format("Exception in VistaDbDataStoreProvider.OnGetTables({0})", (object) dataStore), ex);
        throw;
      }
      return stringList.AsReadOnly();
    }

    private static object InternalGet(IVistaDBTable table, string keyName)
    {
      return !table.Find(string.Format("Key: '{0}'", (object) keyName), "IX_Key", false, false) ? (object) null : table.CurrentRow["Value"].Value;
    }
  }
}
