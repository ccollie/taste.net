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

namespace Taste.Recommender
{
	using System;
    using System.Diagnostics;
    using System.Collections;
    using Iesi.Collections.Generic;
	using System.Collections.Generic;
	using Taste.Common;	
	using Taste.Model;
	using Taste.Recommender;
    using log4net;


	/**
	 * <p>A {@link taste.Recommender.Recommender} that clusters
	 * {@link taste.Model.User}s, then determines
	 * the clusters' top recommendations. This implementation builds clusters by repeatedly merging clusters
	 * until only a certain number remain, meaning that each cluster is sort of a tree of other clusters.</p>
	 *
	 * <p>This {@link taste.Recommender.Recommender} therefore has a few properties to note:</p>
	 * <ul>
	 *  <li>For all {@link taste.Model.User}s in a cluster, recommendations will be the same</li>
	 *  <li>{@link #estimatePreference(Object, Object)} may well return {@link Double#NaN}; it does so when asked
	 *   to estimate preference for an {@link taste.Model.Item} for which no preference is expressed in the
	 *   {@link taste.Model.User}s in the cluster.</li>
	 * </ul>
	 *
	 * <p>This is an <em>experimental</em> implementation which tries to gain a lot of speed at the cost of
	 * accuracy in building clusters, compared to {@link taste.Recommender.TreeClusteringRecommender}.
	 * It will sometimes cluster two other clusters together that may not be the exact closest two clusters
	 * in existence. This may not affect the recommendation quality much, but it potentially speeds up the
	 * clustering process dramatically.</p>
	 *
	 * @author Sean Owen
	 * @since 1.6.1
	 */
	public class TreeClusteringRecommender2 : AbstractRecommender, ClusteringRecommender 
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(TreeClusteringRecommender2));
		
		private readonly ClusterSimilarity clusterSimilarity;
		private readonly int numClusters;
		private readonly double clusteringThreshold;
		private readonly bool clusteringByThreshold;
		
		private IDictionary<Object, IList<RecommendedItem>> topRecsByUserID;		
		private IDictionary<Object, ICollection<User>> clustersByUserID;
        private ICollection<ICollection<User>> allClusters = null;

		private bool clustersBuilt;
		
		private readonly ReentrantLock refreshLock;
		
		private readonly ReentrantLock buildClustersLock;

		/**
		 * @param dataModel {@link taste.Model.DataModel} which provdes {@link taste.Model.User}s
		 * @param clusterSimilarity {@link taste.Recommender.ClusterSimilarity} used to compute
		 *  cluster similarity
		 * @param numClusters desired number of clusters to create
		 * @throws IllegalArgumentException if arguments are <code>null</code>, or <code>numClusters</code> is
		 *  less than 2
		 */
		public TreeClusteringRecommender2(DataModel dataModel,
										  ClusterSimilarity clusterSimilarity,
										  int numClusters) 	
            :base(dataModel)
		{			
			if (clusterSimilarity == null) {
				throw new ArgumentNullException("clusterSimilarity is null");
			}
			if (numClusters < 2) {
				throw new ArgumentException("numClusters must be at least 2");
			}
			this.clusterSimilarity = clusterSimilarity;
			this.numClusters = numClusters;
			this.clusteringThreshold = Double.NaN;
			this.clusteringByThreshold = false;
			this.refreshLock = new ReentrantLock();
			this.buildClustersLock = new ReentrantLock();
		}

		/**
		 * @param dataModel {@link taste.Model.DataModel} which provdes {@link taste.Model.User}s
		 * @param clusterSimilarity {@link taste.Recommender.ClusterSimilarity} used to compute
		 *  cluster similarity
		 * @param clusteringThreshold clustering similarity threshold; clusters will be aggregated into larger
		 *  clusters until the next two nearest clusters' similarity drops below this threshold
		 * @throws IllegalArgumentException if arguments are <code>null</code>, or <code>clusteringThreshold</code> is
		 *  {@link Double#NaN}
		 * @since 1.1.1
		 */
		public TreeClusteringRecommender2(DataModel dataModel,
		                                  ClusterSimilarity clusterSimilarity,
		                                  double clusteringThreshold) 
			:base(dataModel)										  
		{
			if (clusterSimilarity == null) {
				throw new ArgumentNullException("clusterSimilarity is null");
			}
			if (double.IsNaN(clusteringThreshold)) {
				throw new ArgumentException("clusteringThreshold must not be NaN");
			}
			this.clusterSimilarity = clusterSimilarity;
			this.numClusters = int.MinValue;
			this.clusteringThreshold = clusteringThreshold;
			this.clusteringByThreshold = true;
			this.refreshLock = new ReentrantLock();
			this.buildClustersLock = new ReentrantLock();
		}


		/**
		 * {@inheritDoc}
		 */
		
		public override IList<RecommendedItem> Recommend(Object userID, int howMany, Rescorer<Item> rescorer)
		{
			if (userID == null || rescorer == null) 
            {
				throw new ArgumentNullException("userID or rescorer is null");
			}
			if (howMany < 1) 
            {
				throw new ArgumentException("howMany must be at least 1");
			}
			CheckClustersBuilt();

			if (log.IsDebugEnabled) 
            {
				log.Debug("Recommending items for user ID '" + userID + '\'');
			}

            IList<RecommendedItem> recommended;
            topRecsByUserID.TryGetValue(userID, out recommended);
			if (recommended == null) 
            {
                recommended = new List<RecommendedItem>();
				return recommended;
			}

			User theUser = this.DataModel.GetUser(userID);
			List<RecommendedItem> rescored = new List<RecommendedItem>(recommended.Count);
			// Only add items the user doesn't already have a preference for.
			// And that the rescorer doesn't "reject".
			foreach (RecommendedItem recommendedItem in recommended) 
            {
				Item item = recommendedItem.Item;

                if (rescorer.IsFiltered(item))
                {
                    continue;
                }

				if (theUser.GetPreferenceFor(item.ID) == null &&
					!double.IsNaN(rescorer.Rescore(item, recommendedItem.Value))) 
                {
					rescored.Add(recommendedItem);
				}
			}
			rescored.Sort(new ByRescoreComparator(rescorer));

			return rescored;
		}

		/**
		 * {@inheritDoc}
		 */
		public override double EstimatePreference(Object userID, Object itemID)
		{
			if (userID == null || itemID == null) 
            {
				throw new ArgumentNullException("userID or itemID is null");
			}
			DataModel model = this.DataModel;
			User theUser = model.GetUser(userID);
			Preference actualPref = theUser.GetPreferenceFor(itemID);
			if (actualPref != null) 
			{
				return actualPref.Value;
			}
			CheckClustersBuilt();
            IList<RecommendedItem> topRecsForUser;

            if (topRecsByUserID.TryGetValue(userID, out topRecsForUser))
            {
				foreach (RecommendedItem item in topRecsForUser) 
                {
					if (itemID.Equals(item.Item.ID)) 
                    {
						return item.Value;
					}
				}
			}
			// Hmm, we have no idea. The item is not in the user's cluster
			return double.NaN;
		}

		/**
		 * {@inheritDoc}
		 */
		
		public ICollection<User> GetCluster(Object userID)
		{
			if (userID == null) {
				throw new ArgumentNullException("userID is null");
			}
			CheckClustersBuilt();
			ICollection<User> cluster;

            if (!clustersByUserID.TryGetValue(userID, out cluster) || cluster == null)
            {
				return new List<User>();
			} 
            else 
            {
				return cluster;
			}
		}

        public ICollection<ICollection<User>> GetClusters()
        {
            CheckClustersBuilt();
            return allClusters;
        }

		private void CheckClustersBuilt() 
		{
			if (!clustersBuilt) {
				BuildClusters();
			}
		}

		internal class ClusterClusterPair : IComparable<ClusterClusterPair> 
		{
			
			private readonly ICollection<User> cluster1;			
			private readonly ICollection<User> cluster2;			
			private readonly double similarity;
			
			internal ClusterClusterPair(ICollection<User> cluster1,
			                           ICollection<User> cluster2,
			                           double similarity) 
			{
				this.cluster1 = cluster1;
				this.cluster2 = cluster2;
				this.similarity = similarity;
			}

			
			internal ICollection<User> Cluster1
			{
				get {return cluster1;}
			}

			
			internal ICollection<User> Cluster2 
			{
				get {return cluster2;}
			}

			internal double Similarity
			{
				get {return similarity;}
			}
			
			public override int GetHashCode() 
			{
				return cluster1.GetHashCode() ^ cluster2.GetHashCode() ^ similarity.GetHashCode();
			}

			public override bool Equals(Object o) 
			{
				if (!(o is ClusterClusterPair)) 
				{
					return false;
				}
				ClusterClusterPair other = (ClusterClusterPair) o;
				return cluster1.Equals(other.cluster1) &&
				       cluster2.Equals(other.cluster2) &&
				       similarity == other.similarity;
			}
			
			public int CompareTo(ClusterClusterPair other) 
			{
				double otherSimilarity = other.similarity;
				if (similarity > otherSimilarity) {
					return -1;
				} else if (similarity < otherSimilarity) {
					return 1;
				} else {
					return 0;
				}
			}

		}

		private void BuildClusters() 
        {
			try 
			{
				buildClustersLock.Lock();

				DataModel model = this.DataModel;
				int numUsers = model.GetNumUsers();

				if (numUsers == 0) 
                {
                    topRecsByUserID = new Dictionary<object, IList<RecommendedItem>>();
                    clustersByUserID = new Dictionary<object, ICollection<User>>();
				} 
                else 
                {

					LinkedList<ICollection<User>> clusters = new LinkedList<ICollection<User>>();
					// Begin with a cluster for each user:
					foreach (User user in model.GetUsers()) 
                    {
						ICollection<User> newCluster = new HashedSet<User>();
						newCluster.Add(user);
						clusters.AddLast(newCluster);
					}

					bool done = false;
					while (!done) 
                    {
						// We find a certain number of closest clusters...
						bool full = false;
						LinkedList<ClusterClusterPair> queue = new LinkedList<ClusterClusterPair>();
						int i = 0;
                        LinkedListNode<ICollection<User>> it2 = clusters.First;
						foreach (ICollection<User> cluster1 in clusters) 
                        {
							i++;
							//ListIterator<ICollection<User>> it2 = clusters.listIterator(i);
                            it2 = it2.Next;
							while (it2.Next != null) 
                            {
                                it2 = it2.Next;
								ICollection<User> cluster2 = it2.Value;
								double similarity = clusterSimilarity.GetSimilarity(cluster1, cluster2);
								if (!double.IsNaN(similarity) &&
									(!full || similarity > queue.Last.Value.Similarity)) 
                                {
                                    LinkedListNode<ClusterClusterPair> qit = queue.Last;

                                    /// loop looks fishy
									while (qit.Previous != null) 
                                    {
										if (similarity <= qit.Previous.Value.Similarity) 
                                        {
											break;
										}
                                        qit = qit.Previous;
									}
                                    queue.AddAfter(qit, new ClusterClusterPair(cluster1, cluster2, similarity));
									if (full) 
                                    {
										queue.RemoveLast();
									} 
                                    else if (queue.Count > numUsers) 
                                    { 
                                        // use numUsers as queue size limit
										full = true;
										queue.RemoveLast();
									}
								}
							}
						}

						// The first one is definitely the closest pair in existence so we can cluster
						// the two together, put it back into the set of clusters, and start again. Instead
						// we assume everything else in our list of closest cluster pairs is still pretty good,
						// and we cluster them too.

						while (queue.Count > 0) 
                        {
							if (!clusteringByThreshold && clusters.Count <= numClusters) 
                            {
								done = true;
								break;
							}

                            ClusterClusterPair top = queue.First.Value;
                            queue.RemoveFirst();

							if (clusteringByThreshold && top.Similarity < clusteringThreshold) 
                            {
								done = true;
								break;
							}

							ICollection<User> cluster1 = top.Cluster1;
							ICollection<User> cluster2 = top.Cluster2;

							// Pull out current two clusters from clusters
                            clusters.Remove(cluster1);
                            clusters.Remove(cluster2);

							// The only catch is if a cluster showed it twice in the list of best cluster pairs;
							// have to remove the others. Pull out anything referencing these clusters from queue
                            for (LinkedListNode<ClusterClusterPair> qit = queue.First; qit != null; qit = qit.Next)
                            {
								ClusterClusterPair pair = qit.Value;
								ICollection<User> pair1 = pair.Cluster1;
								ICollection<User> pair2 = pair.Cluster2;
                               
								if (pair1 == cluster1 || pair1 == cluster2 || pair2 == cluster1 || pair2 == cluster2) 
                                {
                                    if (qit == queue.First)
                                    {
                                        queue.RemoveFirst();
                                        qit = queue.First;
                                        continue;
                                    }
                                    else
                                    {
                                        LinkedListNode<ClusterClusterPair> temp = qit;
                                        qit = qit.Previous;
                                        queue.Remove(temp);
                                    }
								}
							}

							// Make new merged cluster
							HashedSet<User> merged = new HashedSet<User>(/*cluster1.Count + cluster2.Count*/);
							merged.AddAll(cluster1);
							merged.AddAll(cluster2);

							// Compare against other clusters; update queue if needed
							// That new pair we're just adding might be pretty close to something else, so
							// catch that case here and put it back into our queue
							foreach (ICollection<User> cluster in clusters) 
                            {
								double similarity = clusterSimilarity.GetSimilarity(merged, cluster);
								if (similarity > queue.Last.Value.Similarity) 
                                {
									// Iteration needs to be validated agains Java version
                                    LinkedListNode<ClusterClusterPair> qit = queue.First;
                                    while (qit.Next != null)
                                    {
                                        if (similarity > qit.Next.Value.Similarity)
                                        {
                                            break;
                                        }
                                        qit = qit.Next;
                                    }
                                    queue.AddAfter(qit, new ClusterClusterPair(merged, cluster, similarity));
								}
							}

							// Finally add new cluster to list
							clusters.AddLast(merged);
							
						}

					}

					topRecsByUserID = ComputeTopRecsPerUserID(clusters);
					clustersByUserID = ComputeClustersPerUserID(clusters);

				}

				clustersBuilt = true;
			} finally {
				buildClustersLock.Unlock();
			}
		}

		/** @param clusters */
		
		private static IDictionary<Object, IList<RecommendedItem>> ComputeTopRecsPerUserID(IEnumerable<ICollection<User>> clusters)
		{
			IDictionary<Object, IList<RecommendedItem>> recsPerUser = new Dictionary<Object, IList<RecommendedItem>>();
			foreach (ICollection<User> cluster in clusters) 
			{
				IList<RecommendedItem> recs = ComputeTopRecsForCluster(cluster);
				foreach (User user in cluster) 
				{
					recsPerUser.Add(user.ID, recs);
				}
			}
			//return Collections.unmodifiableMap(recsPerUser);
            return recsPerUser;
		}

		
		private static IList<RecommendedItem> ComputeTopRecsForCluster(ICollection<User> cluster)
		{

			ICollection<Item> allItems = new HashedSet<Item>();
			foreach (User user in cluster) 
			{
                Preference[] prefs = user.GetPreferencesAsArray();
				foreach (Preference pref in prefs) 
				{
					allItems.Add(pref.Item);
				}
			}

			TopItems.Estimator<Item> estimator = new Estimator(cluster);

			IList<RecommendedItem> topItems =
				TopItems.GetTopItems(int.MaxValue, allItems, NullRescorer<Item>.Instance, estimator);

			if (log.IsDebugEnabled) 
            {
				log.Debug("Recommendations are: " + topItems);
			}
			//return Collections.unmodifiableList(topItems);
            return topItems;
		}

		
		private static Dictionary<Object, ICollection<User>> ComputeClustersPerUserID(ICollection<ICollection<User>> clusters) 
        {
			Dictionary<Object, ICollection<User>> clustersPerUser = new Dictionary<Object, ICollection<User>>(clusters.Count);
			foreach (ICollection<User> cluster in clusters) 
            {
				foreach (User user in cluster) 
                {
					clustersPerUser.Add(user.ID, cluster);
				}
			}
			return clustersPerUser;
		}

		/**
		 * {@inheritDoc}
		 */
		public override void Refresh() 
        {
            if (refreshLock.TryLock())
            {
                try
                {
                    refreshLock.Lock();
                    base.Refresh();
                    clusterSimilarity.Refresh();
                    try
                    {
                        BuildClusters();
                    }
                    catch (TasteException te)
                    {
                        log.Warn( "Unexpected excpetion while refreshing", te);
                    }
                }
                finally
                {
                    refreshLock.Unlock();
                }
            }
		}

		
		public override String ToString() 
		{
			return "TreeClusteringRecommender2[clusterSimilarity:" + clusterSimilarity + ']';
		}

		private class Estimator : TopItems.Estimator<Item> 
		{		
			private readonly ICollection<User> cluster;
			
			public Estimator(ICollection<User> cluster) 
			{
				this.cluster = cluster;
			}
			
			public double Estimate(Item item) 
			{
				RunningAverage average = new FullRunningAverage();
				foreach (User user in cluster) 
				{
					Preference pref = user.GetPreferenceFor(item.ID);
					if (pref != null) 
					{
						average.AddDatum(pref.Value);
					}
				}
				return average.Average;
			}
		}
	}

}	