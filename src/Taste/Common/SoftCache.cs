/*
 * Copyright 2005 and onwards Sean Owen
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
    using System.Collections.Generic;
    using Taste.Common;


    /** <p>Implementations can retrieve a value for a given key.</p> */
    public interface SoftCacheRetriever<KK, VV>
    {
        /**
         * @param key key for which a value should be retrieved
         * @return value for key
         * @if an error occurs while retrieving the value
         */       
        VV GetValue(KK key);
    }


    /**
     * <p>An efficient Map-like class which caches values for keys. Values are not "put" into a {@link SoftCache};
     * instead the caller supplies the instance with an implementation of {@link Retriever} which can load the
     * value for a given key.</p>
     *
     * <p>This class is a bit misnamed at this point since it no longer uses <code>SoftReference</code> internally,
     * but hey.</p>
     *
     * <p>The cache does not support <code>null</code> values or keys.</p>
     *
     * <p>Thanks to Amila Jayasooriya for helping evaluate performance of the rewrite of this class, as part of a
     * Google Summer of Code 2007 project.</p>
     *
     * @author Sean Owen
     */
    public class SoftCache<K, V> 
    {
        private readonly LRUCacheMap<K, V> cache;
        private readonly SoftCacheRetriever<K, V> retriever;
        //private readonly Func<K, V> retrieverFunc;

     
        /// <summary>
        /// Creates a new cache based on the given {@link Retriever}.
        /// </summary>
        /// <param name="retriever">object which can retrieve values for keys</param>
        public SoftCache(SoftCacheRetriever<K, V> retriever) 
            : this(retriever, LRUCacheMap<K,V>.NO_MAX_SIZE)
        {

        }


        /// <summary>
        /// Creates a new cache based on the given {@link Retriever} and with given maximum size.
        /// </summary>
        /// <param name="retriever">object which can retrieve values for keys</param>
        /// <param name="maxEntries">maximum number of entries the cache will store before evicting some</param>
        public SoftCache(SoftCacheRetriever<K, V> retriever, int maxEntries)
        {
            if (retriever == null)
            {
                throw new ArgumentNullException("retriever is null");
            }
            if (maxEntries < 1)
            {
                throw new ArgumentException("maxEntries must be at least 1");
            }
            cache = new LRUCacheMap<K, V>(11, maxEntries);
            this.retriever = retriever;
        }


 
        /// <summary>
        /// Returns cached value for a key. If it does not exist, it is loaded 
        /// using a {@link Retriever}.</p>
        /// </summary>
        /// <param name="key">cache key</param>
        /// <returns>value for that key</returns>
        public V Get(K key)
        {
            lock (cache)
            {
                V value;
                if (cache.TryGetValue(key, out value))
                    return value;
            }
            return GetAndCacheValue(key);
        }


        public V this[K key]
        {
            get { return Get(key); }
        }
         

        /// <summary>
        /// Uncaches any existing value for a given key.
        /// </summary>
        /// <param name="key">cache key</param>
        public void Remove(K key)
        {
            lock (cache)
            {
                cache.Remove(key);
            }
        }


        /// <summary>
        /// Clear the cache.
        /// </summary>
        public void Clear()
        {
            lock (cache)
            {
                cache.Clear();
            }
        }

        private V GetAndCacheValue(K key)
        {
            V value = retriever.GetValue(key);
            lock (cache)
            {
                cache.Add(key, value);
            }
            return value;
        }

        public override String ToString()
        {
            return "SoftCache[retriever:" + retriever + ']';
        }

    }

}