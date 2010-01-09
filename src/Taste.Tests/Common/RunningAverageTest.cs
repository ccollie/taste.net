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

namespace Taste.Tests
{
    using Taste.Common;
    using System;
    using NUnit.Framework;

    /**
     * <p>Tests {@link FullRunningAverage}.</p>
     * 
     * @author Sean Owen
     */
    [TestFixture]
    public sealed class RunningAverageTest : TasteTestCase 
    {

        [Test]
	    public void TestFull() 
        {
		    DoTestRunningAverage(new FullRunningAverage());
	    }

        [Test]
	    public void TestCompact() 
        {
		    DoTestRunningAverage(new CompactRunningAverage());
	    }

	    private static void DoTestRunningAverage(RunningAverage runningAverage) 
        {

		    Assert.AreEqual(0, runningAverage.Count);
            
		    Assert.IsTrue(Double.IsNaN(runningAverage.Average));
		    runningAverage.AddDatum(1.0);
		    Assert.AreEqual(1, runningAverage.Count);
		    Assert.AreEqual(1.0, runningAverage.Average);
		    runningAverage.AddDatum(1.0);
		    Assert.AreEqual(2, runningAverage.Count);
		    Assert.AreEqual(1.0, runningAverage.Average);
		    runningAverage.AddDatum(4.0);
		    Assert.AreEqual(3, runningAverage.Count);
		    Assert.AreEqual(2.0, runningAverage.Average);
		    runningAverage.AddDatum(-4.0);
		    Assert.AreEqual(4, runningAverage.Count);
		    Assert.AreEqual(0.5, runningAverage.Average);

		    runningAverage.RemoveDatum(-4.0);
		    Assert.AreEqual(3, runningAverage.Count);
		    Assert.AreEqual(2.0, runningAverage.Average);
		    runningAverage.RemoveDatum(4.0);
		    Assert.AreEqual(2, runningAverage.Count);
		    Assert.AreEqual(1.0, runningAverage.Average);

		    runningAverage.ChangeDatum(0.0);
		    Assert.AreEqual(2, runningAverage.Count);
		    Assert.AreEqual(1.0, runningAverage.Average);
		    runningAverage.ChangeDatum(2.0);
		    Assert.AreEqual(2, runningAverage.Count);
		    Assert.AreEqual(2.0, runningAverage.Average);
	    }
    }
}