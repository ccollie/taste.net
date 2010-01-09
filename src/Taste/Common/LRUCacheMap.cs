using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Taste.Common
{

    public interface ICache<K, V> 
    {
        V Get(K obj);
        void Put(K key, V obj);
        void Put(K key, V obj, long secs);
        bool Remove(K key);
        KeyValuePair<K, V>[] GetAll();
        int Count { get;}
    }


    /// <summary>
    /// Simple LRU Caching dictionary with a configurable max size and
    /// expirable items.
    /// NOT threadsafe
    /// </summary>
    /// <typeparam name="K">Key type</typeparam>
    /// <typeparam name="V">Value type</typeparam>
    [Serializable]
    public class LRUCacheMap<K, V> : ICache<K, V> 
    {
        IDictionary<K, LinkedListNode<CacheItem>> m_map = null;
        int m_maxSize;
        LinkedList<CacheItem> m_list = new LinkedList<CacheItem>();

        public const int NO_MAX_SIZE = int.MaxValue;

        #region Internal Helper Methods
        internal class CacheItem
        {
            public CacheItem(K k, V v, DateTime? e)
            {
                key = k;
                value = v;
                expiration = e;
            }
            public CacheItem() { }
            public K key;
            public V value;
            public DateTime? expiration = null;
        }

        internal void RemoveItem(LinkedListNode<CacheItem> item)
        {
            m_list.Remove(item);
        }

        internal void InsertHead(LinkedListNode<CacheItem> item)
        {
            m_list.AddFirst(item);
        }

        internal void MoveToHead(LinkedListNode<CacheItem> item)
        {
            // remove item from the list
            m_list.Remove(item);
            // jump to the head of the line
            m_list.AddFirst(item);
        }
        #endregion

        public LRUCacheMap()
            :this(NO_MAX_SIZE)
        {
        }

        public LRUCacheMap(int maxObjects)
        {
            m_map = new Dictionary<K, LinkedListNode<CacheItem>>();
            m_maxSize = maxObjects;
        }


        public LRUCacheMap(int initSize, int maxObjects)
        {
            m_map = new SafeDictionary<K, LinkedListNode<CacheItem>>(initSize);
            m_maxSize = maxObjects;
        }

        //ugly
        public KeyValuePair<K, V>[] GetAll() 
	    {
		    KeyValuePair<K,V>[] p = new KeyValuePair<K,V>[m_map.Count];		  
            int count = 0;
            foreach(LinkedListNode<CacheItem> n in m_map.Values)	
            {
                CacheItem cur = n.Value;
                p[count++] = new KeyValuePair<K,V>(cur.key, cur.value);
            }
		    return p;
	    }

        private LinkedListNode<CacheItem> MapGet(K key)
        {
            LinkedListNode<CacheItem> value;
            if (m_map.TryGetValue(key, out value))
                return value;
            return null;
        }

        private LinkedListNode<CacheItem> GetNode(K key)
        {
            LinkedListNode<CacheItem> node;
            if (!m_map.TryGetValue(key, out node))
                return null;

            CacheItem cur = node.Value;

            if (cur.expiration.HasValue && (DateTime.Now > cur.expiration.Value))
            {
                m_map.Remove(key);
                RemoveItem(node);
                return null;
            }
            return node;
        }


        public bool ContainsKey(K key)
        {
            LinkedListNode<CacheItem> item = GetNode(key);
            return (item != null);
        }

        /// <summary>
        /// Here for completeness
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool ContainsValue(V value)
        {
            foreach (CacheItem item in m_list)
            {
                if (item.value.Equals(value))
                    return true;
            }
            return false;
        }

        public bool TryGetValue(K key, out V value)
        {
            LinkedListNode<CacheItem> item = GetNode(key);
            if (item != null)
            {
                value = item.Value.value;
                return true;
            }
            value = default(V);
            return false;
        }

        public V Get(K key)
        {
            LinkedListNode<CacheItem> node = GetNode(key);

            if (node == null)
            {
                return default(V);
            }
            if (node != m_list.First)
            {
                MoveToHead(node);
            }
            return (V)node.Value.value;
        }

        public void Put(K key, V obj)
        {
            Put(key, obj, -1);
        }

        public void Add(K key, V obj)
        {
            Put(key, obj, -1);
        }

        public void Put(K key, V value, long secs)
        {
            if (key == null)
                throw new ArgumentNullException("key cannot be null.");

            if (value == null)
                throw new ArgumentNullException("Value cannot be null.");

            LinkedListNode<CacheItem> node = MapGet(key);
            if (node != null)
            {
                CacheItem cur = node.Value;

                cur.value = value;
                if (secs > 0)
                {
                    cur.expiration = DateTime.Now.AddSeconds(secs);
                }
                else
                {
                    cur.expiration = null;
                }
                MoveToHead(node);
                return;
            }
            if (m_map.Count >= m_maxSize)
            {
                LinkedListNode<CacheItem> last = m_list.Last;
                if (last != null)
                {
                    m_map.Remove(last.Value.key);
                    m_list.RemoveLast();
                }
            }
            DateTime? expires = null;
            if (secs > 0)
            {
                expires = DateTime.Now.AddSeconds(secs);
            }
            CacheItem item = new CacheItem(key, value, expires);
            node = new LinkedListNode<CacheItem>(item);
            InsertHead(node);
            m_map.Add(key, node);
        }

        public V this[K index]
        {
            get
            {
                return Get(index);
            }
            set
            {
                Put(index, value);
            }
        }

        public ICollection<K> Keys
        {
            get { return m_map.Keys; }
        }

        public ICollection<V> Values
        {
            get
            {
                List<V> values = new List<V>(m_list.Count);
                foreach(CacheItem item in m_list)
                {
                    values.Add(item.value);
                }
                return values;
            }
        }

        public bool Remove(K key)
        {
            LinkedListNode<CacheItem> cur = MapGet(key);
            if (cur == null)
            {
                return false;
            }
            bool removed = m_map.Remove(cur.Value.key);
            RemoveItem(cur);
            return removed;
        }

        public void Clear()
        {
            m_map.Clear();
            m_list.Clear();
        }

        public int Count
        {
            get { return m_map.Count; }
        }

        public bool IsEmpty
        {
            get { return Count == 0; }
        }

    }
}
