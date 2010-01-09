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
     * <p>Tests {@link SpearmanCorrelation}.</p>
     *
     * @author Sean Owen
     */
    [TestFixture]
    public class SpearmanCorrelationTest : CorrelationTestCase 
    {
        [Test]
	    public void TestFullCorrelation1()
	    {
            User user1 = GetUser("test1", 1.0, 2.0, 3.0);
            User user2 = GetUser("test2", 1.0, 2.0, 3.0);
            DataModel dataModel = GetDataModel(user1, user2);
            double correlation = new SpearmanCorrelation(dataModel).GetUserCorrelation(user1, user2);
            AssertCorrelationEquals(1.0, correlation);
	    }

        [Test]
	    public void TestFullCorrelation2() 
	    {
		    User user1 = GetUser("test1", 1.0, 2.0, 3.0);
		    User user2 = GetUser("test2", 4.0, 5.0, 6.0);
		    DataModel dataModel = GetDataModel(user1, user2);
		    double correlation = new SpearmanCorrelation(dataModel).GetUserCorrelation(user1, user2);
		    AssertCorrelationEquals(1.0, correlation);
	    }

        [Test]
	    public void TestAnticorrelation()
	    {
            User user1 = GetUser("test1", 1.0, 2.0, 3.0);
            User user2 = GetUser("test2", 3.0, 2.0, 1.0);
            DataModel dataModel = GetDataModel(user1, user2);
            double correlation = new SpearmanCorrelation(dataModel).GetUserCorrelation(user1, user2);
            AssertCorrelationEquals(-1.0, correlation);
	    }

        [Test]
	    public void TestSimple() 
	    {
		    User user1 = GetUser("test1", 1.0, 2.0, 3.0);
		    User user2 = GetUser("test2", 2.0, 3.0, 1.0);
		    DataModel dataModel = GetDataModel(user1, user2);
		    double correlation = new SpearmanCorrelation(dataModel).GetUserCorrelation(user1, user2);
		    AssertCorrelationEquals(-0.5, correlation);
	    }

        [Test]
	    public void TestRefresh() 
        {
		    // Make sure this doesn't throw an exception
		    new SpearmanCorrelation(GetDataModel()).Refresh();
	    }
    }
}