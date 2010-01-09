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
    using Taste.Tests;
	using Taste.Common;
    using Taste.Model;
	using Taste.Correlation;
	using System;
	using System.Collections.Generic;
    using NUnit.Framework;

	/**
	 * <p>Tests {@link AveragingPreferenceInferrer}.</p>
	 *
	 * @author Sean Owen
	 */
    [TestFixture]
	public sealed class AveragingPreferenceInferrerTest : TasteTestCase 
	{
        [Test]
		public void TestInferrer() 
		{
			User user1 = GetUser("test1", 3.0, -2.0, 5.0);
			Item item = new GenericItem<String>("3");
            List<User> ul = new List<User>();
            ul.Add(user1);
			DataModel model = new GenericDataModel(ul);
			PreferenceInferrer inferrer = new AveragingPreferenceInferrer(model);
			double inferred = inferrer.InferPreference(user1, item);
			Assert.AreEqual(2.0, inferred);
		}

	}

}