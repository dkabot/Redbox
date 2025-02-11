using System;
using System.Collections.Generic;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.DataStorage
{
    public sealed class DataTableService : IDataTableService
    {
        private readonly Dictionary<Type, object> LegacyTypeMap = new Dictionary<Type, object>();
        private readonly Dictionary<Type, object> TypeMap = new Dictionary<Type, object>();

        public DataTableService(bool exclusive)
        {
            var service = ServiceLocator.Instance.GetService<IRuntimeService>();
            var dataDescriptor = new DataDescriptor(service.DataPath);
            var d = new CountersDescriptor(service.DataPath, exclusive);
            TypeMap.Add(typeof(IPersistentCounter), new PersistentCounterTable(d));
            TypeMap.Add(typeof(ILocation), new InventoryTable(dataDescriptor));
            TypeMap.Add(typeof(IDumpBinInventoryItem), new DumpbinTable(dataDescriptor));
            TypeMap.Add(typeof(IHardwareCorrectionStatistic), new HardwareCorrectionStatsTable(d));
            TypeMap.Add(typeof(IKioskFunctionCheckData), new KioskFunctionCheckTable(d));
            TypeMap.Add(typeof(IPersistentOption), new SecretOptionsTable(d));
            LegacyTypeMap.Add(typeof(IDumpBinInventoryItem), new LegacyDumpbinTable(d));
            LegacyTypeMap.Add(typeof(IHardwareCorrectionStatistic), new LegacyHardwareCorrectionStatsTable(d));
        }

        public IDataTable<T> GetTable<T>()
        {
            var key = typeof(T);
            return !TypeMap.ContainsKey(key) ? null : (IDataTable<T>)TypeMap[key];
        }

        public IDataTable<T> GetLegacyTable<T>()
        {
            var key = typeof(T);
            return !LegacyTypeMap.ContainsKey(key) ? null : (IDataTable<T>)LegacyTypeMap[key];
        }

        public bool Add<T>(IDataTable<T> table)
        {
            var key = typeof(T);
            if (TypeMap.ContainsKey(key))
                return false;
            TypeMap[key] = table;
            return true;
        }
    }
}