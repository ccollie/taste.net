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

namespace Taste.Transforms
{
	using System;
	using System.Collections.Generic;
	using Taste.Common;
	using Taste.Correlation;
	using Taste.Model;
	using Taste.Transforms;	


    /// <summary>
    /// <p>Normalizes preference values for a <see cref="taste.Model.User">User</see> by converting them to
    /// <a href="http://mathworld.wolfram.com/z-Score.html">"z-scores"</a>. This process
    /// normalizes preference values to adjust for variation in mean and variance of a
    /// user's preferences.</p>
    /// <p>Imagine two users, one who tends to rate every movie he/she sees four or five stars,
    /// and another who uses the full one to five star range when assigning ratings. This
    /// transform normalizes away the difference in scale used by the two users so that both
    /// have a mean preference of 0.0 and a standard deviation of 1.0.</p>
    /// 
    /// @author Sean Owen
    /// </summary>
	public class ZScore : PreferenceTransform2 
	{
		private readonly SoftCache<User, RunningAverageAndStdDev> meanAndStdevs;

		public ZScore() 
		{
			this.meanAndStdevs = new SoftCache<User, RunningAverageAndStdDev>(new MeanStdevRetriever());
			Refresh();
		}

		public double GetTransformedValue(Preference pref)
		{
			RunningAverageAndStdDev meanAndStdev = meanAndStdevs.Get(pref.User);            
			if (meanAndStdev.Count > 1) 
			{
				double stdev = meanAndStdev.StandardDeviation;
				if (stdev > 0.0) 
				{
					return (pref.Value - meanAndStdev.Average) / stdev;
				}
			}
			return 0.0;
		}

		public void Refresh() 
		{
			// do nothing
		}

		public override String ToString() 
		{
			return "ZScore";
		}

		private class MeanStdevRetriever : SoftCacheRetriever<User, RunningAverageAndStdDev> 
		{
			public RunningAverageAndStdDev GetValue(User user)
			{
				RunningAverageAndStdDev running = new FullRunningAverageAndStdDev();
                Preference[] prefs = user.GetPreferencesAsArray();
				foreach (Preference preference in prefs) 
				{
					running.AddDatum(preference.Value);
				}
				return running;
			}
		}

	}

}	