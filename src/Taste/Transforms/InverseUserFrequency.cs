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
    using log4net;


    /// <summary>
    /// Implements an "inverse user frequency" transformation, which boosts preference values for items for which few
    /// users have expressed a preference, and reduces preference values for items for which many users have expressed
    /// a preference. The idea is that these "rare" {@link Item}s are more useful in deciding how similar two users'
    /// tastes are, and so should be emphasized in other calculatioons. This idea is mentioned in
    /// <a href="ftp://ftp.research.microsoft.com/pub/tr/tr-98-12.pdf">Empirical Analysis of Predictive Algorithms for
    /// Collaborative Filtering</a>.</p>
    ///
    /// A scaling factor is computed for each {@link Item} by dividing the total number of users by the number of
    /// users expressing a preference for that item, and taking the log of that value. The log base of this calculation
    /// can be controlled in the constructor. Intuitively, the right value for the base is equal to the average
    /// number of users who express a preference for each item in your Model. If each item has about 100 preferences
    /// on average, 100.0 is a good log base.</p>
    ///
    ///@author Sean Owen
    /// </summary>
	public class InverseUserFrequency : PreferenceTransform2 
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(InverseUserFrequency));
		private readonly DataModel dataModel;
		private double logBase;
		private readonly AtomicReference<Dictionary<Item, Double>> iufFactors;


        /// <summary>
        /// Creates a {@link InverseUserFrequency} transformation. Computations use the given log base.		 
        /// throws IllegalArgumentException if dataModel is <code>null</code> or logBase is {@link Double#NaN} or &lt;= 1.0
        /// </summary>
        /// <param name="dataModel">{@link DataModel} from which to calculate user frequencies</param>
        /// <param name="logBase">calculation logarithm base</param>
		public InverseUserFrequency(DataModel dataModel, double logBase) 
		{
			if (dataModel == null) 
			{
				throw new ArgumentNullException("dataModel is null");
			}
			if (double.IsNaN(logBase) || logBase <= 1.0) 
			{
				throw new ArgumentException("logBase is NaN or <= 1.0");
			}
			this.dataModel = dataModel;
			this.LogBase = logBase;
			this.iufFactors = new AtomicReference<Dictionary<Item, Double>>(new Dictionary<Item, Double>(1009));
			Refresh();
		}

        /// <summary>
        /// Get/Set the log base used in this object's calculations
        /// </summary>
		public double LogBase
		{
			get {return logBase;}
            set { logBase = value; }
		}

		public double GetTransformedValue(Preference pref) 
		{
			double factor;
            if (iufFactors.Value.TryGetValue(pref.Item, out factor))
			{
				return pref.Value * factor;
			}
			return pref.Value;
		}

		public void Refresh() 
		{
			try 
			{
				Counters<Item> itemPreferenceCounts = new Counters<Item>();
				lock (this) 
				{
					int numUsers = 0;
					foreach (User user in dataModel.GetUsers()) 
					{
                        Preference[] prefs = user.GetPreferencesAsArray();
						foreach (Preference preference in prefs) 
						{
							itemPreferenceCounts.Increment(preference.Item);
						}
						numUsers++;
					}
					Dictionary<Item, double> newIufFactors =
						new Dictionary<Item, double>(1 + (4 * itemPreferenceCounts.Count) / 3);
					double logFactor = Math.Log(logBase);
					foreach (KeyValuePair<Item, MutableInteger> entry in itemPreferenceCounts.Map) 
					{
                        int count = entry.Value.Value;
						newIufFactors.Add(entry.Key,
										  Math.Log((double) numUsers / (double) count) / logFactor);
					}

					iufFactors.Set(newIufFactors /* readonly */);
				}
			} 
			catch (TasteException dme) 
			{
				log.Warn( "Unable to refresh", dme);
			}
		}

		public override String ToString() 
		{
			return "InverseUserFrequency[logBase:" + logBase + ']';
		}

	}

}	