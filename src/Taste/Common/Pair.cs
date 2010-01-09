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

    /// <summary>
    /// A simple (ordered) pair of two objects. Elements may be null.
    /// </summary>
    /// <typeparam name="A">Type of first value</typeparam>
    /// <typeparam name="B">Type of second value.</typeparam>
	public class Pair<A, B> 
	{
		private readonly A first;
		private readonly B second;

		public Pair(A first,B second) 
		{
			this.first = first;
			this.second = second;
		}

		public A First
		{
			get {return first;}
		}

		public B Second
		{
			get {return second;}
		}

		public override bool Equals(Object obj) 
		{
			if (!(obj is Pair<A,B>)) {
				return false;
			}
			Pair<A,B> otherPair = (Pair<A,B>) obj;
			return IsEqualOrNulls(first, otherPair.first) &&
				   IsEqualOrNulls(second, otherPair.second);
		}

		static bool IsEqualOrNulls(Object obj1, Object obj2) 
		{
			return obj1 == null ? obj2 == null : obj1.Equals(obj2);
		}
	
		public override int GetHashCode() 
		{
			int firstHash = HashCodeNull(first);
			// Flip top and bottom 16 bits; this makes the hash function probably different
			// for (a,b) versus (b,a)
			//return (firstHash >>>  16 | firstHash << 16) ^ HashCodeNull(second);
            return ( Math.Abs(firstHash >>  16) | firstHash << 16) ^ HashCodeNull(second);
		}

		static int HashCodeNull(Object obj) 
		{
			return obj == null ? 0 : obj.GetHashCode();
		}


		public override String ToString() 
		{
			return "(" + first + "," + second.ToString() + ")";
		}

	}
}