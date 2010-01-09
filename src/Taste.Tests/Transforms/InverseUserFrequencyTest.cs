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

namespace Taste.Tests.Transforms
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using Taste;
    using Taste.Model;
    using Taste.Transforms;
    using NUnit.Framework;

    /**
     * <p>Tests {@link InverseUserFrequency}.</p>
     *
     * @author Sean Owen
     */
    [TestFixture]
    public sealed class InverseUserFrequencyTest : TransformTestCase
    {
        [Test]
        public void TestIUF()
        {
            List<User> users = new List<User>(5);
            users.Add(GetUser("test1", 0.1));
            users.Add(GetUser("test2", 0.2, 0.3));
            users.Add(GetUser("test3", 0.4, 0.5, 0.6));
            users.Add(GetUser("test4", 0.7, 0.8, 0.9, 1.0));
            users.Add(GetUser("test5", 1.0, 1.0, 1.0, 1.0, 1.0));
            GenericDataModel dummy = new GenericDataModel(users);
            InverseUserFrequency iuf = new InverseUserFrequency(dummy, 10.0);

            User user = dummy.GetUser("test5");
            for (int i = 0; i < 5; i++)
            {
                Preference pref = user.GetPreferenceFor(i.ToString());
                Assert.IsNotNull(pref);
                Assert.AreEqual(Math.Log(5.0 / (double)(5 - i)) / Math.Log(iuf.LogBase),
                             iuf.GetTransformedValue(pref), EPSILON);
            }

            // Make sure this doesn't throw an exception
            iuf.Refresh();
        }
    }
}