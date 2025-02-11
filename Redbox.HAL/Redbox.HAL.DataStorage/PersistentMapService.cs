using System;
using System.Collections.Generic;
using System.Threading;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Extensions;
using Redbox.HAL.Component.Model.Threading;

namespace Redbox.HAL.DataStorage
{
    public sealed class PersistentMapService : IPersistentMap, IPersistentMapService
    {
        private readonly ReaderWriterLockSlim Lock = new ReaderWriterLockSlim();
        private readonly List<IPersistentOption> Options = new List<IPersistentOption>();
        private readonly IDataTable<IPersistentOption> Table;

        public PersistentMapService()
        {
            Table = ServiceLocator.Instance.GetService<IDataTableService>().GetTable<IPersistentOption>();
            Options = Table.LoadEntries();
        }

        public void Remove(string key)
        {
            using (new WithUpgradeableReadLock(Lock))
            {
                var persistentOption = Options.Find(each => each.Key == key);
                if (persistentOption == null)
                    return;
                using (new WithWriteLock(Lock))
                {
                    Table.Delete(persistentOption);
                    Options.Remove(persistentOption);
                }
            }
        }

        public T GetValue<T>(string name, T defVal)
        {
            using (new WithUpgradeableReadLock(Lock))
            {
                var persistentOption = Options.Find(each => each.Key == name);
                if (persistentOption != null)
                    return ConversionHelper.ChangeType<T>(persistentOption.Value);
                using (new WithWriteLock(Lock))
                {
                    InsertUnderLock(name, defVal);
                    return defVal;
                }
            }
        }

        public void SetValue<T>(string key, T value)
        {
            using (new WithWriteLock(Lock))
            {
                try
                {
                    var persistentOption = Options.Find(each => each.Key == key);
                    if (persistentOption != null)
                    {
                        persistentOption.UpdateValue(value.ToString());
                        Table.Update(persistentOption);
                    }
                    else
                    {
                        InsertUnderLock(key, value);
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.Instance.Log("[PersistentMapService] SetValue caught an exception.", ex);
                }
            }
        }

        public IPersistentMap GetMap()
        {
            return this;
        }

        private void InsertUnderLock<T>(string key, T val)
        {
            var p = ServiceLocator.Instance.GetService<ITableTypeFactory>().NewPersistentOption(key, val.ToString());
            Table.Insert(p);
            Options.Add(p);
        }
    }
}