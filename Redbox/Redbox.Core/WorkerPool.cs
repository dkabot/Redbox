using System;
using System.Collections.Generic;
using System.Threading;

namespace Redbox.Core
{
    public class WorkerPool
    {
        private readonly object m_syncObject = new object();
        private readonly Queue<WorkItemEntry> m_workItems = new Queue<WorkItemEntry>();
        private volatile bool m_isRunning;
        private int m_runningThreadCount;

        public WorkerPool(int minThreads, int maxThreads)
            : this(minThreads, maxThreads, "IPC Worker Pool")
        {
        }

        public WorkerPool(int minThreads, int maxThreads, string poolName)
        {
            MinimumThreads = minThreads;
            MaximumThreads = maxThreads;
            PoolName = poolName;
        }

        public string PoolName { get; }

        public int MinimumThreads { get; private set; }

        public int MaximumThreads { get; }

        public void Start()
        {
            m_workItems.Clear();
            m_isRunning = true;
            for (var index = 0; index < MaximumThreads; ++index)
            {
                Thread thread;
                if (string.IsNullOrEmpty(PoolName))
                    thread = new Thread(ItemWorker)
                    {
                        IsBackground = true
                    };
                else
                    thread = new Thread(ItemWorker)
                    {
                        Name = string.Format("{0} #{1}", PoolName, index),
                        IsBackground = true
                    };
                thread.Start();
            }
        }

        public void Shutdown()
        {
            lock (m_syncObject)
            {
                m_isRunning = false;
                Monitor.PulseAll(m_syncObject);
            }
        }

        public bool QueueWorkItem(WorkItem item)
        {
            return QueueWorkItem(item, null);
        }

        public bool QueueWorkItem(WorkItem item, object state)
        {
            lock (m_syncObject)
            {
                if (!m_isRunning || item == null || m_runningThreadCount + 1 > MaximumThreads)
                    return false;
                m_workItems.Enqueue(new WorkItemEntry
                {
                    Item = item,
                    State = state
                });
                Monitor.PulseAll(m_syncObject);
                return true;
            }
        }

        internal WorkItemEntry GetWorkItem()
        {
            lock (m_syncObject)
            {
                while (m_workItems.Count == 0)
                {
                    if (!m_isRunning)
                        return null;
                    Monitor.Wait(m_syncObject);
                }

                return m_workItems.Dequeue();
            }
        }

        private void ItemWorker()
        {
            while (m_isRunning)
                try
                {
                    var workItem = GetWorkItem();
                    if (workItem != null)
                        try
                        {
                            Interlocked.Increment(ref m_runningThreadCount);
                            workItem.Execute();
                        }
                        finally
                        {
                            Interlocked.Decrement(ref m_runningThreadCount);
                        }
                }
                catch (Exception ex)
                {
                    LogHelper.Instance.Log("An unhandled exception was raised in WorkerPool.ItemWorker.", ex);
                }
        }

        internal sealed class WorkItemEntry
        {
            public WorkItem Item;
            public object State;

            public void Execute()
            {
                if (Item == null)
                    return;
                Item(State);
            }
        }
    }
}