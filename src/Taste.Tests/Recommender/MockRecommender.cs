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

namespace Taste.Tests.Recommender
{
    using System;
    using Taste.Common;
	using Taste.Model;
	using Taste.Recommender;
	using System.Collections;
	using System.Collections.Generic;


	public class MockRecommender : Recommender 
	{

		private readonly AtomicInteger recommendCount;

		internal MockRecommender(AtomicInteger recommendCount) 
		{
			this.recommendCount = recommendCount;
		}

		public IList<RecommendedItem> Recommend(Object userID, int howMany) 
		{
			recommendCount.IncrementAndGet();
            List<RecommendedItem> l = new List<RecommendedItem>();
            l.Add( new GenericRecommendedItem(new GenericItem<String>("1"), 1.0 ) );
            return l;
		}

		public IList<RecommendedItem> Recommend( Object userID,
											    int howMany,
											    Rescorer<Item> rescorer) 
		{
			return Recommend(userID, howMany);
		}

		public double EstimatePreference( Object userID,  Object itemID) 
		{
			recommendCount.IncrementAndGet();
			return 0.0;
		}

		public void SetPreference(Object userID,  Object itemID,  double value) 
		{
			// do nothing
		}

		public void RemovePreference( Object userID,  Object itemID) 
		{
			// do nothing
		}

		public DataModel DataModel
		{
            get
            {
                User user1 = new GenericUser<String>("1", GetEmptyPreferences());
                User user2 = new GenericUser<String>("2", GetEmptyPreferences());
                User user3 = new GenericUser<String>("3", GetEmptyPreferences());
                List<User> users = new List<User>(3);
                users.Add(user1);
                users.Add(user2);
                users.Add(user3);
                return new GenericDataModel(users);
            }
		}

        private List<Preference> GetEmptyPreferences()
        {
            return new List<Preference>();
        }

		public void Refresh() 
		{
			// do nothing
		}
	}
}