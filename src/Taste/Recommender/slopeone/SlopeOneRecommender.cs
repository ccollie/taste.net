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

namespace Taste.Recommender.SlopeOne
{
	using System;
	using System.Collections.Generic;
    using Iesi.Collections.Generic;
	using Taste.Common;
	using Taste.Model;
	using Taste.Recommender;
	using Taste.Recommender.SlopeOne;
    using log4net;

    /// <summary>
    /// <p>A basic "slope one" Recommender. (See an <a href="http://www.daniel-lemire.com/fr/abstracts/SDM2005.html">
    /// excellent summary here</a> for example.) This <see cref="taste.Recommender.Recommender">Recommender</see> is especially
    /// suitable when user preferences are updating frequently as it can incorporate this information without
    /// expensive recomputation.</p>
    ///  
    /// <p>This implementation can also be used as a "weighted slope one" Recommender.</p>
    /// 
    /// @author Sean Owen
    /// </summary>
	public class SlopeOneRecommender : AbstractRecommender
    {
        #region Private Member Variables
        private static readonly ILog log = LogManager.GetLogger(typeof(SlopeOneRecommender));

		private readonly bool weighted;
		private readonly bool stdDevWeighted;

		private readonly DiffStorage diffStorage;
        #endregion

        #region Constructor

        /// <summary>
        /// <p>Creates a default (weighted) <see cref="taste.Recommender.SlopeOne.SlopeOneRecommender">SlopeOneRecommender</see> based on 
        /// the given <see cref="taste.Model.DataModel">DataModel</see>.</p>
        /// </summary>
        /// <param name="dataModel">the data Model</param>
        public SlopeOneRecommender(DataModel dataModel)
            : this(dataModel, true, true, new MemoryDiffStorage(dataModel, true, false, long.MaxValue))
		{        
        }

        /// <summary>
        /// <p>Creates a <see cref="taste.Recommender.SlopeOne.SlopeOneRecommender">SlopeOneRecommender</see> based on the given 
        /// <see cref="taste.Model.DataModel">DataModel</see>.</p>
        /// <p>If <code>weighted</code> is set, acts as a weighted slope one Recommender.
        /// This implementation also includes an experimental "standard deviation" weighting which weights
        /// item-item ratings diffs with lower standard deviation more highly, on the theory that they are more
        /// reliable.</p>
        /// </summary>
        /// <param name="dataModel"></param>
        /// <param name="weighted">if <code>true</code>, acts as a weighted slope one Recommender</param>
        /// <param name="stdDevWeighted">use optional standard deviation weighting of diffs</param>
        /// <param name="diffStorage"></param>
        /// <remarks>
        /// Throws ArgumentException if <code>diffStorage</code> is null, or stdDevWeighted is set
        /// when weighted is not set
        /// </remarks>
		public SlopeOneRecommender(DataModel dataModel,
								   bool weighted,
								   bool stdDevWeighted,
								   DiffStorage diffStorage) 
            :base(dataModel)
        {
			if (stdDevWeighted && !weighted) 
            {
				throw new ArgumentException("weighted required when stdDevWeighted is set");
			}
			if (diffStorage == null) 
            {
				throw new ArgumentNullException("diffStorage is null");
			}
			this.weighted = weighted;
			this.stdDevWeighted = stdDevWeighted;
			this.diffStorage = diffStorage;
        }

        #endregion

        #region Recommend
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

			User theUser = this.DataModel.GetUser(userID);
			ISet<Item> allItems = diffStorage.GetRecommendableItems(userID);

			TopItems.Estimator<Item> estimator = new Estimator(this, theUser);

			IList<RecommendedItem> topItems = TopItems.GetTopItems(howMany, allItems, rescorer, estimator);

			if (log.IsDebugEnabled) 
            {
				log.Debug("Recommendations are: " + topItems);
			}
			return topItems;
        }
        #endregion

        #region Preferences

        public override double EstimatePreference(Object userID, Object itemID) 
        {
			User theUser = this.DataModel.GetUser(userID);
			Preference actualPref = theUser.GetPreferenceFor(itemID);
			if (actualPref != null) 
            {
				return actualPref.Value;
			}
			return DoEstimatePreference(theUser, itemID);
		}

		private double DoEstimatePreference(User theUser, Object itemID) 
        {
			double count = 0.0;
			double totalPreference = 0.0;
			Preference[] prefs = theUser.GetPreferencesAsArray();
			RunningAverage[] averages = diffStorage.GetDiffs(theUser.ID, itemID, prefs);

			for (int i = 0; i < prefs.Length; i++)
            {
				RunningAverage averageDiff = averages[i];
                if (averageDiff != null)
                {
                    Preference pref = prefs[i];
					double averageDiffValue = averageDiff.Average;
					if (weighted) 
                    {
						double weight = (double) averageDiff.Count;
						if (stdDevWeighted) 
                        {
							double stdev = ((RunningAverageAndStdDev) averageDiff).StandardDeviation;
							if (!Double.IsNaN(stdev)) 
                            {
								weight /= 1.0 + stdev;
							}
							// If stdev is NaN, then it is because count is 1. Because we're weighting by count,
							// the weight is already relatively low. We effectively assume stdev is 0.0 here and
							// that is reasonable enough. Otherwise, dividing by NaN would yield a weight of NaN
							// and disqualify this pref entirely
							// (Thanks Daemmon)
						}
						totalPreference += weight * (pref.Value + averageDiffValue);
						count += weight;
					} 
                    else 
                    {
						totalPreference += pref.Value + averageDiffValue;
						count += 1.0;
					}
				}
			}

			if (count <= 0.0) 
            {
				RunningAverage itemAverage = diffStorage.GetAverageItemPref(itemID);
				return itemAverage == null ? Double.NaN : itemAverage.Average;
			} 
            else 
            {
				return totalPreference / count;
			}
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
			diffStorage.UpdateItemPref(itemID, prefDelta, false);
		}


        public override void RemovePreference(Object userID, Object itemID) 
        {
			DataModel dataModel = this.DataModel;
			User theUser = dataModel.GetUser(userID);
			Preference oldPref = theUser.GetPreferenceFor(itemID);
			base.RemovePreference(userID, itemID);
			if (oldPref != null) 
            {
				diffStorage.UpdateItemPref(itemID, oldPref.Value, true);
			}
        }

        #endregion

        /**
		 * {@inheritDoc}
		 */
		public override void Refresh() 
        {
			diffStorage.Refresh();
		}

		public override String ToString() 
        {
			return "SlopeOneRecommender[weighted:" + weighted + ", stdDevWeighted:" + stdDevWeighted +
				   ", diffStorage:" + diffStorage + ']';
        }

        #region Internal Helper Classes
        internal class Estimator : TopItems.Estimator<Item> 
		{		
			private readonly User theUser;
            private readonly SlopeOneRecommender host;

            internal Estimator(SlopeOneRecommender host, User theUser) 
			{
                this.host = host;
				this.theUser = theUser;
			}

			public double Estimate(Item item) 
			{
				return host.DoEstimatePreference(theUser, item.ID);
			}
        }
        #endregion
    }
}