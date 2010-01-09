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

namespace Taste.Model
{
	using System;
	using System.Collections.Generic;
    using System.Collections.ObjectModel;
	using Taste.Common;

    /// <summary>
    /// <p>A simple <see cref="taste.Model.DataModel">DataModel</see> which uses a given List of <see cref="taste.Model.User">User</see>s as
    /// its data source. This implementation is mostly useful for small experiments and is not
    /// recommended for contexts where performance is important.</p>
    ///
    /// @author Sean Owen
    /// </summary>
	[Serializable]
	public class GenericDataModel : DataModel
	{
		private static IEnumerable<Preference> NO_PREFS = new EmptyEnumerable<Preference>();
        private static Preference[] NO_PREFS_ARRAY = new Preference[0];
		private List<User> users;
		private Dictionary<Object, User> userMap;
		private List<Item> items;
		private Dictionary<Object, Item> itemMap;
		private Dictionary<Object, Preference[]> preferenceForItems;

		/**
		 * <p>Creates a new {@link GenericDataModel} from the given {@link User}s (and their preferences).
		 * This {@link DataModel} retains all this information in memory and is effectively immutable.</p>
		 *
		 * @param users {@link User}s to include in this {@link GenericDataModel}
		 */
		public GenericDataModel(IEnumerable<User> users) 
		{
			if (users == null) 
			{
				throw new ArgumentNullException("users is null");
			}

			this.userMap = new Dictionary<Object, User>();
			this.itemMap = new Dictionary<Object, Item>();

            // I'm abusing generics a little here since I want to use this (huge) map to hold Lists,
            // then arrays, and don't want to allocate two Maps at once here
			Dictionary<Object, List<Preference>> prefsForItems = new Dictionary<Object, List<Preference>>();
			foreach (User user in users) 
			{
				userMap.Add(user.ID, user);
                Preference[] userPrefs = user.GetPreferencesAsArray();

				foreach (Preference preference in userPrefs) 
				{
					Item item = preference.Item;
					Object itemID = item.ID;

                    if (!itemMap.ContainsKey(itemID))
					    itemMap[itemID] = item;

					List<Preference> prefs;
                    if (!prefsForItems.TryGetValue(itemID, out prefs))
                    {
                        prefs = new List<Preference>();
                        prefsForItems.Add(itemID, prefs);
                    }
					prefs.Add(preference);
				}
			}

			List<User> usersCopy = new List<User>(userMap.Values);
            usersCopy.Sort();
			//this.users = Collections.unmodifiableList(usersCopy);
            this.users = usersCopy;

			List<Item> itemsCopy = new List<Item>(itemMap.Values);
            itemsCopy.Sort();
			//this.items = Collections.unmodifiableList(itemsCopy);
            this.items = itemsCopy;

            // Swap out lists for arrays here -- 
            preferenceForItems = new Dictionary<Object, Preference[]>();
            foreach (KeyValuePair<Object, List<Preference>> entry in prefsForItems) 
            {
                List<Preference> list = entry.Value;
                Preference[] prefsAsArray = list.ToArray();
                Array.Sort<Preference>(prefsAsArray, ByUserPreferenceComparer.Instance);
                preferenceForItems.Add(entry.Key, prefsAsArray);
            } 	                 
		}

		/**
		 * <p>Creates a new {@link GenericDataModel} containing an immutable copy of the data from another
		 * given {@link DataModel}.</p>
		 *
		 * @param dataModel {@link DataModel} to copy
		 * @if an error occurs while retrieving the other {@link DataModel}'s users
		 */
		public GenericDataModel(DataModel dataModel)
            :this(dataModel.GetUsers())
		{
		}

		/**
		 * {@inheritDoc}
		 */
		public IEnumerable<User> GetUsers() 
        {
			return users;
		}

		/**
		 * {@inheritDoc}
		 *
		 * @throws NoSuchElementException if there is no such {@link User}
		 */
		public User GetUser(Object id) 
		{
			User user = null;
			if (!userMap.TryGetValue(id, out user))			
			{
				throw new NoSuchElementException();
			}
			return user;
		}

		/**
		 * {@inheritDoc}
		 */
		public IEnumerable<Item> GetItems() 
		{
			return items;
		}


		public Item GetItem(Object id) 
		{
			Item item = null;
			if (!itemMap.TryGetValue(id, out item))
			{
				throw new NoSuchElementException();
			}
			return item;
		}

		public IEnumerable<Preference> GetPreferencesForItem(Object itemID) 
		{
            Preference[] prefs;
            if (!preferenceForItems.TryGetValue(itemID, out prefs))
                prefs = null;
			return prefs == null ? NO_PREFS : prefs;
		}


        public Preference[] GetPreferencesForItemAsArray(Object itemID)
        {
            Preference[] prefs;
            if (!preferenceForItems.TryGetValue(itemID, out prefs))
                prefs = null;
            return prefs == null ? NO_PREFS_ARRAY : prefs;
        }

		/**
		 * {@inheritDoc}
		 */
		public int GetNumItems() 
		{
			return items.Count;
		}

		/**
		 * {@inheritDoc}
		 */
		public int GetNumUsers() 
		{
			return users.Count;
		}


		public void SetPreference(Object userID, Object itemID, double value) 
		{
            throw new NotSupportedException();
		}


		public void RemovePreference(Object userID, Object itemID) 
		{
			throw new NotSupportedException();
		}

		/**
		 * {@inheritDoc}
		 */
		public void Refresh() 
		{
			// Does nothing
		}

		public override String ToString() 
		{
			return "GenericDataModel[users:" + users + ']';
		}

	}

}	