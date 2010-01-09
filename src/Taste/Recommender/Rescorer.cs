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


    /// <summary>
    /// <p>A <see cref="taste.Recommender.Rescorer">Rescorer</see> simply assigns a new "score" to a thing like an
    /// <see cref="taste.Model.Item">Item</see> or <see cref="taste.Model.User">User</see> which a <see cref="taste.Recommender.Recommender">Recommender</see>
    /// is considering returning as a top recommendation. It may be used to arbitrarily re-rank the results
    /// according to application-specific logic before returning recommendations. For example, an application
    /// may want to boost the score of items in a certain category just for one request.</p>
    ///
    /// @author Sean Owen
    /// since 1.1
    /// </summary>
    /// <typeparam name="T"></typeparam>
	public interface Rescorer<T> 
	{
        /// <summary>
        /// Return the modified score
        /// </summary>
        /// <param name="thing">
        /// thing (<see cref="taste.Model.Item">Item</see> or <see cref="taste.Model.User">User</see> really) to rescore
        /// </param>
        /// <param name="originalScore">original score or {@link Double#NaN} to indicate that this should be excluded entirely</param>
        /// <returns></returns>
		double Rescore(T thing, double originalScore);

        bool IsFiltered(T thing);
	}
}