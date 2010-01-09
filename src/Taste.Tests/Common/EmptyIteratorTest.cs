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

namespace taste.tests
{
    using System;
    using System.Collections.Generic;
    using taste.common;
    using NUnit.Framework;

    /**
     * @author Sean Owen
     */
    [TestFixture]
    public class EmptyIteratorTest : TasteTestCase
    {
        [Test]
        public void testIterator()
        {
            IEnumerator<Object> mock = new EmptyEnumerator<Object>();
            Assert.IsFalse(mock.MoveNext());
            try
            {
                mock.MoveNext();
                Assert.Fail("Should have thrown InvalidOperationException");
            }
            catch (System.InvalidOperationException)
            {
                // good
            }
        }

    }
}
