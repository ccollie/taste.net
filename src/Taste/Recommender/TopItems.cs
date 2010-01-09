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
	using Taste.Model;
    using Taste.Correlation;



    /// <summary>
    /// <p>A simple class that refactors the "find top N recommended items" logic that is used in
    /// several places in Taste.</p>
    /// 
    /// author Sean Owen
    /// </summary>
	public class TopItems 
	{
	
		public interface Estimator<T> 
		{
			double Estimate(T thing);
		}
	

		private TopItems() 
		{
		}
		
		public static IList<RecommendedItem> GetTopItems(int howMany,
		                                                IEnumerable<Item> allItems,
		                                                Rescorer<Item> rescorer,
		                                                Estimator<Item> estimator) 
		{
			if (allItems == null || rescorer == null || estimator == null) 
            {
				throw new ArgumentNullException("argument is null");
			}

            LinkedList<RecommendedItem> topItems = new LinkedList<RecommendedItem>();

			bool full = false;

			foreach (Item item in allItems) 
			{
				if (item.IsRecommendable && !rescorer.IsFiltered(item)) 
				{
					double preference = estimator.Estimate(item);
					double rescoredPref = rescorer.Rescore(item, preference);
                    LinkedListNode<RecommendedItem> node = topItems.Last;

    				if (!Double.IsNaN(rescoredPref) && 
                        (!full || rescoredPref > node.Value.Value)) 
					{

	    				// I think this is faster than Collections.binarySearch() over a LinkedList since our
    					// comparisons are cheap, which binarySearch() economizes at the expense of more traversals.
		    			// We also know that the right position tends to be at the end of the list.
					    while (node != null && node.Previous != null) 
                        {
                            node = node.Previous;
						    if (rescoredPref <= node.Value.Value) 
                            {
                                node = node.Next;
							    break;
						    }

                            if (node == topItems.First)
                                break;
					    }

                        RecommendedItem newItem = new GenericRecommendedItem(item, rescoredPref);

                        if (node == null)
                        {
                            topItems.AddFirst(newItem);
                        }
                        else if (topItems.Count == 1)
                        {
                            // special handling in this case is to avoid problems
                            // with negative preferences. Imagine -0.3 being added
                            // first followed by -0.6. If we simply did AddAfter,
                            // those items would be out of sequence - cc
                            if (rescoredPref > node.Value.Value)
                                topItems.AddAfter(node, newItem);
                            else
                                topItems.AddBefore(node, newItem);

                        }
                        else
                        {
                            topItems.AddAfter(node, newItem);
                        }

                        if (full)
                        {
                            topItems.RemoveLast();
                        }
                        else if (topItems.Count > howMany)
                        {
                            full = true;
                            topItems.RemoveLast();
                        }
					}
				}
			}

            List<RecommendedItem> result = new List<RecommendedItem>(topItems.Count);
            foreach (RecommendedItem item in topItems)
            {
                result.Add(item);
            }
            return result;
		}

		
		public static List<User> GetTopUsers(int howMany,
		                                     IEnumerable<User> allUsers,
		                                     Rescorer<User> rescorer,
		                                     Estimator<User> estimator) 
        {
			LinkedList<SimilarUser> topUsers = new LinkedList<SimilarUser>();
			bool full = false;
			foreach (User user in allUsers) 
			{
                if (rescorer.IsFiltered(user))
                    continue;

				double similarity = estimator.Estimate(user);
				double rescoredSimilarity = rescorer.Rescore(user, similarity);

                LinkedListNode<SimilarUser> node = topUsers.Last;
				if (!double.IsNaN(rescoredSimilarity) &&
				    (!full || rescoredSimilarity > node.Value.Similarity)) 
                {

                    //SimilarUser _user = new SimilarUser(user, similarity);
                    SimilarUser _user = new SimilarUser(user, rescoredSimilarity);
                    if (node == null)
                    {
                        topUsers.AddLast(_user);
                    }
                    else if (node.Previous == null) // 1 node
                    {
                        if (rescoredSimilarity > node.Value.Similarity)
                            topUsers.AddAfter(node, _user);
                        else
                            topUsers.AddBefore(node, _user);
                    }
                    else
                    {
                        while (node != null && node.Previous != null && (node != topUsers.First))
                        {
                            node = node.Previous;
                            if (rescoredSimilarity <= node.Value.Similarity)
                            {
                                topUsers.AddBefore(node, _user);
                                break;
                            }
                        }
                    }
					if (full) 
                    {
						topUsers.RemoveLast();
					} 
                    else if (topUsers.Count > howMany) 
                    {
						full = true;
						topUsers.RemoveLast();
					}
				}
			}

			List<User> result = new List<User>(topUsers.Count);
			foreach (SimilarUser similarUser in topUsers) 
			{
				result.Add(similarUser.User);
			}
			return result;
		}


      /**
       * <p>Thanks to tsmorton for suggesting this functionality and writing part of the code.</p>
       *
       * @see GenericItemCorrelation#GenericItemCorrelation(Iterable, int)
       * @see GenericItemCorrelation#GenericItemCorrelation(com.planetj.taste.Correlation.ItemCorrelation , com.planetj.taste.Model.DataModel , int)
       */
	    public static IList<GenericItemCorrelation.ItemItemCorrelation> GetTopItemItemCorrelations(
          int howMany, IEnumerable<GenericItemCorrelation.ItemItemCorrelation> allCorrelations) 
        {
		    LinkedList<GenericItemCorrelation.ItemItemCorrelation> topCorrelations = new LinkedList<GenericItemCorrelation.ItemItemCorrelation>();
		    bool full = false;
		    foreach (GenericItemCorrelation.ItemItemCorrelation correlation in allCorrelations) 
            {
			    double value = correlation.Value;
			    if (!full || value > topCorrelations.Last.Value.Value) 
                {

                    LinkedListNode<GenericItemCorrelation.ItemItemCorrelation> node = topCorrelations.Last;
                    while (node != null && node.Previous != null) 
                    {
                        node = node.Previous;
					    if (value <= node.Value.Value) 
                        {
                            node = node.Next;
						    break;
					    }

                        if (node == topCorrelations.First)
                            break;
				    }

                    if (node == null || topCorrelations.Count == 0)
                    {
                        topCorrelations.AddFirst(correlation);
                    }
                    else
                    {
                        topCorrelations.AddAfter(node, correlation);
                    }

				    if (full) 
                    {
					    topCorrelations.RemoveLast();
				    } 
                    else if (topCorrelations.Count > howMany) 
                    {
					    full = true;
					    topCorrelations.RemoveLast();
				    }
			    }
		    }
            List<GenericItemCorrelation.ItemItemCorrelation> result = new List<GenericItemCorrelation.ItemItemCorrelation>(topCorrelations);
            return result;
	    }

		// Hmm, should this be exposed publicly like {@link taste.Recommender.RecommendedItem}?
		public class SimilarUser : User 
		{	
			private readonly User user;
			private readonly double similarity;
			
			public SimilarUser(User user, double similarity) 
			{
				this.user = user;
				this.similarity = similarity;
			}
			
			public Object ID
			{
				get {return user.ID;}
			}
					
			public Preference GetPreferenceFor(Object itemID) 
			{
				return user.GetPreferenceFor(itemID);
			}
			
			public IEnumerable<Preference> GetPreferences() 
			{
                return user.GetPreferencesAsArray();
			}

            public Preference[] GetPreferencesAsArray()
            {
                return user.GetPreferencesAsArray();
            }

			public User User 
			{
				get {return user;}
			}
			
			public double Similarity
			{
				get {return similarity;}
			}
			
			public override int GetHashCode() 
			{
				return user.GetHashCode() ^ similarity.GetHashCode();
			}

			public override bool Equals(Object o) 
			{
				if (!(o is SimilarUser)) 
				{
					return false;
				}
				SimilarUser other = (SimilarUser) o;
				return user.Equals(other.user) && similarity == other.similarity;
			}
			
			public int CompareTo(User user) 
			{
				return this.user.CompareTo(user);
			}
		}

	}

}	