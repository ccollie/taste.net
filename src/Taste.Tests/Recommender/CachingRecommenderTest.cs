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
    using Taste.Recommender;
    using NUnit.Framework;


    /**
     * <p>Tests {@link com.planetj.taste.impl.Recommender.CachingRecommender}.</p>
     *
     * @author Sean Owen
     */
    [TestFixture]
    public class CachingRecommenderTest : RecommenderTestCase
    {
        [Test]
        public void TestRecommender()
        {
            AtomicInteger recommendCount = new AtomicInteger();
            Recommender mockRecommender = new MockRecommender(recommendCount);

            Recommender cachingRecommender = new CachingRecommender(mockRecommender);
            cachingRecommender.Recommend("1", 1);
            Assert.AreEqual(1, recommendCount.Value);
            cachingRecommender.Recommend("2", 1);
            Assert.AreEqual(2, recommendCount.Value);
            cachingRecommender.Recommend("1", 1);
            Assert.AreEqual(2, recommendCount.Value);
            cachingRecommender.Recommend("2", 1);
            Assert.AreEqual(2, recommendCount.Value);
            cachingRecommender.Refresh();
            cachingRecommender.Recommend("1", 1);
            Assert.AreEqual(3, recommendCount.Value);
            cachingRecommender.Recommend("2", 1);
            Assert.AreEqual(4, recommendCount.Value);
            cachingRecommender.Recommend("3", 1);
            Assert.AreEqual(5, recommendCount.Value);

            // Results from this recommend() method can't be cached:
            Rescorer<Item> rescorer = NullRescorer<Item>.Instance;
            cachingRecommender.Refresh();
            cachingRecommender.Recommend("1", 1, rescorer);
            Assert.AreEqual(6, recommendCount.Value);
            cachingRecommender.Recommend("2", 1, rescorer);
            Assert.AreEqual(7, recommendCount.Value);
            cachingRecommender.Recommend("1", 1, rescorer);
            Assert.AreEqual(8, recommendCount.Value);
            cachingRecommender.Recommend("2", 1, rescorer);
            Assert.AreEqual(9, recommendCount.Value);

            cachingRecommender.Refresh();
            cachingRecommender.EstimatePreference("test1", "1");
            Assert.AreEqual(10, recommendCount.Value);
            cachingRecommender.EstimatePreference("test1", "2");
            Assert.AreEqual(11, recommendCount.Value);
            cachingRecommender.EstimatePreference("test1", "2");
            Assert.AreEqual(11, recommendCount.Value);
        }
    }
}