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

namespace Taste.Tests.Recommender
{
    using Taste.Recommender;
	using Taste.Model;
    using Taste.Transforms;
	using System;
	using System.Collections.Generic;
	using NUnit.Framework;



	/**
	 * <p>Tests {@link NullRescorer}.</p>
	 *
	 * @author Sean Owen
	 */
    [TestFixture]
	public sealed class NullRescorerTest
	{
	    [Test]
        public void TestItemRescorer() 
		{
			Rescorer<Item> rescorer = NullRescorer<Item>.Instance;
			Assert.IsNotNull(rescorer);
			Item item = new GenericItem<String>("test");
			Assert.AreEqual(1.0, rescorer.Rescore(item, 1.0));
			Assert.AreEqual(1.0, rescorer.Rescore(null, 1.0));
			Assert.AreEqual(0.0, rescorer.Rescore(item, 0.0));
			Assert.IsTrue(Double.IsNaN(rescorer.Rescore(item, Double.NaN)));
		}

        [Test]
		public void TestUserRescorer() 
		{
			Rescorer<User> rescorer = NullRescorer<User>.Instance;
			Assert.IsNotNull(rescorer);
			User user = new GenericUser<String>("test", new List<Preference>());
			Assert.AreEqual(1.0, rescorer.Rescore(user, 1.0));
			Assert.AreEqual(1.0, rescorer.Rescore(null, 1.0));
			Assert.AreEqual(0.0, rescorer.Rescore(user, 0.0));
			Assert.IsTrue(Double.IsNaN(rescorer.Rescore(user, Double.NaN)));
		}
	}

}