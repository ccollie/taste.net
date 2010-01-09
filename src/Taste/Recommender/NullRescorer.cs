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
namespace Taste.Recommender
{
	using System;
	using System.Collections.Generic;
	using Taste.Common;	
	using Taste.Model;
	using Taste.Recommender;



    /// <summary>
    /// A simple {@link Rescorer} which always returns the original score.</p>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class NullRescorer<T> : Rescorer<T>
    {
        private static readonly Rescorer<T> itemInstance = new NullRescorer<T>();

        public static Rescorer<T> Instance
        {
            get { return itemInstance; }
        }

        private NullRescorer()
        {
            // do nothing
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="thing">thing to rescore</param>
        /// <param name="originalScore">current score for item</param>
        /// <returns>same originalScore as new score, always</returns>
        public double Rescore(T thing, double originalScore)
        {
            return originalScore;
        }

        public bool IsFiltered(T thing)
        {
            return false;
        }

        public override String ToString()
        {
            return "NullRescorer";
        }
    }

}