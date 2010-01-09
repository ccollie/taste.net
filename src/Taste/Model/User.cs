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


    /// <summary>
    /// <p>Implementations represent a user, who has preferences for <see cref="taste.Model.Item">Item</see>s.</p>
    /// 
    /// @author Sean Owen
    /// </summary>
	public interface User : IComparable<User> 
	{

        /// <summary>
        /// Return a unique user ID
        /// </summary>
		Object ID {get ;}


        /// <summary>
        /// Returns this user's preference for the given item.
        /// </summary>
        /// <param name="itemID">ID of item to get the user's preference for</param>
        /// <returns>user's <see cref="taste.Model.Preference">Preference</see> for that 
        /// <see cref="taste.Model.Item">Item</see>, or <code>null</code> if the user expresses
        /// no such preference
        /// </returns>
		Preference GetPreferenceFor(Object itemID);

        /// <summary>
        /// <p>Returns a sequence of <see cref="taste.Model.Preference">Preference</see>s for this <see cref="taste.Model.User">User</see> which can be 
        /// iterated over. Note that the sequence <em>must</em> be "in order": ordered by <see cref="taste.Model.Item">Item</see>.</p>
        /// </summary>
        /// <returns>return a sequence of <see cref="taste.Model.Preference">Preference</see>s</returns>
		IEnumerable<Preference> GetPreferences();

        /// <summary>
        /// Returns an array of preferences for this User.
        /// Note that the sequence <em>must</em> be "in order": ordered by <see cref="taste.Model.Item">Item</see>.
        /// </summary>
        /// <returns>
        /// an array of preferences
        /// </returns>
        Preference[] GetPreferencesAsArray();
	}
}