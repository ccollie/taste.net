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

namespace Taste.Correlation
{
	using System;
	using System.Collections.Generic;
	using Taste.Common;
	using Taste.Model;
	using Taste.Correlation;
	using Taste.Transforms;
    using log4net;


    /// <summary>
    /// An implementation of the Pearson Correlation. For <see cref="taste.Model.User">User</see>s X and Y, the following values
    /// are calculated:</p>
    /// <ul>
    /// <li>sumX2: sum of the square of all X's preference values</li>
    /// <li>sumY2: sum of the square of all Y's preference values</li>
    /// <li>sumXY: sum of the product of X and Y's preference value for all items for which both
    ///  X and Y express a preference</li>
    /// </ul>
	///
    /// <p>The Correlation is then:
    ///
    /// <p><code>sumXY / sqrt(sumX2 * sumY2)</code></p>
    ///
    /// <p>where <code>size</code> is the number of {@link Item}s in the {@link DataModel}.</p>
    ///
    /// <p>Note that this Correlation "centers" its data, shifts the user's preference values so that
    /// each of their means is 0. This is necessary to achieve expected behavior on all data sets.</p>
    ///
    /// <p>This Correlation implementation is equivalent to the cosine measure Correlation since the data it
    /// receives is assumed to be centered -- mean is 0. The Correlation may be interpreted as the cosine of the
    /// angle between the two vectors defined by the users' preference values.</p>
    ///
    /// author Sean Owen
    /// </summary>
	public sealed class PearsonCorrelation : UserCorrelation, ItemCorrelation 
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(PearsonCorrelation));	
		private readonly DataModel dataModel;		
		private PreferenceInferrer inferrer;		
		private PreferenceTransform2 prefTransform;
		private CorrelationTransform<Object> correlationTransform;
		private bool weighted;



        /// <summary>
        /// Creates a normal (unweighted) <see cref="PearsonCorrelation">PearsonCorrelation</see>.
        /// </summary>
        /// <param name="dataModel"></param>
		public PearsonCorrelation(DataModel dataModel) :this(dataModel, false)
		{
		}

        /// <summary>
        /// Creates a weighted <see cref="PearsonCorrelation">PearsonCorrelation</see>.
        /// </summary>
        /// <param name="dataModel"></param>
		public PearsonCorrelation(DataModel dataModel, bool weighted) 
		{
			if (dataModel == null) 
            {
				throw new ArgumentNullException("dataModel is null");
			}
			this.dataModel = dataModel;
			this.weighted = weighted;	
		}

        /// <summary>
        /// <p>Several subclasses in this package implement this method to actually compute the Correlation
        /// from figures computed over users or items. Note that the computations in this class "center" the
        /// data, such that X and Y's mean are 0.</p>
        ///
        /// <p>Note that the sum of all X and Y values must then be 0. This value isn't passed down into
        /// the standard Correlation computations as a result.</p>
        /// </summary>
        /// <param name="n">total number of users or items</param>
        /// <param name="sumXY">sum of product of user/item preference values, over all items/users prefererred by both users/items</param>
        /// <param name="sumX2">sum of the square of user/item preference values, over the first item/user</param>
        /// <param name="sumY2">sum of the square of the user/item preference values, over the second item/user</param>
        /// <returns>
        /// Correlation value between -1.0 and 1.0, inclusive, or {@link Double#NaN} if no Correlation
        /// can be computed (e.g. when no <see cref="taste.Model.Item">Item</see>s have been rated by both <see cref="taste.Model.User">User</see>s
        /// </returns>
        private static double ComputeResult(int n, double sumXY, double sumX2, double sumY2) 
		{
			if (n == 0) 
			{
				return Double.NaN;
			}
			// Note that sum of X and sum of Y don't appear here since they are assumed to be 0;
			// the data is assumed to be centered.
			double xTerm = Math.Sqrt(sumX2);
			double yTerm = Math.Sqrt(sumY2);
			double denominator = xTerm * yTerm;
			if (denominator == 0.0) 
			{
				// One or both parties has -all- the same ratings;
				// can't really say much Correlation under this measure
				return Double.NaN;
			}
			return sumXY / denominator;
		}

		public DataModel DataModel
		{
			get{return dataModel;}
		}

		public PreferenceInferrer PreferenceInferrer
		{
			get {return inferrer;}
			set
			{
				if (value == null) 
				{
					throw new ArgumentNullException("inferrer is null");
				}
				this.inferrer = value;			
			}
		}


		public PreferenceTransform2 PrefTransform
        {
            get { return prefTransform; }
            set { prefTransform = value; }
		}


		public CorrelationTransform<object> CorrelationTransform
        {
            get { return correlationTransform; }
            set { correlationTransform = value; }
		}


		bool IsWeighted 
		{
			get {return weighted;}
		}


        public double GetUserCorrelation(User user1, User user2) 
		{
			if (user1 == null || user2 == null) 
			{
				throw new ArgumentNullException("user1 or user2 is null");
			}

            Preference[] xPrefs = user1.GetPreferencesAsArray();
            Preference[] yPrefs = user2.GetPreferencesAsArray();

            if (xPrefs.Length == 0 || yPrefs.Length == 0) 
            {
                 return Double.NaN; 	                                         
            }

            Preference xPref = xPrefs[0];
            Preference yPref = yPrefs[0]; 
			Item xIndex = xPref.Item;
			Item yIndex = yPref.Item;

            int xPrefIndex = 1;
            int yPrefIndex = 1;

			double sumX = 0.0;
			double sumX2 = 0.0;
			double sumY = 0.0;
			double sumY2 = 0.0;
			double sumXY = 0.0;
			int count = 0;

			bool hasInferrer = inferrer != null;
			bool hasPrefTransform = prefTransform != null;

			while (true) 
            {
				int compare = xIndex.CompareTo(yIndex);
				if (hasInferrer || compare == 0) 
                {
					double x;
					double y;
					if (compare == 0) 
                    {
						// Both users expressed a preference for the item
						if (hasPrefTransform) 
                        {
							x = prefTransform.GetTransformedValue(xPref);
							y = prefTransform.GetTransformedValue(yPref);
						} else {
							x = xPref.Value;
							y = yPref.Value;
						}
					} else {
						// Only one user expressed a preference, but infer the other one's preference and tally
						// as if the other user expressed that preference
						if (compare < 0) 
                        {
							// X has a value; infer Y's
							if (hasPrefTransform) {
								x = prefTransform.GetTransformedValue(xPref);
							} else {
								x = xPref.Value;
							}
							y = inferrer.InferPreference(user2, xIndex);
						} else {
							// compare > 0
							// Y has a value; infer X's
							x = inferrer.InferPreference(user1, yIndex);
							if (hasPrefTransform) {
								y = prefTransform.GetTransformedValue(yPref);
							} else {
								y = yPref.Value;
							}
						}
					}
					sumXY += x * y;
					sumX += x;
					sumX2 += x * x;
					sumY += y;
					sumY2 += y * y;
					count++;
				}
				if (compare <= 0) 
                {
					if (xPrefIndex == xPrefs.Length) 
                    {
						break;
					}
                    xPref = xPrefs[xPrefIndex++];
					xIndex = xPref.Item;
				}
				if (compare >= 0) 
                {
                    if (yPrefIndex == yPrefs.Length) 
                    {
						break;
					}
                    yPref = yPrefs[yPrefIndex++];
					yIndex = yPref.Item;
				}
			}

			// "Center" the data. If my math is correct, this'll do it.
			double n = (double) count;
			double meanX = sumX / n;
			double meanY = sumY / n;
			double centeredSumXY = sumXY - meanY * sumX - meanX * sumY + n * meanX * meanY;
			double centeredSumX2 = sumX2 - 2.0 * meanX * sumX + n * meanX * meanX;
			double centeredSumY2 = sumY2 - 2.0 * meanY * sumY + n * meanY * meanY;

			double result = ComputeResult(count, centeredSumXY, centeredSumX2, centeredSumY2);

			if (correlationTransform != null) 
            {
				result = correlationTransform.TransformCorrelation(user1, user2, result);
			}

			if (!Double.IsNaN(result)) 
            {
				result = NormalizeWeightResult(result, count, dataModel.GetNumItems());
			}

			if (log.IsDebugEnabled) 
            {
				log.Debug("UserCorrelation between " + user1 + " and " + user2 + " is " + result);
			}
			return result;
		}


        public double GetItemCorrelation(Item item1, Item item2) 
        {

			if (item1 == null || item2 == null) 
            {
				throw new ArgumentNullException("item1 or item2 is null");
			}

            Preference[] xPrefs = dataModel.GetPreferencesForItemAsArray(item1.ID);
            Preference[] yPrefs = dataModel.GetPreferencesForItemAsArray(item2.ID);

            if (xPrefs.Length == 0 || yPrefs.Length == 0)
            {
				return Double.NaN;
			}

			Preference xPref = xPrefs[0];
			Preference yPref = yPrefs[0];
			User xIndex = xPref.User;
			User yIndex = yPref.User;
            
            int xPrefIndex = 1;
            int yPrefIndex = 1;

			double sumX = 0.0;
			double sumX2 = 0.0;
			double sumY = 0.0;
			double sumY2 = 0.0;
			double sumXY = 0.0;
			int count = 0;

			// No, pref inferrers and Transforms don't appy here. I think.

			while (true) 
			{
				int compare = xIndex.CompareTo(yIndex);
				if (compare == 0) 
				{
					// Both users expressed a preference for the item
					double x = xPref.Value;
					double y = yPref.Value;
					sumXY += x * y;
					sumX += x;
					sumX2 += x * x;
					sumY += y;
					sumY2 += y * y;
					count++;
				}
				if (compare <= 0) 
                {
                    if (xPrefIndex == xPrefs.Length)
                    {
						break;
					}
                    xPref = xPrefs[xPrefIndex++];
					xIndex = xPref.User;
				}
				if (compare >= 0) 
                {
                    if (yPrefIndex == yPrefs.Length) 
                    {
						break;
					}
                    yPref = yPrefs[yPrefIndex++];
					yIndex = yPref.User;
				}
			}

			// See comments above on these computations
			double n = (double) count;
			double meanX = sumX / n;
			double meanY = sumY / n;
			double centeredSumXY = sumXY - meanY * sumX - meanX * sumY + n * meanX * meanY;
			double centeredSumX2 = sumX2 - 2.0 * meanX * sumX + n * meanX * meanX;
			double centeredSumY2 = sumY2 - 2.0 * meanY * sumY + n * meanY * meanY;

			double result = ComputeResult(count, centeredSumXY, centeredSumX2, centeredSumY2);

			if (correlationTransform != null) 
            {
				result = correlationTransform.TransformCorrelation(item1, item2, result);
			}

			if (!Double.IsNaN(result)) 
            {
				result = NormalizeWeightResult(result, count, dataModel.GetNumUsers());
			}

			if (log.IsDebugEnabled) 
            {
				log.Debug("UserCorrelation between " + item1 + " and " + item2 + " is " + result);
			}
			return result;
		}

		private double NormalizeWeightResult(double result, int count, int num) 
		{
			if (weighted) 
			{
				double scaleFactor = 1.0 - (double) count / (double) (num + 1);
				if (result < 0.0) 
				{
					result = -1.0 + scaleFactor * (1.0 + result);
				} 
				else 
				{
					result = 1.0 - scaleFactor * (1.0 - result);
				}
			}
			// Make sure the result is not accidentally a little outside [-1.0, 1.0] due to rounding:
			if (result < -1.0) 
            {
				result = -1.0;
			} 
            else if (result > 1.0) 
            {
				result = 1.0;
			}
			return result;
		}



		public void Refresh() 
        {
			dataModel.Refresh();
			if (inferrer != null) 
			{
				inferrer.Refresh();
			}
			if (prefTransform != null) 
			{
				prefTransform.Refresh();
			}
			if (correlationTransform != null) 
			{
				correlationTransform.Refresh();
			}
		}


		public override String ToString() 
		{
			return "PearsonCorrelation[dataModel:" + dataModel + ",inferrer:" + inferrer + ']';
		}

	}
}