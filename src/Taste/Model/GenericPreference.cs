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

namespace Taste.Model
{
	using System;
    using Taste.Common;


    /// <summary>
    /// <p>A simple <see cref="taste.Model.Preference">Preference</see> encapsulating an <see cref="taste.Model.Item">Item</see> and preference value.</p>
    ///
    /// author Sean Owen
    /// </summary>
	[Serializable]	// TODO :  make _really_ generic
	public class GenericPreference : Preference
	{
		private User user;
		private Item item;
		private double value;

		public GenericPreference(User user, Item item, double value) 
		{
			if (item == null) 
			{
				throw new ArgumentNullException("item is null");
			}
			if (Double.IsNaN(value)) 
			{
				throw new ArgumentException("Invalid value: " + value);
			}
			this.user = user;
			this.item = item;
			this.value = value;
		}


        public User User
		{
			get
			{
				if (user == null) 
				{
					throw new IllegalStateException("User was never set");
				}
				return user;
			}
			set
			{
				if (value == null) 
				{
					throw new ArgumentNullException("cannot set user to null");
				}
				this.user = value;			
			}
		}


		public Item Item
		{
			get {return item;}
		}


        public double Value
		{
			get {return value;}
			set
			{
				if (double.IsNaN(value)) 
				{
					throw new ArgumentException("Invalid value: " + value);
				}
				this.value = value;			
			}
		}


		public override String ToString() 
		{
			return "GenericPreference[user: " + user + ", item:" + item + ", value:" + value + ']';
		}

	}

}	