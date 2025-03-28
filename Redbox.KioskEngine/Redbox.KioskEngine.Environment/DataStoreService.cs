using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.IDE;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Redbox.KioskEngine.Environment
{
  public class DataStoreService : IDataStoreService
  {
    private readonly IDictionary<string, DataStoreProvider> m_providers = (IDictionary<string, DataStoreProvider>) new Dictionary<string, DataStoreProvider>();

    public static DataStoreService Instance => Singleton<DataStoreService>.Instance;

    public ErrorList Initialize()
    {
      ErrorList errorList = new ErrorList();
      ServiceLocator.Instance.AddService(typeof (IDataStoreService), (object) DataStoreService.Instance);
      IPreferenceService service = ServiceLocator.Instance.GetService<IPreferenceService>();
      if (service == null)
        return errorList;
      service.AddPreferencePage("DataStoreService_General", "Core", "Engine Core/Data Stores", PreferencePageTarget.LocalSystem, (IPreferencePageHost) new DataStorePreferencePage());
      return errorList;
    }

    public void CloseAllStores() => DataStoreCache.CloseAll();

    public void CloseStore(string dataStore) => this.GetProvider(dataStore).CloseStore(dataStore);

    public void Set(string dataStore, string tableName, string keyName, object data)
    {
      this.GetProvider(dataStore).Set(dataStore, tableName, keyName, data);
    }

    public void BulkSet(string dataStore, string tableName, IDictionary<string, object> values)
    {
      this.GetProvider(dataStore).BulkSet(dataStore, tableName, values);
    }

    public void Clear(string dataStore, string tableName)
    {
      this.GetProvider(dataStore).Clear(dataStore, tableName);
    }

    public object Get(string dataStore, string tableName, string keyName)
    {
      return this.GetProvider(dataStore).Get(dataStore, tableName, keyName);
    }

    public Dictionary<string, object> GetAll(string dataStore, string tableName)
    {
      return this.GetProvider(dataStore).GetAll(dataStore, tableName);
    }

    public void Drop(string dataStore, string tableName)
    {
      this.GetProvider(dataStore).Drop(dataStore, tableName);
    }

    public long Count(string dataStore, string tableName)
    {
      return this.GetProvider(dataStore).Count(dataStore, tableName);
    }

    public void Remove(string dataStore, string tableName, string keyName)
    {
      this.GetProvider(dataStore).Remove(dataStore, tableName, keyName);
    }

    public ReadOnlyCollection<string> GetKeys(string dataStore, string tableName)
    {
      return this.GetProvider(dataStore).GetKeys(dataStore, tableName);
    }

    public ReadOnlyCollection<string> GetTables(string dataStore)
    {
      return this.GetProvider(dataStore).GetTables(dataStore);
    }

    private DataStoreService()
    {
    }

    private DataStoreProvider GetProvider(string dataStore)
    {
      if (this.m_providers.ContainsKey(dataStore))
        return this.m_providers[dataStore];
      DataStoreProvider provider = DataStoreProvider.GetProvider(dataStore);
      this.m_providers[dataStore] = provider;
      return provider;
    }
  }
}
