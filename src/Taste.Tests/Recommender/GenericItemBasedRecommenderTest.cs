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
    using Taste.Correlation;
    using Taste.Common;
    using Taste.Model;
    using Taste.Recommender;
    using NUnit.Framework;


    /**
     * <p>Tests {@link GenericItemBasedRecommender}.</p>
     *
     * @author Sean Owen, Paulo Magalhaes (pevm)
     */
    [TestFixture]
    public class GenericItemBasedRecommenderTest : RecommenderTestCase
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
            Assert.AreEqual(0.1, firstRecommended.Value, EPSILON);
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
            IList<GenericItemCorrelation.ItemItemCorrelation> correlations =
               new List<GenericItemCorrelation.ItemItemCorrelation>(6);
            for (int i = 0; i < 6; i++)
            {
                for (int j = i + 1; j < 6; j++)
                {
                    correlations.Add(
                        new GenericItemCorrelation.ItemItemCorrelation(new GenericItem<String>(i.ToString()),
                                                                       new GenericItem<String>(j.ToString()),
                                                                       1.0 / (1.0 + (double)i + (double)j)));
                }
            }
            ItemCorrelation correlation = new GenericItemCorrelation(correlations);
            Recommender recommender = new GenericItemBasedRecommender(dataModel, correlation);
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
            Item item1 = new GenericItem<String>("0");
            Item item2 = new GenericItem<String>("1");
            Item item3 = new GenericItem<String>("2");
            Item item4 = new GenericItem<String>("3");
            ICollection<GenericItemCorrelation.ItemItemCorrelation> correlations =
               new List<GenericItemCorrelation.ItemItemCorrelation>(6);
            correlations.Add(new GenericItemCorrelation.ItemItemCorrelation(item1, item2, 1.0));
            correlations.Add(new GenericItemCorrelation.ItemItemCorrelation(item1, item3, 0.5));
            correlations.Add(new GenericItemCorrelation.ItemItemCorrelation(item1, item4, 0.2));
            correlations.Add(new GenericItemCorrelation.ItemItemCorrelation(item2, item3, 0.7));
            correlations.Add(new GenericItemCorrelation.ItemItemCorrelation(item2, item4, 0.5));
            correlations.Add(new GenericItemCorrelation.ItemItemCorrelation(item3, item4, 0.9));
            ItemCorrelation correlation = new GenericItemCorrelation(correlations);
            Recommender recommender = new GenericItemBasedRecommender(dataModel, correlation);
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
            Assert.AreEqual(0.1, recommender.EstimatePreference("test1", "2"), EPSILON);
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
            Assert.AreEqual(0.1, firstRecommended.Value, EPSILON);
        }

        [Test]
        public void TestMostSimilar()
        {
            ItemBasedRecommender recommender = buildRecommender();
            IList<RecommendedItem> similar = recommender.MostSimilarItems("0", 2);
            Assert.IsNotNull(similar);
            Assert.AreEqual(2, similar.Count);
            RecommendedItem first = similar[0];
            RecommendedItem second = similar[1];
            Assert.AreEqual("1", first.Item.ID);
            Assert.AreEqual(1.0, first.Value, EPSILON);
            Assert.AreEqual("2", second.Item.ID);
            Assert.AreEqual(0.5, second.Value, EPSILON);
        }

        [Test]
        public void TestMostSimilarToMultiple()
        {
            ItemBasedRecommender recommender = buildRecommender2();
            List<Object> itemIDs = new List<Object>(2);
            itemIDs.Add("0");
            itemIDs.Add("1");
            IList<RecommendedItem> similar = recommender.MostSimilarItems(itemIDs, 2);
            Assert.IsNotNull(similar);
            Assert.AreEqual(2, similar.Count);
            RecommendedItem first = similar[0];
            RecommendedItem second = similar[1];
            Assert.AreEqual("2", first.Item.ID);
            Assert.AreEqual(0.85, first.Value, EPSILON);
            Assert.AreEqual("3", second.Item.ID);
            Assert.AreEqual(-0.3, second.Value, EPSILON);
        }

        public void testRecommendedBecause()
        {
            ItemBasedRecommender recommender = buildRecommender2();
            IList<RecommendedItem> recommendedBecause = recommender.RecommendedBecause("test1", "4", 3);
            Assert.IsNotNull(recommendedBecause);
            Assert.AreEqual(3, recommendedBecause.Count);
            RecommendedItem first = recommendedBecause[0];
            RecommendedItem second = recommendedBecause[1];
            RecommendedItem third = recommendedBecause[2];
            Assert.AreEqual("2", first.Item.ID);
            Assert.AreEqual(0.99, first.Value, EPSILON);
            Assert.AreEqual("3", second.Item.ID);
            Assert.AreEqual(0.4, second.Value, EPSILON);
            Assert.AreEqual("0", third.Item.ID);
            Assert.AreEqual(0.2, third.Value, EPSILON);
        }

        private static ItemBasedRecommender buildRecommender()
        {
            DataModel dataModel = new GenericDataModel(GetMockUsers());
            ICollection<GenericItemCorrelation.ItemItemCorrelation> correlations =
               new List<GenericItemCorrelation.ItemItemCorrelation>(2);
            Item item1 = new GenericItem<String>("0");
            Item item2 = new GenericItem<String>("1");
            Item item3 = new GenericItem<String>("2");
            correlations.Add(new GenericItemCorrelation.ItemItemCorrelation(item1, item2, 1.0));
            correlations.Add(new GenericItemCorrelation.ItemItemCorrelation(item1, item3, 0.5));
            ItemCorrelation correlation = new GenericItemCorrelation(correlations);
            return new GenericItemBasedRecommender(dataModel, correlation);
        }

        private static ItemBasedRecommender buildRecommender2()
        {
            List<User> users = new List<User>(4);
            users.Add(GetUser("test1", 0.1, 0.3, 0.9, 0.8));
            users.Add(GetUser("test2", 0.2, 0.3, 0.3, 0.4));
            users.Add(GetUser("test3", 0.4, 0.3, 0.5, 0.1, 0.1));
            users.Add(GetUser("test4", 0.7, 0.3, 0.8, 0.5, 0.6));
            DataModel dataModel = new GenericDataModel(users);
            ICollection<GenericItemCorrelation.ItemItemCorrelation> correlations =
               new List<GenericItemCorrelation.ItemItemCorrelation>(10);
            Item item1 = new GenericItem<String>("0");
            Item item2 = new GenericItem<String>("1");
            Item item3 = new GenericItem<String>("2");
            Item item4 = new GenericItem<String>("3");
            Item item5 = new GenericItem<String>("4");
            correlations.Add(new GenericItemCorrelation.ItemItemCorrelation(item1, item2, 1.0));
            correlations.Add(new GenericItemCorrelation.ItemItemCorrelation(item1, item3, 0.8));
            correlations.Add(new GenericItemCorrelation.ItemItemCorrelation(item1, item4, -0.6));
            correlations.Add(new GenericItemCorrelation.ItemItemCorrelation(item1, item5, 1.0));
            correlations.Add(new GenericItemCorrelation.ItemItemCorrelation(item2, item3, 0.9));
            correlations.Add(new GenericItemCorrelation.ItemItemCorrelation(item2, item4, 0.0));
            correlations.Add(new GenericItemCorrelation.ItemItemCorrelation(item2, item2, 1.0));
            correlations.Add(new GenericItemCorrelation.ItemItemCorrelation(item3, item4, -0.1));
            correlations.Add(new GenericItemCorrelation.ItemItemCorrelation(item3, item5, 0.1));
            correlations.Add(new GenericItemCorrelation.ItemItemCorrelation(item4, item5, -0.5));
            ItemCorrelation correlation = new GenericItemCorrelation(correlations);
            return new GenericItemBasedRecommender(dataModel, correlation);
        }
    }

}