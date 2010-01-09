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
namespace Taste.Recommender
{
	using System;
    using System.Threading;
    using Iesi.Collections.Generic;
	using System.Collections.Generic;
	using Taste.Common;
	using Taste.Model;
	using Taste.Recommender;
    using log4net;



    /// <summary>
    /// <p>A simple Recommender that always estimates preference for an {@link Item} to be the average of
    /// all known preference values for that <see cref="taste.Model.Item">Item</see>. No information about <see cref="taste.Model.Item">Item</see>s is taken into
    /// account. This implementation is provided for experimentation; while simple and fast, it may not
    /// produce very good recommendations.</p>
    /// 
    /// author Sean Owen
    /// </summary>
	public class ItemAverageRecommender : AbstractRecommender 
	{
		private static ILog log = LogManager.GetLogger(typeof(ItemAverageRecommender));
		
		private readonly IDictionary<Object, RunningAverage> itemAverages;
		private bool averagesBuilt;
		
		private readonly ReentrantLock refreshLock;
		
		private readonly ReaderWriterLock buildAveragesLock;

		public ItemAverageRecommender(DataModel dataModel) 
            :base(dataModel)
        {
			this.itemAverages = new Dictionary<Object, RunningAverage>(1003);
			this.refreshLock = new ReentrantLock();
			this.buildAveragesLock = new ReaderWriterLock();
		}

		/**
		 * {@inheritDoc}
		 */
		
		public override IList<RecommendedItem> Recommend(Object userID, int howMany, Rescorer<Item> rescorer)
		{
			if (userID == null) 
            {
				throw new ArgumentNullException("userID is null");
			}
			if (howMany < 1) 
            {
				throw new ArgumentException("howMany must be at least 1");
			}
			if (rescorer == null) 
            {
				throw new ArgumentNullException("rescorer is null");
			}
			if (log.IsDebugEnabled) 
            {
				log.Debug("Recommending items for user ID '" + userID + '\'');
			}
			CheckAverageDiffsBuilt();

			User theUser = this.DataModel.GetUser(userID);
			ISet<Item> allItems = GetAllOtherItems(theUser);

			TopItems.Estimator<Item> estimator = new Estimator(this);

			IList<RecommendedItem> topItems = TopItems.GetTopItems(howMany, allItems, rescorer, estimator);

			if (log.IsDebugEnabled) 
            {
				log.DebugFormat("Recommendations are: {0} " , topItems);
			}
			return topItems;
		}

		/**
		 * {@inheritDoc}
		 */
		public override double EstimatePreference(Object userID, Object itemID) 
        {
			User theUser = this.DataModel.GetUser(userID);
			Preference actualPref = theUser.GetPreferenceFor(itemID);
			if (actualPref != null) 
            {
				return actualPref.Value;
			}
			CheckAverageDiffsBuilt();
			return DoEstimatePreference(itemID);
		}

		private double DoEstimatePreference(Object itemID) 
		{
            buildAveragesLock.AcquireReaderLock(Constants.INFINITE_TIMEOUT);
			try 
            {
				RunningAverage average = null;
                if (itemAverages.TryGetValue(itemID, out average))
                {
                    return average.Average;
                }
				return Double.NaN;
			} 
            finally 
            {
				buildAveragesLock.ReleaseReaderLock();
			}
		}

		private void CheckAverageDiffsBuilt() 
		{
			if (!averagesBuilt) 
            {
				BuildAverageDiffs();
			}
		}

		private void BuildAverageDiffs() 
        {
            buildAveragesLock.AcquireWriterLock(Constants.INFINITE_TIMEOUT);
			try 
            {	
				foreach (User user in this.DataModel.GetUsers()) 
                {
                    Preference[] prefs = user.GetPreferencesAsArray();
					foreach (Preference pref in prefs) 
                    {
						Object itemID = pref.Item.ID;

						RunningAverage average = null;
                        if (!itemAverages.TryGetValue(itemID, out average))
                        {
							average = new FullRunningAverage();
							itemAverages.Add(itemID, average);
						}
						average.AddDatum(pref.Value);
					}
				}
				averagesBuilt = true;
			} 
            finally 
            {
				buildAveragesLock.ReleaseWriterLock();
			}
		}


        public override void SetPreference(Object userID, Object itemID, double value) 
        {
			double prefDelta;
			try 
            {
				User theUser = this.DataModel.GetUser(userID);
				Preference oldPref = theUser.GetPreferenceFor(itemID);
				prefDelta = oldPref == null ? value : value - oldPref.Value;
			} 
            catch 
            {
				prefDelta = value;
			}
			base.SetPreference(userID, itemID, value);

            buildAveragesLock.AcquireWriterLock(Constants.INFINITE_TIMEOUT);
			try 
            {				
				RunningAverage average = null;
				if (!itemAverages.TryGetValue(itemID, out average))
                {
					RunningAverage newAverage = new FullRunningAverage();
					newAverage.AddDatum(prefDelta);
					itemAverages.Add(itemID, newAverage);
				} 
                else 
                {
					average.ChangeDatum(prefDelta);
				}
			} 
            finally 
            {
				buildAveragesLock.ReleaseWriterLock();
			}
		}

		/**
		 * {@inheritDoc}
		 */
		
		public override void RemovePreference(Object userID, Object itemID) 
		{
			User theUser = this.DataModel.GetUser(userID);
			Preference oldPref = theUser.GetPreferenceFor(itemID);
			base.RemovePreference(userID, itemID);
			if (oldPref != null) 
			{
                buildAveragesLock.AcquireWriterLock(Constants.INFINITE_TIMEOUT);
				try 
				{
                    RunningAverage average = null;
                    if (!itemAverages.TryGetValue(itemID, out average))
                    {
						throw new IllegalStateException("No preferences exist for item ID: " + itemID);
					} 
                    else 
                    {
						average.RemoveDatum(oldPref.Value);
					}
				} 
                finally 
                {
					buildAveragesLock.ReleaseWriterLock();
				}
			}
		}

		/**
		 * {@inheritDoc}
		 */
		public override void Refresh() 
		{
            if (refreshLock.TryLock())
            {
                try
                {
                    base.Refresh();
                    try
                    {
                        BuildAverageDiffs();
                    }
                    catch (TasteException te)
                    {
                        log.Warn( "Unexpected excpetion while refreshing", te);
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
			return "ItemAverageRecommender";
		}

		private class Estimator : TopItems.Estimator<Item> 
		{
            ItemAverageRecommender host;

            public Estimator(ItemAverageRecommender host)
            {
                this.host = host;
            }

			public double Estimate(Item item) 
            {
				return host.DoEstimatePreference(item.ID);
			}
		}

	}

}	