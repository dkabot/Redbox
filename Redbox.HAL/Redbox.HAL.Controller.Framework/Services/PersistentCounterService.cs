using System;
using System.Collections.Generic;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework.Services
{
    internal sealed class PersistentCounterService : IPersistentCounterService
    {
        private readonly Dictionary<string, IPersistentCounter> Counters = new Dictionary<string, IPersistentCounter>();
        private readonly IDataTable<IPersistentCounter> CounterTable;
        private readonly List<string> WeeklyResettableCounters = new List<string>();

        internal PersistentCounterService(IDataTableService dts)
        {
            foreach (TimeoutCounters counter in Enum.GetValues(typeof(TimeoutCounters)))
            {
                var str = KeyFrom(counter);
                if (!WeeklyResettableCounters.Contains(str))
                    WeeklyResettableCounters.Add(str);
            }

            CounterTable = dts.GetTable<IPersistentCounter>();
            var list = CounterTable.LoadEntries();
            using (new DisposeableList<IPersistentCounter>(list))
            {
                foreach (var persistentCounter in list)
                    Counters[persistentCounter.Name] = persistentCounter;
            }
        }

        public IPersistentCounter Find(string name)
        {
            if (Counters.ContainsKey(name))
                return Counters[name];
            var p = ServiceLocator.Instance.GetService<ITableTypeFactory>().NewCounter(name, 0);
            if (CounterTable.Insert(p))
            {
                Counters[name] = p;
                return p;
            }

            LogHelper.Instance.Log("[PersistentCounterService] Unable to create counter {0}", name);
            return null;
        }

        public IPersistentCounter Find(TimeoutCounters counter)
        {
            return Find(KeyFrom(counter));
        }

        public IPersistentCounter Increment(string name)
        {
            try
            {
                var persistentCounter = Find(name);
                if (persistentCounter == null)
                    return null;
                persistentCounter.Increment();
                return CounterTable.Update(persistentCounter) ? persistentCounter : null;
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log(
                    string.Format("FindAndIncrementCounter ( name = {0} ) caught an exception.", name), ex);
                return null;
            }
        }

        public IPersistentCounter Increment(TimeoutCounters counter)
        {
            return Increment(KeyFrom(counter));
        }

        public IPersistentCounter Decrement(string name)
        {
            try
            {
                var persistentCounter = Find(name);
                if (persistentCounter == null)
                    return null;
                persistentCounter.Decrement();
                return CounterTable.Update(persistentCounter) ? persistentCounter : null;
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log(
                    string.Format("[PersistentCounterService] Decrement counter ( {0} ) caught an exception.", name),
                    ex);
                return null;
            }
        }

        public IPersistentCounter Decrement(TimeoutCounters counter)
        {
            return Decrement(KeyFrom(counter));
        }

        public bool Reset(IPersistentCounter counter)
        {
            if (counter == null)
                return false;
            if (counter.Value == 0)
                return true;
            counter.Reset();
            return CounterTable.Update(counter);
        }

        public bool Reset(string name)
        {
            var counter = Find(name);
            return counter != null && Reset(counter);
        }

        public void AddWeeklyResettable(IPersistentCounter counter)
        {
            if (WeeklyResettableCounters.Contains(counter.Name))
                return;
            WeeklyResettableCounters.Add(counter.Name);
        }

        public void ResetWeekly()
        {
            var persistentCounterList = new List<IPersistentCounter>();
            using (new DisposeableList<IPersistentCounter>(persistentCounterList))
            {
                foreach (var resettableCounter in WeeklyResettableCounters)
                {
                    var persistentCounter = Find(resettableCounter);
                    if (persistentCounter != null && persistentCounter.Value != 0)
                    {
                        persistentCounter.Reset();
                        persistentCounterList.Add(persistentCounter);
                    }
                }

                CounterTable.Update(persistentCounterList);
            }
        }

        private string KeyFrom(TimeoutCounters counter)
        {
            return string.Format("{0}Timeout", counter.ToString());
        }
    }
}