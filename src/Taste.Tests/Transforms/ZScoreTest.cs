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
     * <p>Tests {@link ZScore}.</p>
     *
     * @author Sean Owen
     */
    [TestFixture]
    public class ZScoreTest : TransformTestCase
    {
        [Test]
        public void TestOnePref()
        {
            User user = GetUser("test", 1.0);
            PreferenceTransform2 zScore = new ZScore();
            Assert.AreEqual(0.0, zScore.GetTransformedValue(user.GetPreferenceFor("0")), EPSILON);
        }

        [Test]
        public void TestAllSame()
        {
            User user = GetUser("test", 1.0, 1.0, 1.0);
            PreferenceTransform2 zScore = new ZScore();
            Assert.AreEqual(0.0, zScore.GetTransformedValue(user.GetPreferenceFor("0")), EPSILON);
            Assert.AreEqual(0.0, zScore.GetTransformedValue(user.GetPreferenceFor("1")), EPSILON);
            Assert.AreEqual(0.0, zScore.GetTransformedValue(user.GetPreferenceFor("2")), EPSILON);
        }

        [Test]
        public void TestStdev()
        {
            User user = GetUser("test", -1.0, -2.0);
            PreferenceTransform2 zScore = new ZScore();
            Assert.AreEqual(0.707107, zScore.GetTransformedValue(user.GetPreferenceFor("0")), EPSILON);
            Assert.AreEqual(-0.707107, zScore.GetTransformedValue(user.GetPreferenceFor("1")), EPSILON);
        }

        [Test]
        public void TestExample()
        {
            User user = GetUser("test", 5.0, 7.0, 9.0);
            PreferenceTransform2 zScore = new ZScore();
            Assert.AreEqual(-1.0, zScore.GetTransformedValue(user.GetPreferenceFor("0")), EPSILON);
            Assert.AreEqual(0.0, zScore.GetTransformedValue(user.GetPreferenceFor("1")), EPSILON);
            Assert.AreEqual(1.0, zScore.GetTransformedValue(user.GetPreferenceFor("2")), EPSILON);
        }

        [Test]
        public void TestRefresh()
        {
            // Make sure this doesn't throw an exception
            new ZScore().Refresh();
        }
    }

}