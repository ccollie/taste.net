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
    /// <p>Interface implemented by "user-based" recommenders.</p>
    /// 
    /// @author Sean Owen
    /// @since 1.1
    /// </summary>
    public interface UserBasedRecommender : Recommender 
	{

        /// <summary>
        /// Returns <see cref="taste.Model.User">User</see>s most similar to the given user
        /// </summary>
        /// <param name="userID">
        /// ID of <see cref="taste.Model.User">User</see> for which to find most similar other <see cref="taste.Model.User">User</see>s
        /// </param>
        /// <param name="howMany">
        /// howMany desired number of most similar {@link User}s to find
        /// </param>
        /// <returns></returns>
		IList<User> MostSimilarUsers(Object userID, int howMany);

        /// <summary>
        /// Returns a list of <see cref="taste.Model.User">User</see>s most similar to the given user
        /// </summary>
        /// <param name="userID">ID of {@link User} for which to find most similar other {@link User}s</param>
        /// <param name="howMany">desired number of most similar {@link User}s to find</param>
        /// <param name="rescorer">
        /// <see cref="taste.Recommender.Rescorer">Rescorer</see> which can adjust user-user Correlation estimates used to determine most similar users
        /// </param>
        /// <returns>
        /// <see cref="taste.Model.User">User</see>s most similar to the given user
        /// </returns>
		IList<User> MostSimilarUsers(Object userID, int howMany, Rescorer<Pair<User,User>> rescorer);
	}
}