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


    /**
     * <p>Contains methods and resources useful to all classes in this package.</p>
     *
     * @author Sean Owen
     */
    public abstract class AbstractUserNeighborhood : UserNeighborhood
    {
        private readonly UserCorrelation userCorrelation;
        private readonly DataModel dataModel;
        private readonly double samplingRate;
        private readonly Random random = new Random();

        public AbstractUserNeighborhood()
        {
        }

        public AbstractUserNeighborhood(UserCorrelation userCorrelation,
                             DataModel dataModel,
                             double samplingRate)
        {
            if (userCorrelation == null || dataModel == null)
            {
                throw new ArgumentNullException("userCorrelation or dataModel is null");
            }
            if (Double.IsNaN(samplingRate) || samplingRate <= 0.0 || samplingRate > 1.0)
            {
                throw new ArgumentException("samplingRate must be in (0,1]");
            }
            this.userCorrelation = userCorrelation;
            this.dataModel = dataModel;
            this.samplingRate = samplingRate;
        }


        public UserCorrelation UserCorrelation
        {
            get { return userCorrelation; }
        }


        public DataModel DataModel
        {
            get { return dataModel; }
        }

        public bool SampleForUser
        {
            get { return samplingRate >= 1.0 || random.Next() < samplingRate; }
        }

        public abstract ICollection<User> GetUserNeighborhood(Object userID);

        public void Refresh()
        {
            userCorrelation.Refresh();
            dataModel.Refresh();
        }
    }

}
