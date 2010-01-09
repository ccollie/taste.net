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

namespace Taste.Model
{
	using System;

    /// <summary>
    /// <p>Implementations of this interface represent items that <see cref="taste.Model.User">User</see>s have
    /// preferences for, and which can be recommended to them. <see cref="taste.Model.Item">Item</see>s must have
    /// a unique ID of some kind, and must be <see cref="System.IComparable">Comparable</see>.</p>
    /// 
    /// @author Sean Owen
    /// </summary>
	public interface Item : IComparable<Item> 
    {
        /// <summary>
        /// Gets the unique ID for this item
        /// </summary>
		Object ID {get;}

        /// <summary>
        /// Returns true if and only if this <see cref="taste.Model.Item">Item</see> can be recommended to a user;
        /// for example, this could be false for an <see cref="taste.Model.Item">Item</see> that is no longer
        /// available but which remains valuable for recommendation
        /// </summary>
		bool IsRecommendable {get;}
	}

}