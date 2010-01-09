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

namespace Taste.Eval
{
	using System;
	using Taste.Eval;


	/**
	 * @author Sean Owen
	 * @since 1.5.4
	 */
	[Serializable] 
	public class IRStatisticsImpl : IRStatistics
	{
		private readonly double precision;
		private readonly double recall;

		public IRStatisticsImpl(double precision, double recall) 
		{
			if (precision < 0.0 || precision > 1.0) 
			{
				throw new ArgumentException("Illegal precision: " + precision);
			}
			if (recall < 0.0 || recall > 1.0) {
				throw new ArgumentException("Illegal recall: " + recall);
			}
			this.precision = precision;
			this.recall = recall;
		}

		public double Precision
		{
			get {return precision;}
		}

		
		public double Recall 
		{
			get {return recall;}
		}

		public double GetF1Measure() 
		{
			return GetFNMeasure(1.0);
		}

		public double GetFNMeasure(double n) 
		{
			double sum = n * precision + recall;
			return sum == 0.0 ? double.NaN : (1.0 + n) * precision * recall / sum;
		}

	}
}	