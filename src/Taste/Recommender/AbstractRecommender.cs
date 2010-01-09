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

namespace Taste.Recommender
{
	using System;
	using System.Collections.Generic;
    using Iesi.Collections.Generic;
	using Taste.Common;
	using Taste.Model;
	using Taste.Recommender;
    using log4net;
	
	/**
	 * @author Sean Owen
	 */
	public abstract class AbstractRecommender : Recommender 
	{
		private static ILog log = LogManager.GetLogger(typeof(AbstractRecommender));
		
		private readonly DataModel dataModel;	
		private readonly ReentrantLock refreshLock;
		
		protected AbstractRecommender(DataModel dataModel) 
		{
			if (dataModel == null) 
            {
				throw new ArgumentNullException("dataModel is null");
			}
			this.dataModel = dataModel;
			this.refreshLock = new ReentrantLock();
		}

		/**
		 * <p>Default implementation which just calls
		 * {@link Recommender#recommend(Object, int,taste.Recommender.Rescorer)},
		 * with a {@link taste.Recommender.Rescorer} that does nothing.</p>
		 */
		
		public virtual IList<RecommendedItem> Recommend(Object userID, int howMany) 
		{
			return Recommend(userID, howMany, NullRescorer<Item>.Instance);
		}

        public virtual IList<RecommendedItem> Recommend(Object userID, int howMany, Rescorer<Item> rescorer)
        {
            throw new NotImplementedException("The method or operation is not implemented.");
        }

        public virtual double EstimatePreference(Object userID, Object itemID)
        {
            throw new NotImplementedException("The method or operation is not implemented.");
        }

		/**
		 * <p>Default implementation which just calls {@link DataModel#setPreference(Object, Object, double)}.</p>
		 *
		 * @throws ArgumentException if userID or itemID is <code>null</code>, or if value is
		 *  {@link Double#NaN}
		 */
		public virtual void SetPreference(Object userID, Object itemID, double value) 
		{
			if (userID == null || itemID == null) 
			{
				throw new ArgumentNullException("userID or itemID is null");
			}
			if (double.IsNaN(value)) 
			{
				throw new ArgumentException("Invalid value: " + value);
			}
			if (log.IsDebugEnabled) 
            {
				log.Debug("Setting preference for user '" + userID + "', item '" + itemID + "', value " + value);
			}
			dataModel.SetPreference(userID, itemID, value);
		}

		/**
		 * <p>Default implementation which just calls
		 * {@link DataModel#removePreference(Object, Object)} (Object, Object)}.</p>
		 *
		 * @throws ArgumentException if userID or itemID is <code>null</code>
		 */
		public virtual void RemovePreference(Object userID, Object itemID) 
		{
			if (userID == null || itemID == null) {
				throw new ArgumentNullException("userID or itemID is null");
			}
			if (log.IsDebugEnabled) 
            {
				log.Debug("Remove preference for user '" + userID + "', item '" + itemID + '\'');
			}
			dataModel.RemovePreference(userID, itemID);
		}

		/**
		 * {@inheritDoc}
		 */
		
		public DataModel DataModel
		{
			get {return dataModel;}
		}

		/**
		 * {@inheritDoc}
		 */
		public virtual void Refresh() 
		{
            if (refreshLock.TryLock())
            {
                try
                {
                    dataModel.Refresh();
                }
                finally
                {
                    refreshLock.Unlock();
                }
            }
		}

		/**
		 * @param theUser {@link User} being evaluated
		 * @return all {@link Item}s in the {@link DataModel} for which the {@link User} has not expressed a preference
		 * @if an error occurs while listing {@link Item}s
		 */
		
		protected virtual ISet<Item> GetAllOtherItems(User theUser) 
		{
			if (theUser == null) 
            {
				throw new ArgumentNullException("theUser is null");
			}
			ISet<Item> allItems = new HashedSet<Item>();
			foreach (Item item in dataModel.GetItems()) 
			{
				// If not already preferred by the user, add it
				if (theUser.GetPreferenceFor(item.ID) == null) 
				{
					allItems.Add(item);
				}
			}
			return allItems;
		}

	}
}