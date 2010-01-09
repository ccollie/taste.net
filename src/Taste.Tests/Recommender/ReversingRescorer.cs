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

namespace Taste.Tests.Recommender
{
    using Taste.Recommender;
    using Taste.Model;
    using Taste.Transforms;
    using System;
    using NUnit.Framework;

    /// <summary>
    /// <p>Simple {@link Rescorer} which negates the given score, thus reversing
    /// order of rankings.</p>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class ReversingRescorer<T> : Rescorer<T>
    {
        public double Rescore(T thing, double originalScore)
        {
            return double.IsNaN(originalScore) ? Double.NaN : -originalScore;
        }

        public bool IsFiltered(T thing)
        {
            return false;
        }
    }
}