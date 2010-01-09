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

namespace com.planetj.taste.impl.model.file;
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using com.planetj.taste.common;
	using com.planetj.taste.impl.model;
	using com.planetj.taste.impl.common;
	using com.planetj.taste.model;

	/**
	 * <p>A {@link DataModel} backed by a comma-delimited file. This class assumes that each line of the
	 * file contains a user ID, followed by item ID, followed by preferences value, separated by commas.
	 * The preference value is assumed to be parseable as a <code>double</code>. The user and item IDs
	 * are ready literally as Strings and treated as such in the API. Note that this means that whitespace
	 * matters in the data file; they will be treated as part of the ID values.</p>
	 *
	 * <p>This class is not intended for use with very large amounts of data (over, say, a million rows). For
	 * that, {@link com.planetj.taste.impl.model.jdbc.MySQLJDBCDataModel} and a database are more appropriate.
	 * The file will be periodically reloaded if a change is detected.</p>
	 *
	 * @author Sean Owen
	 */
	public class FileDataModel : DataModel {

		private static Logger log = Logger.getLogger(FileDataModel.class.getName());

		private static Timer timer = new Timer(true);
		private static long RELOAD_CHECK_INTERVAL_MS = 60L * 1000L;

		private File dataFile;
		private long lastModified;
		private boolean loaded;
		private DataModel delegate;
		private ReentrantLock refreshLock;
		private ReentrantLock reloadLock;

		/**
		 * @param dataFile file containing preferences data
		 * @throws FileNotFoundException if dataFile does not exist
		 */
		public FileDataModel(File dataFile)
		{
			if (dataFile == null) 
			{
				throw new IllegalArgumentException("dataFile is null");
			}
			if (!dataFile.exists() || dataFile.isDirectory()) {
				throw new FileNotFoundException(dataFile.toString());
			}

			log.info("Creating FileDataModel for file " + dataFile);

			this.dataFile = dataFile;
			this.lastModified = dataFile.lastModified();
			this.refreshLock = new ReentrantLock();
			this.reloadLock = new ReentrantLock();

			// Schedule next refresh
			timer.schedule(new RefreshTimerTask(), RELOAD_CHECK_INTERVAL_MS, RELOAD_CHECK_INTERVAL_MS);
		}

		protected void Reload()
		{
			try 
			{
				reloadLock.lock();
				Dictionary<String, List<Preference>> data = new Dictionary<String, List<Preference>>(1003);

				processFile(data);

				List<User> users = new ArrayList<User>(data.size());
				foreach (KeyValuePair<String, List<Preference>> entries in data) 
				{
					users.Add(buildUser(entries.Key, entries.Value));
				}

				delegate = new GenericDataModel(users);
				loaded = true;

			} finally {
				reloadLock.unlock();
			}
		}

		private void ProcessFile(Dictionary<String, List<Preference>> data) 
		{
			log.info("Reading file info...");
			BufferedReader reader = null;
			try {
				reader = new BufferedReader(new FileReader(dataFile));
				boolean notDone = true;
				while (notDone) 
				{
					String line = reader.readLine();
					if (line != null && line.length() > 0) {
						if (log.isLoggable(Level.FINE)) {
							log.fine("Read line: " + line);
						}
						processLine(line, data);
					} else {
						notDone = false;
					}
				}
			} finally {
				IOUtils.quietClose(reader);
			}
		}

		private void processLine(String line, Dictionary<String, List<Preference>> data) 
		{
			assert reloadLock.isHeldByCurrentThread();
			int commaOne = line.indexOf((int) ',');
			int commaTwo = line.indexOf((int) ',', commaOne + 1);
			if (commaOne < 0 || commaTwo < 0) {
				throw new IllegalArgumentException("Bad line: " + line);
			}
			String userID = line.substring(0, commaOne);
			String itemID = line.substring(commaOne + 1, commaTwo);
			double preferenceValue = Double.valueOf(line.substring(commaTwo + 1));
			List<Preference> prefs = data.get(userID);
			if (prefs == null) {
				prefs = new ArrayList<Preference>();
				data.put(userID, prefs);
			}
			Item item = buildItem(itemID);
			if (log.isLoggable(Level.FINE)) {
				log.fine("Read item " + item + " for user ID " + userID);
			}
			prefs.add(buildPreference(null, item, preferenceValue));
		}

		private void checkLoaded() {
			if (!loaded) {
				try {
					reload();
				} catch (IOException ioe) {
					throw new TasteException(ioe);
				}
			}
		}

		/**
		 * {@inheritDoc}
		 */
		public Iterable<User> getUsers() 
		{
			checkLoaded();
			return delegate.getUsers();
		}

		/**
		 * {@inheritDoc}
		 *
		 * @throws NoSuchElementException if there is no such user
		 */
		public User getUser(Object id) 
		{
			checkLoaded();
			return delegate.getUser(id);
		}

		/**
		 * {@inheritDoc}
		 */
		@NotNull
		public Iterable<? extends Item> getItems() {
			checkLoaded();
			return delegate.getItems();
		}

		/**
		 * {@inheritDoc}
		 */
		@NotNull
		public Item getItem(Object id) {
			checkLoaded();
			return delegate.getItem(id);
		}

		/**
		 * {@inheritDoc}
		 */
		@NotNull
		public Iterable<? extends Preference> getPreferencesForItem(Object itemID) {
			checkLoaded();
			return delegate.getPreferencesForItem(itemID);
		}

		/**
		 * {@inheritDoc}
		 */
		public int getNumItems() {
			checkLoaded();
			return delegate.getNumItems();
		}

		/**
		 * {@inheritDoc}
		 */
		public int getNumUsers() {
			checkLoaded();
			return delegate.getNumUsers();
		}

		/**
		 * @throws UnsupportedOperationException
		 */
		public void setPreference(Object userID, Object itemID, double value) {
			throw new UnsupportedOperationException();
		}

		/**
		 * @throws UnsupportedOperationException
		 */
		public void removePreference(Object userID, Object itemID) {
			throw new UnsupportedOperationException();
		}

		/**
		 * {@inheritDoc}
		 */
		public void refresh() {
			if (refreshLock.isLocked()) {
				return;
			}
			try {
				refreshLock.lock();
				try {
					reload();
				} catch (IOException ioe) {
					log.log(Level.WARNING, "Unexpected exception while refreshing", ioe);			
				}
			} finally {
				refreshLock.unlock();
			}

		}

		/**
		 * Subclasses may override to return a different {@link User} implementation.
		 *
		 * @param id user ID
		 * @param prefs user preferences
		 * @return {@link GenericUser} by default
		 */
		@NotNull
		protected User buildUser(String id, List<Preference> prefs) {
			return new GenericUser<String>(id, prefs);
		}

		/**
		 * Subclasses may override to return a different {@link Item} implementation.
		 *
		 * @param id item ID
		 * @return {@link GenericItem} by default
		 */
		@NotNull
		protected Item buildItem(String id) {
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
		@NotNull	
		protected Preference buildPreference(User user, Item item, double value) 
		{
			return new GenericPreference(user, item, value);
		}

		public override String ToString() 
		{
			return "FileDataModel[dataFile:" + dataFile + ']';
		}

		private class RefreshTimerTask : TimerTask 
		{
			public override void Run() 
			{
				if (loaded) {
					long newModified = dataFile.lastModified();
					if (newModified > lastModified) 
					{
						log.fine("File has changed; reloading...");
						lastModified = newModified;
						try 
						{
							Reload();
						} 
						catch (IOException ioe) 
						{
							log.log(Level.WARNING, "Error while reloading file", ioe);
						}
					}
				}
			}
		}

	}

}	