using System;
using System.Collections.Generic;
using System.Threading;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Threading;

namespace Redbox.HAL.Script.Framework
{
    internal sealed class ExecutionContextList
    {
        private readonly ReaderWriterLockSlim Lock = new ReaderWriterLockSlim();
        private readonly List<ExecutionContext> m_list = new List<ExecutionContext>();

        public ExecutionContext GetByID(string id)
        {
            using (new WithReadLock(Lock))
            {
                return m_list.Find(each => string.Compare(id, each.ID, false) == 0);
            }
        }

        public ExecutionContext[] GetByLabel(string label)
        {
            using (new WithReadLock(Lock))
            {
                var all = m_list.FindAll(each => string.Compare(each.Label, label, true) == 0);
                using (new DisposeableList<ExecutionContext>(all))
                {
                    return all.ToArray();
                }
            }
        }

        public void ForEach(Action<ExecutionContext> action)
        {
            using (new WithReadLock(Lock))
            {
                m_list.ForEach(action);
            }
        }

        public ExecutionContext[] GetByName(string name)
        {
            var res = new List<ExecutionContext>();
            using (new DisposeableList<ExecutionContext>(res))
            {
                using (new WithReadLock(Lock))
                {
                    m_list.ForEach(each =>
                    {
                        if (!each.ProgramName.Equals(name))
                            return;
                        res.Add(each);
                    });
                    return res.ToArray();
                }
            }
        }

        public int CleanupGarbage(bool force)
        {
            var cleaned = 0;
            using (new WithWriteLock(Lock))
            {
                var all = m_list.FindAll(each =>
                    (force && each.IsComplete) || each.Status == ExecutionContextStatus.Garbage);
                using (new DisposeableList<ExecutionContext>(all))
                {
                    all.ForEach(g =>
                    {
                        if (g.IsImmediate)
                            return;
                        g.Dispose();
                        m_list.Remove(g);
                        ++cleaned;
                    });
                }
            }

            return cleaned;
        }

        public ExecutionContext GetEligibleExecutionContext()
        {
            using (new WithReadLock(Lock))
            {
                var all = m_list.FindAll(context => context.EligibleForScheduling);
                all.Sort((x, y) =>
                {
                    var num = x.Priority.CompareTo(y.Priority);
                    return num != 0 ? num : x.CreatedOn.CompareTo(y.CreatedOn);
                });
                foreach (var executionContext in all)
                    if (!executionContext.StartTime.HasValue || DateTime.Now.CompareTo(executionContext.StartTime) >= 0)
                        return executionContext;
                return null;
            }
        }

        public void Add(ExecutionContext item)
        {
            using (new WithWriteLock(Lock))
            {
                m_list.Add(item);
            }
        }

        public ExecutionContext Find(Predicate<ExecutionContext> predicate)
        {
            using (new WithReadLock(Lock))
            {
                return m_list.Find(predicate);
            }
        }

        public List<ExecutionContext> FindAll(Predicate<ExecutionContext> predicate)
        {
            using (new WithReadLock(Lock))
            {
                return m_list.FindAll(predicate);
            }
        }
    }
}