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
    using System.Collections.ObjectModel;
    using Taste.Common;
    using Taste.Model;
    using Taste.Recommender;
    using log4net;

    /// <summary>
    /// <p>A <see cref="taste.Recommender.Recommender">Recommender</see> which caches the results from another 
    /// <see cref="taste.Recommender.Recommender">Recommender</see> in memory.
    /// </summary>
    public class CachingRecommender : Recommender
    {
        private static ILog log = LogManager.GetLogger(typeof(CachingRecommender));
        private readonly Recommender recommender;
        private readonly AtomicInteger maxHowMany;
        private readonly SoftCache<Object, Recommendations> recommendationCache;
        private readonly SoftCache<Pair<object, object>, Double> estimatedPrefCache;
        private readonly ReentrantLock refreshLock;

        #region Constructor

        public CachingRecommender(Recommender recommender) 
	    {
		    if (recommender == null) 
            {
			    throw new ArgumentNullException("Recommender is null");
		    }
		    this.recommender = recommender;
		    this.maxHowMany = new AtomicInteger(1);
		    // Use "num users" as an upper limit on cache size. Rough guess.
            int numUsers = recommender.DataModel.GetNumUsers();
		    this.recommendationCache =
			    new SoftCache<Object, Recommendations>(
				    new RecommendationRetriever(this.recommender, this.maxHowMany),
				    numUsers);
		    this.estimatedPrefCache =
			    new SoftCache<Pair<object, object>, Double>(new EstimatedPrefRetriever(this.recommender), numUsers);
		    this.refreshLock = new ReentrantLock();
        }

        #endregion

        #region Recommend
 

        public IList<RecommendedItem> Recommend(Object userID, int howMany)
        {
            if (userID == null)
            {
                throw new ArgumentNullException("user ID is null");
            }

            if (howMany < 1)
            {
                throw new ArgumentException("howMany must be at least 1");
            }

            lock (maxHowMany)
            {
                if (howMany > maxHowMany.Value)
                {
                    maxHowMany.Set(howMany);
                }
            }

            Recommendations recommendations = recommendationCache[userID];
            IList<RecommendedItem> items = recommendations.Items;
            if (items.Count < howMany && !recommendations.NoMoreRecommendableItems)
            {
                Clear(userID);

                recommendations = recommendationCache[userID];
                items = recommendations.Items;
                if (items.Count < howMany) 
                {
         	  	  	recommendations.NoMoreRecommendableItems = true;
                }
            }
            if (items.Count <= howMany)
                return items;
            if (items is List<RecommendedItem>)
            {
                List<RecommendedItem> temp = (List<RecommendedItem>)items;
                return temp.GetRange(0, howMany);
            }
            else
            {
                List<RecommendedItem> temp = new List<RecommendedItem>(howMany);
                foreach (RecommendedItem item in items)
                {
                    temp.Add(item);
                    if (--howMany == 0)
                        break;
                }
                return temp;
            }
        }


        public IList<RecommendedItem> Recommend(Object userID, int howMany, Rescorer<Item> rescorer)
        {
            // Hmm, hard to recommendationCache this since the rescorer may change
            return recommender.Recommend(userID, howMany, rescorer);
        }

        #endregion

        #region Preferences

        public double EstimatePreference(Object userID, Object itemID)
        {
            return estimatedPrefCache.Get(new Pair<Object, Object>(userID, itemID));
        }


        public void SetPreference(Object userID, Object itemID, double value)
        {
            recommender.SetPreference(userID, itemID, value);
            Clear(userID);
        }


        public void RemovePreference(Object userID, Object itemID)
        {
            recommender.RemovePreference(userID, itemID);
            Clear(userID);
        }

        #endregion


        public DataModel DataModel
        {
            get { return recommender.DataModel; }
        }

        public void Refresh() 
	    {
            if (refreshLock.TryLock())
            {
                try
                {             
                    recommender.Refresh();
                    Clear();
                }
                finally
                {
                    refreshLock.Unlock();
                }
            }
	    }

        /**
         * <p>Clears cached recommendations for the given user.</p>
         *
         * @param userID clear cached data associated with this user ID
         */
        public void Clear(Object userID)
        {
            if (log.IsDebugEnabled)
            {
                log.Debug("Clearing recommendations for user ID '" + userID + "'...");
            }
            recommendationCache.Remove(userID);
        }

        /**
         * <p>Clears all cached recommendations.</p>
         */
        public void Clear()
        {
            log.Debug("Clearing all recommendations...");
            recommendationCache.Clear();
        }


        public override String ToString()
        {
            return "CachingRecommender[Recommender:" + recommender + ']';
        }

        #region Helper Classes

        #region RecommendationRetriever Helper Class

        private class RecommendationRetriever : SoftCacheRetriever<Object, Recommendations>
        {
            private readonly Recommender recommender;
            private readonly AtomicInteger maxHowMany;

            public RecommendationRetriever(Recommender recommender, AtomicInteger maxHowMany)
            {
                this.recommender = recommender;
                this.maxHowMany = maxHowMany;
            }

            public Recommendations GetValue(Object key)
            {
                if (log.IsDebugEnabled)
                {
                    log.Debug("Retrieving new recommendations for user ID '" + key + '\'');
                }
                int count = maxHowMany.Value;
                IList<RecommendedItem> items = recommender.Recommend(key, count);
                ReadOnlyCollection<RecommendedItem> roc = new ReadOnlyCollection<RecommendedItem>(items);
                return new Recommendations(roc);
            }
        }

        #endregion

        #region EstimatedPrefRetriever Helper Class

        private class EstimatedPrefRetriever : SoftCacheRetriever<Pair<object, object>, Double>
        {
            private readonly Recommender recommender;

            public EstimatedPrefRetriever(Recommender recommender)
            {
                this.recommender = recommender;
            }

            public Double GetValue(Pair<object, object> key)
            {
                Object userID = key.First;
                Object itemID = key.Second;
                if (log.IsDebugEnabled)
                {
                    log.Debug("Retrieving estimated preference for user ID '" + userID + "\' and item ID \'" +
                             itemID.ToString() + '\'');
                }
                return recommender.EstimatePreference(userID, itemID);
            }
        }
        #endregion

        #region Recommendations Helper Class

        private sealed class Recommendations
        {
            private readonly IList<RecommendedItem> items;
            private bool noMoreRecommendableItems;

            internal Recommendations(IList<RecommendedItem> items)
            {
                this.items = items;
                this.noMoreRecommendableItems = false;
            }

            internal IList<RecommendedItem> Items
            {
                get { return items; }
            }

            internal bool NoMoreRecommendableItems
            {
                get { return noMoreRecommendableItems; }
                set { noMoreRecommendableItems = value; }
            }
        }
        #endregion

        #endregion

    }


}