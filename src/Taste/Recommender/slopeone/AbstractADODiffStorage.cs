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

namespace Taste.Recommender.SlopeOne
{
    using System;
    using System.Data;
    using System.Data.Common;
    using System.Data.SqlTypes;
    using System.Collections.Generic;
    using Taste.Common;
    using Taste.Model;
    using Taste.Model.Ado;
    using Taste.Recommender;
    using Taste.Recommender.SlopeOne;
    using Iesi.Collections.Generic;
    using log4net;

    /**
     * <p>A  {@link DiffStorage} which stores diffs in a database. Database-specific implementations subclass
     * this abstract class. Note that this implementation has a fairly particular dependence on the
     * {@link taste.Model.DataModel} used; it needs a {@link ADODataModel} attached to the same
     * database since its efficent operation depends on accessing preference data in the database directly.</p>
     * 
     * @author Sean Owen
     * @since 1.6
     */    
    public abstract class AbstractADODiffStorage : DiffStorage
    {
        private static ILog log = LogManager.GetLogger(typeof(AbstractADODiffStorage).Name);

        public static String DEFAULT_DIFF_TABLE = "taste_slopeone_diffs";
        public static String DEFAULT_ITEM_A_COLUMN = "item_id_a";
        public static String DEFAULT_ITEM_B_COLUMN = "item_id_b";
        public static String DEFAULT_COUNT_COLUMN = "count";
        public static String DEFAULT_AVERAGE_DIFF_COLUMN = "average_diff";

        private AbstractADODataModel dataModel;

        private String getDiffSQL;

        private String getDiffsSQL;

        private String getAverageItemPrefSQL;

        private String[] updateDiffSQLs;

        private String[] removeDiffSQLs;

        private String getRecommendableItemsSQL;

        private String deleteDiffsSQL;

        private String createDiffsSQL;

        private String diffsExistSQL;
        private int minDiffCount;

        private ReentrantLock refreshLock;
        protected bool _userStoreProcs = false;

        protected AbstractADODiffStorage(AbstractADODataModel dataModel,
                                          String getDiffSQL,
                                          String getDiffsSQL,
                                          String getAverageItemPrefSQL,
                                          String[] updateDiffSQLs,
                                          String[] removeDiffSQLs,
                                          String getRecommendableItemsSQL,
                                          String deleteDiffsSQL,
                                          String createDiffsSQL,
                                          String diffsExistSQL,
                                          int minDiffCount)
        {
            if (dataModel == null)
            {
                throw new ArgumentNullException("dataModel is null");
            }
            if (minDiffCount < 0)
            {
                throw new ArgumentException("minDiffCount is not positive");
            }
            this.dataModel = dataModel;
            this.getDiffSQL = getDiffSQL;
            this.getDiffsSQL = getDiffsSQL;
            this.getAverageItemPrefSQL = getAverageItemPrefSQL;
            this.updateDiffSQLs = updateDiffSQLs;
            this.removeDiffSQLs = removeDiffSQLs;
            this.getRecommendableItemsSQL = getRecommendableItemsSQL;
            this.deleteDiffsSQL = deleteDiffsSQL;
            this.createDiffsSQL = createDiffsSQL;
            this.diffsExistSQL = diffsExistSQL;
            this.minDiffCount = minDiffCount;
            this.refreshLock = new ReentrantLock();
            if (IsDiffsExist())
            {
                log.Info("Diffs already exist in database; using them instead of recomputing");
            }
            else
            {
                log.Info("No diffs exist in database; recomputing...");
                BuildAverageDiffs();
            }
        }

      
        protected virtual void ConfigureGetDiffCommand(IDbCommand cmd, object itemID1, object itemID2)
        {
            throw new NotImplementedException();
        }

        protected virtual void ConfigureCreateDiffsCommand(IDbCommand cmd, int minDiffCount)
        {            
            throw new NotImplementedException();
        }

        protected virtual void ConfigureDeleteDiffsCommand(IDbCommand cmd)
        {
        }

        protected virtual void ConfigureGetDiffsCommand(IDbCommand cmd, object itemID, object userID)
        {
            throw new NotImplementedException();
        }

        protected virtual void ConfigureAverageItemPrefCommand(IDbCommand cmd, object itemID)
        {
            throw new NotImplementedException();
        }

        protected virtual void ConfigurePartialUpdateCommand(IDbCommand cmd, object itemID, double prefDelta)
        {
            throw new NotImplementedException();
        }

        protected virtual void ConfigureRecommendableItemsCommand(IDbCommand cmd, object userID)
        {
            throw new NotImplementedException();
        }


        public RunningAverage GetDiff(Object itemID1, Object itemID2)
        {
            try
            {
                using (IDbCommand cmd = dataModel.CreateCommand(getDiffSQL))
                {
                    ConfigureGetDiffCommand(cmd, itemID1, itemID2);
                    if (log.IsDebugEnabled)
                    {
                        log.DebugFormat("Executing SQL query: {0} " , getDiffSQL);
                    }
                    using (IDataReader rs = cmd.ExecuteReader())
                    {
                        if (rs.Read())
                        {
                            return new FixedRunningAverage(rs.GetInt32(1), rs.GetDouble(2));
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
            }
            catch (Exception sqle)
            {
                log.Warn("Exception while retrieving diff", sqle);
                throw new TasteException(sqle);
            }
        }

        public RunningAverage[] GetDiffs(Object userID, Object itemID, IList<Preference> prefs)
        {
            int size = prefs.Count;
            RunningAverage[] result = new RunningAverage[size];
            try
            {
                using (IDbCommand cmd = dataModel.CreateCommand(getDiffsSQL))
                {
                    ConfigureGetDiffsCommand(cmd, itemID, userID);
                    if (log.IsDebugEnabled)
                    {
                        log.DebugFormat("Executing SQL query: {0}", getDiffsSQL);
                    }
                    int i = 0;
                    using (IDataReader rs = cmd.ExecuteReader())
                    {
                        while (rs.Read())
                        {
                            result[i++] = new FixedRunningAverage(rs.GetInt32(1), rs.GetDouble(2));
                        }
                    }
                    if (i != size)
                    {
                        throw new TasteException("Wrong number of prefs read from database");
                    }
                }
            }
            catch (Exception sqle)
            {
                log.Warn("Exception while retrieving diff", sqle);
                throw new TasteException(sqle);
            }
            return result;
        }

        public RunningAverage GetAverageItemPref(Object itemID)
        {           
            try
            {              
                using (IDbCommand cmd = this.dataModel.CreateCommand(getAverageItemPrefSQL))
                {
                    ConfigureAverageItemPrefCommand(cmd, itemID);

                    if (log.IsDebugEnabled)
                    {
                        log.Debug("Executing SQL query: " + getAverageItemPrefSQL);
                    }
                    using (IDataReader rs = cmd.ExecuteReader())
                    {
                        if (rs.Read())
                        {
                            int count = rs.GetInt32(1);
                            if (count > 0)
                            {
                                return new FixedRunningAverage(count, rs.GetDouble(2));
                            }
                        }
                    }
                    return null;
                }
            }
            catch (Exception sqle)
            {
                log.Warn("Exception while retrieving average item pref", sqle);
                throw new TasteException(sqle);
            }
        }

        public void UpdateItemPref(Object itemID, double prefDelta, bool remove)
        {
            try
            {
                if (remove)
                {
                    DoPartialUpdate(removeDiffSQLs[0], itemID, prefDelta);
                    DoPartialUpdate(removeDiffSQLs[1], itemID, prefDelta);
                }
                else
                {
                    DoPartialUpdate(updateDiffSQLs[0], itemID, prefDelta);
                    DoPartialUpdate(updateDiffSQLs[1], itemID, prefDelta);
                }
            }
            catch (Exception sqle)
            {
                log.Warn("Exception while updating item diff", sqle);
                throw new TasteException(sqle);
            }
        }

        private void DoPartialUpdate(String sql, Object itemID, double prefDelta)
        {
            using (IDbCommand cmd = dataModel.CreateCommand(sql))
            {
                ConfigurePartialUpdateCommand(cmd, itemID, prefDelta);

                if (log.IsDebugEnabled)
                {
                    log.Debug("Executing SQL update: " + sql);
                }
                cmd.ExecuteNonQuery();
            }
        }

        public ISet<Item> GetRecommendableItems(Object userID)
        {
            try
            {
                using (IDbCommand cmd = dataModel.CreateCommand(getRecommendableItemsSQL))
                {
                    ConfigureRecommendableItemsCommand(cmd, userID);

                    if (log.IsDebugEnabled)
                    {
                        log.Debug("Executing SQL query: " + getRecommendableItemsSQL);
                    }
                    using (IDataReader rs = cmd.ExecuteReader())
                    {
                        ISet<Item> items = new HashedSet<Item>();
                        while (rs.Read())
                        {
                            items.Add(dataModel.GetItem(rs.GetValue(1), true));
                        }
                        return items;
                    }
                }
            }
            catch (Exception sqle)
            {
                log.Warn("Exception while retrieving recommendable items", sqle);
                throw new TasteException(sqle);
            }
        }

        protected virtual void BuildAverageDiffs()
        {                  
            try
            {
                using (IDbCommand cmd = dataModel.CreateCommand(deleteDiffsSQL))
                {
                    if (log.IsDebugEnabled)
                    {
                        log.Debug("Executing SQL update: " + deleteDiffsSQL);
                    }
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception sqle)
            {
                log.Warn("Exception while deleting diffs", sqle);
                throw new TasteException(sqle);
            }
            try
            {
                using (IDbCommand cmd = dataModel.CreateCommand(createDiffsSQL))
                {
                    ConfigureCreateDiffsCommand(cmd, minDiffCount);
                    if (log.IsDebugEnabled)
                    {
                        log.Debug("Executing SQL update: " + createDiffsSQL);
                    }
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception sqle)
            {
                log.Warn("Exception while creating diffs", sqle);
                throw new TasteException(sqle);
            }
        }

        protected virtual bool IsDiffsExist()
        {
            try
            {
                using (IDbCommand cmd = dataModel.CreateCommand(diffsExistSQL))
                {
                    if (log.IsDebugEnabled)
                    {
                        log.Debug("Executing SQL query: " + diffsExistSQL);
                    }
                    object cnt = cmd.ExecuteScalar();
                    return Convert.ToInt64(cnt) > 0;
                }
            }
            catch (Exception sqle)
            {
                log.Warn("Exception while deleting diffs", sqle);
                throw new TasteException(sqle);
            }
        }

        /**
         * {@inheritDoc}
         */
        public void Refresh()
        {
            if (refreshLock.TryLock())
            {
                try
                {
                    refreshLock.Lock();
                    dataModel.Refresh();
                    try
                    {
                        BuildAverageDiffs();
                    }
                    catch (TasteException te)
                    {
                        log.Warn( "Unexpected exception while refreshing", te);
                    }
                }
                finally
                {
                    refreshLock.Unlock();
                }
            }
        }

        public AbstractADODataModel DataModel
        {
            get { return dataModel; }
        }

        internal class FixedRunningAverage : RunningAverage
        {
            private int count;
            private double average;

            internal FixedRunningAverage(int count, double average)
            {
                this.count = count;
                this.average = average;
            }
            public void AddDatum(double datum)
            {
                throw new NotSupportedException();
            }
            public void RemoveDatum(double datum)
            {
                throw new NotSupportedException();
            }
            public void ChangeDatum(double delta)
            {
                throw new NotSupportedException();
            }
            public int Count
            {
                get { return count; }
            }
            public double Average
            {
                get { return average; }
            }
        }
    }

}