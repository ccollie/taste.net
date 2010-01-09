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

namespace Taste.Tests.Eval
{
    using System;
    using Taste.Tests;
    using Taste.Eval;
    using Taste.Recommender;
    using Taste.Recommender.SlopeOne;
    using Taste.Model;   
    using Taste.Common;
    using NUnit.Framework;


    /**
     * @author Sean Owen
     */
    [TestFixture]
    public class GenericRecommenderIRStatsEvaluatorImplTest : TasteTestCase 
    {
        [Test]
	    public void TestEvaluate()  
	    {
		    DataModel model = GetDataModel();
		    RecommenderBuilder builder = new SlopeOneRecommenderBuilder();
		    RecommenderIRStatsEvaluator evaluator = new GenericRecommenderIRStatsEvaluator();
		    IRStatistics stats = evaluator.Evaluate(builder, model, 5, 0.2, 1.0);
		    Assert.IsNotNull(stats);
		    Assert.AreEqual(0.2, stats.Precision, EPSILON);
		    Assert.AreEqual(1.0, stats.Recall, EPSILON);
		    Assert.AreEqual(0.33333, stats.GetF1Measure(), EPSILON);
	    }
    }
}