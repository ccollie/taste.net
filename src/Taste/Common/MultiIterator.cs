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

namespace taste.Common
{
	using System;
	using System.Collections.Generic;
	

	/**
	 * <p>An {@link IEnumerator} which can iterate over the contents of multiple {@link IEnumerator}s.</p>
	 *
	 * @author Sean Owen
	 * @since 1.3.1
	 */
	public class MultiEnumerator<T> : IEnumerator<T> 
	{
		private readonly List<IEnumerator<T>> iterators;
		private IEnumerator<T> currentIEnumerator;

		/**
		 * @throws ArgumentNullException if iterators is null or empty
		 * @param iterators {@link IEnumerator}s to iterate over
		 */
		public MultiEnumerator(IList<IEnumerator<T>> iterators) 
        {
			if (iterators == null || iterators.Count == 0) 
            {
				throw new ArgumentException("iterators is null or empty");
			}
			IEnumerator<T> firstIEnumerator = iterators[0];
			if (firstIEnumerator == null) 
            {
				throw new ArgumentException("An iterator is null");
			}
            iterators.RemoveAt(0);
			currentIEnumerator = firstIEnumerator;
			this.iterators = new List<IEnumerator<T>>(iterators.Count);
			this.iterators.AddAll(iterators);
		}

       
		public bool HasNext() 
        {
			if (currentIEnumerator.hasNext()) 
			{
				return true;
			}
			while (iterators.Count > 0) 
			{
				IEnumerator<T> nextIEnumerator = iterators[0];
				if (nextIEnumerator == null) 
                {
					throw new ArgumentException("An iterator is null");
				}
                iterators.RemoveAt(0);
				currentIEnumerator = nextIEnumerator;
				if (currentIEnumerator.hasNext()) 
                {
					return true;
				}
			}
			return false;
		}


		public T next() 
		{
			if (currentIEnumerator.hasNext()) 
			{
				return currentIEnumerator.next();
			}
			while (iterators != null && iterators.Count > 0) 
			{
                IEnumerator<T> nextIEnumerator = iterators[0];
				if (nextIEnumerator == null) 
				{
					throw new ArgumentNullException("An iterator is null");
				}
                iterators.RemoveAt(0);
				currentIEnumerator = nextIEnumerator;
				if (currentIEnumerator.hasNext()) 
				{
					return currentIEnumerator.next();
				}
			}
			throw new NoSuchElementException();
		}


		public override String ToString() 
		{
			return "MultiIEnumerator[iterators:" + iterators + ']';
		}
	}

}