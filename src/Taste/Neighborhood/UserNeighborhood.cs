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

namespace Taste.Neighborhood
{
	using System;
	using System.Collections.Generic;
	using Taste.Common;
    using Taste.Model;

    /// <summary>
    /// <p>Implementations of this interface compute a "Neighborhood" of <see cref="taste.Model.User">User</see>s like a
    /// given <see cref="taste.Model.User">User</see>. This Neighborhood can be used to compute recommendations then.</p>
    /// </summary>
	public interface UserNeighborhood : Refreshable 
	{

        /// <summary>
        /// Gets a list of users in the neighbourhood of a user with the given id.
        /// </summary>
        /// <param name="userID">ID of user for which a Neighborhood will be computed</param>
        /// <returns>a collection of <see cref="taste.Model.User">User</see>s in the Neighborhood</returns>
        /// <exception cref="taste.Common.TasteException"></exception>
		ICollection<User> GetUserNeighborhood(Object userID);
	}
}