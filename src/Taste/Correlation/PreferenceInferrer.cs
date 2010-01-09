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
    /// <p>Implementations of this interface compute an inferred preference for a <see cref="taste.Model.User">User</see> and an 
    /// <see cref="taste.Model.Item"/>Item</see> that the user has not expressed any preference for. This might be an average of other 
    /// preferences scores from that user, for example. This technique is sometimes called "default voting".</p>
    ///
    /// author Sean Owen
    /// </summary>
    public interface PreferenceInferrer : Refreshable 
	{
        /// <summary>
        /// <p>Infers the given <see cref="taste.Model.User">User</see>'s preference value for an <see cref="taste.Model.Item"/>Item.</p>
        /// </summary>
        /// <param name="user"><see cref="taste.Model.User">User</see> to infer preference for</param>
        /// <param name="item"><see cref="taste.Model.Item">Item</see> to infer preference for</param>
        /// <returns>inferred preference</returns>
		double InferPreference(User user, Item item);
	}
}