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
namespace taste.Tests.Model.File
{
	using System;
	using System.IO;
    using System.Collections.Generic;
	using taste.Common;
    using taste.Correlation;
	using taste.Recommender;
	using taste.Model;
	using taste.Neighborhood;
	using NUnit.Framework;


	/**
	 * <p>Tests {@link com.planetj.taste.impl.model.file.FileDataModel}.</p>
	 *
	 * @author Sean Owen
	 */

    [TestFixture]
	public class FileDataModelTest : TasteTestCase 
	{

		private static readonly File testFile = new File("src/test/com/planetj/taste/impl/model/file/test1.txt");

		private DataModel model;

        [TestFixtureSetUp]
		public override void SetUp()  
		{
			base.SetUp();
			model = new FileDataModel(testFile);
		}

        [Test]
		public void TestFile()  
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

        [Test]
		public void TestItem()  
        {
			Assert.AreEqual("456", model.GetItem("456").ID);
		}

        [Test]
		public void TestGetItems()  
        {
			IEnumerable<Item> items = model.GetItems();
			Assert.IsNotNull(items);
			IEnumerator<Item> it = items.GetEnumerator();
			Assert.IsNotNull(it);
			Assert.IsTrue(it.MoveNext());
			Assert.AreEqual("123", it.Current.ID);
			Assert.IsTrue(it.MoveNext());
			Assert.AreEqual("234", it.Current.ID);
			Assert.IsTrue(it.MoveNext());
			Assert.AreEqual("456", it.Current.ID);
			Assert.IsTrue(it.MoveNext());
			Assert.AreEqual("654", it.Current.ID);
			Assert.IsTrue(it.MoveNext());
			Assert.AreEqual("789", it.Current.ID);
			assertFalse(it.MoveNext());
			try 
            {
				it.MoveNext();
				Assert.Fail("Should throw NoSuchElementException");
			} catch (NoSuchElementException nsee) {
				// good
			}
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
			assertFalse(it.MoveNext());
			try {
				it.MoveNext();
				Assert.Fail("Should throw NoSuchElementException");
			} catch (NoSuchElementException nsee) {
				// good
			}
		}

        [Test]
		public void TestGetNumUsers()  
        {
			Assert.AreEqual(4, model.GetNumUsers());
		}

        [Test]
		public void TestSetPreference()  
        {
			try 
            {
				model.SetPreference(null, null, 0.0);
				Assert.Fail("Should have thrown UnsupportedOperationException");
			} 
            catch (InvalidOperationException) 
            {
				// good
			}
		}

#if false
        AtomicBoolean initialized;

        private void ThreadProc()
        {
			try 
            {
                model.GetNumUsers();
                initialized.set(true);					
            } catch (TasteException te) {
						// oops
            }
        }

        [Test]
		public void TestRefresh()  
        {
			initialized = new AtomicBoolean(false);
            //System.Threading.Thread.
			Runnable initializer = new Runnable() {
				public void run() {
					try {
						model.GetNumUsers();
						initialized.set(true);					
					} catch (TasteException te) {
						// oops
					}
				}
			};
			new Thread(initializer).start();
			Thread.sleep(1000L); // wait a second for thread to start and call getNumUsers()
			model.GetNumUsers(); // should block
			Assert.IsTrue(initialized.Get());
			Assert.AreEqual(4, model.GetNumUsers());
		}

#endif
        [Test]
		public void TestToString() 
		{
			Assert.IsTrue(model.ToString().Length > 0);
		}
	}

}