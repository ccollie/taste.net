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
    using System.Diagnostics;
    using Iesi.Collections.Generic;
	using System.Collections.Generic;
	using Taste.Common;
	using Taste.Model;
	using Taste.Recommender;
    using log4net;


    /// <summary>
    /// <p>A <see cref="taste.Recommender.Recommender">Recommender</see> that clusters <see cref="taste.Model.User">User</see>s, then determines
    /// the clusters' top recommendations. This implementation builds clusters by repeatedly merging clusters
    /// until only a certain number remain, meaning that each cluster is sort of a tree of other clusters.</p>
    /// <p>This <see cref="taste.Recommender.Recommender">Recommender</see> therefore has a few properties to note:</p>
    /// <ul>
    ///     <li>For all <see cref="taste.Model.User">User</see>s in a cluster, recommendations will be the same</li>
    ///     <li>{@link #estimatePreference(Object, Object)} may well return {@link Double#NaN}; it does so when asked
    ///       to estimate preference for an {@link Item} for which no preference is expressed in the {@link User}s in
    ///       the cluster.</li>
    /// </ul>
    /// 
    /// @author Sean Owen
    /// </summary>
	public class TreeClusteringRecommender : AbstractRecommender, ClusteringRecommender 
    {

		private static ILog log = LogManager.GetLogger(typeof(TreeClusteringRecommender).Name);
		
		private readonly ClusterSimilarity clusterSimilarity;
		private readonly int numClusters;
		private readonly double clusteringThreshold;
		private readonly bool clusteringByThreshold;
		private readonly double samplingPercentage;
		
		private Dictionary<Object, IList<RecommendedItem>> topRecsByUserID;		
		private Dictionary<Object, ICollection<User>> clustersByUserID;
		private bool clustersBuilt;
		
		private readonly ReentrantLock refreshLock;
		
		private readonly ReentrantLock buildClustersLock;

		/**
		 * @param dataModel {@link DataModel} which provdes {@link User}s
		 * @param clusterSimilarity {@link ClusterSimilarity} used to compute cluster similarity
		 * @param numClusters desired number of clusters to create
		 * @throws ArgumentException if arguments are <code>null</code>, or <code>numClusters</code> is
		 *  less than 2
		 */
		public TreeClusteringRecommender(DataModel dataModel,
		                                 ClusterSimilarity clusterSimilarity,
		                                 int numClusters) 
            :this(dataModel, clusterSimilarity, numClusters, 1.0)
        {
			
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataModel"><see cref="taste.Model.DataModel">DataModel</see> which provides <see cref="taste.mode.User">User</see>s</param>
        /// <param name="clusterSimilarity"><see cref="taste.Recommender.ClusterSimilarity">ClusterSimilarity</see> used to compute cluster similarity</param>
        /// <param name="numClusters">desired number of clusters to create</param>
        /// <param name="samplingPercentage">
        /// percentage of all cluster-cluster pairs to consider when finding
        /// next-most-similar clusters. Decreasing this value from 1.0 can increase performance at the
        /// cost of accuracy
        /// </param>
        public TreeClusteringRecommender(DataModel dataModel,
		                                 ClusterSimilarity clusterSimilarity,
		                                 int numClusters,
		                                 double samplingPercentage) 
            : base(dataModel)
        {
			if (clusterSimilarity == null) 
            {
				throw new ArgumentNullException("clusterSimilarity is null");
			}
			if (numClusters < 2) {
				throw new ArgumentException("numClusters must be at least 2");
			}
			if (double.IsNaN(samplingPercentage) || samplingPercentage <= 0.0 || samplingPercentage > 1.0) {
				throw new ArgumentException("samplingPercentage is invalid: " + samplingPercentage);
			}
			this.clusterSimilarity = clusterSimilarity;
			this.numClusters = numClusters;
			this.clusteringThreshold = Double.NaN;
			this.clusteringByThreshold = false;
			this.samplingPercentage = samplingPercentage;
			this.refreshLock = new ReentrantLock();
			this.buildClustersLock = new ReentrantLock();
		}

		/**
		 * @param dataModel {@link DataModel} which provdes {@link User}s
		 * @param clusterSimilarity {@link ClusterSimilarity} used to compute cluster similarity
		 * @param clusteringThreshold clustering similarity threshold; clusters will be aggregated into larger
		 *  clusters until the next two nearest clusters' similarity drops below this threshold
		 * @throws ArgumentException if arguments are <code>null</code>, or <code>clusteringThreshold</code> is
		 *  {@link Double#NaN}
		 * @since 1.1.1
		 */
		public TreeClusteringRecommender(DataModel dataModel,
		                                 ClusterSimilarity clusterSimilarity,
		                                 double clusteringThreshold)
            : this(dataModel, clusterSimilarity, clusteringThreshold, 1.0)
        {
			
		}

        /// <summary>
        /// </summary>
        /// <param name="dataModel">
        /// <see cref="taste.Model.DataModel">DataModel</see> which provides <see cref="taste.Model.User">User</see>s
        /// </param>
        /// <param name="clusterSimilarity">
        /// <see cref="taste.Recommender.ClusterSimilarity">ClusterSimilarity</see> used to compute cluster similarity
        /// </param>
        /// <param name="clusteringThreshold">clustering similarity threshold; clusters will be aggregated 
        /// into larger clusters until the next two nearest clusters' similarity drops below this threshold
        /// </param>
        /// <param name="samplingPercentage">
        /// percentage of all cluster-cluster pairs to consider when finding next-most-similar 
        /// clusters. Decreasing this value from 1.0 can increase performance at the cost of accuracy
        /// </param>
        /// <remarks>
        /// Throws ArgumentException if arguments are <code>null</code>, or <code>clusteringThreshold</code> is
        /// {@link Double#NaN}, or samplingPercentage is {@link Double#NaN} or nonpositive or greater than 1.0
        /// </remarks>
		public TreeClusteringRecommender(DataModel dataModel,
		                                 ClusterSimilarity clusterSimilarity,
		                                 double clusteringThreshold,
		                                 double samplingPercentage) 
            : base(dataModel)
        {
			if (clusterSimilarity == null) 
            {
				throw new ArgumentNullException("clusterSimilarity is null");
			}
			if (double.IsNaN(clusteringThreshold)) 
            {
				throw new ArgumentException("clusteringThreshold must not be NaN");
			}
			if (double.IsNaN(samplingPercentage) || samplingPercentage <= 0.0 || samplingPercentage > 1.0) {
				throw new ArgumentException("samplingPercentage is invalid: " + samplingPercentage);
			}
			this.clusterSimilarity = clusterSimilarity;
			this.numClusters = int.MinValue;
			this.clusteringThreshold = clusteringThreshold;
			this.clusteringByThreshold = true;
			this.samplingPercentage = samplingPercentage;
			this.refreshLock = new ReentrantLock();
			this.buildClustersLock = new ReentrantLock();
		}

		/**
		 * {@inheritDoc}
		 */
		
		public override IList<RecommendedItem> Recommend(Object userID, int howMany, Rescorer<Item> rescorer)
		{
			if (userID == null || rescorer == null) {
				throw new ArgumentNullException("userID or rescorer is null");
			}
			if (howMany < 1) {
				throw new ArgumentException("howMany must be at least 1");
			}
			CheckClustersBuilt();

			if (log.IsDebugEnabled) 
            {
				log.Debug("Recommending items for user ID '" + userID + '\'');
			}

            IList<RecommendedItem> recommended;
            if (!topRecsByUserID.TryGetValue(userID, out recommended))
            {
                return new List<RecommendedItem>();
			}

			User theUser = this.DataModel.GetUser(userID);
			List<RecommendedItem> rescored = new List<RecommendedItem>(recommended.Count);
			// Only add items the user doesn't already have a preference for.
			// And that the rescorer doesn't "reject".
			foreach (RecommendedItem recommendedItem in recommended) 
            {
				Item item = recommendedItem.Item;
                if (rescorer.IsFiltered(item))
                    continue;
				if (theUser.GetPreferenceFor(item.ID) == null &&
					!double.IsNaN(rescorer.Rescore(item, recommendedItem.Value))) 
                {
					rescored.Add(recommendedItem);
				}
			}
            rescored.Sort(new ByRescoreComparator(rescorer));

			return rescored;
		}


        public override double EstimatePreference(Object userID, Object itemID) 
        {
			if (userID == null || itemID == null) 
            {
				throw new ArgumentNullException("userID or itemID is null");
			}
			User theUser = this.DataModel.GetUser(userID);
			Preference actualPref = theUser.GetPreferenceFor(itemID);
			if (actualPref != null) 
            {
				return actualPref.Value;
			}
			CheckClustersBuilt();

            IList<RecommendedItem> topRecsForUser = null;
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
			return Double.NaN;
		}

		/**
		 * {@inheritDoc}
		 */
		
		public ICollection<User> GetCluster(Object userID) 
		{
			if (userID == null) 
			{
				throw new ArgumentNullException("userID is null");
			}
			CheckClustersBuilt();

            ICollection<User> cluster = null;
            if (!clustersByUserID.TryGetValue(userID, out cluster))
            {
                return new List<User>();
			} else {
				return cluster;
			}
		}

		private void CheckClustersBuilt() 
		{
			if (!clustersBuilt) {
				BuildClusters();
			}
		}


        private void AddAll(ICollection<User> dest, ICollection<User> src)
        {
            foreach (User user in src)
                dest.Add(user);
        }

		private void BuildClusters() 
        {
			try 
			{
				buildClustersLock.Lock();
				int numUsers = this.DataModel.GetNumUsers();
				if (numUsers > 0) 
                {
					List<ICollection<User>> newClusters = new List<ICollection<User>>(numUsers);
					if (numUsers == 1) 
                    {
                        IEnumerable<User> users = this.DataModel.GetUsers();
                        User onlyUser = null;
                        foreach (User user in users)
                        {
                            onlyUser = user;
                            break;
                        }
                        List<User> single = new List<User>(1);
                        single.Add(onlyUser);
						newClusters.Add(single);
					} else {
						// Begin with a cluster for each user:
						foreach (User user in this.DataModel.GetUsers()) 
						{
							ICollection<User> newCluster = new HashedSet<User>();
							newCluster.Add(user);
							newClusters.Add(newCluster);
						}
						if (clusteringByThreshold) 
                        {
							Pair<ICollection<User>, ICollection<User>> nearestPair = FindNearestClusters(newClusters);
							if (nearestPair != null) 
                            {
								ICollection<User> cluster1 = nearestPair.First;
								ICollection<User> cluster2 = nearestPair.Second;
								while (clusterSimilarity.GetSimilarity(cluster1, cluster2) >= clusteringThreshold) 
                                {
									newClusters.Remove(cluster1);
									newClusters.Remove(cluster2);
									HashedSet<User> merged = new HashedSet<User>(/*cluster1.Count + cluster2.Count*/);
									merged.AddAll(cluster1);
									merged.AddAll(cluster2);
									newClusters.Add(merged);
									nearestPair = FindNearestClusters(newClusters);
									if (nearestPair == null) {
										break;
									}
									cluster1 = nearestPair.First;
									cluster2 = nearestPair.Second;
								}
							}
						} else {
							while (newClusters.Count > numClusters) 
                            {
								Pair<ICollection<User>, ICollection<User>> nearestPair =
									FindNearestClusters(newClusters);
								if (nearestPair == null) 
                                {
									break;
								}
								ICollection<User> cluster1 = nearestPair.First;
								ICollection<User> cluster2 = nearestPair.Second;
								newClusters.Remove(cluster1);
								newClusters.Remove(cluster2);
								HashedSet<User> merged = new HashedSet<User>(/*cluster1.Count + cluster2.Count*/);
								merged.AddAll(cluster1);
								merged.AddAll(cluster2);
								newClusters.Add(merged);
							}
						}
					}
					topRecsByUserID = ComputeTopRecsPerUserID(newClusters);
					clustersByUserID = ComputeClustersPerUserID(newClusters);
				} 
                else 
                {
					topRecsByUserID = new Dictionary<object,IList<RecommendedItem>>();
					clustersByUserID = new Dictionary<object,ICollection<User>>();
				}
				clustersBuilt = true;
			} finally {
				buildClustersLock.Unlock();
			}
		}

		private Pair<ICollection<User>, ICollection<User>> FindNearestClusters(List<ICollection<User>> clusters)
		{
			//Debug.Assert(buildClustersLock.isHeldByCurrentThread());

			int size = clusters.Count;
			Debug.Assert(size >= 2);
			Pair<ICollection<User>, ICollection<User>> nearestPair = null;
			double bestSimilarity = Double.NegativeInfinity;
			Random r = RandomUtils.GetRandom();
			for (int i = 0; i < size; i++) {
				ICollection<User> cluster1 = clusters[i];
				for (int j = i + 1; j < size; j++) {
					if (samplingPercentage >= 1.0 || r.NextDouble() < samplingPercentage) 
					{
						ICollection<User> cluster2 = clusters[j];
						double similarity = clusterSimilarity.GetSimilarity(cluster1, cluster2);
						if (!double.IsNaN(similarity) && similarity > bestSimilarity) 
						{
							bestSimilarity = similarity;
							nearestPair = new Pair<ICollection<User>, ICollection<User>>(cluster1, cluster2);
						}
					}
				}
			}
			return nearestPair;
		}

		
		private static Dictionary<Object, IList<RecommendedItem>> ComputeTopRecsPerUserID(
				IEnumerable<ICollection<User>> clusters) {
			Dictionary<Object, IList<RecommendedItem>> recsPerUser = new Dictionary<Object, IList<RecommendedItem>>();
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
			//return new ReadOnlyCollection<RecommendedItem>(topItems);
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
			return "TreeClusteringRecommender[clusterSimilarity:" + clusterSimilarity + ']';
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