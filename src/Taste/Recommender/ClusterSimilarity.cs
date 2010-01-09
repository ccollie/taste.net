/*
 * Copyright 2006 and onwards Sean Owen
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
	using Taste.Recommender;

    /// <summary>
    /// <p>Returns the "similarity" between two clusters of users, according to some
    /// definition of similarity. Subclassses define different notions of similarity.</p>
    /// 
    /// @author Sean Owen
    /// <seealso cref="taste.Recommender.TreeClusteringRecommender">TreeClusteringRecommender</seealso>
    /// </summary>
	public interface ClusterSimilarity : Refreshable 
	{

        /// <summary>
        /// Returns "distance" between clusters; a positive
        /// </summary>
        /// <param name="cluster1">first cluster of {@link User}s</param>
        /// <param name="cluster2">second cluster of {@link User}s</param>
        /// <returns></returns>
		double GetSimilarity(ICollection<User> cluster1, ICollection<User> cluster2);
	}
}