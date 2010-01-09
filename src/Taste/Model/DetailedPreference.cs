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

namespace Taste.Model
{
	using System;
	using System.Text;


	/**
	 * <p>An expanded version of {@link GenericPreference} which adds more fields;  for now, this only includes
	 * an additional timestamp field. This is provided as a convenience to implementations and
	 * {@link taste.Model.DataModel}s which wish to record and use this information in computations.
	 * This information is not added to {@link taste.Model.GenericPreference} to avoid expanding
	 * memory requirements of the algorithms supplied with Taste, since memory is a limiting factor.</p>
	 *
	 * @author Sean Owen
	 * @since 1.4.5
	 */
	public class DetailedPreference : GenericPreference 
	{
		private readonly long timestamp;

		public DetailedPreference(User user, Item item, double value, long timestamp) 
            : base(user, item, value)
		{
			if (timestamp < 0L) 
			{
				throw new ArgumentException("timestamp is negative");
			}
			this.timestamp = timestamp;
		}

		public long Timestamp
		{
			get {return timestamp;}
		}

		public override String ToString() 
		{
			return "GenericPreference[user: " + User.ToString() + ", item:" + Item.ToString() + ", value:" + Value.ToString() +
			       ", timestamp: " + (new DateTime(timestamp)).ToShortDateString() + ']';
		}

	}

}	