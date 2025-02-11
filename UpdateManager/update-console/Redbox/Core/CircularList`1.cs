using System;
using System.Collections.Generic;

namespace Redbox.Core
{
    internal class CircularList<T>
    {
        private List<T> m_items;

        public CircularList() => this.Reset();

        public CircularList(IEnumerable<T> items)
        {
            this.AddRange(items);
            this.Reset();
        }

        public void AddRange(IEnumerable<T> items)
        {
            this.Items.AddRange(items);
            this.Reset();
        }

        public void Add(T item)
        {
            this.Items.Add(item);
            this.Reset();
        }

        public void Remove(T item)
        {
            this.Items.Remove(item);
            this.Reset();
        }

        public void RemoveAt(int index)
        {
            this.Items.RemoveAt(index);
            this.Reset();
        }

        public void Insert(int index, T item)
        {
            this.Items.Insert(index, item);
            this.Reset();
        }

        public void Clear()
        {
            this.Items.Clear();
            this.Reset();
        }

        public void Reset() => this.CurrentIndex = -1;

        public T Next()
        {
            if (this.Items.Count == 0)
                return default(T);
            ++this.CurrentIndex;
            if (this.CurrentIndex >= this.Items.Count)
                this.CurrentIndex = 0;
            return this.Items[this.CurrentIndex];
        }

        public int Count => this.Items.Count;

        public T this[int index]
        {
            get => this.Items[index];
            set => this.Items[index] = value;
        }

        public T Find(Predicate<T> match) => this.Items.Find(match);

        internal List<T> Items
        {
            get
            {
                if (this.m_items == null)
                    this.m_items = new List<T>();
                return this.m_items;
            }
        }

        internal int CurrentIndex { get; set; }
    }
}
