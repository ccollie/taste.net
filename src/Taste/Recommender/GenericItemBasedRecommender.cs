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
    using Iesi.Collections.Generic;
	using System.Collections.Generic;
	using Taste.Common;
	using Taste.Model;
	using Taste.Recommender;
    using Taste.Correlation;
    using log4net;

    /// <summary>
    /// <p>A simple <see cref="taste.Recommender.Recommender">Recommender</see> which uses a given
    /// <see cref="taste.Model.DataModel">DataModel</see> and <see cref="taste.Correlation.ItemCorrelation">ItemCorrelation</see>
    /// to produce recommendations. This class represents Taste's support for item-based recommenders.</p>
    /// <p>The <see cref="taste.Correlation.ItemCorrelation">ItemCorrelation</see> is the most important point to discuss here. Item-based recommenders
    /// are useful because they can take advantage of something to be very fast: they base their computations
    /// on item Correlation, not user Correlation, and item Correlation is relatively static. It can be
    /// precomputed, instead of re-computed in real time.</p>
    ///
    /// <p>Thus it's strongly recommended that you use <see cref="taste.Correlation.GenericItemCorrelation">GenericItemCorrelation</see>
    /// with pre-computed correlations if you're going to use this class. You can use
    /// <see cref="taste.Correlation.PearsonCorrelation">PearsonCorrelation</see> too, which computes correlations
    /// in real-time, but will probably find this painfully slow for large amounts of data.</p>
    ///	
    /// @author Sean Owen
    /// 
    /// </summary>
    public class GenericItemBasedRecommender : AbstractRecommender, ItemBasedRecommender 
	{
		private static ILog log = LogManager.GetLogger(typeof(GenericItemBasedRecommender));
		
		private readonly ItemCorrelation correlation;
		
		private readonly ReentrantLock refreshLock;

		public GenericItemBasedRecommender(DataModel dataModel, ItemCorrelation correlation) 
            :base(dataModel)
		{
			if (correlation == null) 
            {
				throw new ArgumentNullException("Correlation is null");
			}
			this.correlation = correlation;
			this.refreshLock = new ReentrantLock();
		}

		
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

			User theUser = this.DataModel.GetUser(userID);
			if (GetNumPreferences(theUser) == 0) 
            {
				return new List<RecommendedItem>();
			}

			ISet<Item> allItems = GetAllOtherItems(theUser);

			TopItems.Estimator<Item> estimator = new Estimator(this,theUser);

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
			Item item = this.DataModel.GetItem(itemID);
			return DoEstimatePreference(theUser, item);
		}

		/**
		 * {@inheritDoc}
		 */
		
		public IList<RecommendedItem> MostSimilarItems(Object itemID, int howMany) 
		{
            return MostSimilarItems(itemID, howMany, NullRescorer<Pair<Item, Item>>.Instance);
		}

		
		public IList<RecommendedItem> MostSimilarItems(Object itemID,
		                                              int howMany,
		                                              Rescorer<Pair<Item,Item>> rescorer) 
        {
			if (rescorer == null) 
            {
				throw new ArgumentNullException("rescorer is null");
			}
			Item toItem = this.DataModel.GetItem(itemID);
			TopItems.Estimator<Item> estimator = new MostSimilarEstimator(toItem, correlation, rescorer);
			return DoMostSimilarItems(itemID, howMany, estimator);
		}

		
		public IList<RecommendedItem> MostSimilarItems(List<Object> itemIDs, int howMany) 
        {
            return MostSimilarItems(itemIDs, howMany, NullRescorer<Pair<Item, Item>>.Instance);
		}

		
		
		public IList<RecommendedItem> MostSimilarItems(List<Object> itemIDs,
		                                              int howMany,
		                                              Rescorer<Pair<Item,Item>> rescorer) 
		{
			if (rescorer == null) 
			{
				throw new ArgumentNullException("rescorer is null");
			}
			DataModel model = this.DataModel;
			List<Item> toItems = new List<Item>(itemIDs.Count);
			foreach (Object itemID in itemIDs) 
			{
				toItems.Add(model.GetItem(itemID));
			}
			TopItems.Estimator<Item> estimator = new MultiMostSimilarEstimator(toItems, correlation, rescorer);
			ICollection<Item> allItems = new HashedSet<Item>(/*Model.GetNumItems()*/);
			foreach (Item item in model.GetItems()) 
			{
				allItems.Add(item);
			}
			foreach (Item item in toItems) 
			{
				allItems.Remove(item);
			}
            return TopItems.GetTopItems(howMany, allItems, NullRescorer<Item>.Instance, estimator);
		}

		
		public IList<RecommendedItem> RecommendedBecause(Object userID, Object itemID, int howMany) 
		{
			if (userID == null) 
            {
				throw new ArgumentNullException("userID is null");
			}
			if (itemID == null) 
            {
				throw new ArgumentNullException("itemID is null");
			}
			if (howMany < 1) 
            {
				throw new ArgumentException("howMany must be at least 1");
			}

			DataModel model = this.DataModel;
			User user = model.GetUser(userID);
			Item recommendedItem = model.GetItem(itemID);
			TopItems.Estimator<Item> estimator = new RecommendedBecauseEstimator(user, recommendedItem, correlation);

			ICollection<Item> allUserItems = new HashedSet<Item>();
            Preference[] prefs = user.GetPreferencesAsArray();
			foreach (Preference pref in prefs) 
			{
				allUserItems.Add(pref.Item);
			}
			allUserItems.Remove(recommendedItem);

			return TopItems.GetTopItems(howMany, allUserItems, NullRescorer<Item>.Instance, estimator);
		}

		
		private IList<RecommendedItem> DoMostSimilarItems(Object itemID,
		                                                 int howMany,
		                                                 TopItems.Estimator<Item> estimator) 
		{
			DataModel model = this.DataModel;
			Item toItem = model.GetItem(itemID);
			ICollection<Item> allItems = new HashedSet<Item>(/*Model.GetNumItems()*/);
			foreach (Item item in model.GetItems()) 
			{
				allItems.Add(item);
			}
			allItems.Remove(toItem);
            return TopItems.GetTopItems(howMany, allItems, NullRescorer<Item>.Instance, estimator);
		}

		private double DoEstimatePreference(User theUser, Item item) 
        {
			double preference = 0.0;
			double totalCorrelation = 0.0;
            Preference[] prefs = theUser.GetPreferencesAsArray();
			foreach (Preference pref in prefs) 
            {
				double theCorrelation = correlation.GetItemCorrelation(item, pref.Item);
				if (!double.IsNaN(theCorrelation)) 
                {
					// Why + 1.0? Correlation ranges from -1.0 to 1.0, and we want to use it as a simple
					// weight. To avoid negative values, we add 1.0 to put it in
					// the [0.0,2.0] range which is reasonable for weights
					theCorrelation += 1.0;
					preference += theCorrelation * pref.Value;
					totalCorrelation += theCorrelation;
				}
			}
			return totalCorrelation == 0.0 ? Double.NaN : preference / totalCorrelation;
		}

		private static int GetNumPreferences(User theUser) 
		{
            Preference[] prefs = theUser.GetPreferencesAsArray();
            return prefs.Length;
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
                    correlation.Refresh();
                }
                finally
                {
                    refreshLock.Unlock();
                }
            }
		}

		
		public  override String ToString() 
        {
			return "GenericItemBasedRecommender[Correlation:" + correlation + ']';
        }

        #region Private Estimator Classes

        internal class MostSimilarEstimator : TopItems.Estimator<Item> 
        {			
			private readonly Item toItem;			
			private readonly ItemCorrelation correlation;			
			private readonly Rescorer<Pair<Item,Item>> rescorer;

			internal MostSimilarEstimator( Item toItem,
			                              ItemCorrelation correlation,
			                              Rescorer<Pair<Item,Item>> rescorer) 
            {
				this.toItem = toItem;
				this.correlation = correlation;
				this.rescorer = rescorer;
			}
			public double Estimate(Item item) 
            {
                Pair<Item, Item> pair = new Pair<Item, Item>(toItem, item);
                if (rescorer.IsFiltered(pair))
                    return Double.NaN;

                double originalEstimate = correlation.GetItemCorrelation(toItem, item);
				return rescorer.Rescore(pair, originalEstimate);
			}
		}

		private class Estimator : TopItems.Estimator<Item> 
        {
            private readonly GenericItemBasedRecommender host;
			private readonly User theUser;

			internal Estimator(GenericItemBasedRecommender host, User theUser) 
            {
                this.host = host;
				this.theUser = theUser;
			}

			public double Estimate(Item item) 
            {
				return host.DoEstimatePreference(theUser, item);
			}
		}

		private class MultiMostSimilarEstimator : TopItems.Estimator<Item> 
        {			
			private readonly List<Item> toItems;			
			private readonly ItemCorrelation correlation;			
			private readonly Rescorer<Pair<Item,Item>> rescorer;

			public MultiMostSimilarEstimator(List<Item> toItems,
			                                  ItemCorrelation correlation,
			                                  Rescorer<Pair<Item,Item>> rescorer) 
            {
				this.toItems = toItems;
				this.correlation = correlation;
				this.rescorer = rescorer;
			}

			public double Estimate(Item item) 
            {
				RunningAverage average = new FullRunningAverage();
				foreach (Item toItem in toItems) 
                {
                    Pair<Item, Item> pair = new Pair<Item, Item>(toItem, item);
                    if (rescorer.IsFiltered(pair))
                        continue;
					double estimate = correlation.GetItemCorrelation(toItem, item);
					estimate = rescorer.Rescore(pair, estimate);
					average.AddDatum(estimate);
				}
				return average.Average;
			}
		}

		private class RecommendedBecauseEstimator : TopItems.Estimator<Item> 
		{		
			private readonly User user;		
			private readonly Item recommendedItem;			
			private readonly ItemCorrelation correlation;

			public RecommendedBecauseEstimator(User user,
			                                    Item recommendedItem,
			                                    ItemCorrelation correlation) 
            {
				this.user = user;
				this.recommendedItem = recommendedItem;
				this.correlation = correlation;
			}

			public double Estimate(Item item) 
			{
				Preference pref = user.GetPreferenceFor(item.ID);
				if (pref == null) {
					return Double.NaN;
				}
				double correlationValue = correlation.GetItemCorrelation(recommendedItem, item);
				return (1.0 + correlationValue) * pref.Value;
			}
        }

        #endregion
    }
	
}