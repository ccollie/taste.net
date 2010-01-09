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
    using System.Collections;
	using System.Collections.Generic;
	using Taste.Common;


    /// <summary>
    /// <p>A simple <see cref="taste.Model.User">User</see> which has simply an ID and some collection of
    /// <see cref="taste.Model.Preference">Preference</see>s.</p>
    ///
    /// author Sean Owen
    /// </summary>
    /// <typeparam name="IdT"></typeparam>
	[Serializable]
	public class GenericUser<IdT> : User, IComparable<User> where IdT:IComparable<IdT>
	{		
		private static readonly Preference[] NO_PREFS = new Preference[0];
		private readonly IdT id;
		private readonly IDictionary<Object, Preference> data;
		
		// Use an array for maximum performance
		private Preference[] values;

		public GenericUser(IdT id, ICollection<Preference> preferences) 
		{
			if (id == null) 
			{
				throw new ArgumentNullException("id is null");
			}
			this.id = id;

            data = new Dictionary<Object, Preference>();
			if (preferences == null || preferences.Count == 0) 
			{
				values = NO_PREFS;
			} 
            else 
            {
				values = new Preference[preferences.Count];
                preferences.CopyTo(values, 0);
				foreach (Preference preference in values) 
				{
					// Is this hacky?
					if (preference is GenericPreference) 
					{
						((GenericPreference) preference).User = this;
					}
                    
					data[preference.Item.ID] = preference;
				}
				Array.Sort(values, ByItemPreferenceComparer.Instance);
			}
		}

		public IdT ID
		{
			get {return id;}
		}

		public Preference GetPreferenceFor(Object itemID) 
		{
            Preference result;
            if (data.TryGetValue(itemID, out result))
                return result;
			return null;
		}

		public IEnumerable<Preference> GetPreferences() 
		{
            return values;
		}

        public Preference[] GetPreferencesAsArray()
        {
            return values;
        }

        public void SetPreferences(IEnumerable<Preference> prefs)
        {
            //TODO:
        }


		public override int GetHashCode() 
		{
			return id.GetHashCode();
		}

		public override bool Equals(Object obj) 
		{
			return (obj is User) && ((User) obj).ID.Equals(id);
		}

		public override String ToString() 
		{
			return "User[id:" + id.ToString() + ']';
		}

		public int CompareTo(User o) 
		{
			return id.CompareTo((IdT) o.ID);
		}		

        #region User Members

        object User.ID
        {
            get { return this.id; }
        }

        #endregion
    }

}	