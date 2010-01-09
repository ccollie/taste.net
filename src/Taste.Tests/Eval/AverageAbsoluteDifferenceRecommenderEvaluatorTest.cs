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
    public sealed class AverageAbsoluteDifferenceRecommenderEvaluatorTest : TasteTestCase 
    {
        [Test]
	    public void TestEvaluate() 
	    {
		    DataModel model = GetDataModel();
		    RecommenderBuilder builder = new SlopeOneRecommenderBuilder();
		    RecommenderEvaluator evaluator =
			    new AverageAbsoluteDifferenceRecommenderEvaluator();
		    double eval = evaluator.Evaluate(builder, model, 0.75, 1.0);
		    Assert.AreEqual(0.26387685767414826, eval, EPSILON);
	    }
    }


    internal class SlopeOneRecommenderBuilder : RecommenderBuilder
    {
        public Recommender BuildRecommender(DataModel dataModel) 
        {
            return new SlopeOneRecommender(dataModel);
        }
    }


}