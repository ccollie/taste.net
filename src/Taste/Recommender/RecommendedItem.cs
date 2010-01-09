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
	using Taste.Model;


    /// <summary>
    /// <p>Implementations encapsulate items that are recommended, and include
    /// the <see cref="taste.Model.Item">Item</see> recommended and a value expressing
    /// the strength of the preference.</p>
    ///
    /// @author Sean Owen
    /// </summary>
	public interface RecommendedItem : IComparable<RecommendedItem> 
	{
        /// <summary>
        /// Gets the recommended <see cref="taste.Model.Item">Item</see>
        /// </summary>
		Item Item {get;}

        /// <summary>
        /// <p>Gets a value expressing the strength of the preference for the recommended
        /// <see cref="taste.Model.Item">Item</see>. The range of the values depends on the implementation.
        /// Implementations must use larger values to express stronger preference.</p>       
        /// </summary>
		double Value {get;}
	}
}