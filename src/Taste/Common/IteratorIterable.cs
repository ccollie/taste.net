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

namespace taste.common
{
    using System;
    using System.Collections.Generic;

    /**
     * <p>Simple utility class that makes an {@link IEnumerator} {@link IEnumerable}
     * by returning the {@link IEnumerator} itself.</p>
     *
     * @author Sean Owen
     */
    public sealed class EnumeratorEnumerable<T> : IEnumerable<T>
    {

        private readonly IEnumerator<T> iterator;

        /**
         * <p>Constructs an {@link IEnumeratorIEnumerable} for an {@link IEnumerator}.</p>
         *
         * @param iterator {@link IEnumerator} on which to base this {@link IEnumeratorIEnumerable}
         */
        public EnumeratorEnumerable(IEnumerator<T> iterator)
        {
            if (iterator == null)
            {
                throw new ArgumentException("iterator is null");
            }
            this.iterator = iterator;
        }

        /**
         * {@inheritDoc}
         */
        public IEnumerator<T> iterator()
        {
            return iterator;
        }


        public override String ToString()
        {
            return "IEnumeratorIEnumerable[iterator:" + iterator + ']';
        }
    }

}