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

namespace Taste.Tests.Correlation
{
    using System;
    using System.Collections.Generic;
    using Taste.Correlation;
    using Taste.Model;
    using NUnit.Framework;

    /**
     * <p>Tests {@link com.planetj.taste.impl.Correlation.GenericItemCorrelation}.</p>
     *
     * @author Sean Owen
     */
    [TestFixture]
    public class GenericItemCorrelationTest : CorrelationTestCase
    {
        [Test]
        public void TestSimple()
        {
            Item item1 = new GenericItem<String>("1");
            Item item2 = new GenericItem<String>("2");
            Item item3 = new GenericItem<String>("3");
            Item item4 = new GenericItem<String>("4");
            List<GenericItemCorrelation.ItemItemCorrelation> correlations =
                new List<GenericItemCorrelation.ItemItemCorrelation>(4);
            correlations.Add(new GenericItemCorrelation.ItemItemCorrelation(item1, item2, 0.5));
            correlations.Add(new GenericItemCorrelation.ItemItemCorrelation(item2, item1, 0.6));
            correlations.Add(new GenericItemCorrelation.ItemItemCorrelation(item1, item1, 0.5));
            correlations.Add(new GenericItemCorrelation.ItemItemCorrelation(item1, item3, 0.3));
            GenericItemCorrelation itemCorrelation = new GenericItemCorrelation(correlations);
            Assert.AreEqual(1.0, itemCorrelation.GetItemCorrelation(item1, item1));
            Assert.AreEqual(0.6, itemCorrelation.GetItemCorrelation(item1, item2));
            Assert.AreEqual(0.6, itemCorrelation.GetItemCorrelation(item2, item1));
            Assert.AreEqual(0.3, itemCorrelation.GetItemCorrelation(item1, item3));
            Assert.IsTrue(Double.IsNaN(itemCorrelation.GetItemCorrelation(item3, item4)));
        }

        [Test]
        public void TestFromCorrelation()
        {
            User user1 = GetUser("test1", 1.0, 2.0);
            User user2 = GetUser("test2", 2.0, 5.0);
            User user3 = GetUser("test3", 3.0, 6.0);
            DataModel dataModel = GetDataModel(user1, user2, user3);
            ItemCorrelation otherCorrelation = new PearsonCorrelation(dataModel);
            ItemCorrelation itemCorrelation = new GenericItemCorrelation(otherCorrelation, dataModel);
            AssertCorrelationEquals(1.0,
                                    itemCorrelation.GetItemCorrelation(dataModel.GetItem("0"), dataModel.GetItem("0")));
            AssertCorrelationEquals(0.960768922830523,
                                    itemCorrelation.GetItemCorrelation(dataModel.GetItem("0"), dataModel.GetItem("1")));
        }
    }

}