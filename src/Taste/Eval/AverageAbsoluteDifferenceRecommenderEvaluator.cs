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

namespace Taste.Eval
{
	using System;
	using System.Collections.Generic;
	using Taste.Common;
	using Taste.Model;
	using Taste.Recommender;
    using log4net;

	/**
	 * <p>A {@link taste.Eval.RecommenderEvaluator} which computes the average absolute difference
	 * between predicted and actual ratings for users.</p>
	 *
	 * <p>This algorithm is also called "mean average error".</p>
	 * 
	 * @author Sean Owen
	 */
	public class AverageAbsoluteDifferenceRecommenderEvaluator : AbstractDifferenceRecommenderEvaluator 
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(AverageAbsoluteDifferenceRecommenderEvaluator));

		public override double GetEvaluation(IDictionary<User, ICollection<Preference>> testUserPrefs,
								Recommender recommender)
		{
			RunningAverage average = new FullRunningAverage();
			foreach (KeyValuePair<User, ICollection<Preference>> entry in testUserPrefs) 
			{
				foreach (Preference realPref in entry.Value) 
				{
					User testUser = entry.Key;
					try 
					{
						double estimatedPreference =
							recommender.EstimatePreference(testUser.ID, realPref.Item.ID);
						if (!double.IsNaN(estimatedPreference)) 
						{
							average.AddDatum(Math.Abs(realPref.Value - estimatedPreference));
						}
					} 
					catch (NoSuchElementException nsee) 
					{
						// It's possible that an item exists in the test data but not training data in which case
						// NSEE will be thrown. Just ignore it and move on.
						log.Info("Element exists in test data but not training data: " + testUser.ID.ToString(), nsee);
					}
				}
			}
			return average.Average;
		}

		
		public override String ToString() 
		{
			return "AverageAbsoluteDifferenceRecommenderEvaluator";
		}
	}

}	