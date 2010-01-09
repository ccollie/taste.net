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
namespace Taste.Recommender
{
	using System;
	using System.Collections.Generic;
	using Taste.Common;
    using Taste.Correlation;
	using Taste.Model;
	using Taste.Recommender;


    /// <summary>
    /// <p>Defines cluster similarity as the <em>largest</em> Correlation between any two
    /// <see cref="taste.Model.User">User</see>s in the clusters -- that is, it says that clusters are close
    ///  when <em>some pair</em> of their members has high Correlation.</p>
    ///  
    /// @author Sean Owen
    /// </summary>
	public class NearestNeighborClusterSimilarity : ClusterSimilarity 
	{
		private readonly UserCorrelation correlation;
		private readonly double samplingPercentage;

		/**
		 * <p>Constructs a {@link NearestNeighborClusterSimilarity} based on the given {@link UserCorrelation}.
		 * All user-user correlations are examined.</p>
		 *
		 * @param Correlation
		 */
		public NearestNeighborClusterSimilarity(UserCorrelation correlation) 
            :this(correlation, 1.0)
		{
			
		}

        /// <summary>
        /// <p>Constructs a <see cref="taste.Recommender.NearestNeighborClusterSimilarity">NearestNeighborClusterSimilarity</see> based on the given 
        /// <see cref="taste.Correlation.UserCorrelation">UserCorrelation</see>.
        /// By setting <code>samplingPercentage</code> to a value less than 1.0, this implementation will only examine
        /// that fraction of all user-user correlations between two clusters, increasing performance at the expense
        /// of accuracy.</p>
        /// </summary>
        /// <param name="Correlation"></param>
        /// <param name="samplingPercentage"></param>
		public NearestNeighborClusterSimilarity(UserCorrelation correlation, double samplingPercentage) 
		{
			if (correlation == null) 
            {
				throw new ArgumentNullException("Correlation is null");
			}
			if (double.IsNaN(samplingPercentage) || samplingPercentage <= 0.0 || samplingPercentage > 1.0) {
				throw new ArgumentException("samplingPercentage is invalid: " + samplingPercentage);
			}
			this.correlation = correlation;
			this.samplingPercentage = samplingPercentage;
		}


        public double GetSimilarity(ICollection<User> cluster1,
		                            ICollection<User> cluster2) 
		{
			if (cluster1.Count == 0 || cluster2.Count == 0) 
            {
				return Double.NaN;
			}
			double greatestCorrelation = Double.NegativeInfinity;
            Random rand = RandomUtils.GetRandom();
			foreach (User user1 in cluster1) 
			{
				if (samplingPercentage >= 1.0 || rand.NextDouble() < samplingPercentage) 
				{
					foreach (User user2 in cluster2) 
					{
						double theCorrelation = correlation.GetUserCorrelation(user1, user2);
						if (theCorrelation > greatestCorrelation) 
						{
							greatestCorrelation = theCorrelation;
						}
					}
				}
			}
			// We skipped everything? well, at least try comparing the first Users to get some value
			if (greatestCorrelation == Double.NegativeInfinity) 
			{
                IEnumerator<User> it1 = cluster1.GetEnumerator();
                IEnumerator<User> it2 = cluster2.GetEnumerator();

                if (it1.MoveNext() && it2.MoveNext())
                {
                    return correlation.GetUserCorrelation(it1.Current, it2.Current);
                }
			}
			return greatestCorrelation;
		}

		/**
		 * {@inheritDoc}
		 */
		public void Refresh() 
		{
			correlation.Refresh();
		}

	
		public override String ToString() 
		{
			return "NearestNeighborClusterSimilarity[Correlation:" + correlation + ']';
		}
	}

}	