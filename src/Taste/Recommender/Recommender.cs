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


    /// <summary>
    /// <p>Implementations of this interface can recommend <see cref="taste.Model.Item">Item</see>s for a
    /// <see cref="taste.Model.User">User</see>. Implementations will likely take advantage of several
    /// classes in other packages here to compute this.</p>
    ///
    /// author Sean Owen
    /// </summary>
	public interface Recommender : Refreshable 
	{
        /// <summary>
        /// Returns a List of recommended <see cref="taste.Recommender.RecommendedItem">RecommendedItem</see>s, ordered from most strongly
        /// recommend to least
        /// </summary>
        /// <param name="userID">user for which recommendations are to be computed</param>
        /// <param name="howMany">desired number of recommendations</param>
        /// <returns></returns>
		IList<RecommendedItem> Recommend(Object userID, int howMany);

        /// <summary>
        /// Returns a list of recommended <see cref="taste.Recommender.RecommendedItem">RecommendedItem</see>s, ordered from most strongly
		/// recommend to least
        /// </summary>
        /// <param name="userID">user for which recommendations are to be computed</param>
        /// <param name="howMany">desired number of recommendations</param>
        /// <param name="rescorer">rescoring function to apply before list of recommendations is determined</param>
        /// <returns></returns>
        IList<RecommendedItem> Recommend(Object userID, int howMany, Rescorer<Item> rescorer);


        /// <summary>
        /// Returns an estimated preference if the user has not expressed a preference for the item, or else
        /// the user's actual preference for the item. If a preference cannot be estimated, returns
        /// <see cref="System.Double#NaN">NaN</see>
        /// </summary>
        /// <param name="userID">user ID whose preference is to be estimated</param>
        /// <param name="itemID">item ID to estimate preference for</param>
        /// <returns>the estimated preference for the item</returns>
		double EstimatePreference(Object userID, Object itemID);


        /// <summary>
        /// Sets a user's preference for an item.
        /// </summary>
        /// <param name="userID">user to set preference for</param>
        /// <param name="itemID">item to set preference for</param>
        /// <param name="value">preference value</param>
        void SetPreference(Object userID, Object itemID, double value);


        /// <summary>
        /// Remove a <see cref="taste.Model.User">User</see>'s preference for an item.
        /// </summary>
        /// <param name="userID">user from which to remove preference</param>
        /// <param name="itemID">item for which to remove preference</param>
		void RemovePreference(Object userID, Object itemID);


        /// <summary>
        /// Gets the <see cref="taste.Model.DataModel">DataModel</see> used by this <see cref="taste.Recommender.Recommender">Recommender</see>.
        /// </summary>
		DataModel DataModel {get;}
	}
}