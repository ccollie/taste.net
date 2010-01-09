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
    using Taste.Model;
    using Taste.Neighborhood;
    using NUnit.Framework;
    /**
     * <p>Tests {@link NearestNUserNeighborhood}.</p>
     *
     * @author Sean Owen
     */
    [TestFixture]
    public class NearestNNeighborhoodTest : NeighborhoodTestCase 
    {

	    public void testNeighborhood() 
	    {

		    List<User> users = GetMockUsers();
		    DataModel dataModel = new GenericDataModel(users);

		    ICollection<User> neighborhood =
		        new NearestNUserNeighborhood(1, new DummyCorrelation(), dataModel).GetUserNeighborhood("test1");
		    Assert.IsNotNull(neighborhood);
		    Assert.AreEqual(1, neighborhood.Count);
		    Assert.IsTrue(neighborhood.Contains(users[1]));

		    ICollection<User> neighborhood2 =
		        new NearestNUserNeighborhood(2, new DummyCorrelation(), dataModel).GetUserNeighborhood("test2");
		    Assert.IsNotNull(neighborhood2);
		    Assert.AreEqual(2, neighborhood2.Count);
		    Assert.IsTrue(neighborhood2.Contains(users[0]));
		    Assert.IsTrue(neighborhood2.Contains(users[2]));

		    ICollection<User> neighborhood3 =
		        new NearestNUserNeighborhood(4, new DummyCorrelation(), dataModel).GetUserNeighborhood("test4");
		    Assert.IsNotNull(neighborhood3);
		    Assert.AreEqual(3, neighborhood3.Count);
		    Assert.IsTrue(neighborhood3.Contains(users[0]));
		    Assert.IsTrue(neighborhood3.Contains(users[1]));
		    Assert.IsTrue(neighborhood3.Contains(users[2]));

	    }

        [Test]
	    public void TestRefresh() 
	    {
		    // Make sure this doesn't throw an exception
		    DataModel dataModel = new GenericDataModel( ScalarToList<User>(GetUser("test1", 0.1)));
		    new NearestNUserNeighborhood(1, new DummyCorrelation(), dataModel).Refresh();
	    }
    }
}