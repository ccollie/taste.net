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

namespace Taste.Common
{
    using System;

    /// <summary>
    /// <p>Interface for classes that can keep track of a running average of a series of numbers.
    /// One can add to or remove from the series, as well as update a datum in the series.
    /// The class does not actually keep track of the series of values, just its running average,
    /// so it doesn't even matter if you remove/change a value that wasn't added.</p>
    /// 
    /// @author Sean Owen
    /// </summary>
	public interface RunningAverage 
	{
        /// <summary>
        /// Add a new datum to the average
        /// </summary>
        /// <param name="datum">new item to add to the running average</param>
        /// <remarks>
        /// throws ArgumentException if datum is NaN
        /// </remarks>
		void AddDatum(double datum);

        /// <summary>
        /// Remove a datum from the running average
        /// </summary>
        /// <param name="datum">item to remove from the running average</param>
        /// <remarks>
        /// throws an exception if count == 0
        /// </remarks>
        void RemoveDatum(double datum);

        /// <summary>
        /// </summary>
        /// <param name="delta">amount by which to change a datum in the running average</param>
        /// <remarks>
        /// throws ArgumentException if delta is NaN
        /// throws IllegalStateException if count is 0
        /// </remarks>
		void ChangeDatum(double delta);

		int Count {get;}

		double Average {get;}
	}
	
}