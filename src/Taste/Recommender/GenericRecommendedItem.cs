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

namespace Taste.Recommender
{
	using System;
	using System.Collections.Generic;
	using Taste.Common;
	using Taste.Model;
	using Taste.Recommender;


    /// <summary>
    /// <p>A simple implementation of {@link RecommendedItem}.</p>
    /// 
    /// author Sean Owen
    /// </summary>
	[Serializable] 
	public class GenericRecommendedItem : RecommendedItem
	{
		private readonly Item item;
		private readonly double value;

		public GenericRecommendedItem(Item item, double value) 
		{
			if (item == null) {
				throw new ArgumentNullException("item is null");
			}
			if (double.IsNaN(value)) {
				throw new ArgumentException("value is NaN");
			}
			this.item = item;
			this.value = value;
		}

		
		public Item Item 
		{
			get {return item;}
		}

		public double Value 
		{
			get {return value;}
		}

		
		public override String ToString() 
		{
			return "RecommendedItem[item:" + item + ", value:" + value + ']';
		}


		public override int GetHashCode() 
        {
			return item.GetHashCode() ^ value.GetHashCode();
		}

		public override bool Equals(Object o) 
		{
			if (!(o is GenericRecommendedItem)) 
			{
				return false;
			}
			GenericRecommendedItem other = (GenericRecommendedItem) o;
			return item.Equals(other.item) && value == other.value;
		}

		/**
		 * Defines a natural ordering from most-preferred item (highest value) to least-preferred.
		 *
		 * @param other
		 * @return 1, -1, 0 as this value is less than, greater than or equal to the other's value
		 */
		public int CompareTo(RecommendedItem other) 
		{
			double otherValue = other.Value;
			if (value < otherValue) {
				return 1;
			} else if (value > otherValue) {
				return -1;
			} else {
				return 0;
			}
		}
	}

}	