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
    using System.Collections;
	using System.Collections.Generic;
	
	/**
	 * <p>An {@link IEnumerable} over no elements: always produces an {@link IEnumerator} which iterates
	 * over nothing.</p>
	 * 
	 * @author Sean Owen
	 */
	public sealed class EmptyEnumerable<T> : IEnumerable<T> 
	{
		private readonly List<T> item = new List<T>();

		public EmptyEnumerable() 
		{
		}

		public override String ToString() 
		{
			return "EmptyEnumerable[iterator:" + typeof(T).Name + ']';
		}
	
        #region IEnumerable<T> Members

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
 	        return item.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)item).GetEnumerator();
        }

        #endregion
    }

}
