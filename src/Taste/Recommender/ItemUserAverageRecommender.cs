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
    /// <p>Like <see cref="taste.Recommender.ItemAverageRecommender">ItemAverageRecommender</see>, except that estimated preferences are 
    /// adjusted for the <see cref="taste.Model.User">User</see>s' average preference value. For example, say user X has 
    /// not rated item Y. Item Y's average preference value is 3.5. User X's average preference value is 4.2, and the average 
    /// over all preference values is 4.0. User X prefers items 0.2 higher on average, so, the estimated preference
    /// for user X, item Y is 3.5 + 0.2 = 3.7.</p>
    /// 
    /// author Sean Owen
    /// </summary>
	public class ItemUserAverageRecommender : AbstractRecommender 
    {
		private static ILog log = LogManager.GetLogger(typeof(ItemUserAverageRecommender));
		
		private readonly IDictionary<Object, RunningAverage> itemAverages;		
		private readonly IDictionary<Object, RunningAverage> userAverages;		
		private readonly RunningAverage overallAveragePrefValue;
		private bool averagesBuilt;
		
		private readonly ReentrantLock refreshLock;
		
		private readonly ReaderWriterLock buildAveragesLock;

		public ItemUserAverageRecommender(DataModel dataModel) 
            :base(dataModel)
		{
			this.itemAverages = new Dictionary<Object, RunningAverage>(1003);
			this.userAverages = new Dictionary<Object, RunningAverage>(1003);
			this.overallAveragePrefValue = new FullRunningAverage();
			this.refreshLock = new ReentrantLock();
			this.buildAveragesLock = new ReaderWriterLock();
		}

		
		public override IList<RecommendedItem> Recommend(Object userID, int howMany, Rescorer<Item> rescorer)
		{
			if (userID == null) {
				throw new ArgumentNullException("userID is null");
			}
			if (howMany < 1) {
				throw new ArgumentException("howMany must be at least 1");
			}
			if (rescorer == null) {
				throw new ArgumentNullException("rescorer is null");
			}
			if (log.IsDebugEnabled) {
				log.Debug("Recommending items for user ID '" + userID + '\'');
			}
			CheckAverageDiffsBuilt();

			User theUser = this.DataModel.GetUser(userID);
			ISet<Item> allItems = GetAllOtherItems(theUser);

			TopItems.Estimator<Item> estimator = new Estimator(this, userID);

			IList<RecommendedItem> topItems = TopItems.GetTopItems(howMany, allItems, rescorer, estimator);

			if (log.IsDebugEnabled) 
            {
				log.Debug("Recommendations are: " + topItems);
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
			return DoEstimatePreference(userID, itemID);
		}


		private double DoEstimatePreference(Object userID, Object itemID) 
		{
            buildAveragesLock.AcquireReaderLock(Constants.INFINITE_TIMEOUT);
			try 
			{
                RunningAverage itemAverage;
                if (!itemAverages.TryGetValue(itemID, out itemAverage))
                {
					return Double.NaN;
				}
				RunningAverage userAverage;
                if (!userAverages.TryGetValue(userID, out userAverage))
				{
					return Double.NaN;
				}
				double userDiff = userAverage.Average - overallAveragePrefValue.Average;
				return itemAverage.Average + userDiff;
			} 
            finally 
            {
				buildAveragesLock.ReleaseReaderLock();			
			}
		}

		private void CheckAverageDiffsBuilt() 
		{
			if (!averagesBuilt) {
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
					Object userID = user.ID;
                    Preference[] prefs = user.GetPreferencesAsArray();
					foreach (Preference pref in prefs) 
					{
						Object itemID = pref.Item.ID;
						double value = pref.Value;
						AddDatumAndCrateIfNeeded(itemID, value, itemAverages);
						AddDatumAndCrateIfNeeded(userID, value, userAverages);
						overallAveragePrefValue.AddDatum(value);
					}
				}
				averagesBuilt = true;
			} finally {

				buildAveragesLock.ReleaseWriterLock();
			}
		}

		private static void AddDatumAndCrateIfNeeded(Object itemID,
		                                             double value,
		                                             IDictionary<Object, RunningAverage> averages) 
		{
			RunningAverage itemAverage;
            if (!averages.TryGetValue(itemID, out itemAverage))
            {
				itemAverage = new FullRunningAverage();
				averages.Add(itemID, itemAverage);
			}
			itemAverage.AddDatum(value);
		}

		/**
		 * {@inheritDoc}
		 */
		public override void SetPreference(Object userID, Object itemID, double value) 
		{
			DataModel dataModel = this.DataModel;
			double prefDelta;
			try 
			{
				User theUser = dataModel.GetUser(userID);
				Preference oldPref = theUser.GetPreferenceFor(itemID);
				prefDelta = oldPref == null ? value : value - oldPref.Value;
			} 
            catch (NoSuchElementException) 
            {
				prefDelta = value;
			}
			base.SetPreference(userID, itemID, value);
            buildAveragesLock.AcquireWriterLock(Constants.INFINITE_TIMEOUT);
			try 
			{
                RunningAverage itemAverage;
                if (!itemAverages.TryGetValue(itemID, out itemAverage) || itemAverage == null)
				{
					RunningAverage newItemAverage = new FullRunningAverage();
					newItemAverage.AddDatum(prefDelta);
					itemAverages.Add(itemID, newItemAverage);
				} else {
					itemAverage.ChangeDatum(prefDelta);
				}

				RunningAverage userAverage;

                if (!userAverages.TryGetValue(userID, out userAverage) || userAverage == null)
                {
					RunningAverage newUserAverage = new FullRunningAverage();
					newUserAverage.AddDatum(prefDelta);
					userAverages.Add(userID, newUserAverage);
				} else {
					userAverage.ChangeDatum(prefDelta);
				}
				overallAveragePrefValue.ChangeDatum(prefDelta);
			} finally {
				buildAveragesLock.ReleaseWriterLock();
			}
		}

		/**
		 * {@inheritDoc}
		 */
		public override void RemovePreference(Object userID, Object itemID) 
		{
			DataModel dataModel = this.DataModel;
			User theUser = dataModel.GetUser(userID);
			Preference oldPref = theUser.GetPreferenceFor(itemID);
			base.RemovePreference(userID, itemID);
			if (oldPref != null) 
			{
				double value = oldPref.Value;
                buildAveragesLock.AcquireWriterLock(Constants.INFINITE_TIMEOUT);
				try 
                {
                    RunningAverage itemAverage;

                    if (!itemAverages.TryGetValue(itemID, out itemAverage) || itemAverage == null)
                    {
						throw new IllegalStateException("No preferences exist for item ID: " + itemID);
					}
					itemAverage.RemoveDatum(value);

					RunningAverage userAverage;
                    if (!userAverages.TryGetValue(userID, out userAverage) || userAverage == null)
                    {
						throw new IllegalStateException("No preferences exist for user ID: " + userID);
					}
					userAverage.RemoveDatum(value);
					overallAveragePrefValue.RemoveDatum(value);
				} finally {
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
                    refreshLock.Lock();
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
			return "ItemUserAverageRecommender";
        }

        #region Internal Estimator class

        internal sealed class Estimator : TopItems.Estimator<Item> 
		{		
			private readonly Object userID;
            private readonly ItemUserAverageRecommender host;

            internal Estimator(ItemUserAverageRecommender host, Object userID) 
			{
                this.host = host;
				this.userID = userID;
			}
			public double Estimate(Item item) 
			{
				return host.DoEstimatePreference(userID, item.ID);
			}
        }
        #endregion

    }

}	