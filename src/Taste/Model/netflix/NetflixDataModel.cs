/*
 * Copyright 2006 and onwards Sean Owen
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

namespace taste.Model.Netflix
{
	using System;
	using System.IO;
	using System.Collections.Generic;
	using taste.Common;
	using taste.Model;


	/**
	 * @author Sean Owen
	 * @since 1.3.5
	 */
	public class NetflixDataModel : DataModel 
    {

		private static readonly Logger log = Logger.GetLogger(typeof(NetflixDataModel));

		private readonly DataModel _delegate;

		public NetflixDataModel(FileInfo dataDirectory)
		{
			if (dataDirectory == null) 
            {
				throw new ArgumentNullException("dataDirectory is null");
			}
			if (!dataDirectory.Exists || !dataDirectory.IsDirectory()) 
            {
				throw new FileNotFoundException(dataDirectory.toString());
			}

			log.Info("Creating NetflixDataModel for directory: " + dataDirectory);

			log.info("Reading movie data...");
			List<NetflixMovie> movies = ReadMovies(dataDirectory);

			log.info("Reading preference data...");
			List<User> users = ReadUsers(dataDirectory, movies);

			log.Info("Creating delegate DataModel...");
			_delegate = new GenericDataModel(users);
		}

		private static List<User> ReadUsers(FileInfo dataDirectory, List<NetflixMovie> movies)
		{
			Dictionary<int, List<Preference>> userIDPrefMap =
				new Dictionary<int, List<Preference>>(104395301, 1.0f);
				//new HashMap<Integer, List<Preference>>(15485867, 1.0f);

			int counter = 0;
			FilenameFilter filenameFilter = new FilenameFilter(
                    delegate(File dir, String filename) 
                    {
					    return filename.startsWith("mv_");
					    //return filename.startsWith("mv_000");
				    }
                );
			
			foreach (FileInfo movieFile in new File(dataDirectory, "training_set").ListFiles(filenameFilter)) 
            {
				BufferedReader reader = new BufferedReader(new InputStreamReader(new FileInputStream(movieFile)));
				String line = reader.readLine();
				if (line == null) {
					throw new IOException("Can't read first line of file " + movieFile);
				}
				int movieID = int.parse(line.substring(0, line.length() - 1));
				NetflixMovie movie = movies.get(movieID - 1);
				if (movie == null) 
                {
					throw new ArgumentException("No such movie: " + movieID);
				}
				while ((line = reader.readLine()) != null) 
                {
					counter++;
					if (counter % 100000 == 0) 
                    {
						log.Info("Processed " + counter + " prefs");
					}
					int firstComma = line.indexOf((int) ',');
					Int32 userID = Int32.Parse(line.Substring(0, firstComma));
					int secondComma = line.IndexOf((int) ',', firstComma + 1);
					double rating = Double.Parse(line.Substring(firstComma + 1, secondComma));
					List<Preference> userPrefs = userIDPrefMap.get(userID);
					if (userPrefs == null) 
                    {
						userPrefs = new List<Preference>();
						userIDPrefMap.Add(userID, userPrefs);
					}
					userPrefs.Add(new GenericPreference(null, movie, rating));
				}
				IOUtils.quietClose(reader);
			}

			List<User> users = new List<User>(userIDPrefMap.Count);
			foreach (KeyValuePair<int, List<Preference>> entry in userIDPrefMap) 
            {
				users.Add(new GenericUser<int>(entry.Key, entry.Value));
			}
			return users;
		}

		private static List<NetflixMovie> readMovies(File dataDirectory) 
		{
			List<NetflixMovie> movies = new List<NetflixMovie>(17770);
			BufferedReader reader =
				new BufferedReader(new InputStreamReader(new FileStream(new File(dataDirectory, "movie_titles.txt"))));
			String line;
			while ((line = reader.readLine()) != null) 
			{
				int firstComma = line.indexOf((int) ',');
				int id = Integer.valueOf(line.substring(0, firstComma));
				int secondComma = line.indexOf((int) ',', firstComma + 1);
				String title = line.substring(secondComma + 1);
				movies.Add(new NetflixMovie(id, title));
			}
			IOUtils.quietClose(reader);
			return movies;
		}


		/**
		 * {@inheritDoc}
		 */
		public IEnumerable<User> GetUsers() 
		{
			return _delegate.GetUsers();
		}

		/**
		 * {@inheritDoc}
		 *
		 * @throws NoSuchElementException if there is no such user
		 */
		public User GetUser(Object id) 
		{
			return _delegate.GetUser(id);
		}

		/**
		 * {@inheritDoc}
		 */
		public IEnumerable<Item> GetItems() 
		{
			return _delegate.getItems();
		}

		/**
		 * {@inheritDoc}
		 */
		public Item GetItem(Object id) 
		{
			return _delegate.GetItem(id);
		}

		/**
		 * {@inheritDoc}
		 */
		public IEnumerable<Preference> GetPreferencesForItem(Object itemID) 
		{
			return _delegate.GetPreferencesForItem(itemID);
		}

		/**
		 * {@inheritDoc}
		 */
		public int GetNumItems() 
		{
			return _delegate.GetNumItems();
		}

		/**
		 * {@inheritDoc}
		 */
		public int GetNumUsers() 
		{
			return _delegate.GetNumUsers();
		}

		/**
		 * @throws UnsupportedOperationException
		 */
		public void SetPreference(Object userID, Object itemID, double value) 
		{
			throw new NotSupportedException();
		}

		/**
		 * @throws UnsupportedOperationException
		 */
		public void RemovePreference(Object userID, Object itemID) 
		{
			throw new NotSupportedException();
		}

		/**
		 * {@inheritDoc}
		 */
		public void Refresh() 
		{
			// do nothing
		}

	}
}