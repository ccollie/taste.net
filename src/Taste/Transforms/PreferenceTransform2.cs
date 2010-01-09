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

namespace Taste.Transforms
{
	using System;
	using System.Collections.Generic;
	using Taste.Common;
	using Taste.Model;

    /// <summary>
    /// <p>Implementations encapsulate a transform on a <see cref="taste.Model.Preference">Preference</see>'s value. These transformations are
    /// typically applied to values before they are used to compute a Correlation value. They are typically not
    /// applied elsewhere; in particular <see cref="taste.Model.DataModel">DataModel</see>s no longer use a transform
    /// like this to transform all of their preference values at the source.</p>
    /// <p>This class sort of replaces the <code>PreferenceTransform</code> interface. It operates similarly, but
    /// is applied a bit differently within the framework. As such I wanted to make this an entirely new interface,
    /// but couldn't pick a name that seemed as applicable. Hence the simplistic name.</p>
    /// 
    ///author Sean Owen
    /// </summary>
	public interface PreferenceTransform2 : Refreshable 
	{
		double GetTransformedValue(Preference pref);
	}
	
}