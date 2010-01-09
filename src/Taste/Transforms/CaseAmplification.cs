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

namespace Taste.Transforms
{
	using System;
	using Taste.Transforms;

    /// <summary>
    /// <p>Applies "case amplification" to correlations. This essentially makes big values bigger
    /// and small values smaller by raising each score to a power. It could however be used to achieve the
    /// opposite effect.</p>
    /// </summary>
	public class CaseAmplification : CorrelationTransform<Object> 
	{
		private readonly double factor;

        /// <summary>
        /// <p>Creates a <see cref="taste.transforma.CaseAmplification">CaseAmplification</see> transformation based on the given factor.</p>
        /// </summary>
        /// <param name="factor">transformation factor</param>
		public CaseAmplification(double factor) 
		{
			if (double.IsNaN(factor) || factor == 0.0) 
			{
				throw new ArgumentException("factor is 0 or NaN");
			}
			this.factor = factor;
		}



        /// <summary>
        /// <p>Transforms one Correlation value. This implementation is such that it's possible to define this
        /// transformation on one value in isolation. The "thing" parameters are therefore unused.</p>
        /// </summary>
        /// <param name="thing1">unused</param>
        /// <param name="thing2">unused</param>
        /// <param name="value">Correlation to transform</param>
        /// <returns>
        /// return <code>value<sup>factor</sup></code> if value is nonnegative;
        ///  <code>-value<sup>-factor</sup></code> otherwise
        /// </returns>
		public double TransformCorrelation(Object thing1, Object thing2, double value) 
		{
			return value < 0.0 ? -Math.Pow(-value, factor) : Math.Pow(value, factor);
		}


        public void Refresh() 
		{
			// do nothing
		}

		public override String ToString() 
		{
			return "CaseAmplification[factor:" + factor + ']';
		}
	}

}	