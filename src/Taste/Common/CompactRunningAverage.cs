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
    using Taste.Common;

    /// <summary>
    /// <p>Like <see cref="taste.Common.FullRunningAverage">FullRunningAverage</see> but uses smaller values (short, float)
    /// to conserve memory. This is used in <see cref="taste.Recommender.SlopeOne.SlopeOneRecommender">SlopeOneRecommender</see>
    /// since it may allocate tens of millions of such objects.</p>
    /// </summary>
	[Serializable] 
	public class CompactRunningAverage : RunningAverage
	{

		private char count;
		private float average;

		public CompactRunningAverage() 
		{
			count = (char) 0;
			average = float.NaN;
		}

		public virtual void AddDatum(double datum) 
		{
			Debug.Assert((int) count >= 0);
            if ((int)count < 65535) // = 65535 = 2^16 - 1
            { 
				if ((int) ++count == 1) 
                {
					average = (float) datum;
				} 
                else 
                {
					average = (float)
						((double) average * ((double) ((int) count - 1) / (double) count) + datum / (double) count);
				}
			}
		}

		public virtual void RemoveDatum(double datum) 
		{
			Debug.Assert((int) count >= 0);
			if ((int) count == 0) 
			{
				throw new IllegalStateException();
			}
			if ((int) --count == 0) 
			{
				average = float.NaN;
			} 
            else 
            {
				average = (float)
					((double) average * ((double) ((int) count + 1) / (double) count) - datum / (double) count);
			}
		}

		public virtual void ChangeDatum(double delta) 
		{
			Debug.Assert((int) count >= 0);
			if ((int) count == 0) 
			{
				throw new IllegalStateException();
			}
			average += (float) (delta / (double) count);
		}

		public int Count
		{
			get {return (int) count;}
		}

		public double Average
		{
			get {return (double) average;}
		}

		public override String ToString() 
		{
			return average.ToString();
		}
	}
	
}