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
	using Taste.Common;
	using Taste.Model.Ado;
    using Taste.Recommender;

	/**
	 * <p>MySQL-specific implementation. Should be used in conjunction with a
	 * {@link MySQLDataModel}. This implementation stores item-item diffs in a MySQL
	 * database and encapsulates some other slope-one-specific operations that are needed
	 * on the preference data in the database. It assumes the database has a schema like:</p>
	 *
	 * <table>
	 * <tr><th>item_id_a</th><th>item_id_b</th><th>average_diff</th><th>count</th></tr>
	 * <tr><td>123</td><td>234</td><td>0.5</td><td>5</td></tr>
	 * <tr><td>123</td><td>789</td><td>-1.33</td><td>3</td></tr>
	 * <tr><td>234</td><td>789</td><td>2.1</td><td>1</td></tr>
	 * </table>
	 *
	 * <p><code>item_id_a</code> and <code>item_id_b</code> must have type compatible with
	 * the <code>String</code> type. <code>average_diff</code> must be compatible with
	 * <code>double</code> and <code>count</code> must be compatible with <code>int</code>.</p>
	 *
	 * <p>The following command sets up a suitable table in MySQL:</p>
	 *
	 * <pre>
	 * CREATE TABLE taste_slopeone_diffs (
	 *   item_id_a VARCHAR(10) NOT NULL,
	 *   item_id_b VARCHAR(10) NOT NULL,
	 *   average_diff FLOAT NOT NULL,
	 *   count INT NOT NULL,
	 *   PRIMARY KEY (item_id_a, item_id_b),
	 *   INDEX (item_id_a),
	 *   INDEX (item_id_b)
	 * )
	 * </pre>
	 *
	 * @author Sean Owen
	 * @since 1.6
	 */
	public class OleDBDiffStorage : AbstractADODiffStorage 
    {
		private const int DEFAULT_MIN_DIFF_COUNT = 2;

		public OleDBDiffStorage(OleDBDataModel dataModel) 
            :	this(dataModel,
				 AbstractADODataModel.DEFAULT_PREFERENCE_TABLE,
				 AbstractADODataModel.DEFAULT_USER_ID_COLUMN,
				 AbstractADODataModel.DEFAULT_ITEM_ID_COLUMN,
				 AbstractADODataModel.DEFAULT_PREFERENCE_COLUMN,
				 DEFAULT_DIFF_TABLE,
				 DEFAULT_ITEM_A_COLUMN,
				 DEFAULT_ITEM_B_COLUMN,
				 DEFAULT_COUNT_COLUMN,
				 DEFAULT_AVERAGE_DIFF_COLUMN,
				 DEFAULT_MIN_DIFF_COUNT)
        {
		}

		public OleDBDiffStorage(OleDBDataModel dataModel,
									String preferenceTable,
									String userIDColumn,
									String itemIDColumn,
									String preferenceColumn,
									String diffsTable,
									String itemIDAColumn,
									String itemIDBColumn,
									String countColumn,
									String avgColumn,
									int minDiffCount) 
			:base(dataModel,
				  // getDiffSQL
				  "SELECT " + countColumn + ", " + avgColumn + " FROM " + diffsTable +
				  " WHERE " + itemIDAColumn + "=? AND "+ itemIDBColumn + "=? UNION " +
				  "SELECT " + countColumn + ", " + avgColumn + " FROM " + diffsTable +
				  " WHERE " + itemIDAColumn + "=? AND "+ itemIDBColumn + "=?",
				  // getDiffsSQL
				  "SELECT " + countColumn + ", " + avgColumn + ", " + itemIDAColumn + " FROM " + diffsTable + ", " +
				  preferenceTable + " WHERE " + itemIDBColumn + "=? AND " + itemIDAColumn + " = " + itemIDColumn +
				  " AND " + userIDColumn + "=? ORDER BY " + itemIDAColumn,
				  // getAverageItemPrefSQL
				  "SELECT COUNT(1), AVG(" + preferenceColumn + ") FROM " + preferenceTable +
				  " WHERE " + itemIDColumn + "=?",
				  // updateDiffSQLs
				  new String[] {
					  "UPDATE " + diffsTable + " SET " + avgColumn + " = " + avgColumn + " - (? / " + countColumn +
					  ") WHERE " + itemIDAColumn + "=?",
					  "UPDATE " + diffsTable + " SET " + avgColumn + " = " + avgColumn + " + (? / " + countColumn +
					  ") WHERE "+ itemIDBColumn + "=?"
				  },
				  // removeDiffSQL
				  new String[] {
					  "UPDATE " + diffsTable + " SET " + countColumn + " = " + countColumn + "-1, " +
					  avgColumn + " = " + avgColumn + " * ((" + countColumn + " + 1) / CAST(" + countColumn +
					  " AS DECIMAL)) + ? / CAST(" + countColumn + " AS DECIMAL) WHERE " + itemIDAColumn + "=?",
					  "UPDATE " + diffsTable + " SET " + countColumn + " = " + countColumn + "-1, " +
					  avgColumn + " = " + avgColumn + " * ((" + countColumn + " + 1) / CAST(" + countColumn +
					  " AS DECIMAL)) - ? / CAST(" + countColumn + " AS DECIMAL) WHERE "+ itemIDBColumn + "=?"
				  },
				  // getRecommendableItemsSQL
				  "SELECT id FROM " +
				  "(SELECT " + itemIDAColumn + " AS id FROM " + diffsTable + ", " + preferenceTable +
				  " WHERE "+ itemIDBColumn + " = item_id AND " + userIDColumn + "=? UNION DISTINCT" +
				  " SELECT "+ itemIDBColumn + " AS id FROM " + diffsTable + ", " + preferenceTable +
				  " WHERE " + itemIDAColumn + " = item_id AND " + userIDColumn +
				  "=?) possible_item_ids WHERE id NOT IN (SELECT " + itemIDColumn + " FROM " + preferenceTable +
				  " WHERE " + userIDColumn + "=?)",
				  // deleteDiffsSQL
				  "DELETE FROM " + diffsTable,
				  // createDiffsSQL
				  "INSERT INTO " + diffsTable + " (" + itemIDAColumn + ", "+ itemIDBColumn + ", " + avgColumn +
				  ", " + countColumn + ") SELECT prefsA." + itemIDColumn + ", prefsB." + itemIDColumn + ',' +
				  " AVG(prefsB." + preferenceColumn + " - prefsA." + preferenceColumn + ")," +
				  " COUNT(1) AS count FROM " + preferenceTable + " prefsA, " + preferenceTable + " prefsB WHERE prefsA." +
				  userIDColumn + " = prefsB." + userIDColumn + " AND prefsA." + itemIDColumn + " < prefsB." +
				  itemIDColumn + ' ' + " GROUP BY prefsA." + itemIDColumn +
				  ", prefsB." + itemIDColumn + " HAVING count >=?",
				  // diffsExistSQL
				  "SELECT COUNT(1) FROM " + diffsTable,
				  minDiffCount)

        {
        }

        #region Parameter Overrides

        #endregion
        /*
		public static void main(String... args)
        {
			Logger.GetLogger("com.planetj.taste").setLevel(Level.FINE);
			MysqlDataSource dataSource = new MysqlDataSource();
			dataSource.setUser("mysql");
			dataSource.setDatabaseName("test");
			dataSource.setServerName("localhost");
			DataSource pooledDataSource = new ConnectionPoolDataSource(dataSource);
			MySQLADODataModel Model = new MySQLADODataModel(pooledDataSource);
			MySQLADODiffStorage diffStorage = new MySQLADODiffStorage(Model);
			Recommender slopeOne = new SlopeOneRecommender(Model, true, false, diffStorage);
			long start = System.currentTimeMillis();
			System.out.println(slopeOne.recommend(args[0], 20));
			long end = System.currentTimeMillis();
			System.out.println(end - start);
		}
		 */
	}
}