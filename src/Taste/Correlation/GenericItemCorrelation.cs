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

namespace Taste.Correlation
{
	using System;
	using System.Collections.Generic;
	using Taste.Common;
	using Taste.Model;
	using Taste.Correlation;
	using Taste.Transforms;



    /// <summary>
    /// A "generic" <see cref="ItemCorrelation"/> which takes a static list of precomputed <see cref="taste.Model.Item"/>
    /// correlations and bases its responses on that alone. The values may have been precomputed
    /// offline by another process, stored in a file, and then read and fed into an instance of this class.
    ///
    /// This is perhaps the best <see cref="taste.Correlation.ItemCorrelation"/> to use with
    /// <see cref="taste.Recommender.GenericItemBasedRecommender"/>, for now, since the point of item-based
    /// recommenders is that they can take advantage of the fact that item similarity is relatively static,
    /// can be precomputed, and then used in computation to gain a significant performance advantage.
    ///
    /// author Sean Owen
    /// </summary>
	public class GenericItemCorrelation : ItemCorrelation 
	{
		private readonly IDictionary<Item, Dictionary<Item, Double>> correlationMaps = new Dictionary<Item, Dictionary<Item, Double>>(1009);


        /// <summary>
        /// Creates a <see cref="GenericItemCorrelation"/> from a precomputed list of {@link ItemItemCorrelation}s. Each
        /// represents the Correlation between two distinct items. Since Correlation is assumed to be symmetric,
        /// it is not necessary to specify Correlation between item1 and item2, and item2 and item1. Both are the same.
        /// It is also not necessary to specify a Correlation between any item and itself; these are assumed to be 1.0.</p>
        ///
        /// Note that specifying a Correlation between two items twice is not an error, but, the later value will
        /// win.
        ///
        /// </summary>
        /// <param name="correlations">set of {@link ItemItemCorrelation}s on which to base this instance</param>
		public GenericItemCorrelation(IEnumerable<ItemItemCorrelation> correlations) 
		{
			foreach (ItemItemCorrelation iic in correlations) 
			{
				Item correlationItem1 = iic.Item1;
				Item correlationItem2 = iic.Item2;
				int compare = correlationItem1.CompareTo(correlationItem2);
				if (compare != 0) 
                {
					// Order them -- first key should be the "smaller" one
					Item item1;
					Item item2;
					if (compare < 0) 
                    {
						item1 = correlationItem1;
						item2 = correlationItem2;
					} 
                    else 
                    {
						item1 = correlationItem2;
						item2 = correlationItem1;
					}
					Dictionary<Item, Double> map = null;
					if (!correlationMaps.TryGetValue(item1, out map)) 
                    {
						map = new Dictionary<Item, Double>(1009);
						correlationMaps.Add(item1, map);
					}
					map[item2] =  iic.Value;
				}
				// else Correlation between item and itself already assumed to be 1.0
			}
		}

 
        /// <summary>
        /// <p>Builds a list of item-item correlations given an {@link ItemCorrelation} implementation and a
        /// <see cref="DataModel">DataModel</see>, rather than a list of {@link ItemItemCorrelation}s.</p>
        /// <p>It's valid to build a <see cref="GenericItemCorrelation"/> this way, but perhaps missing some of the point 
        /// of an item-based Recommender. Item-based recommenders use the assumption that item-item correlations
        /// are relatively fixed, and might be known already independent of user preferences. Hence it is useful
        /// to inject that information, using {@link GenericItemCorrelation(java.util.Collection)}.</p>
        /// </summary>
        /// <param name="otherCorrelation">otherCorrelation other {@link ItemCorrelation} to get correlations from</param>
        /// <param name="dataModel">dataModel data Model to get {@link Item}s from</param>
		public GenericItemCorrelation(ItemCorrelation otherCorrelation, DataModel dataModel)
		{
			List<Item> items = EnumeratorUtils.EnumerableToList<Item>(dataModel.GetItems());
			int size = items.Count;
			for (int i = 0; i < size; i++) 
            {
				Item item1 = items[i];
				for (int j = i + 1; j < size; j++) 
                {
					Item item2 = items[j];
					double correlation = otherCorrelation.GetItemCorrelation(item1, item2);
					Dictionary<Item, Double> map = null;
					if (!correlationMaps.TryGetValue(item1, out map)) 
                    {
						map = new Dictionary<Item, Double>(1009);
						correlationMaps.Add(item1, map);
					}
					map.Add(item2, correlation);
				}
			}
		}

		/**
		 * <p>Returns the Correlation between two items. Note that Correlation is assumed to be symmetric, that
		 * <code>itemCorrelation(item1, item2) == itemCorrelation(item2, item1)</code>, and that
		 * <code>itemCorrelation(item1, item1) == 1.0</code> for all items.</p>
		 *
		 * @param item1 first item
		 * @param item2 second item
		 * @return Correlation between the two
		 */
		public double GetItemCorrelation(Item item1, Item item2) 
		{
			int compare = item1.CompareTo(item2);
			if (compare == 0) 
			{
				return 1.0;
			}
			Item first;
			Item second;
			if (compare < 0) 
			{
				first = item1;
				second = item2;
			} else {
				first = item2;
				second = item1;
			}
			Dictionary<Item, Double> nextMap = null;
			if (!correlationMaps.TryGetValue(first, out nextMap)) 
            {
				return Double.NaN;
			}

			Double correlation;
            if (nextMap.TryGetValue(second, out correlation))
                return correlation;

            return Double.NaN;
		}

		/**
		 * {@inheritDoc}
		 */
		public void Refresh() 
        {
			// Do nothing
		}

		/**
		 * Encapsulates a Correlation between two items. Correlation must be in the range [-1.0,1.0].
		 */
		public class ItemItemCorrelation 
        {
			private Item item1;
			private Item item2;
			private double value;

			/**
			 * @param item1 first item
			 * @param item2 second item
			 * @param value Correlation between the two
			 * @throws IllegalArgumentException if value is NaN, less than -1.0 or greater than 1.0
			 */
			public ItemItemCorrelation(Item item1, Item item2, double value) 
			{
				if (item1 == null || item2 == null) 
				{
					throw new ArgumentNullException("An item is null");
				}
				if (Double.IsNaN(value) || value < -1.0 || value > 1.0) 
				{
					throw new ArgumentException("Illegal value: " + value);
				}
				this.item1 = item1;
				this.item2 = item2;
				this.value = value;
			}

			
			public Item Item1
			{
				get {return item1;}
			}

			public Item Item2
			{
				get {return item2;}
			}

			public double Value
			{
				get {return value;}
			}

			public override String ToString() 
			{
				return "ItemItemCorrelation[" + item1 + ',' + item2 + ':' + value + ']';
			}
		}

	}
}