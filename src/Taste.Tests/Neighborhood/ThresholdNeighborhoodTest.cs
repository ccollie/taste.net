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

namespace Taste.Tests.Neighborhood
{
    using System;
    using System.Collections.Generic;
    using Taste.Correlation;
    using Taste.Neighborhood;
    using Taste.Model;
    using NUnit.Framework;

    /**
     * <p>Tests {@link ThresholdUserNeighborhood}.</p>
     *
     * @author Sean Owen
     */
    [TestFixture]
    public class ThresholdNeighborhoodTest : NeighborhoodTestCase 
    {

        [Test]
	    public void TestNeighborhood()
	    {

            List<User> users = GetMockUsers();
            DataModel dataModel = new GenericDataModel(users);

            ICollection<User> neighborhood =
		        new ThresholdUserNeighborhood(20.0, new DummyCorrelation(), dataModel).GetUserNeighborhood("test1");
		    Assert.IsNotNull(neighborhood);
		    Assert.IsTrue(neighborhood.Count == 0);

		     ICollection<User> neighborhood2 =
		        new ThresholdUserNeighborhood(10.0, new DummyCorrelation(), dataModel).GetUserNeighborhood("test1");
		    Assert.IsNotNull(neighborhood2);
		    Assert.AreEqual(1, neighborhood2.Count);
		    Assert.IsTrue(neighborhood2.Contains(users[1]));

		     ICollection<User> neighborhood3 =
		        new ThresholdUserNeighborhood(1.0, new DummyCorrelation(), dataModel).GetUserNeighborhood("test2");
		    Assert.IsNotNull(neighborhood3);
		    Assert.AreEqual(3, neighborhood3.Count);
		    Assert.IsTrue(neighborhood3.Contains(users[0]));
		    Assert.IsTrue(neighborhood3.Contains(users[2]));
		    Assert.IsTrue(neighborhood3.Contains(users[3]));
	    }

        [Test]
	    public void TestRefresh()
	    {
		    // Make sure this doesn't throw an exception
            IList<User> modelUsers = new List<User>();
            modelUsers.Add(GetUser("test1", 0.1));
		    DataModel dataModel = new GenericDataModel( modelUsers );		
		    new ThresholdUserNeighborhood(20.0, new DummyCorrelation(), dataModel).Refresh();
	    }
    }

}