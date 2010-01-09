/*
 * Copyright 2007 and onwards Sean Owen
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

namespace Taste.Transforms
{
	using System;
	using System.Collections.Generic;
	using Taste.Common;
	using Taste.Model;


    /// <summary>
    /// <p>Implementations encapsulate some transformation on Correlation values between two
    /// things, where things might be <see cref="taste.Model.User">User</see>s or <see cref="taste.Model.Item">Item</see>s or
    /// something else.</p>
    /// 
    /// author Sean Owen
    /// @since 1.7
    /// </summary>
    /// <typeparam name="T"></typeparam>
	public interface CorrelationTransform<T> : Refreshable 
	{
        /// <summary>
        /// Transforms a Correlation between two items;
        /// </summary>
        /// <param name="thing1"></param>
        /// <param name="thing2"></param>
        /// <param name="value">
        /// original Correlation between thing1 and thing2 (should be in [-1,1])
        /// </param>
        /// <returns>the transformed Correlation</returns>
        /// <remarks>
        /// returned value should be in [-1,1]
        /// </remarks>
		double TransformCorrelation(T thing1, T thing2, double value);
	}
}