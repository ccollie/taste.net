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

namespace Taste.Common
{
	using System;
	using System.Collections.Generic;

	/**
	 * <p>Extends {@link FullRunningAverage} to add a running standard deviation computation.</p>
	 *
	 * @author Sean Owen
	 * @since 1.5
	 */
	public class FullRunningAverageAndStdDev : FullRunningAverage, RunningAverageAndStdDev
	{
		private double stdDev;
		private double sumX2;

		public FullRunningAverageAndStdDev() 
		{
			stdDev = Double.NaN;
		}

		public double StandardDeviation
		{
			get {return stdDev;}
		}

		public override void AddDatum(double datum) 
		{
			base.AddDatum(datum);
			sumX2 += datum * datum;
			RecomputeStdDev();
		}
	
		public override void RemoveDatum(double datum) 
		{
			base.RemoveDatum(datum);
			sumX2 -= datum * datum;
			RecomputeStdDev();
		}

		/**
		 * @throws UnsupportedOperationException
		 */
		public override void ChangeDatum(double delta) 
		{
			throw new NotSupportedException();
		}

		private void RecomputeStdDev() 
		{
			int count = Count;
			if (count > 1) 
			{
				double average = Average;			
				stdDev = Math.Sqrt((sumX2 - average * average * (double) count) / (double) (count - 1));
			} else {
				stdDev = Double.NaN;
			}
		}

		public override String ToString() 
		{
			return (Average.ToString() + ',' + stdDev);
		}
	}

}