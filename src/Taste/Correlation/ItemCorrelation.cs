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

namespace Taste.Correlation
{
	using System;
	using Taste.Common;
	using Taste.Model;


    /// <summary>
    ///<p>Implementations of this interface define a notion of itemCorrelation between two 
    /// {@link taste.Model.Item}s. Implementations should return values in the range -1.0 to 1.0, with
    /// 1.0 representing perfect Correlation.</p>
    ///
    /// author Sean Owen
    /// <see cref="UserCorrelation">UserCorrelation</see>
    /// </summary>
	public interface ItemCorrelation : Refreshable 
	{

        /// <summary>
        /// Returns the "itemCorrelation", or degree of similarity, of two <see cref="Item">item</see>s, based
        /// on the preferences that {@link taste.Model.User}s have expressed for the items.</p>
        /// </summary>
        /// <param name="item1">first item</param>
        /// <param name="item2">second item</param>
        /// <returns></returns>
		double GetItemCorrelation(Item item1, Item item2);
	}
}