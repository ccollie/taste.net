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

namespace Taste.Model
{
	using System;
    using System.Collections.Generic;
	

    /// <summary>
    /// <p> <see cref="System.Collections.Generic.IComparer<T>">IComparator</see> that orders <see cref="taste.Model.Preference">Preference</see>s by  
    /// <see cref="taste.Model.Item">Item</see>.</p>
    ///
    /// author Sean Owen
    /// </summary>
	public class ByItemPreferenceComparer : IComparer<Preference>
	{
		private static readonly IComparer<Preference> instance = new ByItemPreferenceComparer();

		private ByItemPreferenceComparer() 
		{
			// singleton
		}

		public static IComparer<Preference> Instance
		{
			get {return instance;}
		}

		public int Compare(Preference x, Preference y) 
		{
			return x.Item.CompareTo(y.Item);
		}

		public override String ToString() 
		{
			return "ByItemPreferenceComparator";
		}
	}

}