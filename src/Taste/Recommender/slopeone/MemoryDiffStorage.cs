/*
 * Copyright 2007 and onwards Sean Owen
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

namespace Taste.Recommender.SlopeOne
{
	using System;
    using System.Threading;
	using System.Collections.Generic;
    using Iesi.Collections.Generic;
	using Taste.Common;
	using Taste.Model;
	using Taste.Recommender;
    using Taste.Recommender.SlopeOne;
    using log4net;

    /// <summary>
    /// <p>An implementation of {@link DiffStorage} that merely stores item-item diffs in memory.
    /// It is fast, but can consume a great deal of memory.</p>
    /// 
    /// @author Sean Owen
    /// </summary>
	public class MemoryDiffStorage : DiffStorage 
    {
		private static ILog log = LogManager.GetLogger(typeof(MemoryDiffStorage).Name);
		private DataModel dataModel;
		private bool stdDevWeighted;
		private bool compactAverages;
		private long maxEntries;
		
		private Dictionary<Object, Dictionary<Object, RunningAverage>> averageDiffs;
		
		private Dictionary<Object, RunningAverage> averageItemPref;

		private ReaderWriterLock buildAverageDiffsLock;

		private ReentrantLock refreshLock;

        /// <summary>
        /// <p>Creates a new {@link MemoryDiffStorage}.</p>
        /// <p>See {@link taste.Recommender.SlopeOne.SlopeOneRecommender} for the
        /// meaning of <code>stdDevWeighted</code>. If <code>compactAverages</code>
        /// is set, this uses alternate data structures ({@link CompactRunningAverage} versus
        /// {@link FullRunningAverage}) that use almost 50% less memory but store item-item
        /// averages less accurately. <code>maxEntries</code> controls the maximum number of item-item average
        /// preference differences that will be tracked internally. After the limit is reached,
        /// if a new item-item pair is observed in the data it will be ignored. This is recommended for large datasets.
        /// The first <code>maxEntries</code> item-item pairs observed in the data are tracked. Assuming that item 
        /// ratings are reasonably distributed among users, this should only ignore item-item pairs that are very 
        /// infrequently co-rated by a user.</p>
        /// <p>  The intuition is that data on these infrequently co-rated item-item pairs is less reliable and should
        /// be the first that is ignored. This parameter can be used to limit the memory requirements of
        /// {@link SlopeOneRecommender}, which otherwise grow as the square of the number of items that exist 
        /// in the {@link DataModel}. Memory requirements can reach gigabytes with only about 10000 items, so this may
        /// be necessary on larger datasets.</p>
        /// </summary>
        /// <param name="dataModel"></param>
        /// <param name="stdDevWeighted">see  {@link taste.Recommender.SlopeOne.SlopeOneRecommender}</param>
        /// <param name="compactAverages">
        /// if <code>true</code>, use {@link CompactRunningAverage} instead of {@link FullRunningAverage} internally
        /// </param>
        /// <param name="maxEntries">maximum number of item-item average preference differences to track internally</param>
		public MemoryDiffStorage(DataModel dataModel,
		                         bool stdDevWeighted,
		                         bool compactAverages,
		                         long maxEntries) 
		{
			if (dataModel == null) 
            {
				throw new ArgumentNullException("dataModel is null");
			}
			if (maxEntries <= 0L) 
            {
				throw new ArgumentException("maxEntries must be positive");
			}
			this.dataModel = dataModel;
			this.stdDevWeighted = stdDevWeighted;
			this.compactAverages = compactAverages;
			this.maxEntries = maxEntries;
			this.averageDiffs = new Dictionary<Object, Dictionary<Object, RunningAverage>>(1003);
			this.averageItemPref = new Dictionary<Object, RunningAverage>(101);
			this.buildAverageDiffsLock = new ReaderWriterLock();
			this.refreshLock = new ReentrantLock();
			BuildAverageDiffs();
		}


		public RunningAverage GetDiff(Object itemID1, Object itemID2) 
		{
            RunningAverage average = null;
            Dictionary<Object, RunningAverage> level2Map;

            averageDiffs.TryGetValue(itemID1, out level2Map);
			if (level2Map != null) 
            {
                level2Map.TryGetValue(itemID2, out average);
			}
			bool inverted = false;
			if (average == null) 
			{
                averageDiffs.TryGetValue(itemID2, out level2Map);
				if (level2Map != null) 
				{
                    level2Map.TryGetValue(itemID1, out average);
					inverted = true;
				}
			}
			if (inverted) 
			{
				if (average == null) 
                {
					return null;
				}
                if (stdDevWeighted)
                {
                    return new InvertedRunningAverageAndStdDev((RunningAverageAndStdDev) average);
                }
				return new InvertedRunningAverage(average);
			} 
            else 
            {
				return average;
			}
		}

		public  RunningAverage[] GetDiffs(Object userID, Object itemID,
		                                            IList<Preference> prefs) 
		{
            buildAverageDiffsLock.AcquireReaderLock(Constants.INFINITE_TIMEOUT);
            try
            {

                int size = prefs.Count;
                RunningAverage[] result = new RunningAverage[size];
                int i = 0;
                foreach (Preference pref in prefs)
                {
                    Object prefItemID = pref.Item.ID;
                    result[i++] = GetDiff(prefItemID, itemID);
                }
                return result;
            }
            finally
            {
                buildAverageDiffsLock.ReleaseReaderLock();
            }
		}


		public RunningAverage GetAverageItemPref(Object itemID) 
		{
            RunningAverage avg;
            if (averageItemPref.TryGetValue(itemID, out avg))
                return avg;
            return null;
		}

		public void UpdateItemPref(Object itemID, double prefDelta, bool remove) 
		{
			if (!remove && stdDevWeighted) 
			{
				throw new NotSupportedException("Can't update only when stdDevWeighted is set");
			}
            buildAverageDiffsLock.AcquireReaderLock(Constants.INFINITE_TIMEOUT);
			try 
			{
				foreach (KeyValuePair<Object, Dictionary<Object, RunningAverage>> entry in averageDiffs) 
                {
					bool matchesItemID1  = itemID.Equals(entry.Key);
					foreach (KeyValuePair<Object, RunningAverage> entry2 in entry.Value) 
                    {
						RunningAverage average = entry2.Value;
						if (matchesItemID1) 
                        {
							if (remove) 
                            {
								average.RemoveDatum(prefDelta);
							} 
                            else 
                            {
								average.ChangeDatum(-prefDelta);
							}
						} 
                        else if (itemID.Equals(entry2.Key)) 
						{
							if (remove) 
                            {
								average.RemoveDatum(-prefDelta);
							} 
                            else 
                            {
								average.ChangeDatum(prefDelta);
							}
						}
					}
				}
				RunningAverage itemAverage;
                if (averageItemPref.TryGetValue(itemID,out itemAverage) && itemAverage != null)
                {
					itemAverage.ChangeDatum(prefDelta);
				}
			} 
            finally 
            {
				buildAverageDiffsLock.ReleaseReaderLock();
			}
		}

		public ISet<Item> GetRecommendableItems(Object userID) 
        {
			User user = dataModel.GetUser(userID);
			ISet<Item> result = new HashedSet<Item>(/*dataModel.GetNumItems()*/);
			foreach (Item item in dataModel.GetItems()) 
            {
				// If not already preferred by the user, add it
				if (user.GetPreferenceFor(item.ID) == null) 
                {
					result.Add(item);
				}
			}
			return result;
		}

		private void BuildAverageDiffs()
		{
			log.Info("Building average diffs...");

            buildAverageDiffsLock.AcquireWriterLock(Constants.INFINITE_TIMEOUT);
			try 
            {
				long averageCount = 0L;
				foreach (User user in dataModel.GetUsers()) 
                {
					if (log.IsDebugEnabled) 
                    {
						log.Debug("Processing prefs for user " + user + "...");
					}
					// Save off prefs for the life of this loop iteration
					Preference[] userPreferences = user.GetPreferencesAsArray();

					int length = userPreferences.Length;
					for (int i = 0; i < length; i++) {
						Preference prefA = userPreferences[i];
						double prefAValue = prefA.Value;
						Object itemIDA = prefA.Item.ID;

                        Dictionary<Object, RunningAverage> aMap;
                        if (!averageDiffs.TryGetValue(itemIDA, out aMap))
                        {
							aMap = new Dictionary<Object, RunningAverage>();
							averageDiffs.Add(itemIDA, aMap);
						}
						for (int j = i + 1; j < length; j++) 
                        {
							// This is a performance-critical block
							Preference prefB = userPreferences[j];
							Object itemIDB = prefB.Item.ID;

							RunningAverage average;
                            if (!aMap.TryGetValue(itemIDB, out average))
                                average = null;
							if (average == null && averageCount < maxEntries) 
							{
								average = BuildRunningAverage();
								aMap.Add(itemIDB, average);
								averageCount++;
							}
							if (average != null) 
                            {
								average.AddDatum(prefB.Value - prefAValue);
							}

						}

						RunningAverage itemAverage;
                        if (!averageItemPref.TryGetValue(itemIDA, out itemAverage))
                        {
							itemAverage = BuildRunningAverage();
							averageItemPref.Add(itemIDA, itemAverage);
						}
						itemAverage.AddDatum(prefAValue);
					}
				}

				// Go back and prune inconsequential diffs. "Inconsequential" means, here, an average
				// so small (< 1 / numItems^3) that it contributes very little to computations
				double numItems = (double) dataModel.GetNumItems();
				double threshold = 1.0 / numItems / numItems / numItems;

                
                List<object> toRemove = new List<object>();

                ICollection<Dictionary<Object, RunningAverage>> items = averageDiffs.Values;
				foreach (Dictionary<Object, RunningAverage> map in items) 
				{
                    foreach (KeyValuePair<object, RunningAverage> it2 in map)
                    {
                        RunningAverage average = it2.Value;
                        if (Math.Abs(average.Average) < threshold)
                        {
                            // we cant delete an iterated item, so collect and delete later
                            toRemove.Add(it2.Key);
                        }
                    }
                    if (toRemove.Count > 0)
                    {
                        foreach (object obj in toRemove)
                        {
                            map.Remove(obj);
                        }
                        toRemove.Clear();
                    }
					if (map.Count == 0) 
                    {
                       // averageDiffs.Remove -- we have to find the key of the map
					}
				}

			} 
            finally 
            {
				buildAverageDiffsLock.ReleaseWriterLock();
			}
		}

		private RunningAverage BuildRunningAverage() 
        {
			if (stdDevWeighted) 
            {
                if (compactAverages)
                    return new CompactRunningAverageAndStdDev();

                return new FullRunningAverageAndStdDev();
			} 
            else 
            {
                if (compactAverages)
                    return new CompactRunningAverage();

				return new FullRunningAverage();
			}
		}

		/**
		 * {@inheritDoc}
		 */
		public void Refresh() 
		{
            if (refreshLock.TryLock())
            {
                try
                {
                    dataModel.Refresh();
                    try
                    {
                        BuildAverageDiffs();
                    }
                    catch (TasteException te)
                    {
                        log.Warn( "Unexpected exception while refreshing", te);
                    }
                }
                finally
                {
                    refreshLock.Unlock();
                }
            }
		}

		public override String ToString() 
		{
			return "MemoryDiffStorage";
		}

	}

}	