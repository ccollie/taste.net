/*
 * Copyright 2007 and onwards Sean Owen
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
	using System.Collections;
    using Iesi.Collections.Generic;
    using System.Collections.Generic;	
	using Taste.Common;
	using Taste.Model;
	using Taste.Recommender;


	/**
	 * <p>For each {@link taste.Model.User}, these implementation determine the top <code>n</code> preferences,
	 * then evaluate the IR statistics based on a {@link DataModel} that does not have these values.
	 * This number <code>n</code> is the "at" value, as in "precision at 5". For example, this would mean precision
	 * evaluated by removing the top 5 preferences for a {@link User} and then finding the percentage of those 5
	 * {@link taste.Model.Item}s included in the top 5 recommendations for that user.</p>
	 *
	 * @author Sean Owen
	 * @since 1.5.4
	 */
	public class GenericRecommenderIRStatsEvaluator : RecommenderIRStatsEvaluator 
	{

		private Random random;

		public GenericRecommenderIRStatsEvaluator() 
		{
			random = RandomUtils.GetRandom();
		}

		public IRStatistics Evaluate(RecommenderBuilder recommenderBuilder,
		                             DataModel dataModel,
		                             int at,
		                             double relevanceThreshold,
		                             double evaluationPercentage) 
		{

			if (recommenderBuilder == null) {
				throw new ArgumentNullException("recommenderBuilder is null");
			}
			if (dataModel == null) {
				throw new ArgumentNullException("dataModel is null");
			}
			if (at < 1) {
				throw new ArgumentException("at must be at least 1");
			}
			if (double.IsNaN(evaluationPercentage) || evaluationPercentage <= 0.0 || evaluationPercentage > 1.0) 
			{
				throw new ArgumentException("Invalid evaluationPercentage: " + evaluationPercentage);
			}
			if (double.IsNaN(relevanceThreshold)) {
				throw new ArgumentException("Invalid relevanceThreshold: " + evaluationPercentage);
			}

			RunningAverage precision = new FullRunningAverage();
			RunningAverage recall = new FullRunningAverage();
			foreach (User user in dataModel.GetUsers()) 
			{
				Object id = user.ID;
				if (random.NextDouble() < evaluationPercentage) 
				{
					ICollection<Item> relevantItems = new HashedSet<Item>(/* at */);
                    Preference[] prefs = user.GetPreferencesAsArray();

					foreach (Preference pref in prefs) 
					{
						if (pref.Value >= relevanceThreshold) 
                        {
							relevantItems.Add(pref.Item);
						}
					}
					int numRelevantItems = relevantItems.Count;
					if (numRelevantItems > 0) 
                    {
						ICollection<User> trainingUsers = new List<User>(dataModel.GetNumUsers());
						foreach (User user2 in dataModel.GetUsers()) 
                        {
							if (id.Equals(user2.ID)) 
							{
								ICollection<Preference> trainingPrefs = new List<Preference>();
                                prefs = user2.GetPreferencesAsArray();
								foreach (Preference pref in prefs) 
								{
									if (!relevantItems.Contains(pref.Item)) 
									{
										trainingPrefs.Add(pref);
									}
								}
								if (trainingPrefs.Count > 0) 
								{
									User trainingUser = new GenericUser<String>(id.ToString(), trainingPrefs);
									trainingUsers.Add(trainingUser);
								}
							} 
                            else 
                            {
								trainingUsers.Add(user2);
							}

						}
						DataModel trainingModel = new GenericDataModel(trainingUsers);
						Recommender recommender = recommenderBuilder.BuildRecommender(trainingModel);

						try 
						{
							trainingModel.GetUser(id);
						} 
                        catch (NoSuchElementException) 
                        {
							continue; // Oops we excluded all prefs for the user -- just move on
						}
						
						int intersectionSize = 0;
						foreach (RecommendedItem recommendedItem in recommender.Recommend(id, at)) 
						{
							if (relevantItems.Contains(recommendedItem.Item)) 
							{
								intersectionSize++;
							}
						}
						precision.AddDatum((double) intersectionSize / (double) at);
						recall.AddDatum((double) intersectionSize / (double) numRelevantItems);					
					}
				}
			}

			return new IRStatisticsImpl(precision.Average, recall.Average);
		}
	}
	
}