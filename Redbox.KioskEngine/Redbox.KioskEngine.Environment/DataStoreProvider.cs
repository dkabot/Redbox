using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.ProductLookupCatalog;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace Redbox.KioskEngine.Environment
{
  internal class DataStoreProvider : IDataStoreService
  {
    public static DataStoreProvider GetProvider(string dataStore)
    {
      string path = DataStoreProvider.GetPath(dataStore);
      return File.Exists(path) && Archive.IsValid(path) ? (DataStoreProvider) new ProductLookupDataStoreProvider() : (DataStoreProvider) new VistaDbDataStoreProvider();
    }

    public void RegisterDataStoreProvider(string dataStore, string provider)
    {
    }

    public void Set(string dataStore, string tableName, string keyName, object data)
    {
      this.OnSet(dataStore, tableName, keyName, data);
    }

    public void BulkSet(string dataStore, string tableName, IDictionary<string, object> values)
    {
      this.OnBulkSet(dataStore, tableName, values);
    }

    public void Clear(string dataStore, string tableName) => this.OnClear(dataStore, tableName);

    public object Get(string dataStore, string tableName, string keyName)
    {
      return this.OnGet(dataStore, tableName, keyName);
    }

    public Dictionary<string, object> GetAll(string dataStore, string tableName)
    {
      return this.OnGetAll(dataStore, tableName);
    }

    public void Drop(string dataStore, string tableName) => this.OnDrop(dataStore, tableName);

    public long Count(string dataStore, string tableName) => this.OnCount(dataStore, tableName);

    public void Remove(string dataStore, string tableName, string keyName)
    {
      this.OnRemove(dataStore, tableName, keyName);
    }

    public void CloseAllStores() => this.OnCloseAllStores();

    public void CloseStore(string dataStore) => this.OnCloseStore(dataStore);

    public ReadOnlyCollection<string> GetKeys(string dataStore, string tableName)
    {
      return this.OnGetKeys(dataStore, tableName);
    }

    public ReadOnlyCollection<string> GetTables(string dataStore) => this.OnGetTables(dataStore);

    protected DataStoreProvider()
    {
    }

    protected virtual void OnSet(string dataStore, string tableName, string keyName, object data)
    {
    }

    protected virtual void OnBulkSet(
      string dataStore,
      string tableName,
      IDictionary<string, object> values)
    {
    }

    protected virtual void OnClear(string dataStore, string tableName)
    {
    }

    protected virtual object OnGet(string dataStore, string tableName, string keyName)
    {
      return (object) null;
    }

    protected virtual Dictionary<string, object> OnGetAll(string dataStore, string tableName)
    {
      return (Dictionary<string, object>) null;
    }

    protected virtual void OnDrop(string dataStore, string tableName)
    {
    }

    protected virtual long OnCount(string dataStore, string tableName) => 0;

    protected virtual void OnRemove(string dataStore, string tableName, string keyName)
    {
    }

    protected virtual void OnCloseAllStores()
    {
    }

    protected virtual void OnCloseStore(string dataStore)
    {
    }

    protected virtual ReadOnlyCollection<string> OnGetKeys(string dataStore, string tableName)
    {
      return new List<string>().AsReadOnly();
    }

    protected virtual ReadOnlyCollection<string> OnGetTables(string dataStore)
    {
      return new List<string>().AsReadOnly();
    }

    internal static string GetPath(string dataStore)
    {
      return Path.Combine(ServiceLocator.Instance.GetService<IEngineApplication>().DataPath, string.Format("{0}.data", (object) dataStore));
    }
  }
}
