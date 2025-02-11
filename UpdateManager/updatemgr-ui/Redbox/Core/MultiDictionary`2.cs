using System;
using System.Collections;
using System.Collections.Generic;

namespace Redbox.Core
{
    internal class MultiDictionary<K, V> :
      IDictionary<K, ICollection<V>>,
      ICollection<KeyValuePair<K, ICollection<V>>>,
      IEnumerable<KeyValuePair<K, ICollection<V>>>,
      IEnumerable
    {
        private readonly Dictionary<K, ICollection<V>> m_dict;

        public MultiDictionary()
          : this((IEqualityComparer<K>)EqualityComparer<K>.Default, (IEqualityComparer<V>)EqualityComparer<V>.Default)
        {
        }

        public MultiDictionary(IEqualityComparer<K> keyCompare)
          : this(keyCompare, (IEqualityComparer<V>)EqualityComparer<V>.Default)
        {
        }

        public MultiDictionary(IEqualityComparer<K> keyCompare, IEqualityComparer<V> valueCompare)
        {
            this.m_dict = new Dictionary<K, ICollection<V>>(keyCompare);
        }

        public IEnumerator<KeyValuePair<K, ICollection<V>>> GetEnumerator()
        {
            return (IEnumerator<KeyValuePair<K, ICollection<V>>>)this.m_dict.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => (IEnumerator)this.GetEnumerator();

        public void Add(K key, V value)
        {
            if (!this.m_dict.ContainsKey(key))
                this.m_dict[key] = (ICollection<V>)new HashSet<V>();
            this.m_dict[key].Add(value);
        }

        public void AddMany(K key, ICollection<V> values)
        {
            if (!this.m_dict.ContainsKey(key))
                this.m_dict[key] = (ICollection<V>)new HashSet<V>();
            foreach (V v in (IEnumerable<V>)values)
                this.m_dict[key].Add(v);
        }

        public void Add(K key, ICollection<V> values) => this.AddMany(key, values);

        void ICollection<KeyValuePair<K, ICollection<V>>>.Add(KeyValuePair<K, ICollection<V>> item)
        {
            this.Add(item.Key, item.Value);
        }

        public void Clear() => this.m_dict.Clear();

        bool ICollection<KeyValuePair<K, ICollection<V>>>.Contains(KeyValuePair<K, ICollection<V>> item)
        {
            return this.m_dict.ContainsKey(item.Key);
        }

        public void CopyTo(KeyValuePair<K, ICollection<V>>[] array, int arrayIndex)
        {
            if (array.Length - arrayIndex < this.m_dict.Count)
                throw new ArgumentException("array.Length is too small.", nameof(array));
            int index = arrayIndex;
            foreach (KeyValuePair<K, ICollection<V>> keyValuePair in this.m_dict)
            {
                array[index] = keyValuePair;
                ++index;
            }
        }

        bool ICollection<KeyValuePair<K, ICollection<V>>>.Remove(KeyValuePair<K, ICollection<V>> item)
        {
            return this.m_dict.Remove(item.Key);
        }

        public int Count => this.m_dict.Count;

        bool ICollection<KeyValuePair<K, ICollection<V>>>.IsReadOnly => false;

        public bool ContainsKey(K key) => this.m_dict.ContainsKey(key);

        public bool Remove(K key) => this.m_dict.Remove(key);

        public bool TryGetValue(K key, out ICollection<V> value)
        {
            return this.m_dict.TryGetValue(key, out value);
        }

        public ICollection<V> this[K key]
        {
            get => (ICollection<V>)new HashSet<V>((IEnumerable<V>)this.m_dict[key]);
            set => this.m_dict[key] = (ICollection<V>)new HashSet<V>((IEnumerable<V>)value);
        }

        public ICollection<K> Keys => (ICollection<K>)this.m_dict.Keys;

        public ICollection<ICollection<V>> Values => (ICollection<ICollection<V>>)this.m_dict.Values;
    }
}
