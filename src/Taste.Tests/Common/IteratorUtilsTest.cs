/*
 * Copyright 2007 and onwards Sean Owen
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

namespace taste.tests.commin
{
    using NUnit.Framework;
    using System;
    using System.Collections.Generic;


    /**
     * @author Sean Owen
     */
    public class IteratorUtilsTest : TestCase
    {

        private static List<String> TEST_DATA;
        static IteratorUtilsTest()
        {
            List<String> temp = new List<String>(3);
            temp.add("bar");
            temp.add("baz");
            temp.add("foo");
            TEST_DATA = Collections.unmodifiableList(temp);
        }

        public void testArray()
        {
            String[] data = TEST_DATA.toArray(new String[3]);
            Assert.AreEqual(TEST_DATA, IteratorUtils.iterableToList(new ArrayIterator<String>(data)));
        }

        public void testList()
        {
            Assert.AreEqual(TEST_DATA, IteratorUtils.iterableToList(TEST_DATA));
        }

        public void testCollection() 
        {
		    List<String> data = new TreeSet<String>();
		    data.Add("foo");
		    data.Add("bar");
		    data.Add("baz");
		    Assert.AreEqual(TEST_DATA, IteratorUtils.iterableToList(data));
	    }

        public void testComparator() 
        {
		    List<String> data = new List<String>(3);
		    data.Add("baz");
		    data.Add("bar");
		    data.Add("foo");
		    Assert.AreEqual(TEST_DATA, IteratorUtils.iterableToList(data, String.CASE_INSENSITIVE_ORDER));
	    }

        public void TestEmpty()
        {
            Assert.AreEqual(0, IteratorUtils.IterableToList(new List<Object>(0)).Count);
        }
    }
}