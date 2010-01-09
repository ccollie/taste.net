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
    /// <p>A <see cref="taste.Model.Preference">Preference</see> encapsulates an <see cref="taste.Model.Item">Item</see> and a preference value, 
    /// which indicates the strength of the preference for it. <see cref="taste.Model.Preference">Preference</see>s are associated
    /// to <see cref="taste.Model.User">User</see>s.</p>
    ///
    /// @author Sean Owen
    /// </summary>
	public interface Preference 
	{
        /// <summary>
        /// Gets the <see cref="taste.Model.User">User</see> who prefers the <see cref="taste.Model.Item">Item</see>
        /// </summary>
		User User {get;}


        /// <summary>
        /// Gets the <see cref="taste.Model.Item">Item</see> that is preferred
        /// </summary>
        Item Item { get;}

        /// <summary>
        /// Gets and sets the strength of the preference for that item. Zero should indicate "no preference either way";
        /// positive values indicate preference and negative values indicate dislike
        /// </summary>
		double Value {get; set;}
	}
}