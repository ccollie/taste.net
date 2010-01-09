/*
 * Copyright 2006 and onwards Sean Owen
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
    using System.Linq;
    using System.Collections.Generic;


    /**
     * <p>{@link java.util.IEnumerator}-related methods without a better home.</p>
     *
     * @author Sean Owen
     */
    public class EnumeratorUtils
    {

        private EnumeratorUtils() { }

        /**
         * @param iterable {@link IEnumerable} whose contents are to be put into a {@link List}
         * @return a {@link List} with the objects one gets by iterating over the given {@link IEnumerable}
         */
        public static List<K> EnumerableToList<K>(IEnumerable<K> iterable)
        {
            return EnumerableToList(iterable, null);
        }

        /**
         * @param iterable {@link IEnumerable} whose contents are to be put into a {@link List}
         * @param comparator {@link Comparator} defining the sort order of the returned {@link List}
         * @return a {@link List} with the objects one gets by iterating over the given {@link IEnumerable},
         *  sorted according to the given {@link Comparator}
         */
        public static List<K> EnumerableToList<K>(IEnumerable<K> iterable, IComparer<K> comparator)
        {
            if (iterable == null)
            {
                throw new ArgumentNullException("iterable is null");
            }
            List<K> list;
            if (iterable is List<K>)
            {
                list = (List<K>)iterable;
            }
            else
            {
                list = iterable.ToList<K>();
            }
            if (comparator != null)
            {
                list.Sort(comparator);
            }
            return list;
        }
    }

}