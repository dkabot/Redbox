using System;
using System.Collections.Generic;
using System.Threading;

namespace Redbox.Core
{
    internal class WorkerPool
    {
        private int m_runningThreadCount;
        private volatile bool m_isRunning;
        private readonly object m_syncObject = new object();
        private readonly Queue<WorkerPool.WorkItemEntry> m_workItems = new Queue<WorkerPool.WorkItemEntry>();

        public WorkerPool(int minThreads, int maxThreads)
          : this(minThreads, maxThreads, "IPC Worker Pool")
        {
        }

        public WorkerPool(int minThreads, int maxThreads, string poolName)
        {
            this.MinimumThreads = minThreads;
            this.MaximumThreads = maxThreads;
            this.PoolName = poolName;
        }

        public void Start()
        {
            this.m_workItems.Clear();
            this.m_isRunning = true;
            for (int index = 0; index < this.MaximumThreads; ++index)
            {
                Thread thread;
                if (string.IsNullOrEmpty(this.PoolName))
                    thread = new Thread(new ThreadStart(this.ItemWorker))
                    {
                        IsBackground = true
                    };
                else
                    thread = new Thread(new ThreadStart(this.ItemWorker))
                    {
                        Name = string.Format("{0} #{1}", (object)this.PoolName, (object)index),
                        IsBackground = true
                    };
                thread.Start();
            }
        }

        public void Shutdown()
        {
            lock (this.m_syncObject)
            {
                this.m_isRunning = false;
                Monitor.PulseAll(this.m_syncObject);
            }
        }

        public bool QueueWorkItem(WorkItem item) => this.QueueWorkItem(item, (object)null);

        public bool QueueWorkItem(WorkItem item, object state)
        {
            lock (this.m_syncObject)
            {
                if (!this.m_isRunning || item == null || this.m_runningThreadCount + 1 > this.MaximumThreads)
                    return false;
                this.m_workItems.Enqueue(new WorkerPool.WorkItemEntry()
                {
                    Item = item,
                    State = state
                });
                Monitor.PulseAll(this.m_syncObject);
                return true;
            }
        }

        public string PoolName { get; private set; }

        public int MinimumThreads { get; private set; }

        public int MaximumThreads { get; private set; }

        internal WorkerPool.WorkItemEntry GetWorkItem()
        {
            lock (this.m_syncObject)
            {
                while (this.m_workItems.Count == 0)
                {
                    if (!this.m_isRunning)
                        return (WorkerPool.WorkItemEntry)null;
                    Monitor.Wait(this.m_syncObject);
                }
                return this.m_workItems.Dequeue();
            }
        }

        private void ItemWorker()
        {
            while (this.m_isRunning)
            {
                try
                {
                    WorkerPool.WorkItemEntry workItem = this.GetWorkItem();
                    if (workItem != null)
                    {
                        try
                        {
                            Interlocked.Increment(ref this.m_runningThreadCount);
                            workItem.Execute();
                        }
                        finally
                        {
                            Interlocked.Decrement(ref this.m_runningThreadCount);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.Instance.Log("An unhandled exception was raised in WorkerPool.ItemWorker.", ex);
                }
            }
        }

        internal sealed class WorkItemEntry
        {
            public WorkItem Item;
            public object State;

            public void Execute()
            {
                if (this.Item == null)
                    return;
                this.Item(this.State);
            }
        }
    }
}
