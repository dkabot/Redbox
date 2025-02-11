using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;

namespace Redbox.Core
{
    internal class MostRecentlyUsedDictionary<K, V>
    {
        private readonly LinkedList<V> m_list = new LinkedList<V>();
        private readonly HybridDictionary m_linkToKey = new HybridDictionary();
        private readonly Dictionary<K, LinkedListNode<V>> m_dictionary = new Dictionary<K, LinkedListNode<V>>();

        public MostRecentlyUsedDictionary() => this.Capacity = 50;

        public MostRecentlyUsedDictionary(int maxItems)
        {
            this.Capacity = maxItems;
            this.Capacity = 50;
        }

        public V this[K key]
        {
            get
            {
                LinkedListNode<V> node;
                if (!this.m_dictionary.TryGetValue(key, out node))
                    return default(V);
                this.m_list.Remove(node);
                this.m_list.AddFirst(node);
                return node.Value;
            }
            set
            {
                if (this.m_dictionary.ContainsKey(key))
                {
                    LinkedListNode<V> linkedListNode = this.m_dictionary[key];
                    linkedListNode.Value = value;
                    this.m_list.Remove(linkedListNode);
                    this.m_list.AddFirst(linkedListNode);
                    this.m_dictionary[key] = linkedListNode;
                    this.m_linkToKey[(object)linkedListNode] = (object)key;
                }
                else
                    this.Add(key, value);
            }
        }

        public void Add(K key, V value)
        {
            LinkedListNode<V> key1 = this.m_list.AddFirst(value);
            this.m_dictionary.Add(key, key1);
            this.m_linkToKey[(object)key1] = (object)key;
            if (this.m_dictionary.Keys.Count < this.Capacity)
                return;
            LinkedListNode<V> last = this.m_list.Last;
            if (last == null)
                return;
            K key2 = (K)this.m_linkToKey[(object)last];
            if (this.PurgedFromDictionary != null && this.PurgedFromDictionary.GetInvocationList().Length != 0)
                this.PurgedFromDictionary(key2, last.Value);
            this.Remove(key2);
        }

        public bool Contains(K key)
        {
            int num = this.m_dictionary.ContainsKey(key) ? 1 : 0;
            if (num == 0)
                return num != 0;
            LinkedListNode<V> node = this.m_dictionary[key];
            this.m_list.Remove(node);
            this.m_list.AddFirst(node);
            return num != 0;
        }

        public void Remove(K key)
        {
            LinkedListNode<V> linkedListNode;
            if (!this.m_dictionary.TryGetValue(key, out linkedListNode))
                return;
            this.m_dictionary.Remove(key);
            if (linkedListNode == null)
                return;
            this.m_list.Remove(linkedListNode);
            this.m_linkToKey.Remove((object)linkedListNode);
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder(this.Capacity);
            stringBuilder.Append("[");
            foreach (V v in this.m_list)
            {
                if (stringBuilder.Length > 1)
                    stringBuilder.Append(", ");
                stringBuilder.Append(v.ToString());
            }
            stringBuilder.Append("]");
            return stringBuilder.ToString();
        }

        public ICollection<LinkedListNode<V>> Values
        {
            get => (ICollection<LinkedListNode<V>>)this.m_dictionary.Values;
        }

        public ICollection<K> Keys => (ICollection<K>)this.m_dictionary.Keys;

        public int Capacity { get; set; }

        public event PurgedFromDictionaryDelegate<K, V> PurgedFromDictionary;
    }
}
