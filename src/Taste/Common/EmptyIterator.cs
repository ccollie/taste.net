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

	/**
	 * <p>An empty {@link IEnumerator}, which iterates over nothing.</p>
	 * 
	 * @author Sean Owen
	 */
	public sealed class EmptyEnumerator<T> : IEnumerator<T> 
	{
		/**
		 * @return never returns anything
		 * @throws NoSuchElementException
		 */
		public T Next() 
		{
			throw new NoSuchElementException();
		}

		/**
		 * @throws UnsupportedOperationException
		 */
		public void Remove() 
        {
			throw new NotSupportedException();
		}

		public override String ToString() 
		{
			return "EmptyIEnumerator";
		}

        #region IEnumerator<T> Members

        T IEnumerator<T>.Current
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        #endregion

        #region IDisposable Members

        void IDisposable.Dispose()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion

        #region IEnumerator Members

        object System.Collections.IEnumerator.Current
        {
            get { return null; }
        }

        bool System.Collections.IEnumerator.MoveNext()
        {
            return false;
        }

        void System.Collections.IEnumerator.Reset()
        {
        }

        #endregion
    }

}
