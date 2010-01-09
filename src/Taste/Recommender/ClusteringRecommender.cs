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
    /// <p>Interface implemented by "clustering" recommenders.</p>
    /// </summary>
	public interface ClusteringRecommender : Recommender 
	{
        /// <summary>
        /// <p>Returns the cluster of users to which the given {@link User}, denoted by user ID,
        ///  belongs.</p>
        /// </summary>
        /// <param name="userID">user ID for which to find a cluster</param>
        /// <returns>A collection of <see cref="taste.Model.User">User</see>s in the requested user's cluster</returns>
		ICollection<User> GetCluster(Object userID);

        /**
         * <p>Returns all clusters of users.</p>
         *
         * @return {@link Collection} of {@link Collection}s of {@link User}s
         * @throws TasteException if an error occurs while accessing the {@link Taste.Model.DataModel}
         * @since 1.7
         */
        //ICollection<ICollection<User>> GetClusters();
	}
}