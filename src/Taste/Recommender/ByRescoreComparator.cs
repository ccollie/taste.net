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
    using System.Diagnostics;
    using System.Collections.Generic;
    using Taste.Common;
    using Taste.Model;
    using Taste.Recommender;


    /// <summary>
    /// <p>A simple <see cref="taste.Recommender.Rescorer">Rescorer</see> which always returns the original score.</p>
    /// 
    /// author Sean Owen
    /// </summary>
    [Serializable]
    public class ByRescoreComparator :  IComparer<RecommendedItem>
    {
        private readonly Rescorer<Item> rescorer;

        public ByRescoreComparator(Rescorer<Item> rescorer)
        {
            if (rescorer == null)
            {
                throw new ArgumentNullException("rescorer is null");
            }
            this.rescorer = rescorer;
        }

        public int Compare(RecommendedItem o1, RecommendedItem o2) 
	    {
		    double rescored1 = rescorer.Rescore(o1.Item, o1.Value);
		    double rescored2 = rescorer.Rescore(o2.Item, o2.Value);
		    Debug.Assert(!double.IsNaN(rescored1));
		    Debug.Assert(!double.IsNaN(rescored2));
		    if (rescored1 < rescored2) 
		    {
			    return 1;
		    } 
            else if (rescored1 > rescored2) 
            {
			    return -1;
		    } 
            else 
            {
			    return 0;
		    }
	    }


        public override String ToString()
        {
            return "ByRescoreComparator[rescorer:" + rescorer + ']';
        }

    }

}