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
    using Taste.Correlation;
    using Taste.Neighborhood;
	using Taste.Recommender;
    using Iesi.Collections.Generic;
    using log4net;


    /// <summary>
    /// <p>A simple <see cref="taste.Recommender.Recommender">Recommender</see> which uses a given 
    /// <see cref="taste.Model.DataModel">DataModel</see> and <see cref="taste.Neighborhood.UserNeighborhood">UserNeighborhood</see>
    /// to produce recommendations.</p>
    /// 
    /// author Sean Owen
    /// </summary>
	public class GenericUserBasedRecommender : AbstractRecommender, UserBasedRecommender 
	{
		private static ILog log = LogManager.GetLogger(typeof(GenericUserBasedRecommender));		
		private readonly UserNeighborhood neighborhood;		
		private readonly UserCorrelation correlation;	
		private readonly ReentrantLock refreshLock;

		public GenericUserBasedRecommender(DataModel dataModel,
		                                   UserNeighborhood neighborhood,
		                                   UserCorrelation correlation)
            : base(dataModel)
        {
			if (neighborhood == null) 
            {
				throw new ArgumentNullException("Neighborhood is null");
			}
			this.neighborhood = neighborhood;
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
				log.DebugFormat("Recommending items for user ID '{0}'", userID);
			}

			User theUser = this.DataModel.GetUser(userID);
			ICollection<User> theNeighborhood = neighborhood.GetUserNeighborhood(userID);
			if (log.IsDebugEnabled) 
            {
				log.DebugFormat("UserNeighborhood is: {0} " , neighborhood);
			}

			if (theNeighborhood.Count == 0) 
            {
                return new List<RecommendedItem>();
			}

			ISet<Item> allItems = GetAllOtherItems(theNeighborhood, theUser);
			if (log.IsDebugEnabled) 
            {
				log.Debug("Items in Neighborhood which user doesn't prefer already are: " + allItems);
			}


			TopItems.Estimator<Item> estimator = new Estimator(this, theUser, theNeighborhood);

			IList<RecommendedItem> topItems = TopItems.GetTopItems(howMany, allItems, rescorer, estimator);

			if (log.IsDebugEnabled) 
            {
				log.Debug("Recommendations are: " + topItems);  // TODO: Format this better
			}
			return topItems;
		}


        public override double EstimatePreference(Object userID, Object itemID) 
		{
			DataModel model = this.DataModel;
			User theUser = model.GetUser(userID);
			Preference actualPref = theUser.GetPreferenceFor(itemID);
			if (actualPref != null) 
            {
				return actualPref.Value;
			}
			ICollection<User> theNeighborhood = neighborhood.GetUserNeighborhood(userID);
			Item item = model.GetItem(itemID);
			return DoEstimatePreference(theUser, theNeighborhood, item);
		}

		
		public IList<User> MostSimilarUsers(Object userID, int howMany) 
		{
			return MostSimilarUsers(userID, howMany, NullRescorer<Pair<User,User>>.Instance);
		}

		
		public IList<User> MostSimilarUsers(Object userID,
		                                   int howMany,
		                                   Rescorer<Pair<User,User>> rescorer) 
		{
			if (rescorer == null) 
            {
				throw new ArgumentNullException("rescorer is null");
			}
			User toUser = this.DataModel.GetUser(userID);
			TopItems.Estimator<User> estimator = new MostSimilarEstimator(toUser, correlation, rescorer);
			return DoMostSimilarUsers(userID, howMany, estimator);
		}

		
		private IList<User> DoMostSimilarUsers(Object userID,
		                                      int howMany,
		                                      TopItems.Estimator<User> estimator) 
		{
			DataModel model = this.DataModel;
			User toUser = model.GetUser(userID);
			ICollection<User> allUsers = new HashedSet<User>(/*Model.GetNumUsers()*/);
			foreach (User user in model.GetUsers()) 
			{
				allUsers.Add(user);
			}
			allUsers.Remove(toUser);
			return TopItems.GetTopUsers(howMany, allUsers, NullRescorer<User>.Instance, estimator);
		}


		private double DoEstimatePreference(User theUser, ICollection<User> theNeighborhood, Item item) 
		{
			if (theNeighborhood.Count == 0) 
			{
				return Double.NaN;
			}
			double preference = 0.0;
			double totalCorrelation = 0.0;
			foreach (User user in theNeighborhood) 
			{
                if (!user.Equals(theUser))
                {
                    // See GenericItemBasedRecommender.doEstimatePreference() too
                    Preference pref = user.GetPreferenceFor(item.ID);
                    if (pref != null)
                    {
                        double theCorrelation = correlation.GetUserCorrelation(user, pref.User) + 1.0;
                        if (!Double.IsNaN(theCorrelation))
                        {
                            preference += theCorrelation * pref.Value;
                            totalCorrelation += theCorrelation;
                        }

                    }
                }
			}
			return totalCorrelation == 0.0 ? Double.NaN : preference / totalCorrelation;
		}

		
		private static ISet<Item> GetAllOtherItems(IEnumerable<User> theNeighborhood, User theUser) 
		{
			ISet<Item> allItems = new HashedSet<Item>();
			foreach (User user in theNeighborhood) 
			{
                Preference[] prefs = user.GetPreferencesAsArray();
				foreach (Preference preference in prefs) 
				{
					Item item = preference.Item;
					// If not already preferred by the user, add it
					if (theUser.GetPreferenceFor(item.ID) == null) 
					{
						allItems.Add(item);
					}
				}
			}
			return allItems;
		}


        public override void Refresh() 
		{
            if (refreshLock.TryLock())
            {
                try
                {
                    refreshLock.Lock();
                    base.Refresh();
                    neighborhood.Refresh();
                }
                finally
                {
                    refreshLock.Unlock();
                }
            }
		}

		
		
		public override String ToString() 
        {
			return "GenericUserBasedRecommender[Neighborhood:" + neighborhood + ']';
        }

        #region Internal Estimator classes

        internal class MostSimilarEstimator : TopItems.Estimator<User> 
		{		
			private readonly User toUser;
			
			private readonly UserCorrelation correlation;
			
			private readonly Rescorer<Pair<User,User>> rescorer;
			internal MostSimilarEstimator( User toUser,
			                              UserCorrelation correlation,
			                              Rescorer<Pair<User,User>> rescorer) 
            {
				this.toUser = toUser;
				this.correlation = correlation;
				this.rescorer = rescorer;
			}

			public double Estimate(User user) 
            {             
                Pair<User, User> pair = new Pair<User, User>(toUser, user);
                if (rescorer.IsFiltered(pair))
                    return Double.NaN;

                double originalEstimate = correlation.GetUserCorrelation(toUser, user);
				return rescorer.Rescore(pair, originalEstimate);
			}
		}

		private class Estimator : TopItems.Estimator<Item> 
        {			
			private readonly ICollection<User> theNeighborhood;
            GenericUserBasedRecommender host;
            User user;

            public Estimator(GenericUserBasedRecommender host, User theUser, ICollection<User> theNeighborhood) 
			{
                this.host = host;
                this.user = theUser;
				this.theNeighborhood = theNeighborhood;
			}

			public double Estimate(Item item) 
            {
				return host.DoEstimatePreference(user, theNeighborhood, item);
			}
        }

        #endregion
    }

}	