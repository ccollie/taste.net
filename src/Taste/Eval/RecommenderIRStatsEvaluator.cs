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
    using Taste.Common;
    using Taste.Model;

    /**
     * <p>Implementations collect information retrieval-related statistics on a
     * {@link taste.Recommender.Recommender}'s performance, including precision,
     * recall and f-measure.</p>
     *
     * <p>See <a href="http://en.wikipedia.org/wiki/Information_retrieval">Information retrieval</a>.
     *
     * @author Sean Owen
     * @since 1.5.4
     */
    public interface RecommenderIRStatsEvaluator
    {        
        /**
         * @param recommenderBuilder object that can build a {@link taste.Recommender.Recommender} to test
         * @param dataModel dataset to test on
         * @param at as in, "precision at 5". The number of recommendations to consider when evaluating
         *  precision, etc.
         * @param relevanceThreshold {@link taste.Model.Item}s whose preference value is at least
         *  this value are considered "relevant" for the purposes of computations
         * @return {@link IRStatistics} with resulting precision, recall, etc.
         * @if an error occurs while accessing the {@link DataModel}
         */
        IRStatistics Evaluate(RecommenderBuilder recommenderBuilder,
                                DataModel dataModel, int at,
                                double relevanceThreshold,
                                double evaluationPercentage);

    }

}