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

namespace Taste.Neighborhood
{
    using System;
    using System.Collections.Generic;

    using Taste.Common;
    using Taste.Model;
    using Taste.Correlation;
    using log4net;


    /// <summary>
    /// <p>Computes a neigbhorhood consisting of the nearest n <see cref="taste.item.User">User</see>s to a given <see cref="taste.item.User">User</see>.
    /// "Nearest" is defined by the given <see cref="taste.Correlation.UserCorrelation">UserCorrelation</see>.</p>
    /// </summary>
    public sealed class NearestNUserNeighborhood : AbstractUserNeighborhood 
    {
	    private static ILog log = LogManager.GetLogger(typeof(NearestNUserNeighborhood));

	    private SoftCache<Object, ICollection<User>> cache;

        /// <summary>
        /// construct a NearestNUserNeighborhood
        /// </summary>
        /// <param name="n">Neighborhood size</param>
        /// <param name="userCorrelation">nearness metric</param>
        /// <param name="dataModel">data Model</param>
        public NearestNUserNeighborhood(int n,
	                                    UserCorrelation userCorrelation,
	                                    DataModel dataModel) 
               :this(n, userCorrelation, dataModel, 1.0)
        {		
	    }

        /// <summary>
        /// construct a NearestNUserNeighborhood
        /// </summary>
        /// <param name="n">n Neighborhood size</param>
        /// <param name="userCorrelation">nearness metric</param>
        /// <param name="dataModel">data Model</param>
        /// <param name="samplingRate">percentage of users to consider when building Neighborhood -- decrease to
        /// trade quality for performance</param>
	    public NearestNUserNeighborhood(int n,
	                                    UserCorrelation userCorrelation,
	                                    DataModel dataModel,
	                                    double samplingRate)
            :base(userCorrelation, dataModel, samplingRate)
        {		    
		    if (n < 1) 
            {
			    throw new ArgumentException("n must be at least 1");
		    }
		    this.cache = new SoftCache<Object, ICollection<User>>(new Retriever(this,n), dataModel.GetNumUsers());
	    }

	    /**
	     * {@inheritDoc}
	     */
	    public override ICollection<User> GetUserNeighborhood(Object userID)
        {
		    return cache[userID];
	    }

	    public override String ToString() 
        {
		    return "NearestNUserNeighborhood";
	    }


	    internal class Retriever : SoftCacheRetriever<Object, ICollection<User>> 
        {
		    private int n;
            private NearestNUserNeighborhood owner;

		    public Retriever(NearestNUserNeighborhood parent, int n) 
            {
			    this.n = n;
                this.owner = parent;
		    }

		    public ICollection<User> GetValue(Object key) 
            {
			    if (log.IsInfoEnabled) 
                {
				    log.Info("Computing Neighborhood around user ID '" + key + '\'');
			    }

			    DataModel dataModel = owner.DataModel;
			    User theUser = dataModel.GetUser(key);
			    UserCorrelation userCorrelationImpl = owner.UserCorrelation;

			    LinkedList<UserCorrelationPair> queue = new LinkedList<UserCorrelationPair>();
			    bool full = false;
			    foreach (User user in dataModel.GetUsers()) 
                {
				    if (owner.SampleForUser && !key.Equals(user.ID)) 
                    {
					    double theCorrelation = userCorrelationImpl.GetUserCorrelation(theUser, user);
					    if (!Double.IsNaN(theCorrelation) && (!full || theCorrelation >  queue.Last.Value.Correlation)) 
                        {                            
                            LinkedListNode<UserCorrelationPair> iterator = queue.Last;
						    while (iterator != null && iterator.Previous != null) 
                            {
                                iterator = iterator.Previous;
                                  
							    if (theCorrelation <= iterator.Value.Correlation) 
                                {
								    iterator = iterator.Next;
								    break;
							    }
                                if (iterator == queue.First)
                                    break;
						    }

                            UserCorrelationPair pair = new UserCorrelationPair(user, theCorrelation);
                            if (iterator == null)
                            {
                                queue.AddFirst(pair);
                            }
                            else
                            {
                                queue.AddAfter(iterator, pair);
                            }
						    if (full) 
                            {
							    queue.RemoveLast();
						    } 
                            else if (queue.Count > n) 
                            {
							    full = true;
							    queue.RemoveLast();
						    }
					    }
				    }
			    }

			    IList<User> neighborhood = new List<User>(queue.Count);
			    foreach (UserCorrelationPair pair in queue) 
                {
				    //assert pair != null;
				    neighborhood.Add(pair.User);
			    }

			    if (log.IsInfoEnabled) 
                {
				    log.Info("UserNeighborhood around user ID '" + key + "' is: " + neighborhood);
			    }

			    //return Collections.unmodifiableList(Neighborhood);
                return neighborhood;
		    }
        }

        #region Internal helper class

        internal class UserCorrelationPair : IComparable<UserCorrelationPair> 
        {

		    User user;
		    double theCorrelation;

		    public UserCorrelationPair(User user, double theCorrelation) 
            {
			    this.user = user;
			    this.theCorrelation = theCorrelation;
		    }

            public double Correlation
            {
                get {return theCorrelation;}
            }

            public User User
            {
                get { return this.user; }
            }

		    public override int GetHashCode() 
            {
			    return user.GetHashCode() ^ theCorrelation.GetHashCode();
		    }

		    public override bool Equals(Object o) 
            {
			    if (!(o is UserCorrelationPair)) 
                {
				    return false;
			    }
			    UserCorrelationPair other = (UserCorrelationPair) o;
			    return user.Equals(other.user) && theCorrelation == other.theCorrelation;
		    }

		    public int CompareTo(UserCorrelationPair otherPair) 
            {
			    double otherCorrelation = otherPair.theCorrelation;
			    return theCorrelation > otherCorrelation ? -1 : theCorrelation < otherCorrelation ? 1 : 0;
		    }
        }

        #endregion

    }

}