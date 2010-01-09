/*
 * Copyright 2007 and onwards, Sean Owen
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

namespace Taste.Tests
{
    using Taste.Common;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using NUnit.Framework;


    /**
     * <p>Tests .</p>
     *
     * @author Clayton Collie
     */
    [TestFixture]
    public class LRUCacheMapTest : TasteTestCase
    {

        [Test]
        public void TestPutAndGet()
        {
            LRUCacheMap<String, String> map = new LRUCacheMap<String, String>();
            Assert.IsNull(map["foo"]);
            
            map.Add("foo", "bar");
            Assert.AreEqual("bar", map["foo"]);
        }

        [Test]
        public void TestRemove()
        {
            LRUCacheMap<String, String> map = new LRUCacheMap<String, String>();
            map.Add("foo", "bar");
            map.Remove("foo");
            Assert.AreEqual(0,  map.Count);
            Assert.IsTrue(map.IsEmpty);
            Assert.IsNull(map["foo"]);
        }

        [Test]
        public void TestClear()
        {
            LRUCacheMap<String, String> map = new LRUCacheMap<String, String>();
            map.Add("foo", "bar");
            map.Clear();
            Assert.AreEqual(0, map.Count);
            Assert.IsTrue(map.IsEmpty);
            Assert.IsNull(map["foo"]);
        }

        [Test]
        public void TestSizeEmpty()
        {
            LRUCacheMap<String, String> map = new LRUCacheMap<String, String>();
            Assert.AreEqual(0, map.Count);
            Assert.IsTrue(map.IsEmpty);
            map.Add("foo", "bar");
            Assert.AreEqual(1, map.Count);
            Assert.IsFalse(map.IsEmpty);
            map.Remove("foo");
            Assert.AreEqual(0, map.Count);
            Assert.IsTrue(map.IsEmpty);
        }

        [Test]
        public void TestContains()
        {
            LRUCacheMap<String, String> map = BuildTestFastMap();
            Assert.IsTrue(map.ContainsKey("foo"));
            Assert.IsTrue(map.ContainsKey("baz"));
            Assert.IsTrue(map.ContainsKey("alpha"));
            Assert.IsTrue(map.ContainsValue("bar"));
            Assert.IsTrue(map.ContainsValue("bang"));
            Assert.IsTrue(map.ContainsValue("beta"));
            Assert.IsFalse(map.ContainsKey("something"));
        }


        [Test]
        public void TestNull() 
        {
            LRUCacheMap<String, String> map = new LRUCacheMap<String, String>();
            try
            {
                map.Add(null, "bar");
                Assert.Fail("Should have thrown NullReferenceException");
            }
            catch (ArgumentNullException)
            {
            }

            try
            {
                map.Add("foo", null);
                Assert.Fail("Should have thrown ArgumentNullException");
            }
            catch (ArgumentNullException)
            {
                // good
            }
            try
            {
                string v = map[null];
                Assert.Fail("Should have thrown ArgumentNullException");
            }
            catch (ArgumentNullException)
            {

            }          
        }


        public void testGrow()
        {
            LRUCacheMap<String, String> map = new LRUCacheMap<String, String>(1, LRUCacheMap<string, string>.NO_MAX_SIZE);
            map.Add("foo", "bar");
            map.Add("baz", "bang");
            Assert.AreEqual("bar", map["foo"]);
            Assert.AreEqual("bang", map["baz"]);
        }

        [Test]
        public void TestKeys()
        {
            LRUCacheMap<String, String> map = BuildTestFastMap();
            ICollection<String> expected = new List<String>(3);
            expected.Add("foo");
            expected.Add("baz");
            expected.Add("alpha");
            
            ICollection<String> actual = map.Keys;
            Assert.IsTrue(ContainsAll<string>(expected, actual));
            Assert.IsTrue(ContainsAll<string>(actual, expected));
        }

        [Test]
        public void TestValues()
        {
            LRUCacheMap<String, String> map = BuildTestFastMap();
            ICollection<String> expected = new List<String>(3);
            expected.Add("bar");
            expected.Add("bang");
            expected.Add("beta");
            ICollection<String> actual = map.Values;
            Assert.IsTrue(ContainsAll<string>(expected, actual));
            Assert.IsTrue(ContainsAll<string>(actual, expected));
        }

#if false        
        [Test]
        public void TestEntries() 
        {
            LRUCacheMap<String, String> map = BuildTestFastMap();
            ICollection<String> expectedKeys = new List<String>(3);
		    expectedKeys.Add("foo");
		    expectedKeys.Add("baz");
		    expectedKeys.Add("alpha");

            ICollection<String> expectedValues = new List<String>(3);
		    expectedValues.Add("bar");
		    expectedValues.Add("bang");
		    expectedValues.Add("beta");
		    Assert.AreEqual(3, expectedValues.Count);
		    foreach (KeyValuePair<string,string> entry in map) 
            {
			    expectedKeys.Remove(entry.Key);
			    expectedValues.Remove(entry.Value);
		    }
		    Assert.AreEqual(0, expectedKeys.Count);
		    Assert.AreEqual(0, expectedValues.Count);
	    }
#endif


        [Test]
        public void TestVersusDictionary()
        {
            LRUCacheMap<int, String> actual = new LRUCacheMap<int, String>(1, 10000);
            IDictionary<int, String> expected = new Dictionary<int, String>(10000);
            Random r = RandomUtils.GetRandom();
            for (int i = 0; i < 10000; i++)
            {
                double d = r.NextDouble();
                int key = r.Next(100);
                if (d < 0.4)
                {
                    Assert.AreEqual(expected[key], actual[key]);
                }
                else
                {
                    if (d < 0.7)
                    {
                        expected.Add(key, "foo");
                        actual.Add(key, "foo");
                        //Assert.AreEqual(expected.Add(key, "foo"), actual.Add(key,"foo"));
                    }
                    else
                    {
                        Assert.AreEqual(expected.Remove(key), actual.Remove(key));
                    }
                    Assert.AreEqual(expected.Count, actual.Count);
                }
            }
        }

        [Test]
        public void TestMaxSize()
        {
            LRUCacheMap<String, String> map = new LRUCacheMap<String, String>(1, 1);
            map.Add("foo", "bar");
            Assert.AreEqual(1, map.Count);
            map.Add("baz", "bang");
            Assert.AreEqual(1, map.Count);
            Assert.IsNull(map["foo"]);
            map.Add("baz", "buzz");
            Assert.AreEqual(1, map.Count);
            Assert.AreEqual("buzz", map["baz"]);
        }


        private static LRUCacheMap<String, String> BuildTestFastMap()
        {
            LRUCacheMap<String, String> map = new LRUCacheMap<String, String>();
            map.Add("foo", "bar");
            map.Add("baz", "bang");
            map.Add("alpha", "beta");
            return map;
        }
    }

}