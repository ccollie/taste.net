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
	using taste.Model;


/**
 * @author Sean Owen
 * @since 1.3.5
 */
	public sealed class NetflixMovie : Item 
    {
		private readonly Integer id;
		private readonly String title;

		public NetflixMovie(Integer id, String title) 
		{
			if (id == null || title == null) {
				throw new ArgumentNullException("ID or title is null");
			}
			this.id = id;
			this.title = title;
		}


		public Object ID 
		{
			get {return id;}
		}


		public String Title
		{
			get {return title;}
		}

		public bool IsRecommendable
		{
			get {return true;}
		}

		public int GetHashCode() 
		{
			return id.GetHashCode();
		}

		public override bool Equals(Object obj) 
		{
			return (obj is NetflixMovie) && ((NetflixMovie) obj).id.equals(id);
		}

		public int CompareTo(Item item) 
		{
			return this.id.compareTo(((NetflixMovie) item).id);
		}

		public override String ToString() 
		{
			return id + ":" + title;
		}
	}
}