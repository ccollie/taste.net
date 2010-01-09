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

namespace Taste.Tests
{
	using Taste;
	using Taste.Common;
    using Taste.Model;
	using Taste.Correlation;
    using Taste.Recommender;
    using Taste.Recommender.SlopeOne;
	using Taste.Neighborhood;

    using System;
	using System.Collections;
    using System.Threading;
	using System.Collections.Generic;
	using log4net;
    using Retlang;

	using NUnit.Framework;


    /**
     * <p>Generates load on the whole implementation, for profiling purposes mostly.</p>
     *
     * @author Sean Owen
     */
    [TestFixture]
    public class LoadTest : TasteTestCase 
    {
	    private static readonly ILog log = LogManager.GetLogger(typeof(LoadTest));

	    private const int NUM_USERS = 800;
	    private const int NUM_ITEMS = 800;
	    private const int NUM_PREFS = 20;
	    private const int NUM_THREADS = 4;

	    private Random random = RandomUtils.GetRandom();

        [SetUp]
	    protected override void SetUp() 
        {
		    base.SetUp();
		    //SetLogLevel(Level.INFO);
	    }

        [Test]
	    public void TestSlopeOneLoad() 
	    {
		    DataModel model = CreateModel();
		    Taste.Recommender.Recommender recommender = new CachingRecommender(new SlopeOneRecommender(model));
		    DoTestLoad(recommender, 30);
	    }

        [Test]
	    public void TestItemLoad() 
	    {
		    DataModel model = CreateModel();
            ItemCorrelation itemCorrelation = new PearsonCorrelation(model);
            var recommender = new CachingRecommender(new GenericItemBasedRecommender(model, itemCorrelation));
		    DoTestLoad(recommender, 60);
	    }

        [Test]
	    public void TestUserLoad() 
	    {
		    DataModel model = CreateModel();
		    UserCorrelation userCorrelation = new PearsonCorrelation(model);
		    userCorrelation.PreferenceInferrer = new AveragingPreferenceInferrer(model);
		    UserNeighborhood neighborhood = new NearestNUserNeighborhood(10, userCorrelation, model);
            var recommender =
			    new CachingRecommender(new GenericUserBasedRecommender(model, neighborhood, userCorrelation));
		    DoTestLoad(recommender, 20);
	    }
       
	    private DataModel CreateModel() 
	    {
		    List<Item> items = new List<Item>(NUM_ITEMS);
		    for (int i = 0; i < NUM_ITEMS; i++) 
		    {
			    items.Add(new GenericItem<String>(i.ToString()));
		    }

		    List<User> users = new List<User>(NUM_USERS);
		    for (int i = 0; i < NUM_USERS; i++) 
		    {
                int numPrefs = (int)(random.NextDouble() * NUM_PREFS) + 1;
			    List<Preference> prefs = new List<Preference>(numPrefs);
			    for (int j = 0; j < numPrefs; j++) 
			    {
				    prefs.Add(new GenericPreference(null, items[random.Next(NUM_ITEMS)], random.NextDouble()));
			    }
			    GenericUser<String> user = new GenericUser<String>(i.ToString(), prefs);
			    users.Add(user);
		    }

		    return new GenericDataModel(users);
	    }

        private void DoTestLoad(Taste.Recommender.Recommender recommender, int allowedTimeSec)
	    {
           
            // TODO: how to specify number of threads
            PoolQueue queue = new PoolQueue(new DefaultThreadPool(), new CommandExecutor());
  		   
            Command cmd = delegate
            {
                for (int i = 0; i < NUM_USERS; i++)
                {
                    string id = random.Next(NUM_USERS).ToString();
                    recommender.Recommend(id, 10);
                    if (i % 100 == 50)
                    {
                        recommender.Refresh();
                    }
                }
            };

		    long start = DateTime.Now.Ticks;
		    for (int i = 0; i < NUM_THREADS; i++) 
		    {
                queue.Enqueue(cmd);			    
		    }
		    long end = DateTime.Now.Ticks;	

		    double timeMS = TimeSpan.FromTicks(end - start).TotalMilliseconds;
		    log.Info("Load test completed in " + timeMS + "ms");
		    Assert.IsTrue(timeMS < 1000L * (long) allowedTimeSec);
	    }

    }
}