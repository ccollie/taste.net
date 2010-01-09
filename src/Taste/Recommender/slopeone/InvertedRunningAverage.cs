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
    using System.Collections.Generic;
    using Taste.Common;


    public class InvertedRunningAverage : RunningAverage 
    {
	    private readonly RunningAverage _delegate;

	    public InvertedRunningAverage(RunningAverage _delegate) 
        {
		    this._delegate = _delegate;
	    }

	    public void AddDatum(double datum) 
        {
		    throw new NotSupportedException();
	    }

	    public void RemoveDatum(double datum) 
        {
		    throw new NotSupportedException();
	    }

	    public void ChangeDatum(double delta) 
        {
		    throw new NotSupportedException();
	    }

	    public int Count
        {
		    get {return _delegate.Count;}
	    }

	    public double Average 
        {
		    get {return _delegate.Average;}
	    }
    }

}