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
    /// <p>Implementations of this interface define a notion of itemCorrelation between two
    /// <see cref="taste.Model.User">User</see>s. Implementations should return values in the range -1.0 to 1.0, with
    /// 1.0 representing perfect Correlation.</p>
    ///
    /// author Sean Owen
    /// <seealso cref="taste.Correlation.ItemCorrelation">ItemCorrelation</seealso>
    /// </summary>
    public interface UserCorrelation : Refreshable 
	{
        /// <summary>
        /// <p>Returns the "itemCorrelation", or degree of similarity, of two <see cref="taste.Model.User">User</see>s, based
        /// on the their preferences.</p>
        /// </summary>
        /// <param name="user1">first user</param>
        /// <param name="user2">second user</param>
        /// <returns>itemCorrelation between the two users</returns>
        double GetUserCorrelation(User user1, User user2);

        /// <summary>
        /// <p>Attaches a <see cref="taste.Correlation.PreferenceInferrer">PreferenceInferrer</see> see to the 
        /// <see cref="taste.Correlation.UserCorrelation">UserCorrelation</see> implementation.</p>
        /// </summary>
        PreferenceInferrer PreferenceInferrer { set;}
	}

}