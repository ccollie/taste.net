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

namespace Taste.Common
{
	using System;
    using System.Diagnostics;


	/**
	 * @author Sean Owen
	 */
	[Serializable]
	public class WeightedRunningAverage : RunningAverage
	{			
		private double totalWeight;
		private double average;

		public WeightedRunningAverage() 
		{
			totalWeight = 0.0;
			average = Double.NaN;
		}

		public void AddDatum(double datum) 
		{
			AddDatum(datum, 1.0);
		}

		public void AddDatum(double datum, double weight) 
		{
			Debug.Assert(totalWeight >= 0.0);
			double oldTotalWeight = totalWeight;
			totalWeight += weight;
			if (oldTotalWeight <= 0.0) 
            {
				average = datum * weight;
			} 
            else 
            {
				average = average * (oldTotalWeight / totalWeight) + datum / totalWeight;
			}
		}

		public void RemoveDatum(double datum) 
		{
			RemoveDatum(datum, 1.0);
		}

		public void RemoveDatum(double datum, double weight) 
		{
			Debug.Assert(totalWeight >= 0.0);
			double oldTotalWeight = totalWeight;
			totalWeight -= weight;
			if (totalWeight <= 0.0) 
            {
				average = Double.NaN;
				totalWeight = 0.0;
			} else {
				average = average * (oldTotalWeight / totalWeight) - datum / totalWeight;
			}
		}

		
		public void ChangeDatum(double delta) 
		{
			ChangeDatum(delta, 1.0);
		}

		public void ChangeDatum(double delta, double weight) 
		{
			Debug.Assert(totalWeight >= 0.0);
			if (weight > totalWeight) 
			{
				throw new ArgumentException();
			}
			average += (delta * weight) / totalWeight;
		}

		public double TotalWeight
		{
			get {return totalWeight;}
		}

		/**
		 * @return {@link #getTotalWeight()}
		 */
		public int Count
		{
			get {return (int) totalWeight;}
		}

		public double Average
		{
			get {return average;}
		}
	
		public override String ToString() 
		{
			return average.ToString();
		}
	}
	
}