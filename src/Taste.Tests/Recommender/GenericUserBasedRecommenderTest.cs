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

namespace Taste.Tests.Recommender
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using Taste;
    using Taste.Common;
    using Taste.Model;
    using Taste.Correlation;
    using Taste.Recommender;
    using Taste.Neighborhood;
    using NUnit.Framework;

    /**
     * <p>Tests {@link com.planetj.taste.impl.Recommender.GenericUserBasedRecommender}.</p>
     *
     * @author Sean Owen, Paulo Magalhaes (pevm)
     */
    [TestFixture]
    public class GenericUserBasedRecommenderTest : RecommenderTestCase
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
            Assert.AreEqual(0.3, firstRecommended.Value);
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
            UserCorrelation correlation = new PearsonCorrelation(dataModel);
            UserNeighborhood neighborhood = new NearestNUserNeighborhood(2, correlation, dataModel);
            Recommender recommender = new GenericUserBasedRecommender(dataModel, neighborhood, correlation);
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
            UserCorrelation correlation = new PearsonCorrelation(dataModel);
            UserNeighborhood neighborhood = new NearestNUserNeighborhood(1, correlation, dataModel);
            Recommender recommender = new GenericUserBasedRecommender(dataModel, neighborhood, correlation);
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
            Assert.AreEqual(0.3, recommender.EstimatePreference("test1", "2"));
        }

        /**
         * Contributed test case that verifies fix for bug
         *  <a href="http://sourceforge.net/tracker/index.php?func=detail&amp;aid=1396128&amp;group_id=138771&amp;atid=741665">
         * 1396128</a>.
         */
        [Test]
        public void TestBestRating()
        {
            Recommender recommender = buildRecommender();
            IList<RecommendedItem> recommended = recommender.Recommend("test1", 1);
            Assert.IsNotNull(recommended);
            Assert.AreEqual(1, recommended.Count);
            RecommendedItem firstRecommended = recommended[0];
            // item one should be recommended because it has a greater rating/score
            Assert.AreEqual(new GenericItem<String>("2"), firstRecommended.Item);
            Assert.AreEqual(0.3, firstRecommended.Value, EPSILON);
        }

        [Test]
        public void TestMostSimilar()
        {
            UserBasedRecommender recommender = buildRecommender();
            IList<User> similar = recommender.MostSimilarUsers("test1", 2);
            Assert.IsNotNull(similar);
            Assert.AreEqual(2, similar.Count);
            User first = similar[0];
            User second = similar[1];
            Assert.AreEqual("test2", first.ID);
            Assert.AreEqual("test3", second.ID);
        }

        [Test]
        public void TestIsolatedUser()
        {
            List<User> users = new List<User>(3);
            users.Add(GetUser("test1", 0.1, 0.2));
            users.Add(GetUser("test2", 0.2, 0.3, 0.3, 0.6));
            users.Add(GetUser("test3", 0.4, 0.4, 0.5, 0.9));
            users.Add(GetUser("test4"));
            DataModel dataModel = new GenericDataModel(users);
            UserCorrelation correlation = new PearsonCorrelation(dataModel);
            UserNeighborhood neighborhood = new NearestNUserNeighborhood(3, correlation, dataModel);
            UserBasedRecommender recommender = new GenericUserBasedRecommender(dataModel, neighborhood, correlation);
            ICollection<User> mostSimilar = recommender.MostSimilarUsers("test4", 3);
            Assert.IsNotNull(mostSimilar);
            Assert.AreEqual(0, mostSimilar.Count);
        }

        private static UserBasedRecommender buildRecommender()
        {
            DataModel dataModel = new GenericDataModel(GetMockUsers());
            UserCorrelation correlation = new PearsonCorrelation(dataModel);
            UserNeighborhood neighborhood = new NearestNUserNeighborhood(1, correlation, dataModel);
            return new GenericUserBasedRecommender(dataModel, neighborhood, correlation);
        }
    }
}