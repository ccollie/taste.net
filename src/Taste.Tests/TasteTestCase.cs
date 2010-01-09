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

using System;
using System.Collections.Generic;

using Taste.Model;
using Taste.Common;

using log4net;
using NUnit.Framework;

namespace Taste.Tests
{

	/**
	 * @author Sean Owen
	 */
    [TestFixture]
	public abstract class TasteTestCase
	{
		/** "Close enough" value for floating-point comparisons. */
		public const double EPSILON = 0.00001;

        [TestFixtureSetUp]
		protected virtual void SetUp() 
		{
			// make sure we always show all log output during tests
			//SetLogLevel(FINEST);

			RandomUtils.UseTestSeed();
		}

		protected static void SetLogLevel(int level) 
		{
			//ILog log = LogManager.GetLogger("taste.impl");
		}

		public static User GetUser(String userID, params double[] values) 
		{
			List<Preference> prefs = new List<Preference>(values.Length);
			int i = 0;
			foreach (double value in values) 
			{
				prefs.Add(new GenericPreference(null, new GenericItem<String>(i.ToString()), value));
				i++;
			}
			return new GenericUser<String>(userID, prefs);
		}

		public static DataModel GetDataModel(params User[] users) 
		{
			return new GenericDataModel(users);
		}

		public static DataModel GetDataModel() 
		{
			return new GenericDataModel(GetMockUsers());
		}

		public static List<User> GetMockUsers() 
		{
			List<User> users = new List<User>(4);
			users.Add(GetUser("test1", 0.1, 0.3));
			users.Add(GetUser("test2", 0.2, 0.3, 0.3));
			users.Add(GetUser("test3", 0.4, 0.3, 0.5));
			users.Add(GetUser("test4", 0.7, 0.3, 0.8));
			return users;
		}

        public static IList<T> ScalarToList<T>(T item)
        {
            List<T> items = new List<T>();
            items.Add(item);
            return items;
        }

        public static bool ContainsAll<T>(ICollection<T> parent, ICollection<T> candidate)
        {
            foreach (T item in candidate)
                if (!parent.Contains(item))
                    return false;
            return true;
        }
	}


}