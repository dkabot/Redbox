using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;

namespace Redbox.Core
{
    public class MostRecentlyUsedDictionary<K, V>
    {
        private readonly Dictionary<K, LinkedListNode<V>> m_dictionary = new Dictionary<K, LinkedListNode<V>>();
        private readonly HybridDictionary m_linkToKey = new HybridDictionary();
        private readonly LinkedList<V> m_list = new LinkedList<V>();

        public MostRecentlyUsedDictionary()
        {
            Capacity = 50;
        }

        public MostRecentlyUsedDictionary(int maxItems)
        {
            Capacity = maxItems;
            Capacity = 50;
        }

        public V this[K key]
        {
            get
            {
                LinkedListNode<V> node;
                if (!m_dictionary.TryGetValue(key, out node))
                    return default;
                m_list.Remove(node);
                m_list.AddFirst(node);
                return node.Value;
            }
            set
            {
                if (m_dictionary.ContainsKey(key))
                {
                    var linkedListNode = m_dictionary[key];
                    linkedListNode.Value = value;
                    m_list.Remove(linkedListNode);
                    m_list.AddFirst(linkedListNode);
                    m_dictionary[key] = linkedListNode;
                    m_linkToKey[linkedListNode] = key;
                }
                else
                {
                    Add(key, value);
                }
            }
        }

        public ICollection<LinkedListNode<V>> Values => m_dictionary.Values;

        public ICollection<K> Keys => m_dictionary.Keys;

        public int Capacity { get; set; }

        public void Add(K key, V value)
        {
            var key1 = m_list.AddFirst(value);
            m_dictionary.Add(key, key1);
            m_linkToKey[key1] = key;
            if (m_dictionary.Keys.Count < Capacity)
                return;
            var last = m_list.Last;
            if (last == null)
                return;
            var key2 = (K)m_linkToKey[last];
            if (PurgedFromDictionary != null && PurgedFromDictionary.GetInvocationList().Length != 0)
                PurgedFromDictionary(key2, last.Value);
            Remove(key2);
        }

        public bool Contains(K key)
        {
            var num = m_dictionary.ContainsKey(key) ? 1 : 0;
            if (num == 0)
                return num != 0;
            var node = m_dictionary[key];
            m_list.Remove(node);
            m_list.AddFirst(node);
            return num != 0;
        }

        public void Remove(K key)
        {
            LinkedListNode<V> linkedListNode;
            if (!m_dictionary.TryGetValue(key, out linkedListNode))
                return;
            m_dictionary.Remove(key);
            if (linkedListNode == null)
                return;
            m_list.Remove(linkedListNode);
            m_linkToKey.Remove(linkedListNode);
        }

        public override string ToString()
        {
            var stringBuilder = new StringBuilder(Capacity);
            stringBuilder.Append("[");
            foreach (var v in m_list)
            {
                if (stringBuilder.Length > 1)
                    stringBuilder.Append(", ");
                stringBuilder.Append(v);
            }

            stringBuilder.Append("]");
            return stringBuilder.ToString();
        }

        public event PurgedFromDictionaryDelegate<K, V> PurgedFromDictionary;
    }
}