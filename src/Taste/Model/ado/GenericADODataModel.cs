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
	using System.IO;
	using Taste.Common;


	/**
	 * <p>A generic {@link taste.Model.DataModel} designed for use with other ADO data sources;
	 * one just specifies all necessary SQL queries to the constructor here. Optionally, the queries can
	 * be specified from a {@link Properties} object, {@link File}, or {@link InputStream}. This class is
	 * most appropriate when other existing implementations of {@link AbstractADODataModel} are not suitable.
	 * If you are using this class to support a major database, consider contributing a specialized implementation
	 * of {@link AbstractADODataModel} to the project for this database.</p>
	 *
	 * @author Sean Owen
	 */
	public class GenericADODataModel : AbstractADODataModel 
	{
		public const String CONNECTION_STRING_KEY = "connectionString";
		public const String GET_USER_SQL_KEY = "getUserSQL";
		public const String GET_NUM_USERS_SQL_KEY = "getNumUsersSQL";
		public const String GET_NUM_ITEMS_SQL_KEY = "getNumItemsSQL";
		public const String SET_PREFERENCE_SQL_KEY = "setPreferenceSQL";
		public const String REMOVE_PREFERENCE_SQL_KEY = "removePreferenceSQL";
		public const String GET_USERS_SQL_KEY = "getUsersSQL";
		public const String GET_ITEMS_SQL_KEY = "getItemsSQL";
		public const String GET_ITEM_SQL_KEY = "getItemSQL";
		public const String GET_PREFS_FOR_ITEM_SQL_KEY = "getPrefsForItemSQL";
		/** @since 1.3.2 */
		public const String GET_USERS_PREFERRING_ITEM_SQL_KEY = "getUsersPreferringItemSQL";

		/**
		 * <p>Specifies all SQL queries in a {@link Properties} object. See the <code>*_KEY</code>
		 * constants in this class (e.g. {@link #GET_USER_SQL_KEY}) for a list of all keys which
		 * must map to a value in this object.</p>
		 *
		 * @param props {@link Properties} object containing values
		 * @if anything goes wrong during initialization
		 */
		public GenericADODataModel(Properties props)
            :	base(            
			      props.getProperty(GET_USER_SQL_KEY),
			      props.getProperty(GET_NUM_USERS_SQL_KEY),
			      props.getProperty(GET_NUM_ITEMS_SQL_KEY),
			      props.getProperty(SET_PREFERENCE_SQL_KEY),
				  props.getProperty(REMOVE_PREFERENCE_SQL_KEY),
			      props.getProperty(GET_USERS_SQL_KEY),
			      props.getProperty(GET_ITEMS_SQL_KEY),
			      props.getProperty(GET_ITEM_SQL_KEY),
			      props.getProperty(GET_PREFS_FOR_ITEM_SQL_KEY),
			      props.getProperty(GET_USERS_PREFERRING_ITEM_SQL_KEY))

		{
            this.ConnectionString = props.getProperty(CONNECTION_STRING_KEY);
		}

        private string lookupDataSource(string dataSourceName)
        {
            return dataSourceName;
        }
		/**
		 * <p>See {@link #GenericADODataModel(java.util.Properties)}. This constructor reads values
		 * from a file instead, as if with {@link Properties#load(InputStream)}. So, the file
		 * should be in standard Java properties file format -- containing <code>key=value</code> pairs,
		 * one per line.</p>
		 *
		 * @param propertiesFile properties file
		 * @if anything goes wrong during initialization
		 */
		public GenericADODataModel(FileInfo propertiesFile) 
            : this(GetPropertiesFromFile(propertiesFile))
		{
		}

		/**
		 * <p>See {@link #GenericADODataModel(Properties)}. This constructor reads values
		 * from a resource available in the classpath, as if with {@link Class#getResourceAsStream(String)} and
		 * {@link Properties#load(InputStream)}. This is useful if your configuration file is, for example,
		 * packaged in a JAR file that is in the classpath.</p>
		 *
		 * @param resourcePath path to resource in classpath (e.g. "/com/foo/TasteSQLQueries.properties")
		 * @if anything goes wrong during initialization
		 */
		public GenericADODataModel(String resourcePath)
            : this(GetPropertiesFromStream( new FileStream(resourcePath, FileMode.Open)))
		{
		}


		private static Properties GetPropertiesFromFile(FileInfo file)
		{
			try 
			{
				return GetPropertiesFromStream(new FileStream(file.FullName, FileMode.Open));
			} 
			catch (FileNotFoundException fnfe) 
			{
				throw new TasteException(fnfe);
			}
		}

		private static Properties GetPropertiesFromStream(Stream istr) 
		{
			try 
			{
				Properties props = new Properties();
				props.Load(istr);
				return props;
			} 
			catch (IOException ioe) 
			{
				throw new TasteException(ioe);
			}
		}
	}

}	