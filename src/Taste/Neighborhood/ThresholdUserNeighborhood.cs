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
    using Taste.Model;

    using Taste.Correlation;
    using Taste.Common;
    using log4net;


    /**
     * <p>Computes a neigbhorhood consisting of all {@link User}s whose similarity to the
     * given {@link User} meets or exceeds a certain threshold. Similartiy is defined by the given
     * {@link UserCorrelation}.</p>
     *
     * @author Sean Owen
     */
    public sealed class ThresholdUserNeighborhood : AbstractUserNeighborhood
    {

        private static readonly ILog log = LogManager.GetLogger(typeof(ThresholdUserNeighborhood));

        private readonly SoftCache<Object, ICollection<User>> cache;

        /**
         * @param threshold similarity threshold
         * @param userCorrelation similarity metric
         * @param dataModel data Model
         * @throws IllegalArgumentException if threshold is {@link Double#NaN},
         *  or if samplingRate is not positive and less than or equal to 1.0, or if userCorrelation
         *  or dataModel are <code>null</code>
         */
        public ThresholdUserNeighborhood(double threshold,
                                         UserCorrelation userCorrelation,
                                         DataModel dataModel)
            : this(threshold, userCorrelation, dataModel, 1.0)
        {
        }

        /**
         * @param threshold similarity threshold
         * @param userCorrelation similarity metric
         * @param dataModel data Model
         * @param samplingRate percentage of users to consider when building Neighborhood -- decrease to
         *  trade quality for performance
         * @throws IllegalArgumentException if threshold or samplingRate is {@link Double#NaN},
         *  or if samplingRate is not positive and less than or equal to 1.0, or if userCorrelation
         *  or dataModel are <code>null</code>
         * @since 1.3
         */
        public ThresholdUserNeighborhood(double threshold,
                                         UserCorrelation userCorrelation,
                                         DataModel dataModel,
                                         double samplingRate)
            : base(userCorrelation, dataModel, samplingRate)
        {
            if (Double.IsNaN(threshold))
            {
                throw new ArgumentException("threshold must not be NaN");
            }
            this.cache = new SoftCache<Object, ICollection<User>>(new Retriever(this, threshold), dataModel.GetNumUsers());
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
            return "ThresholdUserNeighborhood";
        }


        internal sealed class Retriever : SoftCacheRetriever<Object, ICollection<User>>
        {
            private double threshold;
            private ThresholdUserNeighborhood _owner;

            public Retriever(ThresholdUserNeighborhood owner, double threshold)
            {
                this.threshold = threshold;
                this._owner = owner;
            }

            public ICollection<User> GetValue(Object key)
            {
                if (log.IsInfoEnabled)
                {
                    log.Info("Computing Neighborhood around user ID '" + key + '\'');
                }

                DataModel dataModel = _owner.DataModel;
                User theUser = dataModel.GetUser(key);
                IList<User> neighborhood = new List<User>();
                IEnumerable<User> users = dataModel.GetUsers();
                UserCorrelation userCorrelationImpl = _owner.UserCorrelation;

                IEnumerator<User> en = users.GetEnumerator();
                while (en.MoveNext())
                {
                    User user = en.Current;
                    if (_owner.SampleForUser && !key.Equals(user.ID))
                    {
                        double theCorrelation = userCorrelationImpl.GetUserCorrelation(theUser, user);
                        if (!Double.IsNaN(theCorrelation) && theCorrelation >= threshold)
                        {
                            neighborhood.Add(user);
                        }
                    }
                }

                if (log.IsInfoEnabled)
                {
                    log.Info("UserNeighborhood around user ID '" + key + "' is: " + neighborhood);
                }

                //return Collections.unmodifiableList(Neighborhood);
                return neighborhood;
            }
        }
    }

}