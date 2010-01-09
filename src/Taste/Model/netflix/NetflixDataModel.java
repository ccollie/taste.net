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

package com.planetj.taste.impl.model.netflix;

import com.planetj.taste.common.TasteException;
import com.planetj.taste.impl.model.GenericDataModel;
import com.planetj.taste.impl.model.GenericPreference;
import com.planetj.taste.impl.model.GenericUser;
import com.planetj.taste.impl.common.IOUtils;
import com.planetj.taste.model.DataModel;
import com.planetj.taste.model.Item;
import com.planetj.taste.model.Preference;
import com.planetj.taste.model.User;

import org.jetbrains.annotations.NotNull;

import java.io.BufferedReader;
import java.io.File;
import java.io.FileInputStream;
import java.io.FileNotFoundException;
import java.io.FilenameFilter;
import java.io.IOException;
import java.io.InputStreamReader;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;
import java.util.logging.Logger;

/**
 * @author Sean Owen
 * @since 1.3.5
 */
public final class NetflixDataModel implements DataModel {

	private static final Logger log = Logger.getLogger(NetflixDataModel.class.getName());

	private final DataModel delegate;

	public NetflixDataModel(final File dataDirectory) throws IOException {
		if (dataDirectory == null) {
			throw new IllegalArgumentException("dataDirectory is null");
		}
		if (!dataDirectory.exists() || !dataDirectory.isDirectory()) {
			throw new FileNotFoundException(dataDirectory.toString());
		}

		log.info("Creating NetflixDataModel for directory: " + dataDirectory);

		log.info("Reading movie data...");
		final List<NetflixMovie> movies = readMovies(dataDirectory);

		log.info("Reading preference data...");
		final List<User> users = readUsers(dataDirectory, movies);

		log.info("Creating delegate DataModel...");
		delegate = new GenericDataModel(users);
	}

	@NotNull
	private static List<User> readUsers(final File dataDirectory, final List<NetflixMovie> movies) throws IOException {
		final Map<Integer, List<Preference>> userIDPrefMap =
			new HashMap<Integer, List<Preference>>(104395301, 1.0f);
			//new HashMap<Integer, List<Preference>>(15485867, 1.0f);

		int counter = 0;
		final FilenameFilter filenameFilter = new FilenameFilter() {
			public boolean accept(final File dir, final String filename) {
				return filename.startsWith("mv_");
				//return filename.startsWith("mv_000");
			}
		};
		for (final File movieFile : new File(dataDirectory, "training_set").listFiles(filenameFilter)) {
			final BufferedReader reader = new BufferedReader(new InputStreamReader(new FileInputStream(movieFile)));
			String line = reader.readLine();
			if (line == null) {
				throw new IOException("Can't read first line of file " + movieFile);
			}
			final int movieID = Integer.parseInt(line.substring(0, line.length() - 1));
			final NetflixMovie movie = movies.get(movieID - 1);
			if (movie == null) {
				throw new IllegalArgumentException("No such movie: " + movieID);
			}
			while ((line = reader.readLine()) != null) {
				counter++;
				if (counter % 100000 == 0) {
					log.info("Processed " + counter + " prefs");
				}
				final int firstComma = line.indexOf((int) ',');
				final Integer userID = Integer.valueOf(line.substring(0, firstComma));
				final int secondComma = line.indexOf((int) ',', firstComma + 1);
				final double rating = Double.parseDouble(line.substring(firstComma + 1, secondComma));
				List<Preference> userPrefs = userIDPrefMap.get(userID);
				if (userPrefs == null) {
					userPrefs = new ArrayList<Preference>();
					userIDPrefMap.put(userID, userPrefs);
				}
				userPrefs.add(new GenericPreference(null, movie, rating));
			}
			IOUtils.quietClose(reader);
		}

		final List<User> users = new ArrayList<User>(userIDPrefMap.size());
		for (final Map.Entry<Integer, List<Preference>> entry : userIDPrefMap.entrySet()) {
			users.add(new GenericUser<Integer>(entry.getKey(), entry.getValue()));
		}
		return users;
	}

	@NotNull
	private static List<NetflixMovie> readMovies(final File dataDirectory) throws IOException {
		final List<NetflixMovie> movies = new ArrayList<NetflixMovie>(17770);
		final BufferedReader reader =
			new BufferedReader(new InputStreamReader(new FileInputStream(new File(dataDirectory, "movie_titles.txt"))));
		String line;
		while ((line = reader.readLine()) != null) {
			final int firstComma = line.indexOf((int) ',');
			final Integer id = Integer.valueOf(line.substring(0, firstComma));
			final int secondComma = line.indexOf((int) ',', firstComma + 1);
			final String title = line.substring(secondComma + 1);
			movies.add(new NetflixMovie(id, title));
		}
		IOUtils.quietClose(reader);
		return movies;
	}


	/**
	 * {@inheritDoc}
	 */
	@NotNull
	public Iterable<? extends User> getUsers() throws TasteException {
		return delegate.getUsers();
	}

	/**
	 * {@inheritDoc}
	 *
	 * @throws java.util.NoSuchElementException if there is no such user
	 */
	@NotNull
	public User getUser(final Object id) throws TasteException {
		return delegate.getUser(id);
	}

	/**
	 * {@inheritDoc}
	 */
	@NotNull
	public Iterable<? extends Item> getItems() throws TasteException {
		return delegate.getItems();
	}

	/**
	 * {@inheritDoc}
	 */
	@NotNull
	public Item getItem(final Object id) throws TasteException {
		return delegate.getItem(id);
	}

	/**
	 * {@inheritDoc}
	 */
	@NotNull
	public Iterable<? extends Preference> getPreferencesForItem(final Object itemID) throws TasteException {
		return delegate.getPreferencesForItem(itemID);
	}

	/**
	 * {@inheritDoc}
	 */
	public int getNumItems() throws TasteException {
		return delegate.getNumItems();
	}

	/**
	 * {@inheritDoc}
	 */
	public int getNumUsers() throws TasteException {
		return delegate.getNumUsers();
	}

	/**
	 * @throws UnsupportedOperationException
	 */
	public void setPreference(final Object userID, final Object itemID, final double value) {
		throw new UnsupportedOperationException();
	}

	/**
	 * @throws UnsupportedOperationException
	 */
	public void removePreference(final Object userID, final Object itemID) {
		throw new UnsupportedOperationException();
	}

	/**
	 * {@inheritDoc}
	 */
	public void refresh() {
		// do nothing
	}

}
