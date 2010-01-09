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

namespace Taste.Tests.Correlation
{
    using System;
    using System.Collections.Generic;
    using Taste.Model;
    using Taste.Correlation;
    using NUnit.Framework;

    /**
     * <p>Tests {@link PearsonCorrelation}.</p>
     *
     * @author Sean Owen
     */
    [TestFixture]
    public class PearsonCorrelationTest : CorrelationTestCase
    {

        [Test]
        public void TestFullCorrelation1()
        {
            User user1 = GetUser("test1", 3.0, -2.0);
            User user2 = GetUser("test2", 3.0, -2.0);
            DataModel dataModel = GetDataModel(user1, user2);
            double correlation = new PearsonCorrelation(dataModel).GetUserCorrelation(user1, user2);
            AssertCorrelationEquals(1.0, correlation);
        }

        [Test]
        public void TestFullCorrelation1Weighted()
        {
            User user1 = GetUser("test1", 3.0, -2.0);
            User user2 = GetUser("test2", 3.0, -2.0);
            DataModel dataModel = GetDataModel(user1, user2);
            double correlation = new PearsonCorrelation(dataModel, true).GetUserCorrelation(user1, user2);
            AssertCorrelationEquals(1.0, correlation);
        }

        [Test]
        public void TestFullCorrelation2()
        {
            User user1 = GetUser("test1", 3.0, 3.0);
            User user2 = GetUser("test2", 3.0, 3.0);
            DataModel dataModel = GetDataModel(user1, user2);
            double correlation = new PearsonCorrelation(dataModel).GetUserCorrelation(user1, user2);
            // Yeah, undefined in this case
            Assert.IsTrue(Double.IsNaN(correlation));
        }

        [Test]
        public void TestNoCorrelation1()
        {
            User user1 = GetUser("test1", 3.0, -2.0);
            User user2 = GetUser("test2", -3.0, 2.0);
            DataModel dataModel = GetDataModel(user1, user2);
            double correlation = new PearsonCorrelation(dataModel).GetUserCorrelation(user1, user2);
            AssertCorrelationEquals(-1.0, correlation);
        }

        [Test]
        public void testNoCorrelation1Weighted()
        {
            User user1 = GetUser("test1", 3.0, -2.0);
            User user2 = GetUser("test2", -3.0, 2.0);
            DataModel dataModel = GetDataModel(user1, user2);
            double correlation = new PearsonCorrelation(dataModel, true).GetUserCorrelation(user1, user2);
            AssertCorrelationEquals(-1.0, correlation);
        }

        [Test]
        public void testNoCorrelation2()
        {
            Preference pref1 = new GenericPreference(null, new GenericItem<String>("1"), 1.0);
            GenericUser<String> user1 = new GenericUser<String>("test1", ScalarToList<Preference>(pref1));
            Preference pref2 = new GenericPreference(null, new GenericItem<String>("2"), 1.0);
            GenericUser<String> user2 = new GenericUser<String>("test2", ScalarToList<Preference>(pref2));
            DataModel dataModel = GetDataModel(user1, user2);
            double correlation = new PearsonCorrelation(dataModel).GetUserCorrelation(user1, user2);
            Assert.IsTrue(Double.IsNaN(correlation));
        }

        [Test]
        public void testNoCorrelation3()
        {
            User user1 = GetUser("test1", 90.0, 80.0, 70.0);
            User user2 = GetUser("test2", 70.0, 80.0, 90.0);
            DataModel dataModel = GetDataModel(user1, user2);
            double correlation = new PearsonCorrelation(dataModel).GetUserCorrelation(user1, user2);
            AssertCorrelationEquals(-1.0, correlation);
        }

        [Test]
        public void TestSimple()
        {
            User user1 = GetUser("test1", 1.0, 2.0, 3.0);
            User user2 = GetUser("test2", 2.0, 5.0, 6.0);
            DataModel dataModel = GetDataModel(user1, user2);
            double correlation = new PearsonCorrelation(dataModel).GetUserCorrelation(user1, user2);
            AssertCorrelationEquals(0.9607689228305227, correlation);
        }

        [Test]
        public void TestSimpleWeighted()
        {
            User user1 = GetUser("test1", 1.0, 2.0, 3.0);
            User user2 = GetUser("test2", 2.0, 5.0, 6.0);
            DataModel dataModel = GetDataModel(user1, user2);
            double correlation = new PearsonCorrelation(dataModel, true).GetUserCorrelation(user1, user2);
            AssertCorrelationEquals(0.9901922307076306, correlation);
        }

        [Test]
        public void TestFullItemCorrelation1()
        {
            User user1 = GetUser("test1", 3.0, 3.0);
            User user2 = GetUser("test2", -2.0, -2.0);
            DataModel dataModel = GetDataModel(user1, user2);
            double correlation =
               new PearsonCorrelation(dataModel).GetItemCorrelation(dataModel.GetItem("0"), dataModel.GetItem("1"));
            AssertCorrelationEquals(1.0, correlation);
        }

        [Test]
        public void TestFullItemCorrelation2()
        {
            User user1 = GetUser("test1", 3.0, 3.0);
            User user2 = GetUser("test2", 3.0, 3.0);
            DataModel dataModel = GetDataModel(user1, user2);
            double correlation =
               new PearsonCorrelation(dataModel).GetItemCorrelation(dataModel.GetItem("0"), dataModel.GetItem("1"));
            // Yeah, undefined in this case
            Assert.IsTrue(Double.IsNaN(correlation));
        }

        [Test]
        public void TestNoItemCorrelation1()
        {
            User user1 = GetUser("test1", 3.0, -3.0);
            User user2 = GetUser("test2", -2.0, 2.0);
            DataModel dataModel = GetDataModel(user1, user2);
            double correlation =
               new PearsonCorrelation(dataModel).GetItemCorrelation(dataModel.GetItem("0"), dataModel.GetItem("1"));
            AssertCorrelationEquals(-1.0, correlation);
        }

        [Test]
        public void TestNoItemCorrelation2()
        {
            Preference pref1 = new GenericPreference(null, new GenericItem<String>("1"), 1.0);
            GenericUser<String> user1 = new GenericUser<String>("test1", ScalarToList<Preference>(pref1));
            Preference pref2 = new GenericPreference(null, new GenericItem<String>("2"), 1.0);
            GenericUser<String> user2 = new GenericUser<String>("test2", ScalarToList<Preference>(pref2));
            DataModel dataModel = GetDataModel(user1, user2);
            double correlation =
               new PearsonCorrelation(dataModel).GetItemCorrelation(dataModel.GetItem("1"), dataModel.GetItem("2"));
            Assert.IsTrue(Double.IsNaN(correlation));
        }

        [Test]
        public void TestNoItemCorrelation3()
        {
            User user1 = GetUser("test1", 90.0, 70.0);
            User user2 = GetUser("test2", 80.0, 80.0);
            User user3 = GetUser("test3", 70.0, 90.0);
            DataModel dataModel = GetDataModel(user1, user2, user3);
            double correlation =
               new PearsonCorrelation(dataModel).GetItemCorrelation(dataModel.GetItem("0"), dataModel.GetItem("1"));
            AssertCorrelationEquals(-1.0, correlation);
        }

        [Test]
        public void TestSimpleItem()
        {
            User user1 = GetUser("test1", 1.0, 2.0);
            User user2 = GetUser("test2", 2.0, 5.0);
            User user3 = GetUser("test3", 3.0, 6.0);
            DataModel dataModel = GetDataModel(user1, user2, user3);
            double correlation =
               new PearsonCorrelation(dataModel).GetItemCorrelation(dataModel.GetItem("0"), dataModel.GetItem("1"));
            AssertCorrelationEquals(0.9607689228305227, correlation);
        }

        [Test]
        public void TestSimpleItemWeighted()
        {
            User user1 = GetUser("test1", 1.0, 2.0);
            User user2 = GetUser("test2", 2.0, 5.0);
            User user3 = GetUser("test3", 3.0, 6.0);
            DataModel dataModel = GetDataModel(user1, user2, user3);
            ItemCorrelation itemCorrelation = new PearsonCorrelation(dataModel, true);
            double correlation = itemCorrelation.GetItemCorrelation(dataModel.GetItem("0"), dataModel.GetItem("1"));
            AssertCorrelationEquals(0.9901922307076306, correlation);
        }

        [Test]
        public void TestRefresh()
        {
            // Make sure this doesn't throw an exception
            new PearsonCorrelation(GetDataModel()).Refresh();
        }
    }

}