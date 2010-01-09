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

namespace Taste.Model.Ado
{
    using System;
    using System.Data;
    using System.Data.SqlTypes;
    using Taste.Common;
    using Taste.Model;

    /// <summary>
    ///
    /// <p>A {@link DataModel} backed by a Ole DB database and accessed via ADO. It may work with other
    /// ADO databases. By default, this class assumes that there is table "taste_preferences" table with the
    /// following schema:</p>
    ///
    /// <table>
    /// <tr><th>user_id</th><th>item_id</th><th>preference</th></tr>
    /// <tr><td>ABC</td><td>123</td><td>0.9</td></tr>
    /// <tr><td>ABC</td><td>456</td><td>0.1</td></tr>
    /// <tr><td>DEF</td><td>123</td><td>0.2</td></tr>
    /// <tr><td>DEF</td><td>789</td><td>0.3</td></tr>
    /// </table>
    ///
    /// <p><code>user_id</code> must have a type compatible with the <code>String</code> type.
    /// <code>item_id</code> must have a type compatible with the <code>String</code> type.
    /// <code>preference</code> must have a type compatible with the <code>double</code> type.
    /// For example, the following command sets up a suitable table in MySQL, complete with
    /// primary key and indexes:</p>
    ///
    /// <pre>
    /// CREATE TABLE taste_preferences (
    ///   user_id VARCHAR(10) NOT NULL,
    ///   item_id VARCHAR(10) NOT NULL,
    ///   preference FLOAT NOT NULL,
    ///   PRIMARY KEY (user_id, item_id),
    ///   INDEX (user_id),
    ///   INDEX (item_id)
    /// )
    /// </pre>
    ///
    /// <h3>Performance Notes</h3>
    ///
    /// <p>See the notes in {@link AbstractADODataModel} regarding using connection pooling. It's pretty vital
    /// to performance.</p>
    ///
    /// <p>Some experimentation suggests that MySQL's InnoDB engine is faster than MyISAM for these kinds of
    /// applications. While MyISAM is the default and, I believe, generally considered the lighter-weight and faster
    /// of the two engines, my guess is the row-level locking of InnoDB helps here. Your mileage may vary.</p>
    ///
    /// <p>Here are some key settings that can be tuned for MySQL, and suggested size for a data set of around
    /// 1 million elements:</p>
    ///
    /// <ul>
    ///  <li>innodb_buffer_pool_size=64M</li>
    ///  <li>myisam_sort_buffer_size=64M</li>
    ///  <li>query_cache_limit=64M</li>
    ///  <li>query_cache_min_res_unit=512K</li>
    ///  <li>query_cache_type=1</li>
    ///  <li>query_cache_size=64M</li>
    /// </ul>
    /// 
    /// <p>Thanks to Amila Jayasooriya for contributing MySQL notes above as part of Google Summer of Code 2007.</p>
    ///
    /// @author Sean Owen
    /// </summary>
    public class OleDBDataModel : AbstractADODataModel
    {
        private IDbConnection connection;
        private DbType userIdType = DbType.AnsiString;
        private DbType itemIdType = DbType.AnsiString;
 
#if false
        /**
         * <p>Creates a {@link MySQLADODataModel} using the default {@link DataSource}
         * found under the given name, and using default table/column names.</p>
         *
         * @param dataSourceName name of {@link DataSource} to look up
         * @if {@link DataSource} can't be found
         */
        public MySQLADODataModel(String connString)
            : this(connString,
                 DEFAULT_PREFERENCE_TABLE,
                 DEFAULT_USER_ID_COLUMN,
                 DEFAULT_ITEM_ID_COLUMN,
                 DEFAULT_PREFERENCE_COLUMN)
        {
        }
#endif

        /**
         * <p>Creates a {@link MySQLADODataModel} using the given {@link DataSource}
         * and default table/column names.</p>
         *
         * @param dataSource {@link DataSource} to use
         */
        public OleDBDataModel(IDbConnection conn) :
            this(conn,
                 DEFAULT_PREFERENCE_TABLE,
                 DEFAULT_USER_ID_COLUMN,
                 DEFAULT_ITEM_ID_COLUMN,
                 DEFAULT_PREFERENCE_COLUMN,
                 DbType.AnsiString,
                 DbType.AnsiString)

        {
        }

        /**
         * <p>Creates a {@link OLEDBDataModel} using the given {@link DataSource}
         * and default table/column names.</p>
         *
         * @param dataSource {@link DataSource} to use
         * @param preferenceTable name of table containing preference data
         * @param userIDColumn user ID column name
         * @param itemIDColumn item ID column name
         * @param preferenceColumn preference column name
         */
        public OleDBDataModel(IDbConnection conn,
            String preferenceTable,
            String userIDColumn,
            String itemIDColumn,
            String preferenceColumn,
            DbType userIDType,
            DbType itemIDType)
            :  base(
                // getUserSQL
                    " SELECT " + itemIDColumn + ", " + preferenceColumn + 
                    " FROM " + preferenceTable +
                    " WHERE " + userIDColumn + "=? ORDER BY " + itemIDColumn,
                // getNumItemsSQL
                  "SELECT COUNT(DISTINCT " + itemIDColumn + ") FROM " + preferenceTable,
                // getNumUsersSQL
                  "SELECT COUNT(DISTINCT " + userIDColumn + ") FROM " + preferenceTable,
                // setPreferenceSQL
                  "INSERT INTO " + preferenceTable + " SET " + userIDColumn + "=?, " + itemIDColumn +
                  "=?, " + preferenceColumn + "=? ON DUPLICATE KEY UPDATE " + preferenceColumn + "=?",
                // removePreference SQL
                  "DELETE FROM " + preferenceTable + " WHERE " + userIDColumn + "=? AND " + itemIDColumn + "=?",
                // getUsersSQL
                  "SELECT " + itemIDColumn + ", " + preferenceColumn + ", " + userIDColumn + " FROM " +
                  preferenceTable + " ORDER BY " + userIDColumn + ", " + itemIDColumn,
                // getItemsSQL
                  "SELECT DISTINCT " + itemIDColumn + " FROM " + preferenceTable + " ORDER BY " + itemIDColumn,
                // getItemSQL
                  "SELECT 1 FROM " + preferenceTable + " WHERE " + itemIDColumn + "=?",
                // getPrefsForItemSQL
                    " SELECT " + preferenceColumn + ", " + userIDColumn + 
                    " FROM "  +  preferenceTable + 
                    " WHERE " + itemIDColumn + "=? ORDER BY " + userIDColumn,
                // getUsersPreferringItemSQL
                    " SELECT DISTINCT " + userIDColumn + 
                    " FROM " + preferenceTable + 
                    " WHERE " + itemIDColumn + "=? ORDER BY " + userIDColumn)        
        {
            connection = conn;
            this.userIdType = userIDType;
            this.itemIdType = itemIDType;
        }

        protected void AddUserIdParameter(IDbCommand cmd, string name, object id)
        {
            AddParameter(cmd, name, this.userIdType, id);
        }

        protected void AddUserIdParameter(IDbCommand cmd, object id)
        {
            AddParameter(cmd, "@userID", this.userIdType, id);
        }

        protected void AddItemIdParameter(IDbCommand cmd, string name, object id)
        {
            AddParameter(cmd, name, this.itemIdType, id);
        }

        protected void AddItemIdParameter(IDbCommand cmd, object id)
        {
            AddParameter(cmd, "@itemID", this.itemIdType, id);
        }
        protected override void ConfigureGetUserCommand(IDbCommand command, object id)
        {
            AddUserIdParameter(command, id);
        }

        protected override void ConfigureGetItemsCommand(IDbCommand command)
        {
        }

        protected override void ConfigureGetItemCommand(IDbCommand command, object itemId)
        {
            AddItemIdParameter(command, itemId);
        }

        protected override void ConfigureItemPreferencesCommand(IDbCommand command, object itemId)
        {
            AddItemIdParameter(command, itemId);
        }

        protected override void ConfigureSetPreferenceCommand(IDbCommand command, Object userID, Object itemID, double value)
        {
            AddUserIdParameter(command, userID);
            AddItemIdParameter(command, itemID);
            AddParameter(command, "@value", DbType.Double, value);
        }

        protected override void ConfigureRemovePreferenceCommand(IDbCommand command, Object userID, Object itemID)
        {
            AddUserIdParameter(command, userID);
            AddItemIdParameter(command, itemID);
        }

        public override IDbConnection GetConnection()
        {
            if (connection == null)
            {
            }
            return connection;
        }

    }
}