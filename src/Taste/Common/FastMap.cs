/*
 * Copyright 2007 and onwards, Sean Owen
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

namespace Taste.Common
{
    using System;
    using System.Diagnostics;
    using System.Collections;
    using System.Collections.Generic;

    /**
     * <p>This is an optimized {@link Map} implementation, based on algorithms described in Knuth's
     * "Art of Computer Programming", Vol. 3, p. 529.</p>
     *
     * <p>It should be faster than {@link java.util.HashMap} in some cases, but not all. Its main feature is
     * a "max size" and the ability to transparently, efficiently and semi-intelligently evict old entries
     * when max size is exceeded.</p>
     *
     * <p>This is still a work in progress and I'm putting it in CVS so other devs can look at it.
     * I wouldn't go using it too much yet.</p>
     *
     * <p>This class is not a bit thread-safe.</p>
     *
     * <p>This implementation does not allow <code>null</code> as a key or value.</p>
     *
     * @author Sean Owen
     * @since 1.5.3
     */
    public class FastMap<K, V> : 
        IDictionary<K, V>, 
        IEnumerable<KeyValuePair<K,V>>,
        ICollection<KeyValuePair<K,V>>
    {

        /** The largest prime less than 2<sup>31</sup>-1 that is the smaller of a twin prime pair. */
        private const int MAX_INT_SMALLER_TWIN_PRIME = 2147482949;

        public const int NO_MAX_SIZE = int.MaxValue;

        #region Private Member Variables
        /**
         * Dummy object used to represent a key that has been removed. Package-private to allow direct access
         * by inner classes. No harm in exposing it.
         */
        private static readonly K REMOVED = default(K);
        private static readonly V NULL_VALUE = default(V);
        private K[] keys;
        private V[] values;
        private int hashSize;
        private int numEntries;
        private int numSlotsUsed;
        private ICollection<K> keyCollection;
        private ICollection<V> valueCollection;
        private int maxSize;
        private BitArray recentlyAccessed;
        private int generation = 0;

        #endregion

        #region Constructor

        /**
         * Creates a new {@link FastMap} with default capacity.
         */
        public FastMap() : this(11, NO_MAX_SIZE)
        {            
        }

        /**
         * Creates a new {@link FastMap} whose capacity can accommodate the given number of entries without rehash.</p>
         *
         * @param size desired capacity
         * @param maxSize max capacity
         * @throws ArgumentException if size is less than 1 or at least half of {@link #MAX_INT_SMALLER_TWIN_PRIME}
         */
        public FastMap(int size, int maxSize)
        {
            if (size < 1)
            {
                throw new ArgumentException("size must be at least 1");
            }
            if (size >= MAX_INT_SMALLER_TWIN_PRIME / 2)
            {
                throw new ArgumentException("size must be less than " + MAX_INT_SMALLER_TWIN_PRIME / 2);
            }
            if (maxSize < 1)
            {
                throw new ArgumentException("maxSize must be at least 1");
            }
            hashSize = NextTwinPrime(2 * size);
            keys = new K[hashSize];
            values = new V[hashSize];
            this.maxSize = maxSize;
            this.recentlyAccessed = (maxSize == int.MaxValue) ? null : new BitArray(maxSize);
        }

        #endregion

        private int Find(Object key)
        {
            int theHashCode = key.GetHashCode() & 0x7FFFFFFF; // make sure it's positive
            int jump = 1 + theHashCode % (hashSize - 2);
            int index = theHashCode % hashSize;
            K currentKey = keys[index];
            while (currentKey != null && (currentKey.Equals(REMOVED) || !key.Equals(currentKey)))
            {
                if (index < jump)
                {
                    index += hashSize - jump;
                }
                else
                {
                    index -= jump;
                }
                currentKey = keys[index];
            }
            //Debug.Assert(currentKey == null || key.Equals(currentKey));
            Debug.WriteIf(index < 0, index);
            return index;
        }

        public V get(Object key)
        {
            if (key == null)
            {
                return default(V);
            }
            int index = Find(key);
            if (recentlyAccessed != null)
            {
                recentlyAccessed[index] = true;
            }
            return values[index];
        }

        public bool TryGetValue(K key, out V value)
        {
            if (key == null)
                throw new ArgumentNullException("key");

            int index = Find(key);
            //cc - review
            if (index >= 0) // added by cc
            {
                if (recentlyAccessed != null && index < recentlyAccessed.Length)
                {
                    recentlyAccessed.Set(index, true);
                }
                if (values[index] != null)
                {
                    value = values[index];
                    return true;
                }
            }

            // we did not find the slot
            value = default(V);
            return false;
        }

        public V this[K key]
        {
            get { return get((object)key); }
            set
            {
                put(key, value);
            }
        }

        public int Count
        {
            get { return numEntries; }
        }

        public bool IsEmpty
        {
            get { return numEntries == 0; }
        }

        public bool ContainsKey(K key)
        {
            return key != null && keys[Find(key)] != null;
        }

        public bool ContainsValue(V value)
        {
            if (value == null)
            {
                return false;
            }
            foreach (V theValue in values)
            {
                if (theValue != null && value.Equals(theValue))
                {
                    return true;
                }
            }
            return false;
        }

        /**
         * @throws ArgumentNullException if key or value is null
         */
        public void Add(K key, V value)
        {
            if (key == null || value == null)
            {
                throw new ArgumentNullException();
            }
            Debug.Assert(numSlotsUsed <= hashSize / 2);

            if (numSlotsUsed >= hashSize / 2)
            {
                GrowAndRehash();
            }
            // Here we may later consider implementing Brent's variation described on page 532
            int index = Find(key);
            if (keys[index] == null)
            {
                // If size is limited,
                if (recentlyAccessed != null && numEntries >= maxSize)
                {
                    // and we're too large, clear some old-ish entry
                    ClearStaleEntry(index);
                }
                Debug.Assert(numEntries < maxSize);
                keys[index] = key;
                Debug.Assert(values[index] == null);
                values[index] = value;
                numEntries++;
                numSlotsUsed++;
                generation++;
            }
            else
            {
                throw new ArgumentException("The key already exists in the collection");
            }
        }

        /**
         * @throws ArgumentNullException if key or value is null
         */
        public V put(K key, V value) 
        {
		    if (key == null || value == null) 
            {
			    throw new ArgumentNullException();
		    }
		    Debug.Assert(numSlotsUsed <= hashSize / 2);

		    if (numSlotsUsed >= hashSize / 2) 
            {
			    GrowAndRehash();
		    }
		    // Here we may later consider implementing Brent's variation described on page 532
		    int index = Find(key);
		    if (keys[index] == null) 
            {
			    // If size is limited,
			    if (recentlyAccessed != null && numEntries >= maxSize) 
                {
				    // and we're too large, clear some old-ish entry
				    ClearStaleEntry(index);
			    }
			    Debug.Assert(numEntries < maxSize);
			    keys[index] = key;
			    Debug.Assert(values[index] == null);
			    values[index] = value;
			    numEntries++;
			    numSlotsUsed++;
                generation++;
			    return value;
		    } else {
			    V oldValue = values[index];
			    Debug.Assert(oldValue != null);
			    values[index] = value;
                generation++;
			    return oldValue;
		    }
	    }

        private void ClearStaleEntry(int index)
        {
            Debug.Assert(keys[index] == null);
            while (true)
            {
                K currentKey;
                do
                {
                    Debug.Assert(index >= 0);
                    if (index == 0)
                    {
                        index = hashSize - 1;
                    }
                    else
                    {
                        index--;
                    }
                    currentKey = keys[index];
                } while (currentKey == null || currentKey.Equals(REMOVED));
                Debug.Assert(recentlyAccessed != null);
                if (recentlyAccessed[index])
                {
                    recentlyAccessed[index] = false;
                }
                else
                {
                    break;
                }
            }
            // Delete the entry
            keys[index] = REMOVED;
            numEntries--;
            Debug.Assert(numEntries >= 0);
            Debug.Assert(values[index] != null);
            values[index] = NULL_VALUE;
        }

        public void AddAll(IDictionary<K, V> map)
        {
            if (map == null)
            {
                throw new ArgumentNullException();
            }
            foreach (KeyValuePair<K, V> entry in map)
            {
                Add(entry.Key, entry.Value);
            }
        }

        public bool Remove(K key)
        {
            if (key == null)
            {
                return false;
            }
            int index = Find(key);
            if (keys[index] == null)
            {
                return false;
            }
            else
            {
                keys[index] = REMOVED;
                numEntries--;
                Debug.Assert(numEntries >= 0);
                V oldValue = values[index];
                Debug.Assert(oldValue != null);
                values[index] = NULL_VALUE;
                // don't decrement numSlotsUsed
                generation++;
                return true;
            }
            // Could un-set recentlyAccessed's bit but doesn't matter
        }

        public void Clear()
        {
            numEntries = 0;
            numSlotsUsed = 0;
            Array.Clear(keys, 0, keys.Length);
            Array.Clear(values, 0, values.Length);
            if (recentlyAccessed != null)
            {
                recentlyAccessed.SetAll(false); 
                //recentlyAccessed.Length = 0;
            }
            generation++;
        }

        public ICollection<K> Keys
        {
            get
            {
                if (keyCollection == null)
                {
                    keyCollection = new KeyCollection(this);
                }
                return keyCollection;
            }
        }

        public ICollection<V> Values
        {
            get
            {
                if (valueCollection == null)
                {
                    valueCollection = new ValueCollection(this);
                }
                return valueCollection;
            }
        }

        #region Hashing

        public void Rehash()
        {
            Rehash(hashSize);
        }

        private void GrowAndRehash()
        {
            if (hashSize >= MAX_INT_SMALLER_TWIN_PRIME >> 1)
            {
                throw new IllegalStateException("Can't grow any more");
            }
            Rehash(NextTwinPrime(2 * hashSize));
        }

        private void Rehash(int newHashSize)
        {
            K[] oldKeys = keys;
            V[] oldValues = values;
            numEntries = 0;
            numSlotsUsed = 0;
            if (recentlyAccessed != null)
            {
                recentlyAccessed.SetAll(false);
            }
            hashSize = newHashSize;
            keys = new K[hashSize];
            values = new V[hashSize];
            int length = oldKeys.Length;
            for (int i = 0; i < length; i++)
            {
                K key = oldKeys[i];
                if (key != null && !key.Equals(REMOVED))
                {
                    Add(key, oldValues[i]);
                }
            }
        }
        #endregion

        #region Iteration Helpers

        private void GotoNext(ref int position)
        {
            int length = values.Length;
            while (position < length && values[position] == null)
            {
                position++;
            }
        }

        private bool HasNext(ref int position)
        {
            GotoNext(ref position);
            return position < values.Length;
        }

        #endregion

        #region Simple methods for finding a next larger prime

        /**
         * <p>Finds next-largest "twin primes": numbers p and p+2 such that both are prime. Finds the smallest such p such
         * that the smaller twin, p, is greater than or equal to n. Returns p+2, the larger of the two twins.</p>
         */
        private static int NextTwinPrime(int n)
        {
            if (n > MAX_INT_SMALLER_TWIN_PRIME)
            {
                throw new ArgumentException();
            }
            int next = NextPrime(n);
            while (IsNotPrime(next + 2))
            {
                next = NextPrime(next + 4);
            }
            return next + 2;
        }

        /**
         * <p>Finds smallest prime p such that p is greater than or equal to n.</p>
         */
        private static int NextPrime(int n)
        {
            Debug.Assert(n > 1);
            // Make sure the number is odd. Is this too clever?
            n |= 0x1;
            // There is no problem with overflow since Integer.MAX_INT is prime, as it happens
            while (IsNotPrime(n))
            {
                n += 2;
            }
            return n;
        }

        /**
         * @param n
         * @return <code>true</code> iff n is not a prime
         */
        private static bool IsNotPrime(int n)
        {
            if (n < 2)
            {
                throw new ArgumentException();
            }
            if ((n & 0x1) == 0)
            { // even
                return true;
            }
            int max = 1 + (int)Math.Sqrt((double)n);
            for (int d = 3; d <= max; d += 2)
            {
                if (n % d == 0)
                {
                    return true;
                }
            }
            return false;
        }

        #endregion

        private void iteratorRemove(int lastNext)
        {
            if (lastNext >= values.Length)
            {
                throw new NoSuchElementException();
            }
            if (lastNext < 0)
            {
                throw new IllegalStateException();
            }
            values[lastNext] = NULL_VALUE;
            keys[lastNext] = REMOVED;
            numEntries--;
        }


        #region IEnumerator<KeyValuePair<K, V>>

        [Serializable]
        public struct KeyValueEnumerator : IEnumerator<KeyValuePair<K, V>>,
            IDisposable, IDictionaryEnumerator, IEnumerator
        {
            FastMap<K, V> map;
            int position;
            int stamp;

            internal KeyValueEnumerator(FastMap<K, V> dictionary)
            {
                this.map = dictionary;
                stamp = dictionary.generation;

                // The following stanza is identical to IEnumerator.Reset (),
                // but because of the definite assignment rule, we cannot call it here.
                position = 0;
            }

            public bool MoveNext()
            {
                VerifyState();
                ++position;
                return map.HasNext(ref position);
            }

            public KeyValuePair<K, V> Current
            {
                get
                {
                    VerifyCurrent();
                    return new KeyValuePair<K, V>(
                        map.keys[position],
                        map.values[position]
                    );
                }
            }

            internal K CurrentKey
            {
                get
                {
                    VerifyCurrent();
                    return map.keys[position];
                }
            }

            internal V CurrentValue
            {
                get
                {
                    VerifyCurrent();
                    return map.values[position];
                }
            }

            object IEnumerator.Current
            {
                get { return Current; }
            }

            void IEnumerator.Reset()
            {
                position = 0;
            }

            DictionaryEntry IDictionaryEnumerator.Entry
            {
                get
                {
                    VerifyCurrent();
                    return new DictionaryEntry(
                        map.keys[position],
                        map.values[position]
                    );
                }
            }

            object IDictionaryEnumerator.Key
            {
                get
                {
                    VerifyCurrent();
                    return map.keys[position];
                }
            }

            object IDictionaryEnumerator.Value
            {
                get
                {
                    VerifyCurrent();
                    return map.values[position];
                }
            }

            void VerifyState()
            {
                if (map == null)
                    throw new ObjectDisposedException(null);
                if (map.generation != stamp)
                    throw new InvalidOperationException("out of sync");
            }

            void VerifyCurrent()
            {
                VerifyState();
                if (position == -1)
                    throw new InvalidOperationException("Current is not valid");
            }

            public void Dispose()
            {
                map = null;
            }
        }

        #endregion

        #region IEnumerable<KeyValuePair<K,V>> Members

        IEnumerator<KeyValuePair<K, V>> IEnumerable<KeyValuePair<K, V>>.GetEnumerator()
        {
            return new KeyValueEnumerator(this);
        }

        #endregion

        #region ICollection<KeyValuePair<K,V>> Members

        void ICollection<KeyValuePair<K, V>>.Add(KeyValuePair<K, V> item)
        {
            Add(item.Key, item.Value);
        }

        void ICollection<KeyValuePair<K, V>>.Clear()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        bool ICollection<KeyValuePair<K, V>>.Contains(KeyValuePair<K, V> item)
        {
            int index = Find(item.Key);
            return ((keys[index] != null) && (values[index] != null));
        }

        void ICollection<KeyValuePair<K, V>>.CopyTo(KeyValuePair<K, V>[] array, int arrayIndex)
        {
            if (array == null)
                throw new ArgumentNullException("array");
            if (arrayIndex < 0)
                throw new ArgumentOutOfRangeException("arrayIndex");
            if (array.Length - arrayIndex < this.Count)
                throw new ArgumentException("Destination array cannot hold the requested elements!");

            int position = 0;
            while (HasNext(ref position))
            {
                array[arrayIndex++] = new KeyValuePair<K, V>(keys[position], values[position++]);
            }
        }

        int ICollection<KeyValuePair<K, V>>.Count
        {
            get { return this.Count; }
        }

        bool ICollection<KeyValuePair<K, V>>.IsReadOnly
        {
            get { return true; }
        }

        bool ICollection<KeyValuePair<K, V>>.Remove(KeyValuePair<K, V> item)
        {
            return Remove(item.Key);
        }

        #endregion

        #region KeyCollection

        // This collection is a read only collection
        [Serializable]
        public sealed class KeyCollection : ICollection<K>, IEnumerable<K>, ICollection, IEnumerable
        {
            FastMap<K, V> map;

            public KeyCollection(FastMap<K, V> dictionary)
            {
                if (dictionary == null)
                    throw new ArgumentNullException("dictionary");
                this.map = dictionary;
            }

            public void CopyTo(K[] array, int index)
            {
                if (array == null)
                    throw new ArgumentNullException("array");
                if (index < 0)
                    throw new ArgumentOutOfRangeException("index");
                // we want no exception for index==array.Length && dictionary.Count == 0
                if (index > array.Length)
                    throw new ArgumentException("index larger than largest valid index of array");
                if (array.Length - index < map.Count)
                    throw new ArgumentException("Destination array cannot hold the requested elements!");
                int position = 0;
                while (map.HasNext(ref position))
                {
                    array[index++] = map.keys[position++];
                }
            }

            public Enumerator GetEnumerator()
            {
                return new Enumerator(map);
            }

            void ICollection<K>.Add(K item)
            {
                throw new NotSupportedException("this is a read-only collection");
            }

            void ICollection<K>.Clear()
            {
                throw new NotSupportedException("this is a read-only collection");
            }

            bool ICollection<K>.Contains(K item)
            {
                return map.ContainsKey(item);
            }

            bool ICollection<K>.Remove(K item)
            {
                throw new NotSupportedException("this is a read-only collection");
            }

            IEnumerator<K> IEnumerable<K>.GetEnumerator()
            {
                //int position = 0;
                return this.GetEnumerator();
            }

            void ICollection.CopyTo(Array array, int index)
            {
                CopyTo((K[])array, index);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }

            public int Count
            {
                get { return map.Count; }
            }

            bool ICollection<K>.IsReadOnly
            {
                get { return true; }
            }

            bool ICollection.IsSynchronized
            {
                get { return false; }
            }

            object ICollection.SyncRoot
            {
                get { return ((ICollection)map).SyncRoot; }
            }

            [Serializable]
            public struct Enumerator : IEnumerator<K>, IDisposable, IEnumerator
            {
                FastMap<K, V>.KeyValueEnumerator host_enumerator;

                internal Enumerator(FastMap<K, V> host)
                {
                    host_enumerator = new KeyValueEnumerator(host);
                }

                public void Dispose()
                {
                    host_enumerator.Dispose();
                }

                public bool MoveNext()
                {
                    return host_enumerator.MoveNext();
                }

                public K Current
                {
                    get { return host_enumerator.CurrentKey; }
                }

                object IEnumerator.Current
                {
                    get { return host_enumerator.CurrentKey; }
                }

                void IEnumerator.Reset()
                {
                    ((IEnumerator)host_enumerator).Reset();
                }
            }
        }
        #endregion

        #region ValueCollection

        // This collection is a read only collection
        [Serializable]
        public sealed class ValueCollection : ICollection<V>, IEnumerable<V>, ICollection, IEnumerable
        {
            FastMap<K, V> map;

            public ValueCollection(FastMap<K, V> map)
            {
                if (map == null)
                    throw new ArgumentNullException("dictionary");
                this.map = map;
            }

            public void CopyTo(V[] array, int index)
            {
                if (array == null)
                    throw new ArgumentNullException("array");
                if (index < 0)
                    throw new ArgumentOutOfRangeException("index");
                // we want no exception for index==array.Length && dictionary.Count == 0
                if (index > array.Length)
                    throw new ArgumentException("index larger than largest valid index of array");
                if (array.Length - index < map.Count)
                    throw new ArgumentException("Destination array cannot hold the requested elements!");

                int position = 0;
                while (map.HasNext(ref position))
                {
                    array[index++] = map.values[position++];
                }
            }

            public Enumerator GetEnumerator()
            {
                return new Enumerator(map);
            }

            void ICollection<V>.Add(V item)
            {
                throw new NotSupportedException("this is a read-only collection");
            }

            void ICollection<V>.Clear()
            {
                throw new NotSupportedException("this is a read-only collection");
            }

            bool ICollection<V>.Contains(V item)
            {
                return map.ContainsValue(item);
            }

            bool ICollection<V>.Remove(V item)
            {
                throw new NotSupportedException("this is a read-only collection");
            }

            IEnumerator<V> IEnumerable<V>.GetEnumerator()
            {
                return this.GetEnumerator();
            }

            void ICollection.CopyTo(Array array, int index)
            {
                CopyTo((V[])array, index);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }

            public int Count
            {
                get { return map.Count; }
            }

            bool ICollection<V>.IsReadOnly
            {
                get { return true; }
            }

            bool ICollection.IsSynchronized
            {
                get { return false; }
            }

            object ICollection.SyncRoot
            {
                get { return ((ICollection)map).SyncRoot; }
            }

            [Serializable]
            public struct Enumerator : IEnumerator<V>, IDisposable, IEnumerator
            {
                FastMap<K, V>.KeyValueEnumerator host_enumerator;

                internal Enumerator(FastMap<K, V> host)
                {
                    host_enumerator = new FastMap<K,V>.KeyValueEnumerator(host);
                }

                public void Dispose()
                {
                    host_enumerator.Dispose();
                }

                public bool MoveNext()
                {
                    return host_enumerator.MoveNext();
                }

                public V Current
                {
                    get { return host_enumerator.CurrentValue; }
                }

                object IEnumerator.Current
                {
                    get { return host_enumerator.CurrentValue; }
                }

                void IEnumerator.Reset()
                {
                    ((IEnumerator)host_enumerator).Reset();
                }
            }
        }
        #endregion

        #region IEnumerable Members
        IEnumerator IEnumerable.GetEnumerator()
        {
            return new KeyValueEnumerator(this);
        }
        #endregion
    }


}