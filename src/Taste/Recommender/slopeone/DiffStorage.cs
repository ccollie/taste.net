/*
 * Copyright 2007 and onwards Sean Owen
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
	using Taste.Common;
	using Taste.Model;
    using Iesi.Collections.Generic;


    /// <summary>
    /// <p>Implementations store item-item preference diffs for a
    /// <see cref="taste.Recommender.SlopeOne.SlopeOneRecommender">SlopeOneRecommender</see>.
    /// It actually does a bit more for this implementation, like listing all items that may be
    /// considered for recommedation, in order to maximize what implementations can do to optimize the 
    /// slope-one algorithm.</p>
    /// 
    /// @author Sean Owen
    /// @since 1.6
    /// <seealso cref="taste.Recommender.SlopeOne.SlopeOneRecommender">SlopeOneRecommender</seealso>
    /// </summary>
    public interface DiffStorage : Refreshable 
	{
        /// <summary>
        /// return the <see cref="taste.Common.RunningAverage">RunningAverage</see> encapsulating the average difference in preferences
        /// between items corresponding to <code>itemID1</code> and <code>itemID2</code>, in that direction; that is,
        /// it's the average of item 2's preferences minus item 1's preferences
        /// </summary>
        /// <param name="itemID1"></param>
        /// <param name="itemID2"></param>
        /// <returns></returns>
		RunningAverage GetDiff(Object itemID1, Object itemID2);

        /// <summary>
        /// Gets an array of <see cref="taste.Common.RunningAverage">RunningAverage</see>s for that user's item-item diffs
        /// </summary>
        /// <param name="userID">user ID to get diffs for</param>
        /// <param name="itemID">itemID to assess</param>
        /// <param name="prefs">user's preferendces</param>
        /// <returns></returns>
		RunningAverage[] GetDiffs(Object userID, Object itemID, IList<Preference> prefs);


        /// <summary>
        /// Returns a <see cref="taste.Common.RunningAverage">RunningAverage</see> encapsulating the average preference for the given item
        /// </summary>
        /// <param name="itemID"></param>
        /// <returns></returns>
		RunningAverage GetAverageItemPref(Object itemID);

        /// <summary>
        /// <p>Updates internal data structures to reflect an update in a preference value for an item.</p>
        /// </summary>
        /// <param name="itemID">item to update preference value for</param>
        /// <param name="prefDelta">amount by which preference value changed (or its old value, if being removed</param>
        /// <param name="remove">if <code>true</code>, operation reflects a removal rather than change of preference</param>
        void UpdateItemPref(Object itemID, double prefDelta, bool remove);


        /// <summary>
        /// Return <see cref="taste.Model.Item">Item</see>s that may possibly be recommended to the given user, which may not be all
        /// <see cref="taste.Model.Item">Item</see>s since the item-item diff matrix may be sparses
        /// </summary>
        /// <param name="userID"></param>
        /// <returns></returns>
		ISet<Item> GetRecommendableItems(Object userID);
	}
}