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
	using System.Collections.Generic;
	using System.Data;
    using Taste.Model;
	using Taste.Common;
    using log4net;


	/**
	 * <p>An abstract superclass for ADO-related {@link DataModel} implementations, providing most of the Common
	 * functionality that any such implementation would need.</p>
	 *
	 * <p>Performance will be a concern with any ADO-based {@link DataModel}. There are going to be lots of
	 * simultaneous reads and some writes to one table. Make sure the table is set up optimally -- for example,
	 * you'll want to establish indexes.</p>
	 *
	 * <p>You'll also want to use connection pooling of some kind. </p>
	 *
	 * <p>Also note: this default implementation assumes that the user and item ID keys are {@link String}s, for
	 * maximum flexibility. You can override this behavior by subclassing an implementation and overriding
	 * {@link #buildItem(String)} and {@link #buildUser(String, List)}. If you don't, just make sure you use
	 * {@link String}s as IDs throughout your code. If your IDs are really numeric, and you use, say, {@link Long}
	 * for IDs in the rest of your code, you will run into subtle problems because the {@link Long} values won't
	 * be equal to or compare correctly to the underlying {@link String} key values.</p>
	 *
	 * @author Sean Owen
	 */
	public abstract class AbstractADODataModel : ADODataModel 
	{

		private static readonly ILog log = LogManager.GetLogger(typeof(AbstractADODataModel));

		public const String DEFAULT_PREFERENCE_TABLE = "taste_preferences";
		public const String DEFAULT_USER_ID_COLUMN = "user_id";
		public const String DEFAULT_ITEM_ID_COLUMN = "item_id";
		public const String DEFAULT_PREFERENCE_COLUMN = "preference";
		
		protected String getUserSQL;
        protected String getNumItemsSQL;
        protected String getNumUsersSQL;
        protected String setPreferenceSQL;
        protected String removePreferenceSQL;
        protected String getUsersSQL;
        protected String getItemsSQL;		
		protected String getItemSQL;		
		protected String getPrefsForItemSQL;		
		protected String getUsersPreferringItemSQL;
        protected string connectionString;

        private readonly bool _userStoreProcs = false;

        public AbstractADODataModel()
        {
        }


		protected AbstractADODataModel(String getUserSQL,
		                                String getNumItemsSQL,
		                                String getNumUsersSQL,
		                                String setPreferenceSQL,
		                                String removePreferenceSQL,
		                                String getUsersSQL,
		                                String getItemsSQL,
		                                String getItemSQL,
		                                String getPrefsForItemSQL,
		                                String getUsersPreferringItemSQL) {

			log.Debug("Creating AbstractADOModel...");
			checkNotNullAndLog("getUserSQL", getUserSQL);
			checkNotNullAndLog("getNumItemsSQL", getNumItemsSQL);
			checkNotNullAndLog("getNumUsersSQL", getNumUsersSQL);
			checkNotNullAndLog("setPreferenceSQL", setPreferenceSQL);
			checkNotNullAndLog("removePreferenceSQL", removePreferenceSQL);
			checkNotNullAndLog("getUsersSQL", getUsersSQL);
			checkNotNullAndLog("getItemsSQL", getItemsSQL);
			checkNotNullAndLog("getItemSQL", getItemSQL);
			checkNotNullAndLog("getPrefsForItemSQL", getPrefsForItemSQL);
			checkNotNullAndLog("getUsersPreferringItemSQL", getUsersPreferringItemSQL);

			this.getUserSQL = getUserSQL;
			this.getNumItemsSQL = getNumItemsSQL;
			this.getNumUsersSQL = getNumUsersSQL;
			this.setPreferenceSQL = setPreferenceSQL;
			this.removePreferenceSQL = removePreferenceSQL;
			this.getUsersSQL = getUsersSQL;
			this.getItemsSQL = getItemsSQL;
			this.getItemSQL = getItemSQL;
			this.getPrefsForItemSQL = getPrefsForItemSQL;
			this.getUsersPreferringItemSQL = getUsersPreferringItemSQL;
		}

		private static void checkNotNullAndLog(String argName, Object value) 
		{
			if (value == null || String.IsNullOrEmpty(value.ToString())) 
            {
				throw new ArgumentException(argName + " is null or empty");
			}
			if (log.IsDebugEnabled) 
			{
				log.DebugFormat("{0} : {1}", argName, value);
			}
        }


        #region Misc Helpers

        static public IDataParameter AddParameter(IDbCommand Cmd, string ParameterName, DbType DbType,
            ParameterDirection Direction)
        {
            if (Cmd == null)
                throw new System.ArgumentNullException("Cmd");
            if (ParameterName == null)
                throw new System.ArgumentNullException("ParameterName");

            IDataParameter param = Cmd.CreateParameter();
            Cmd.Parameters.Add(param);
            param.ParameterName = ParameterName;
            param.Direction = Direction;
            param.DbType = DbType;
            return param;
        }

        static public IDataParameter AddParameter(IDbCommand Cmd, string ParameterName, DbType DbType)
        {
            if (Cmd == null)
                throw new System.ArgumentNullException("Cmd");
            if (ParameterName == null)
                throw new System.ArgumentNullException("ParameterName");

            IDataParameter param = Cmd.CreateParameter();
            Cmd.Parameters.Add(param);
            param.ParameterName = ParameterName;
            param.DbType = DbType;
            return param;
        }

        static public IDataParameter AddParameter(IDbCommand Cmd, string ParameterName, DbType DbType, object value)
        {
            IDataParameter param = AddParameter(Cmd, ParameterName, DbType);
            param.Value = value;
            return param;
        }

        public virtual IDbCommand CreateCommand(IDbConnection conn, string sql, bool isStoredProc)
        {
            IDbCommand cmd = conn.CreateCommand();
            cmd.CommandType = (isStoredProc) ? CommandType.StoredProcedure : CommandType.Text;
            cmd.CommandText = sql;
            cmd.Connection = conn;
            return cmd;
        }

        public IDbCommand CreateCommand(IDbConnection conn, string sql)
        {
            // todo: regex. if it starts with insert, select or delete, then text else storedproc
            return CreateCommand(conn, sql, _userStoreProcs);
        }

        public IDbCommand CreateCommand(string sql)
        {
            return CreateCommand(GetConnection(), sql);
        }

        #endregion
        
        public virtual IDbConnection GetConnection()
        {
            throw new NotImplementedException("GetConnection()");
        }


		
		public IEnumerable<User> GetUsers() 
		{
			log.Debug("Retrieving all users...");
            IDbCommand cmd = CreateCommand(this.getUsersSQL);
            ConfigureGetUsersCommand(cmd);

            using (ResultSetUserEnumerator it1 = new ResultSetUserEnumerator(this, cmd))
            {
                IEnumerator<User> userIter = it1.GetEnumerator();
                userIter.Reset();
                while (userIter.MoveNext())
                {
                    yield return userIter.Current;
                }
            }
        }


        #region Properties

        public String GetUserSQL
        {
            get { return getUserSQL; }
            set { getUserSQL = value; }
        }

        public String GetNumItemsSQL
        {
            get { return getNumItemsSQL; }
            set { getNumItemsSQL = value; }
        }

        public String GetNumUsersSQL
        {
            get { return getNumUsersSQL; }
            set { getNumUsersSQL = value; }
        }

        public String SetPreferenceSQL
        {
            get { return setPreferenceSQL; }
            set { setPreferenceSQL = value; }
        }

        public String RemovePreferenceSQL
        {
            get { return removePreferenceSQL; }
            set { removePreferenceSQL = value; }
        }

        public String GetUsersSQL
        {
            get { return getUsersSQL; }
            set { getUsersSQL = value; }
        }

        public String GetItemsSQL
        {
            get { return getItemsSQL; }
            set { getItemsSQL = value; }
        }

        public String GetItemSQL
        {
            get { return getItemsSQL; }
            set { getItemsSQL = value; }
        }

        public String GetPrefsForItemSQL
        {
            get { return getPrefsForItemSQL; }
            set { getPrefsForItemSQL = value; }
        }

        public String GetUsersPreferringItemSQL
        {
            get { return getUsersPreferringItemSQL; }
            set { getUsersPreferringItemSQL = value; }
        }

        #endregion

        #region Subclass overrides

        protected virtual void ConfigureGetUserCountCommand(IDbCommand command)
        {
        }

        protected virtual void ConfigureGetItemCountCommand(IDbCommand command)
        {
        }

        protected virtual void ConfigureGetUsersCommand(IDbCommand command)
        {
        }

        protected virtual void ConfigureGetUserCommand(IDbCommand command, object id)
        {
            throw new NotImplementedException();
        }

        protected virtual void ConfigureGetItemsCommand(IDbCommand command)
        {
        }

        protected virtual void ConfigureGetItemCommand(IDbCommand command, object itemId)
        {
            throw new NotImplementedException();
        }

        protected virtual void ConfigureItemPreferencesCommand(IDbCommand command, object itemId)
        {
            throw new NotImplementedException();
        }

        protected virtual void ConfigureSetPreferenceCommand(IDbCommand command, Object userID, Object itemID, double value)
        {
            throw new NotImplementedException();
        }

        protected virtual void ConfigureRemovePreferenceCommand(IDbCommand command, Object userID, Object itemID)
        {
            throw new NotImplementedException();
        }

        #endregion

		
		public User GetUser(Object id) 
		{
			if (log.IsDebugEnabled) 
            {
				log.Debug("Retrieving user ID '" + id + "'...");
			}

			String idString = id.ToString();
            try
            {
                using (IDbCommand cmd = CreateCommand(this.getUserSQL))
                {
                    ConfigureGetUserCommand(cmd, id);
                    if (log.IsDebugEnabled)
                    {
                        log.Debug("Executing SQL query: " + cmd.CommandText);
                    }
                    using (IDataReader rs = cmd.ExecuteReader())
                    {
                        List<Preference> prefs = new List<Preference>();

                        while (rs.Read())
                        {
                            AddPreference(rs, prefs);
                        }
                        if (prefs.Count == 0)
                        {
                            throw new NoSuchElementException();
                        }
                        return BuildUser(idString, prefs);
                    }
                }
			} 
            catch (Exception sqle) 
            {
				log.Warn( "Exception while retrieving user", sqle);
				throw new TasteException(sqle);
			} 
		}


		/**
		 * {@inheritDoc}
		 */
		
		public IEnumerable<Item> GetItems() 
		{
			log.Debug("Retrieving all items...");      
            IDbCommand cmd = CreateCommand(this.getItemsSQL);
            ConfigureGetItemsCommand(cmd);
            using (ResultSetItemEnumerator enumerator = new ResultSetItemEnumerator(this, cmd))
            {
                IEnumerator<Item> items = enumerator.GetEnumerator();
                items.Reset();
                while (items.MoveNext())
                {
                    yield return items.Current;
                }
            }
		}

		/**
		 * {@inheritDoc}
		 */
		
		public Item GetItem(Object id) 
		{
			return GetItem(id, false);
		}

		/**
		 * {@inheritDoc}
		 */
		
		public Item GetItem(Object id, bool assumeExists)
		{
			if (assumeExists) 
			{
				return BuildItem((String) id);
			}

			if (log.IsDebugEnabled) 
			{
				log.Debug("Retrieving item ID '" + id + "'...");
			}

			using (IDbCommand cmd = CreateCommand(this.getItemSQL))
            {
    			try 	    		
                {
                    ConfigureGetItemCommand(cmd, id);

    				if (log.IsDebugEnabled) 
                    {
		    			log.Debug("Executing SQL query: " + getItemSQL);
				    }
                    using (IDataReader rs = cmd.ExecuteReader())
                    {
                        if (rs.Read())
                        {
                            return BuildItem((String)id);
                        }
                        else
                        {
                            throw new NoSuchElementException();
                        }
                    }
			    } 
                catch (Exception sqle) 
                {
    				log.Warn( "Exception while retrieving item", sqle);
	    			throw new TasteException(sqle);
		    	}
            }
		}

		/**
		 * {@inheritDoc}
		 */

        public IEnumerable<Preference> GetPreferencesForItem(Object itemID)
        {
            return DoGetPreferencesForItem(itemID);
        }


        public Preference[] GetPreferencesForItemAsArray(Object itemID)
        {
            List<Preference> preferences = DoGetPreferencesForItem(itemID);
            return (preferences == null) ? null : preferences.ToArray();
        }
		

		protected List<Preference> DoGetPreferencesForItem(Object itemID)
		{
			if (log.IsDebugEnabled) 
			{
				log.Debug("Retrieving preferences for item ID '" + itemID + "'...");
			}
			Item item = GetItem(itemID);

			try 
            {
                using (IDbCommand cmd = CreateCommand(this.getPrefsForItemSQL))
                {

                    ConfigureItemPreferencesCommand(cmd, itemID);

                    if (log.IsDebugEnabled)
                    {
                        log.Debug("Executing SQL query: " + getPrefsForItemSQL);
                    }

                    using (IDataReader rs = cmd.ExecuteReader())
                    {
                        List<Preference> prefs = new List<Preference>();
                        while (rs.Read())
                        {
                            double preference = rs.GetDouble(0);
                            String userID = rs.GetString(1);
                            Preference pref = BuildPreference(BuildUser(userID, null), item, preference);
                            prefs.Add(pref);
                        }
                        return prefs;
                    }
                }
			} 
            catch (Exception sqle) 
            {
				log.Warn( "Exception while retrieving prefs for item", sqle);
				throw new TasteException(sqle);
			} 
		}


		/**
		 * {@inheritDoc}
		 */
		public int GetNumItems()
		{
			return GetNumThings("items", getNumItemsSQL);
		}

		/**
		 * {@inheritDoc}
		 */
		public int GetNumUsers()
		{
			return GetNumThings("users", getNumUsersSQL);
		}

		private int GetNumThings(String name, String sql)
		{
			log.Debug("Retrieving number of " + name + " in Model...");

			try 
            {
                using (IDbCommand cmd = CreateCommand(sql))
                {
                    if (name == "items")
                        ConfigureGetItemsCommand(cmd);
                    else if (name == "users")
                        ConfigureGetUserCountCommand(cmd);

                    object data = cmd.ExecuteScalar();
                    if (log.IsDebugEnabled)
                    {
                        log.Debug("Executing SQL query: " + sql);
                    }
                    return (int)data;
                }
		    } 
            catch (Exception sqle) 
            {
			    log.Warn( "Exception while retrieving number of " + name, sqle);
			    throw new TasteException(sqle);
		    }
		}


        public string ConnectionString
        {
            get { return connectionString; }
            set { connectionString = value; }
        }

		/**
		 * {@inheritDoc}
		 */
		public void SetPreference(Object userID, Object itemID, double value)
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
				log.Debug("Setting preference for user '" + userID.ToString() + "', item '" + itemID.ToString() + "', value " + value);
			}

            try
            {
                using (IDbCommand cmd = CreateCommand(this.setPreferenceSQL))
                {
                    ConfigureSetPreferenceCommand(cmd, userID, itemID, value);

                    if (log.IsDebugEnabled)
                    {
                        log.Debug("Executing SQL update: " + setPreferenceSQL);
                    }
                    cmd.ExecuteNonQuery();
                }

            }
            catch (Exception sqle)
            {
                log.Warn( "Exception while setting preference", sqle);
                throw new TasteException(sqle);
            }
		}


		/**
		 * {@inheritDoc}
		 */
		public void RemovePreference(Object userID, Object itemID)
		{
			if (userID == null || itemID == null) 			
			{
				throw new ArgumentNullException("userID or itemID is null");
			}

			if (log.IsDebugEnabled) 
            {
				log.Debug("Removing preference for user '" + userID + "', item '" + itemID + '\'');
			}

            using (IDbCommand cmd = CreateCommand(this.removePreferenceSQL))
            {
                try
                {
                    ConfigureRemovePreferenceCommand(cmd, userID, itemID);
                    cmd.ExecuteNonQuery();
                }
                catch (Exception sqle)
                {
                    log.Warn( "Exception while removing preference", sqle);
                    throw new TasteException(sqle);
                }
            }
		}

		/**
		 * {@inheritDoc}
		 */
		public void Refresh() 
		{
			// do nothing
		}


		private void AddPreference(IDataReader rs, ICollection<Preference> prefs)
		{
			Item item = BuildItem(rs.GetString(0));
			double preferenceValue = rs.GetDouble(1);
			prefs.Add(BuildPreference(null, item, preferenceValue));
		}

		/**
		 * <p>Default implementation which returns a new {@link GenericUser} with {@link String} IDs.
		 * Subclasses may override to return a different {@link User} implementation.</p>
		 *
		 * @param id user ID
		 * @param prefs user preferences
		 * @return {@link GenericUser} by default
		 */
		
		protected virtual User BuildUser(String id, List<Preference> prefs) 
		{
			return new GenericUser<String>(id, prefs);
		}

		/**
		 * <p>Default implementation which returns a new {@link GenericItem} with {@link String} IDs.
		 * Subclasses may override to return a different {@link Item} implementation.</p>
		 *
		 * @param id item ID
		 * @return {@link GenericItem} by default
		 */
		
		protected virtual Item BuildItem(String id) 
		{
			return new GenericItem<String>(id);
		}

		/**
		 * Subclasses may override to return a different {@link Preference} implementation.
		 *
		 * @param user {@link User}
		 * @param item {@link Item}
		 * @return {@link GenericPreference} by default
		 */
		protected virtual Preference BuildPreference(User user, Item item, double value) 
        {
			return new GenericPreference(user, item, value);
		}

		/**
		 * <p>An {@link java.util.IEnumerator} which returns {@link taste.Model.User}s from a
		 * {@link System.IDataReader}. This is a useful
		 * way to iterate over all user data since it does not require all data to be read into memory
		 * at once. It does however require that the DB connection be held open. Note that this class will
		 * only release database resources after {@link #hasNext()} has been called and has returned false;
		 * callers should make sure to "drain" the entire set of data to avoid tying up database resources.</p>
		 *
		 * @author Sean Owen
		 */
        public class ResultSetUserEnumerator : IEnumerator<User>
        {
			private IDataReader reader;
            private IDbCommand cmd = null;
            User current = null;
            String currentUserID = null;
            private readonly AbstractADODataModel host;

            public ResultSetUserEnumerator(AbstractADODataModel host, IDbCommand cmd) 
            {
                if (cmd == null)
                    throw new ArgumentNullException("cmd");

                this.cmd = cmd;
                this.host = host;
				if (log.IsDebugEnabled) 
                {
					log.DebugFormat("Executing SQL query: {0}", cmd.CommandText);
				}
			}

            private User ReadUser(IDataReader rdr, ref string currentUserID)
            {
                try
                {
                    String userID = rdr.GetString(2);
                    List<Preference> prefs = new List<Preference>();

                    if (String.IsNullOrEmpty(currentUserID))
                    {
                        currentUserID = userID;
                    }

                    // itemId,  preferenceValue,  userId

                    while (userID == currentUserID)
                    {
                        // else add a new preference for the current user
                        host.AddPreference(rdr, prefs);
                        if (rdr.Read())
                        {
                            currentUserID = rdr.GetString(2);
                        }
                        else
                        {
                            break;
                        }
                    }

                   return host.BuildUser(userID, prefs);
                }
                catch (Exception sqle)
                {
                    // No good way to handle this since we can't throw an exception
                    log.Warn("Exception while iterating over users", sqle);
                    throw new ArgumentException("Can't retrieve more due to exception: " + sqle);
                }
            }
			
            public IEnumerator<User> GetEnumerator()
            {
                using (IDataReader rdr = cmd.ExecuteReader())
                {
                    String lastUser = null;

                    while (rdr.Read())
                    {
                        User user = ReadUser(rdr, ref lastUser);
                        yield return user;
                    }
                }                
			}

            #region IEnumerator<User> Members

            User IEnumerator<User>.Current
            {
                get 
                {
                    if (current == null)
                    {
                        current = ReadUser(reader, ref currentUserID);
                    }
                    return current;
                }
            }

            #endregion

            #region IDisposable Members

            void IDisposable.Dispose()
            {
                if (reader != null)
                    reader.Dispose();
                if (cmd != null)
                    cmd.Dispose();
            }

            #endregion

            #region IEnumerator Members

            object System.Collections.IEnumerator.Current
            {
                get
                {
                    if (current == null)
                    {
                        current = ReadUser(reader, ref currentUserID);
                    }
                    return current;
                }
            }

            bool System.Collections.IEnumerator.MoveNext()
            {
                return (reader != null && reader.Read()) ;
            }

            void System.Collections.IEnumerator.Reset()
            {
                if (reader != null)
                    reader.Dispose();
                currentUserID = null;
                try
                {
                    reader = cmd.ExecuteReader();
                }
                catch (Exception e)
                {
                    throw new TasteException(e);
                }
            }

            #endregion
        }

        /// <summary>
        /// <p>An IEnumerator which returns {@link taste.Model.Item}s from a
        /// {@link System.Data.IDataReader}. This is a useful way to iterate over all user data since it does not require
        /// all data to be read into memory at once. It does however require that the DB connection be held open. Note
        /// that this class will only release database resources after {@link #hasNext()} has been called and has returned
        /// <code>false</code>; callers should make sure to "drain" the entire set of data to avoid tying up database
        /// resources.</p>
        /// </summary>
        internal class ResultSetItemEnumerator : IEnumerator<Item> 
		{
			private IDataReader reader;
            private IDbCommand _cmd;
            private Item current = null;
            private readonly AbstractADODataModel _host;

            internal ResultSetItemEnumerator(AbstractADODataModel host, IDbCommand cmd) 
			{
                this._cmd = cmd;
                this._host = host;
				if (log.IsDebugEnabled) 
				{
					log.Debug("Executing SQL query: " + cmd.CommandText);
				}
			}

			/**
			 * {@inheritDoc}
			 */

            public IEnumerator<Item> GetEnumerator()
            {
                IDataReader itemReader;
				try 
                {
                    itemReader = _cmd.ExecuteReader();
				} 
                catch (Exception sqle) 
                {
					// No good way to handle this since we can't throw an exception
					log.Warn( "Exception while iterating over items", sqle);
					throw new ArgumentException("Can't retrieve more due to exception: " + sqle);
				}
                while (itemReader.Read())
                {
                    yield return _host.BuildItem(itemReader.GetString(1));
                }

			}

            #region IEnumerator<Item> Members

            Item IEnumerator<Item>.Current
            {
                get 
                {
                    if (current == null)
                    {
                        current = _host.BuildItem(reader.GetString(1));
                    }
                    return current;
                }
            }

            #endregion

            #region IDisposable Members

            void IDisposable.Dispose()
            {
                if (reader != null)
                    reader.Dispose();
                if (_cmd != null)
                    _cmd.Dispose();
            }

            #endregion

            #region IEnumerator Members

            object System.Collections.IEnumerator.Current
            {
                get 
                {
                    if (current == null)
                    {
                        current = _host.BuildItem(reader.GetString(1));
                    }
                    return current;
                }
            }

            bool System.Collections.IEnumerator.MoveNext()
            {
                if (reader != null && reader.Read())
                {
                    current = null;
                    return true;
                }
                return false;
            }

            void System.Collections.IEnumerator.Reset()
            {
                try
                {
                    if (reader != null) reader.Dispose();
                    reader = _cmd.ExecuteReader();
                }
                catch (Exception e)
                {
                    throw new TasteException(e);
                }
            }

            #endregion
        }

	}

}	