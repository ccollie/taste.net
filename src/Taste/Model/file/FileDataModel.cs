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

namespace Taste.Model.file
{
	using System;
    using System.Diagnostics;
	using System.Collections.Generic;
	using System.IO;
    using System.Threading;
	using Taste.Common;
	using Taste.Model;
    using log4net;

	/**
	 * <p>A {@link DataModel} backed by a comma-delimited file. This class assumes that each line of the
	 * file contains a user ID, followed by item ID, followed by preferences value, separated by commas.
	 * The preference value is assumed to be parseable as a <code>double</code>. The user and item IDs
	 * are ready literally as Strings and treated as such in the API. Note that this means that whitespace
	 * matters in the data file; they will be treated as part of the ID values.</p>
	 *
	 * <p>This class is not intended for use with very large amounts of data (over, say, a million rows). For
	 * that, {@link taste.Model.Ado.MySQLDataModel} and a database are more appropriate.
	 * The file will be periodically reloaded if a change is detected.</p>
	 *
	 * @author Sean Owen
	 */
	public class FileDataModel : DataModel, IDisposable
    {
		private static ILog log = LogManager.GetLogger(typeof(FileDataModel));

		private static Timer timer = null;
		private static long RELOAD_CHECK_INTERVAL_MS = 60L * 1000L;

		private string dataFile;
		private DateTime lastModified;
		private bool loaded;
		private DataModel delegateModel;
		private ReentrantLock refreshLock;
		private ReentrantLock reloadLock;
        private bool useReload = true;

		/**
		 * @param dataFile file containing preferences data
		 * @throws FileNotFoundException if dataFile does not exist
		 */
		public FileDataModel(String dataFile, bool autoReload)
		{
			if (String.IsNullOrEmpty(dataFile)) 
			{
				throw new ArgumentNullException("dataFile is null");
			}
            if (File.Exists(dataFile))
            {
				throw new FileNotFoundException(dataFile);
			}

            if (log.IsInfoEnabled)
            {
                log.Info("Creating FileDataModel for file " + dataFile);
            }

			this.dataFile = dataFile;
			this.lastModified = File.GetLastWriteTime(dataFile);
			this.refreshLock = new ReentrantLock();
			this.reloadLock = new ReentrantLock();

            this.useReload = autoReload;
            if (autoReload)
            {
                // Create the delegate that invokes methods for the timer.
                TimerCallback timerDelegate =
                    new TimerCallback(CheckStatus);

                // Schedule next refresh
                timer = new Timer(timerDelegate, this, RELOAD_CHECK_INTERVAL_MS, RELOAD_CHECK_INTERVAL_MS);
            }
		}

        public FileDataModel(String dataFile) : this(dataFile, true)
        {
        }

		protected void Reload()
		{
            reloadLock.Lock();
			try 
			{
				Dictionary<String, List<Preference>> data = new Dictionary<String, List<Preference>>(1003);

				ProcessFile(data);

				List<User> users = new List<User>(data.Count);
				foreach (KeyValuePair<String, List<Preference>> entries in data) 
				{
					users.Add(BuildUser(entries.Key, entries.Value));
				}

				delegateModel = new GenericDataModel(users);
				loaded = true;

			} 
            finally 
            {
				reloadLock.Unlock();
			}
		}

		protected virtual void ProcessFile(Dictionary<String, List<Preference>> data) 
		{
			log.Info("Reading file info...");
			using (BufferedStream bufStream = new BufferedStream(new FileStream(dataFile, FileMode.Open)))
            {
                using (StreamReader reader = new StreamReader(bufStream))
                {
                    bool notDone = true;
                    while (notDone)
                    {
                        String line = reader.ReadLine();
                        if (line != null && line.Length > 0)
                        {
                            if (log.IsDebugEnabled)
                            {
                                log.Debug("Read line: " + line);
                            }
                            ProcessLine(line, data);
                        }
                        else
                        {
                            notDone = false;
                        }
                    }
                }
            }
		}

		protected virtual void ProcessLine(String line, Dictionary<String, List<Preference>> data) 
		{
			//Debug.Assert(reloadLock.isHeldByCurrentThread());
			int commaOne = line.IndexOf(',');
			int commaTwo = line.IndexOf(',', commaOne + 1);
			if (commaOne < 0 || commaTwo < 0) 
            {
				throw new ArgumentException("Bad line: " + line);
			}
			String userID = line.Substring(0, commaOne);
			String itemID = line.Substring(commaOne + 1, commaTwo);
			double preferenceValue = Convert.ToDouble(line.Substring(commaTwo + 1));

			List<Preference> prefs;
            if (!data.TryGetValue(userID, out prefs))
            {
				prefs = new List<Preference>();
				data.Add(userID, prefs);
			}
			Item item = BuildItem(itemID);
			if (log.IsDebugEnabled) 
            {
				log.Debug("Read item " + item + " for user ID " + userID);
			}
			prefs.Add(BuildPreference(null, item, preferenceValue));
		}

		private void CheckLoaded() 
        {
			if (!loaded) 
            {
				try 
                {
					Reload();
				} 
                catch (IOException ioe) 
                {
					throw new TasteException(ioe);
				}
			}
		}

		public IEnumerable<User> GetUsers() 
		{
			CheckLoaded();
			return delegateModel.GetUsers();
		}

		/**
		 * {@inheritDoc}
		 *
		 * @throws NoSuchElementException if there is no such user
		 */
		public User GetUser(Object id) 
		{
			CheckLoaded();
			return delegateModel.GetUser(id);
		}

		/**
		 * {@inheritDoc}
		 */
		public IEnumerable<Item> GetItems() 
        {
			CheckLoaded();
			return delegateModel.GetItems();
		}

		/**
		 * {@inheritDoc}
		 */
		public Item GetItem(Object id) 
        {
			CheckLoaded();
			return delegateModel.GetItem(id);
		}

		/**
		 * {@inheritDoc}
		 */
		public IEnumerable<Preference> GetPreferencesForItem(Object itemID) 
        {
			CheckLoaded();
			return delegateModel.GetPreferencesForItem(itemID);
		}


        public Preference[] GetPreferencesForItemAsArray(Object itemID)
        {
            CheckLoaded();
            return delegateModel.GetPreferencesForItemAsArray(itemID);
        }

		/**
		 * {@inheritDoc}
		 */
		public int GetNumItems() 
        {
			CheckLoaded();
			return delegateModel.GetNumItems();
		}

		/**
		 * {@inheritDoc}
		 */
		public int GetNumUsers() 
        {
			CheckLoaded();
			return delegateModel.GetNumUsers();
		}

		public void SetPreference(Object userID, Object itemID, double value) 
        {
			throw new NotSupportedException();
		}

		public void RemovePreference(Object userID, Object itemID) 
        {
			throw new NotSupportedException();
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
                    try
                    {
                        Reload();
                    }
                    catch (IOException ioe)
                    {
                        log.Warn("Unexpected exception while refreshing", ioe);
                    }
                }
                finally
                {
                    refreshLock.Unlock();
                }
            }

		}

		/**
		 * Subclasses may override to return a different {@link User} implementation.
		 *
		 * @param id user ID
		 * @param prefs user preferences
		 * @return {@link GenericUser} by default
		 */
		protected User BuildUser(String id, List<Preference> prefs) 
        {
			return new GenericUser<String>(id, prefs);
		}

		/**
		 * Subclasses may override to return a different {@link Item} implementation.
		 *
		 * @param id item ID
		 * @return {@link GenericItem} by default
		 */
		protected Item BuildItem(String id) 
        {
			return new GenericItem<String>(id);
		}

		/**
		 * Subclasses may override to return a different {@link Preference} implementation.
		 *
		 * @param user {@link User} who expresses the preference
		 * @param item preferred {@link Item}
		 * @param value preference value
		 * @return {@link GenericPreference} by default
		 */
		protected Preference BuildPreference(User user, Item item, double value) 
		{
			return new GenericPreference(user, item, value);
		}

		public override String ToString() 
		{
			return "FileDataModel[dataFile:" + dataFile + ']';
		}

         // This method is called by the timer delegate.
        private void CheckStatus(Object stateInfo)
        {
            CheckReload();
        }

        protected virtual void CheckReload() 
        {
			if (loaded) 
            {
				DateTime newModified = File.GetLastWriteTime(dataFile);
				if (newModified > lastModified) 
				{
					log.Debug("File has changed; reloading...");
					lastModified = newModified;
					try 
					{
						Reload();
					} 
					catch (IOException ioe) 
					{
						log.Warn("Error while reloading file", ioe);
					}
				}
			}
        }


        #region IDisposable Members

        void IDisposable.Dispose()
        {
            if (timer != null)
                timer.Dispose();            
        }

        #endregion
    }

}	