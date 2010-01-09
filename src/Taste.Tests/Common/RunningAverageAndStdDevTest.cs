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

namespace Taste.Tests.Common
{
    using System;
    using Taste.Model;
    using Taste.Common;
    using Taste.Correlation;
    using Taste.Recommender;
    using Taste.Eval;
    using NUnit.Framework;

    /**
     * @author Sean Owen
     * @since 1.5
     */
    [TestFixture]
    public sealed class RunningAverageAndStdDevTest : TasteTestCase 
    {

        [Test]
	    public void TestFull() 
	    {
		    DoTestAverageAndStdDev(new FullRunningAverageAndStdDev());
	    }

        [Test]
	    public void TestCompact() 
	    {
		    DoTestAverageAndStdDev(new CompactRunningAverageAndStdDev());
	    }

	    private static void DoTestAverageAndStdDev(RunningAverageAndStdDev average) 
	    {
		    Assert.AreEqual(0, average.Count);
		    Assert.IsTrue(Double.IsNaN(average.Average));
		    Assert.IsTrue(Double.IsNaN(average.StandardDeviation));

		    average.AddDatum(6.0);
		    Assert.AreEqual(1, average.Count);
		    Assert.AreEqual(6.0, average.Average, EPSILON);
		    Assert.IsTrue(Double.IsNaN(average.StandardDeviation));

		    average.AddDatum(6.0);
		    Assert.AreEqual(2, average.Count);
		    Assert.AreEqual(6.0, average.Average, EPSILON);
		    Assert.AreEqual(0.0, average.StandardDeviation, EPSILON);

		    average.RemoveDatum(6.0);
		    Assert.AreEqual(1, average.Count);
		    Assert.AreEqual(6.0, average.Average, EPSILON);
		    Assert.IsTrue(Double.IsNaN(average.StandardDeviation));

		    average.AddDatum(-4.0);
		    Assert.AreEqual(2, average.Count);
		    Assert.AreEqual(1.0, average.Average, EPSILON);
		    Assert.AreEqual(5.0 * 1.4142135623730951, average.StandardDeviation, EPSILON);

		    average.RemoveDatum(4.0);
		    Assert.AreEqual(1, average.Count);
		    Assert.AreEqual(-2.0, average.Average, EPSILON);
		    Assert.IsTrue(Double.IsNaN(average.StandardDeviation));

	    }

    }
}