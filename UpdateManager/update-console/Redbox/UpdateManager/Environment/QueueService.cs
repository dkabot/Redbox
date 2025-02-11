using Redbox.Core;
using Redbox.UpdateManager.ComponentModel;
using System;
using System.IO;
using System.Reflection;

namespace Redbox.UpdateManager.Environment
{
    internal class QueueService : IQueueService
    {
        private string m_root;
        private PersistentList<QueueService.QueueEntry> m_queueFile;
        private readonly object m_queueLock = new object();

        public static QueueService Instance => Singleton<QueueService>.Instance;

        public void Initialize(string path)
        {
            string str = ServiceLocator.Instance.GetService<IMacroService>().ExpandProperties(path);
            if (!Path.IsPathRooted(str))
                str = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), str);
            this.m_root = Path.Combine(str, ".queue");
            this.m_queueFile = new PersistentList<QueueService.QueueEntry>(this.m_root);
            ServiceLocator.Instance.AddService(typeof(IQueueService), (object)this);
        }

        public string PeekRaw(string label)
        {
            foreach (QueueService.QueueEntry queueEntry in this.m_queueFile)
            {
                if (queueEntry != null && string.Compare(queueEntry.Label, label, true) == 0)
                    return queueEntry.Entry;
            }
            return (string)null;
        }

        public T Peek<T>(string label)
        {
            string json = this.PeekRaw(label);
            return json != null ? json.ToObject<T>() : default(T);
        }

        public void Enqueue(string label, object entry)
        {
            this.Write(new QueueService.QueueEntry()
            {
                Label = label,
                Entry = entry.ToJson()
            });
        }

        public void EnqueueRaw(string label, string entry)
        {
            this.Write(new QueueService.QueueEntry()
            {
                Label = label,
                Entry = entry
            });
        }

        public void Dequeue(string label)
        {
            int index = this.m_queueFile.AsList().FindIndex((Predicate<QueueService.QueueEntry>)(each => each != null && string.Compare(each.Label, label, true) == 0));
            if (index == -1)
                return;
            this.m_queueFile.RemoveAt(index);
        }

        private QueueService()
        {
        }

        private void Write(QueueService.QueueEntry entry)
        {
            lock (this.m_queueLock)
                this.m_queueFile.Add(entry);
        }

        private class QueueEntry
        {
            public string Label { get; set; }

            public string Entry { get; set; }
        }
    }
}
