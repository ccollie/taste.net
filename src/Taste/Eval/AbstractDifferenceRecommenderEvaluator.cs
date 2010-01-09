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
    using Taste.Eval;
	using Taste.Model;
	using Taste.Recommender;
    using log4net;

	/**
	 * <p>Abstract superclass of a couple implementations, providing shared functionality.</p>
	 *
	 * @author Sean Owen
	 * @since 1.3.5
	 */
	public abstract class AbstractDifferenceRecommenderEvaluator : RecommenderEvaluator 
	{

		private static readonly ILog log = LogManager.GetLogger(
                typeof(AbstractDifferenceRecommenderEvaluator));

		private readonly Random random;

		public AbstractDifferenceRecommenderEvaluator() 
		{
			random = RandomUtils.GetRandom();
		}

		/**
		 * {@inheritDoc}
		 */
		public double Evaluate(RecommenderBuilder recommenderBuilder,
		                       DataModel dataModel,
		                       double trainingPercentage,
		                       double evaluationPercentage)
		{

			if (recommenderBuilder == null) 
            {
				throw new ArgumentNullException("recommenderBuilder is null");
			}
			if (dataModel == null) 
            {
				throw new ArgumentNullException("dataModel is null");
			}
			if (double.IsNaN(trainingPercentage) || trainingPercentage <= 0.0 || trainingPercentage >= 1.0) 
            {
				throw new ArgumentException("Invalid trainingPercentage: " + trainingPercentage);
			}
			if (double.IsNaN(evaluationPercentage) || evaluationPercentage <= 0.0 || evaluationPercentage > 1.0) 
            {
				throw new ArgumentException("Invalid evaluationPercentage: " + evaluationPercentage);
			}

			log.Info("Beginning evaluation using " + trainingPercentage + " of " + dataModel);

			int numUsers = dataModel.GetNumUsers();
			ICollection<User> trainingUsers = new List<User>(1 + (int) (trainingPercentage * (double) numUsers));
			IDictionary<User, ICollection<Preference>> testUserPrefs =
				new Dictionary<User, ICollection<Preference>>(1 + (int) ((1.0 - trainingPercentage) * (double) numUsers));

			foreach (User user in dataModel.GetUsers()) 
            {
				if (random.NextDouble() < evaluationPercentage) 
                {
					ICollection<Preference> trainingPrefs = new List<Preference>();
					ICollection<Preference> testPrefs = new List<Preference>();
                    Preference[] prefs = user.GetPreferencesAsArray();

					foreach (Preference pref in prefs) 
					{
						Item itemCopy = new GenericItem<String>(pref.Item.ID.ToString());
						Preference newPref = new GenericPreference(null, itemCopy, pref.Value);
						if (random.NextDouble() < trainingPercentage) 
						{
							trainingPrefs.Add(newPref);
						} else {
							testPrefs.Add(newPref);
						}
					}
					if (log.IsDebugEnabled) {
						log.Debug("Training against " + trainingPrefs.Count + " preferences");
						log.Debug("Evaluating accuracy of " + testPrefs.Count + " preferences");
					}
					if (trainingPrefs.Count > 0) 
					{
						User trainingUser = new GenericUser<String>(user.ID.ToString(), trainingPrefs);
						trainingUsers.Add(trainingUser);
						if (testPrefs.Count > 0) 
						{
							testUserPrefs.Add(trainingUser, testPrefs);
						}
					}
				}
			}

			DataModel trainingModel = new GenericDataModel(trainingUsers);
			Recommender recommender = recommenderBuilder.BuildRecommender(trainingModel);
			double result = GetEvaluation(testUserPrefs, recommender);
			log.Info("Evaluation result: " + result);
			return result;
		}

		public abstract double GetEvaluation(IDictionary<User, ICollection<Preference>> testUserPrefs, 
		                              Recommender recommender);

	}
}