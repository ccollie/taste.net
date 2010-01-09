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
	using Taste.Transforms;


    /// <summary>
    /// <p>Like <see cref="taste.Correlation.PearsonCorrelation">PearsonCorrelation</see>, but compares relative ranking of preference 
    /// values instead of preference values themselves. That is, each <see cref="taste.Model.User">User</see>'s preferences are sorted and 
    /// then assign a rank as their preference value, with 1 being assigned to the least preferred item. Then the Pearson itemCorrelation of 
    /// these rank values is computed.</p>
    ///
    /// author Sean Owen
    /// </summary>
	public class SpearmanCorrelation : UserCorrelation 
	{
		private UserCorrelation rankingUserCorrelation;
		private ReentrantLock refreshLock;

		public SpearmanCorrelation(DataModel dataModel) 
		{
			if (dataModel == null) 
			{
				throw new ArgumentNullException("dataModel is null");
			}
			this.rankingUserCorrelation = new PearsonCorrelation(dataModel);
			this.refreshLock = new ReentrantLock();
		}

		public SpearmanCorrelation(UserCorrelation rankingUserCorrelation) 
		{
			if (rankingUserCorrelation == null) 
			{
				throw new ArgumentNullException("rankingUserCorrelation is null");
			}
			this.rankingUserCorrelation = rankingUserCorrelation;
			this.refreshLock = new ReentrantLock();		
		}

		public double GetUserCorrelation(User user1, User user2) 
		{
			if (user1 == null || user2 == null) 
            {
				throw new ArgumentNullException("user1 or user2 is null");
			}
			return rankingUserCorrelation.GetUserCorrelation(new RankedPreferenceUser(user1),
			                                              new RankedPreferenceUser(user2));
		}


        public PreferenceInferrer PreferenceInferrer 
		{
            set { rankingUserCorrelation.PreferenceInferrer = value; }
		}


        public void Refresh() 
		{
            if (refreshLock.TryLock())
            {
                try
                {
                    refreshLock.Lock();
                    rankingUserCorrelation.Refresh();
                }
                finally
                {
                    refreshLock.Unlock();
                }
            }
        }

        #region RankedPreferenceUser

        /// <summary>
        /// <p>A simple <see cref="taste.Model.User"/>User</see> decorator which will always 
        /// return the underlying <see cref="taste.Model.User"/>User</see>'s preferences in order by value.</p>
        /// </summary>
		private class RankedPreferenceUser : User 
		{

			private User delegateUser;

			public RankedPreferenceUser(User delegateUser) 
			{
				this.delegateUser = delegateUser;
			}

			public Object ID
			{
				get {return delegateUser.ID;}
			}


			public Preference GetPreferenceFor(Object itemID) 
			{
				throw new NotSupportedException();
			}


			public IEnumerable<Preference> GetPreferences()
			{
				// todo: cache this
                Preference[] source = delegateUser.GetPreferencesAsArray();
                int length = source.Length;
                Preference[] sortedPrefs = new Preference[length];
                Array.Copy(source, sortedPrefs, length);
				for (int i = 0; i < length; i++) 
				{
					Item item = sortedPrefs[i].Item;
					sortedPrefs[i] = new GenericPreference(this, item, (double) (i + 1));
				}
                Array.Sort<Preference>(sortedPrefs, ByValuePreferenceComparer.Instance);
				return sortedPrefs;
			}


            public Preference[] GetPreferencesAsArray() 
            {
                Preference[] sortedPrefs = delegateUser.GetPreferencesAsArray();
                Array.Sort(sortedPrefs, ByValuePreferenceComparer.Instance);
                for (int i = 0; i < sortedPrefs.Length; i++) 
                {
                    sortedPrefs[i] = new GenericPreference(this, sortedPrefs[i].Item, (double) (i + 1));
                }
                Array.Sort(sortedPrefs, ByItemPreferenceComparer.Instance);
                return sortedPrefs;
            }
  	 

			public override int GetHashCode() 
			{
				return delegateUser.GetHashCode();
			}

			public override bool Equals(Object o) 
			{
				return (o is RankedPreferenceUser) && 
					delegateUser.Equals(((RankedPreferenceUser) o).delegateUser);
			}

			public int CompareTo(User user) 
			{
				return delegateUser.CompareTo(user);
			}

			public override String ToString() 
            {
				return "RankedPreferenceUser[user:" + delegateUser.ToString() + ']';
			}
        }

        #endregion
    }

}