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

	/**
	 * <p>Implementations encapsulate information retrieval-related statistics about
	 * a {@link taste.Recommender.Recommender}'s recommendations.</p>
	 *
	 * <p>See <a href="http://en.wikipedia.org/wiki/Information_retrieval">Information retrieval</a>.</p>
	 *
	 * @author Sean Owen
	 * @since 1.5.4
	 */
	public interface IRStatistics 
	{

		/**
		 * <p>See <a href="http://en.wikipedia.org/wiki/Information_retrieval#Precision">Precision</a>.</p>
		 */
        double Precision { get;}

		/**
		 * <p>See <a href="http://en.wikipedia.org/wiki/Information_retrieval#Recall">Recall</a>.</p>
		 */
        double Recall { get;}

		/**
		 * <p>See <a href="http://en.wikipedia.org/wiki/Information_retrieval#F-measure">F-measure</a>.</p>
		 */
		double GetF1Measure();

		/**
		 * <p>See <a href="http://en.wikipedia.org/wiki/Information_retrieval#F-measure">F-measure</a>.</p>
		 */
		double GetFNMeasure(double n);
	}

}