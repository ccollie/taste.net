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

namespace Taste.Eval
{
	using System;
	using Taste.Common;
	using Taste.Model;
    using Taste.Recommender;

    /// <summary>
    /// <p>Implementations of this inner interface are simple helper classes which create a
    /// <see cref="taste.Recommender.Recommender">Recommender</see> to be evaluated based on the given 
    /// <see cref="taste.Model.DataModel">DataModel</see>.</p>
    /// </summary>
	public interface RecommenderBuilder 
	{
        /// <summary>
        /// <p>Builds a <see cref="taste.Recommender.Recommender">Recommender</see> implementation to be evaluated, using the given <see cref="taste.Model.DataModel">DataModel</see>.</p>		 
        /// </summary>
        /// <param name="dataModel">
        /// <see cref="taste.Model.DataModel">DataModel</see> to build the <see cref="taste.Recommender.Recommender">Recommender</see> on
        /// </param>
        /// <returns>
        /// A <see cref="taste.Recommender.Recommender">Recommender</see> based upon the given <see cref="taste.Model.DataModel">DataModel</see>
        /// </returns>
		Recommender BuildRecommender(DataModel dataModel);
	}
}