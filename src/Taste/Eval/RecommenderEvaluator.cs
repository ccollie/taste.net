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
	

    /// <summary>
    /// <p>Implementations of this interface evaluate the quality of a
    /// <see cref="taste.Recommender.Recommender">Recommender</see>'s recommendations.</p>
    /// </summary>
	public interface RecommenderEvaluator 
	{

        /// <summary>
        /// <p>Evaluates the quality of a <see cref="taste.Recommender.Recommender">Recommender</see>'s recommendations.
        /// The range of values that may be returned depends on the implementation, but <em>lower</em> values must mean better recommendations, with 0 being the
        /// lowest / best possible evaluation, meaning a perfect match. This method does not accept a <see cref="taste.Recommender.Recommender">Recommender</see> directly, 
        /// but rather a {@link RecommenderBuilder} which can build the <see cref="taste.Recommender.Recommender">Recommender</see> to test on top of a given <see cref="taste.Model.DataModel">DataModel</see>.</p>
        /// 
        /// <p>Implementations will take a certain percentage of the preferences supplied by the given <see cref="taste.Model.DataModel">DataModel</see>
        /// as "training data". This is typically most of the data, like 90%. This data is used to produce recommendations,
        /// and the rest of the data is compared against estimated preference values to see how much the
        /// <see cref="taste.Recommender.Recommender">Recommender</see>'s predicted preferences match the user's real
        /// preferences. Specifically, for each user, this percentage of the user's ratings are used to produce recommendatinos, and for each user,
        /// the remaining preferences are compared against the user's real preferences.</p>
        /// 
        /// <p>For large datasets, it may be desirable to only evaluate based on a small percentage of the data.
        /// <code>evaluationPercentage</code> controls how many of the <see cref="taste.Model.DataModel">DataModel</see>'s users are used in
        /// evaluation.</p>
        /// 
        /// <p>To be clear, <code>trainingPercentage</code> and <code>evaluationPercentage</code> are not related.
        /// They do not need to add up to 1.0, for example.</p>
        /// </summary>
        /// <param name="recommenderBuilder">object that can build a <see cref="taste.Recommender.Recommender">Recommender</see> to test</param>
        /// <param name="dataModel">dataset to test on</param>
        /// <param name="trainingPercentage">
        /// percentage of each user's preferences to use to produce recommendations; the rest
        /// are compared to estimated preference values to evaluate <see cref="taste.Recommender.Recommender">Recommender</see>
        /// performance</param>
        /// <param name="evaluationPercentage">
        /// percentage of users to use in evaluation
        /// </param>
        /// <returns>
        /// a "score" representing how well the <see cref="taste.Recommender.Recommender">Recommender</see>'s
        /// estimated preferences match real values; <em>lower</em> scores mean a better match and 0 is a perfect match
        /// </returns>
		double Evaluate(RecommenderBuilder recommenderBuilder,
		                DataModel dataModel,
		                double trainingPercentage,
		                double evaluationPercentage);

	}
}	