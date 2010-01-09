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

namespace Taste.Tests.Recommender.slopeone
{
    using System;
    using System.Collections.Generic;
    using Taste.Tests;
    using Taste.Tests.Recommender;
    using Taste.Common;
    using Taste.Model;
    using Taste.Recommender;
    using Taste.Recommender.SlopeOne;
    using NUnit.Framework;


    /**
     * <p>Tests {@link taste.Recommender.SlopeOne.SlopeOneRecommender}.</p>
     *
     * @author Sean Owen
     */
    [TestFixture]
    public class SlopeOneRecommenderTest : RecommenderTestCase 
    {

        [Test]
	    public void TestRecommender() 
	    {
		    Recommender recommender = buildRecommender();
		    IList<RecommendedItem> recommended = recommender.Recommend("test1", 1);
		    Assert.IsNotNull(recommended);
		    Assert.AreEqual(1, recommended.Count);
		    RecommendedItem firstRecommended = recommended[0];
		    Assert.AreEqual(new GenericItem<String>("2"), firstRecommended.Item);
		    Assert.AreEqual(0.34803885284992736, firstRecommended.Value, EPSILON);
	    }

        [Test]
	    public void TestHowMany() 
	    {
		    List<User> users = new List<User>(3);
		    users.Add(GetUser("test1", 0.1, 0.2));
		    users.Add(GetUser("test2", 0.2, 0.3, 0.3, 0.6));
		    users.Add(GetUser("test3", 0.4, 0.4, 0.5, 0.9));
		    users.Add(GetUser("test4", 0.1, 0.4, 0.5, 0.8, 0.9, 1.0));
		    users.Add(GetUser("test5", 0.2, 0.3, 0.6, 0.7, 0.1, 0.2));
		    DataModel dataModel = new GenericDataModel(users);
            Recommender recommender = new SlopeOneRecommender(dataModel);
            IList<RecommendedItem> fewRecommended = recommender.Recommend("test1", 2);
            IList<RecommendedItem> moreRecommended = recommender.Recommend("test1", 4);
		    for (int i = 0; i < fewRecommended.Count; i++) 
            {
			    Assert.AreEqual(fewRecommended[i].Item, moreRecommended[i].Item);
		    }
	    }

        [Test]
	    public void TestRescorer()
        {
            List<User> users = new List<User>(3);
		    users.Add(GetUser("test1", 0.1, 0.2));
		    users.Add(GetUser("test2", 0.2, 0.3, 0.3, 0.6));
		    users.Add(GetUser("test3", 0.4, 0.4, 0.5, 0.9));
            DataModel dataModel = new GenericDataModel(users);
            Recommender recommender = new SlopeOneRecommender(dataModel);
            IList<RecommendedItem> originalRecommended = recommender.Recommend("test1", 2);
            IList<RecommendedItem> rescoredRecommended =
			    recommender.Recommend("test1", 2, new ReversingRescorer<Item>());
		    Assert.IsNotNull(originalRecommended);
		    Assert.IsNotNull(rescoredRecommended);
		    Assert.AreEqual(2, originalRecommended.Count);
		    Assert.AreEqual(2, rescoredRecommended.Count);
		    Assert.AreEqual(originalRecommended[0].Item, rescoredRecommended[1].Item);
		    Assert.AreEqual(originalRecommended[1].Item, rescoredRecommended[0].Item);
	    }

        [Test]
	    public void TestEstimatePref()
        {
            Recommender recommender = buildRecommender();
		    Assert.AreEqual(0.34803885284992736, recommender.EstimatePreference("test1", "2"), EPSILON);
	    }

        [Test]
	    public void TestBestRating() 
        {
            List<User> users = new List<User>(3);
		    users.Add(GetUser("test1", 0.0, 0.3));
		    users.Add(GetUser("test2", 0.2, 0.3, 0.3));
		    users.Add(GetUser("test3", 0.4, 0.3, 0.5));
            DataModel dataModel = new GenericDataModel(users);
            Recommender recommender = new SlopeOneRecommender(dataModel);
            IList<RecommendedItem> recommended = recommender.Recommend("test1", 1);
		    Assert.IsNotNull(recommended);
		    Assert.AreEqual(1, recommended.Count);
            RecommendedItem firstRecommended = recommended[0];
		    // item one should be recommended because it has a greater rating/score
		    Assert.AreEqual(new GenericItem<String>("2"), firstRecommended.Item);
		    Assert.AreEqual(0.2400938676203033, firstRecommended.Value, EPSILON);
	    }

	    public void testDiffStdevBehavior()
	    {
		    List<User> users = new List<User>(3);
		    users.Add(GetUser("test1", 0.1, 0.2));
		    users.Add(GetUser("test2", 0.2, 0.3, 0.6));
		    DataModel dataModel = new GenericDataModel(users);
		    Recommender recommender = new SlopeOneRecommender(dataModel);
		    Assert.AreEqual(0.5, recommender.EstimatePreference("test1", "2"), EPSILON);
	    }

	    private static Recommender buildRecommender() 
	    {
		    DataModel dataModel = new GenericDataModel(GetMockUsers());
		    return new SlopeOneRecommender(dataModel);
	    }
    }
}