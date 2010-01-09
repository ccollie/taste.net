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

namespace Taste.Tests.Neighborhood
{
    using System;
    using System.Collections.Generic;
    using Taste.Correlation;
    using Taste.Recommender;
    using Taste.Model;
    using NUnit.Framework;

    /**
     * @author Sean Owen
     */
    public sealed class DummyCorrelation : UserCorrelation, ItemCorrelation 
    {

	    public double GetUserCorrelation(User user1, User user2) 
	    {
            Preference p1 = null, p2 = null;              
		    foreach (Preference pref1 in user1.GetPreferences())
		    {
                p1 = pref1;
			    break;
		    }
		    foreach (Preference pref2 in user2.GetPreferences())
		    {
                p2 = pref2;
			    break;
		    }
		    return 1.0 / Math.Abs(p1.Value - p2.Value);
	    }

	    public double GetItemCorrelation(Item item1, Item item2) 
	    {
		    // Make up something wacky
		    return (double) (item1.GetHashCode() - item2.GetHashCode());
	    }

        public PreferenceInferrer PreferenceInferrer
	    {
            set { throw new InvalidOperationException(); }
	    }

	    public void Refresh() 
	    {
		    // do nothing
	    }
    }

}
