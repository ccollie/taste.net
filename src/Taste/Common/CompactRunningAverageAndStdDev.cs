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
	/**
	 * <p>Extends {@link CompactRunningAverage} to add a running standard deviation computation.</p>
	 *
	 * @author Sean Owen
	 * @since 1.5
	 */
	[Serializable]
	public class CompactRunningAverageAndStdDev : CompactRunningAverage, RunningAverageAndStdDev
	{

		private float stdDev;
		private float sumX2;

		public CompactRunningAverageAndStdDev() 
		{
			stdDev = float.NaN;
		}

		public double StandardDeviation
		{
			get {return (double) stdDev;}
		}

		public override void AddDatum(double datum) 
		{
			base.AddDatum(datum);
			sumX2 += (float) (datum * datum);
			RecomputeStdDev();
		}

		
		public override void RemoveDatum(double datum) 
		{
			base.RemoveDatum(datum);
			sumX2 -= (float) (datum * datum);
			RecomputeStdDev();
		}

	
		public override void ChangeDatum(double delta) 
		{
			throw new NotSupportedException();
		}

		private void RecomputeStdDev() 
		{
			int count = this.Count;
			if (count > 1) 
			{
				double average = this.Average;
				stdDev = (float) Math.Sqrt(((double) sumX2 - average * average * (double) count) / (double) (count - 1));
			} else {
				stdDev = float.NaN;
			}
		}

		public override String ToString() 
		{
			return Average.ToString() + ',' + stdDev.ToString();
		}
	}

}