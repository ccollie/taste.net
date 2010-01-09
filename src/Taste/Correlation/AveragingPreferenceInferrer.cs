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
	using System.Collections.Generic;
	using Taste.Common;
	using Taste.Model;
	using Taste.Correlation;
	using Taste.Transforms;	
	


    /// <summary>
    /// Compute an inferred preference for a {@link User} and an {@link Item}
    /// that the user has not expressed any preference for. This is an average of other preferences scores
    /// from that user, for example. This technique is sometimes called "default voting".
    /// 
    /// author Sean Owen
    /// </summary>
	public class AveragingPreferenceInferrer : PreferenceInferrer 
	{
		private static readonly SoftCacheRetriever<User, Double> RETRIEVER = new PrefRetriever();

		private readonly SoftCache<User, Double> averagePreferenceValue;

		public AveragingPreferenceInferrer(DataModel dataModel) 
		{
			averagePreferenceValue = new SoftCache<User, Double>(RETRIEVER, dataModel.GetNumUsers());
			Refresh();
		}


        public double InferPreference(User user,Item item) 
		{
			if (user == null || item == null) 
			{
				throw new ArgumentNullException("user or item is null");
			}
			return averagePreferenceValue.Get(user);
		}


		public void Refresh() 
		{
			averagePreferenceValue.Clear();
        }

        #region PrefRetriever

        private sealed class PrefRetriever : SoftCacheRetriever<User, Double> 
		{		
			public Double GetValue(User key) 
			{
				RunningAverage average = new FullRunningAverage();
                
                Preference[] prefs = key.GetPreferencesAsArray();
                if (prefs.Length == 0)
                    return 0.0;

				foreach (Preference pref in prefs) 
				{
					average.AddDatum(pref.Value);
				}
				return average.Average;
			}
		}


		public override String ToString() 
		{
			return "AveragingPreferenceInferrer";
        }

        #endregion

    }

}
