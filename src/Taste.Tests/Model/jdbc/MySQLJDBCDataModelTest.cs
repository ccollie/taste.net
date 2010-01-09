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
namespace taste.tests.model.jdbc
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using taste.Tests;
    using taste.Common;
    using taste.Model;
    using taste.Model.ado;
    using taste.Correlation;
    using taste.Neighborhood;
    using taste.Recommender;
    using taste.Recommender.Slopeone;
    using NUnit.Framework;

    /**
     * <p>Tests {@link com.planetj.taste.impl.model.jdbc.MySQLJDBCDataModel}.</p>
     *
     * <p>Requires a MySQL 4.x+ database running on the localhost, with a passwordless user named "mysql" available,
     * a database named "test".</p>
     *
     * @author Sean Owen
     */
    public class MySQLJDBCDataModelTest : TasteTestCase 
    {

	    private DataModel model;

	    protected override void SetUp() 
        {

		    base.SetUp();

		    MysqlDataSource dataSource = new MysqlDataSource();
		    dataSource.setUser("mysql");
		    dataSource.setDatabaseName("test");
		    dataSource.setServerName("localhost");

		    IDbConnection connection = dataSource.getConnection();
		    try {

			    PreparedStatement dropStatement =
			        connection.prepareStatement("DROP TABLE IF EXISTS " + DEFAULT_PREFERENCE_TABLE);
			    try 
                {
				    dropStatement.Execute();
			    } finally {
				    dropStatement.Close();
			    }

			    IDbCommand createStatement =
			        connection.prepareStatement("CREATE TABLE " + DEFAULT_PREFERENCE_TABLE + " (" +
			                                    DEFAULT_USER_ID_COLUMN + " VARCHAR(4) NOT NULL, " +
			                                    DEFAULT_ITEM_ID_COLUMN + " VARCHAR(4) NOT NULL, " +
			                                    DEFAULT_PREFERENCE_COLUMN + " FLOAT NOT NULL, " +
					                            "PRIMARY KEY (" + DEFAULT_USER_ID_COLUMN + ", " +
					                            DEFAULT_ITEM_ID_COLUMN +  "), " +
			                                    "INDEX (" + DEFAULT_USER_ID_COLUMN + "), " +
					                            "INDEX (" + DEFAULT_ITEM_ID_COLUMN + ") )");
			    try 
                {
				    createStatement.execute();
			    } finally {
				    createStatement.close();
			    }

			    IDbCommand insertStatement =
			        connection.prepareStatement("INSERT INTO " + DEFAULT_PREFERENCE_TABLE +
			                                    " VALUES (?, ?, ?)");
			    try {
				    String[] users =
					    new String[]{"A123", "A123", "A123", "B234", "B234", "C345", "C345", "C345", "C345", "D456"};
				    String[] itemIDs =
					    new String[]{"456", "789", "654", "123", "234", "789", "654", "123", "234", "456"};
				    double[] preferences = new double[]{0.1, 0.6, 0.7, 0.5, 1.0, 0.6, 0.7, 1.0, 0.5, 0.1};
				    for (int i = 0; i < users.Length; i++) 
                    {
					    insertStatement.setString(1, users[i]);
					    insertStatement.setString(2, itemIDs[i]);
					    insertStatement.setDouble(3, preferences[i]);
					    insertStatement.execute();
				    }
			    } 
                finally 
                {
				    insertStatement.close();
			    }

		    } finally {
			    connection.close();
		    }

		    model = new MySQLJDBCDataModel(dataSource);
	    }

	    public void testStatements() 
        {
		    Assert.AreEqual(4, model.GetNumUsers());
		    Assert.AreEqual(5, model.GetNumItems());
		    Assert.AreEqual(new GenericUser<String>("A123", new List<Preference>()), model.GetUser("A123"));
		    Assert.AreEqual(new GenericItem<String>("456"), model.GetItem("456"));
		    Preference pref = model.GetUser("A123").GetPreferenceFor("456");
		    Assert.IsNotNull(pref);
		    Assert.AreEqual(0.1, pref.Value, EPSILON);
		    model.SetPreference("A123", "456", 0.2);
		    Preference pref1 = model.GetUser("A123").GetPreferenceFor("456");
		    Assert.IsNotNull(pref1);
		    Assert.AreEqual(0.2, pref1.Value, EPSILON);
		    model.RemovePreference("A123", "456");
		    Assert.IsNull(model.GetUser("A123").GetPreferenceFor("456"));
	    }

        [Test]
	    public void TestDatabase()
        {
		    UserCorrelation userCorrelation = new PearsonCorrelation(model);
		    UserNeighborhood neighborhood = new NearestNUserNeighborhood(2, userCorrelation, model);
		    Recommender recommender = new GenericUserBasedRecommender(model, neighborhood, userCorrelation);
		    Assert.AreEqual(2, recommender.Recommend("A123", 3).Count);
		    Assert.AreEqual(2, recommender.Recommend("B234", 3).Count);
		    Assert.AreEqual(1, recommender.Recommend("C345", 3).Count);

		    // Make sure this doesn't throw an exception
		    model.Refresh();
	    }

	    public void TestItem() 
        {
		    Assert.AreEqual("456", model.GetItem("456").ID);
	    }

        [Test]
	    public void TestPreferencesForItem() 
        {
		    IEnumerable<Preference> prefs = model.GetPreferencesForItem("456");
		    Assert.IsNotNull(prefs);
            
		    IEnumerator<Preference> it = prefs.GetEnumerator();
		    Assert.IsNotNull(it);
		    Assert.IsTrue(it.MoveNext());
		    Preference pref1 = it.Current;
		    Assert.AreEqual("A123", pref1.User.ID);
		    Assert.AreEqual("456", pref1.Item.ID);
		    Assert.IsTrue(it.MoveNext());
		    Preference pref2 = it.Current;
		    Assert.AreEqual("D456", pref2.User.ID);
		    Assert.AreEqual("456", pref2.Item.ID);
		    Assert.IsFalse(it.MoveNext());
		    try 
            {
			    it.MoveNext();
			    Assert.Fail("Should throw InvalidOperation");
		    } catch (System.InvalidOperationException) 
            {
			    // good
		    }
	    }

        [Test]
	    public void TestPreferencesForItemOrder() 
	    {
		    foreach (Item item in model.GetItems()) 
		    {
			    IEnumerable<Preference> prefs = model.GetPreferencesForItem(item.ID);
			    User lastUser = null;
			    foreach (Preference pref in prefs) 
			    {
				    User thisUser = pref.User;
				    if (lastUser != null) 
                    {
					    String lastID = (String) lastUser.ID;
					    String ID = (String) thisUser.ID;
					    Assert.IsTrue(lastID.CompareTo(ID) < 0);
				    }
				    lastUser = thisUser;
			    }
		    }
	    }

        [Test]
	    public void TestSetPreference()
	    {
		    model.SetPreference("A123", "409", 2.0);
		    Preference pref = model.GetUser("A123").GetPreferenceFor("409");
		    Assert.IsNotNull(pref);
		    Assert.AreEqual(2.0, pref.Value);
		    model.SetPreference("A123", "409", 1.0);		
		    Preference pref1 = model.GetUser("A123").GetPreferenceFor("409");
		    Assert.IsNotNull(pref1);
		    Assert.AreEqual(1.0, pref1.Value);
	    }

        [Test]
	    public void TestSetPrefMemoryDiffUpdates()
	    {
		    DiffStorage diffStorage = new MemoryDiffStorage(model, false, false, long.MaxValue);
		    Recommender recommender = new SlopeOneRecommender(model, true, true, diffStorage);
		    Assert.AreEqual(0.5, diffStorage.GetDiff("456", "789").Average, EPSILON);
		    recommender.SetPreference("A123", "456", 0.7);
		    Assert.AreEqual(-0.1, diffStorage.GetDiff("456", "789").Average, EPSILON);
	    }
    }
}