using System;
using System.Collections;
using TaskSchedulerInterop;

namespace TaskScheduler
{
    internal class TriggerList : IList, ICollection, IEnumerable, IDisposable
    {
        private ITask iTask;
        private ArrayList oTriggers;

        internal TriggerList(ITask iTask)
        {
            this.iTask = iTask;
            ushort Count = 0;
            iTask.GetTriggerCount(out Count);
            this.oTriggers = new ArrayList((int)Count + 5);

            for (int TriggerIndex = 0; TriggerIndex < (int)Count; ++TriggerIndex)
            {
                ITaskTrigger trigger;
                iTask.GetTrigger((ushort)TriggerIndex, out trigger);
                ushort newIndex;
                iTask.CreateTrigger(out newIndex, out trigger);
                this.oTriggers.Add(trigger);
            }
        }

        public void RemoveAt(int index)
        {
            if (index >= this.Count)
                throw new ArgumentOutOfRangeException(nameof(index), (object)index, "Failed to remove Trigger. Index out of range.");
            ((Trigger)this.oTriggers[index]).Unbind();
            this.oTriggers.RemoveAt(index);
            this.iTask.DeleteTrigger((ushort)index);
        }

        void IList.Insert(int index, object value)
        {
            throw new NotImplementedException("TriggerList does not support Insert().");
        }

        public void Remove(Trigger trigger)
        {
            int index = this.IndexOf(trigger);
            if (index == -1)
                return;
            this.RemoveAt(index);
        }

        void IList.Remove(object value) => this.Remove(value as Trigger);

        public bool Contains(Trigger trigger) => this.IndexOf(trigger) != -1;

        bool IList.Contains(object value) => this.Contains(value as Trigger);

        public void Clear()
        {
            for (int index = this.Count - 1; index >= 0; --index)
                this.RemoveAt(index);
        }

        public int IndexOf(Trigger trigger)
        {
            for (int index = 0; index < this.Count; ++index)
            {
                if (this[index].Equals((object)trigger))
                    return index;
            }
            return -1;
        }

        int IList.IndexOf(object value) => this.IndexOf(value as Trigger);

        public int Add(Trigger trigger)
        {
            if (trigger.Bound)
                throw new ArgumentException("A Trigger cannot be added if it is already in a list.");
            ushort NewTriggerIndex;
            ITaskTrigger Trigger;
            this.iTask.CreateTrigger(out NewTriggerIndex, out Trigger);
            trigger.Bind(Trigger);
            if (this.oTriggers.Add((object)trigger) != (int)NewTriggerIndex)
                throw new ApplicationException("Assertion Failure");
            return (int)NewTriggerIndex;
        }

        int IList.Add(object value) => this.Add(value as Trigger);

        public bool IsReadOnly => false;

        public Trigger this[int index]
        {
            get
            {
                return index < this.Count ? (Trigger)this.oTriggers[index] : throw new ArgumentOutOfRangeException(nameof(index), (object)index, "TriggerList collection");
            }
            set
            {
                Trigger trigger = index < this.Count ? (Trigger)this.oTriggers[index] : throw new ArgumentOutOfRangeException(nameof(index), (object)index, "TriggerList collection");
                value.Bind(trigger);
                this.oTriggers[index] = (object)value;
            }
        }

        object IList.this[int index]
        {
            get => (object)this[index];
            set => this[index] = value as Trigger;
        }

        public bool IsFixedSize => false;

        public int Count => this.oTriggers.Count;

        public void CopyTo(Array array, int index)
        {
            if (this.oTriggers.Count > array.Length - index)
                throw new ArgumentException("Array has insufficient space to copy the collection.");
            for (int index1 = 0; index1 < this.oTriggers.Count; ++index1)
                array.SetValue(((Trigger)this.oTriggers[index1]).Clone(), index + index1);
        }

        public bool IsSynchronized => false;

        public object SyncRoot => (object)null;

        public IEnumerator GetEnumerator() => (IEnumerator)new TriggerList.Enumerator(this);

        public void Dispose()
        {
            foreach (Trigger oTrigger in this.oTriggers)
                oTrigger.Unbind();
            this.oTriggers = (ArrayList)null;
            this.iTask = (ITask)null;
        }

        private class Enumerator : IEnumerator
        {
            private TriggerList outer;
            private int currentIndex;

            internal Enumerator(TriggerList outer)
            {
                this.outer = outer;
                this.Reset();
            }

            public bool MoveNext() => ++this.currentIndex < this.outer.oTriggers.Count;

            public void Reset() => this.currentIndex = -1;

            public object Current => this.outer.oTriggers[this.currentIndex];
        }
    }
}
