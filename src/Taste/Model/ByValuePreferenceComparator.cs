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
	using Taste.Model;

    /// <summary>
    /// <p> <see cref="System.Collections.Generic.IComparer<T>">IComparator</see> that orders <see cref="taste.Model.Preference">Preference</see>s 
    /// from least to most preferres -- that is, in order of ascending value.</p>
    ///
    /// author Sean Owen
    /// </summary>
	[Serializable]
	public sealed class ByValuePreferenceComparer : IComparer<Preference>
	{
		private static readonly IComparer<Preference> instance = new ByValuePreferenceComparer();

		private ByValuePreferenceComparer() 
		{
			// singleton
		}

		public static IComparer<Preference> Instance
		{
			get {return instance;}
		}

		public int Compare(Preference x, Preference y) 
		{
            return x.Value.CompareTo(y.Value);
		}

		public override String ToString() 
		{
			return "ByValuePreferenceComparer";
		}
	}

}	