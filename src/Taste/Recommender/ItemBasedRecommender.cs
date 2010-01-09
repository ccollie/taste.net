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
    /// <p>Interface implemented by "item-based" recommenders.</p>
    /// 
    /// @author Sean Owen
    /// @since 1.1	 
    /// </summary>
	public interface ItemBasedRecommender : Recommender 
	{
        /// <summary>
        /// Returns <see cref="taste.Model.Item">Item</see>s most similar to the given item, ordered from most similar to least
        /// </summary>
        /// <param name="itemID">ID of <see cref="taste.Model.Item">Item</see> for which to find most similar other <see cref="taste.Model.Item">Item</see>s</param>
        /// <param name="howMany">
        /// desired number of most similar <see cref="taste.Model.Item">Item</see>s to find
        /// </param>
        /// <returns></returns>
        IList<RecommendedItem> MostSimilarItems(Object itemID, int howMany);


        /// <summary>
        /// Returns <see cref="taste.Model.Item">Item</see>s most similar to the given item, ordered from most similar to least
        /// </summary>
        /// <param name="itemID">ID of <see cref="taste.Model.Item">Item</see> for which to find most similar other <see cref="taste.Model.Item">Item</see>s</param>
        /// <param name="howMany">
        /// desired number of most similar <see cref="taste.Model.Item">Item</see>s to find
        /// </param>
        /// <param name="rescorer">
        /// <see cref="taste.recommeder.Restorer">Rescorer</see> which can adjust item-item Correlation estimates used to determine most similar items
        /// </param>
        /// <returns></returns>
		IList<RecommendedItem> MostSimilarItems(Object itemID,
		                                       int howMany,
		                                       Rescorer<Pair<Item,Item>> rescorer);

        /// <summary>
        /// Returns <see cref="taste.Model.Item">Item</see>s most similar to the given item, ordered from most similar to least
        /// </summary>
        /// <param name="itemIDs">IDs of <see cref="taste.Model.Item">Item</see> for which to find most similar other <see cref="taste.Model.Item">Item</see>s</param>
        /// <param name="howMany">
        /// desired number of most similar <see cref="taste.Model.Item">Item</see>s to find
        /// </param>
        /// <returns></returns>
		IList<RecommendedItem> MostSimilarItems(List<Object> itemIDs, int howMany);


        /// <summary>
        /// Returns <see cref="taste.Model.Item">Item</see>s most similar to the given items, ordered from most similar to least
        /// </summary>
        /// <param name="itemIDs">IDs of <see cref="taste.Model.Item">Item</see> for which to find most similar other <see cref="taste.Model.Item">Item</see>s</param>
        /// <param name="howMany">desired number of most similar <see cref="taste.Model.Item">Item</see>s to find</param>
        /// <param name="rescorer">
        /// <see cref="taste.Recommender.Rescorer">Rescorer</see> which can adjust item-item Correlation estimates used to determine most similar items
        /// </param>
        /// <returns></returns>
		IList<RecommendedItem> MostSimilarItems(List<Object> itemIDs,
		                                       int howMany,
		                                       Rescorer<Pair<Item,Item>> rescorer);

        /// <summary>
        /// <p>Lists the <see cref="taste.mode.Item">Item</see>s that were most influential in recommending a given item to a given user.
        /// Exactly how this is determined is left to the implementation, but, generally this will return items
        /// that the user prefers and that are similar to the given item.</p>
        /// 
        /// <p>This returns a list} of <see cref="taste.Recommender.RecommendedItem">RecommendedItem</see> which is a little misleading since it's
        /// returning recommend<strong>ing</strong> items, but, I thought it more natural to just reuse this
        /// class since it encapsulates an <see cref="taste.Model.Item">Item</see> and value. The value here does not necessarily have
        /// a consistent interpretation or expected range; it will be higher the more influential the <see cref="taste.Model.Item">Item</see>
        /// was in the recommendation.</p>
        /// </summary>
        /// <param name="userID">
        /// ID of the <see cref="taste.Model.User">User</see> who was recommended the <see cref="taste.Model.Item">Item</see>
        /// </param>
        /// <param name="itemID">ID of the <see cref="taste.Model.Item">Item</see> that was recommended</param>
        /// <param name="howMany">maximum number of {@link Item}s to return</param>
        /// <returns>
        /// List of <see cref="taste.Recommender.RecommendedItem">RecommendedItem</see>, ordered from most influential in recommended the given
        /// <see cref="taste.Model.Item">Item</see> to least
        /// </returns>
		IList<RecommendedItem> RecommendedBecause(Object userID, Object itemID, int howMany);
	}
}