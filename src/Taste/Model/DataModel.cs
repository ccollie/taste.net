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
	using Taste.Common;


    /// <summary>
    /// <p>Implementations represent a repository of information about <see cref="taste.Model.User">User</see>s and their
    /// associated <see cref="taste.Model.Preference">Preference</see>s for <see cref="taste.Model.Item">Item</see>s.</p>
    ///
    /// author Sean Owen
    /// </summary>
	public interface DataModel : Refreshable 
	{
        /// <summary>
        /// Returns a list of all <see cref="taste.Model.User">User</see>s in the Model, ordered by <see cref="taste.Model.User">User</see>.
        /// </summary>
        /// <returns></returns>
		IEnumerable<User> GetUsers();

        /// <summary>
        /// Returns a <see cref="taste.Model.User">User</see> by id; 		
		/// Throws NoSuchElementException if there is no such {@link User}
        /// </summary>
        /// <param name="id">user ID</param>
        /// <returns><see cref="taste.Model.User">User</see> who has that ID</returns>
		User GetUser(Object id);

        /// <summary>
        /// Returns a enumeration of all <see cref="taste.Model.Item">Item</see>s in the Model, 
        /// order by <see cref="taste.Model.Item">Item</see>.
        /// </summary>
        /// <returns></returns>
		IEnumerable<Item> GetItems();


        /// <summary>
        /// Returns an item by id
        /// <remarks>
        /// throws NoSuchElementException if there is no such <see cref="taste.Model.Item">Item</see>
        /// </remarks>
        /// </summary>
        /// <param name="id">item ID</param>
        /// <returns><see cref="taste.Model.Item">Item</see> that has the given ID</returns>
		Item GetItem(Object id);


        /// <summary>
        /// Get all existing <see cref="taste.item.Preference">Preference</see>s expressed for that item, 
        /// ordered by <see cref="taste.item.User">User</see>
        /// </summary>
        /// <param name="itemID">item ID</param>
        /// <returns>Item preferences</returns>
		IEnumerable<Preference> GetPreferencesForItem(Object itemID);


        /// <summary>
        /// Returns all existing <see cref="taste.Model.Preference">Preference</see>s expressed for a specific item, 
        /// ordered by <see cref="taste.Model.User">User</see>, as an array
        /// <remarks>
        /// throws TasteException if an error occurs while accessing the data
        /// </remarks>
        /// </summary>
        /// <param name="itemID">item ID</param>
        /// <returns></returns>
        Preference[] GetPreferencesForItemAsArray(Object itemID);

        /// <summary>
        /// Returns the total number of <see cref="taste.Model.Item">Item</see>s known to the Model. This is generally the union
        /// of all <see cref="taste.Model.Item">Item</see>s preferred by at least one <see cref="taste.Model.User">User</see> but could include more.
        /// </summary>
        /// <returns></returns>
        int GetNumItems();


        /// <summary>
        /// Returns the total number of <see cref="taste.Model.User">User</see>s known to the Model.
        /// </summary>
        /// <returns>The number of Users in the Model</returns>
		int GetNumUsers();


        /// <summary>
        /// <p>Sets a particular preference (item plus rating) for a user.</p>
        /// </summary>
        /// <param name="userID">user to set preference for</param>
        /// <param name="itemID">item to set preference for</param>
        /// <param name="value">preference value</param>
		void SetPreference(Object userID, Object itemID, double value);

        /// <summary>
        /// <p>Removes a particular preference for a user.</p>
        /// </summary>
        /// <param name="userID">user from which to remove preference</param>
        /// <param name="itemID">item to remove preference for</param>
		void RemovePreference(Object userID, Object itemID);
	}
}