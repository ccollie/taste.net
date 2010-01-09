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

    [Serializable]
    public class GenericItem<ItemT, IdT> : GenericItem<IdT> 
        where IdT : IComparable<IdT>
    {
        private readonly ItemT data;

        public GenericItem(ItemT item, IdT id) : this(item, id, true)
		{
			
		}

        public GenericItem(ItemT item, IdT id, bool recommendable)
            : base(id, recommendable)
        {
            this.data = item;
        }

        public ItemT Data
        {
            get { return data; }
        }
    }

    /// <summary>
    /// <p>An <see cref="taste.Model.Item">Item</see> which has no data other than an ID. This may be most useful for writing tests.</p>
    /// @author Sean Owen
    /// </summary>
    /// <typeparam name="K"></typeparam>
    [Serializable]
	public class GenericItem<K> : Item where K:IComparable<K>
	{
		private readonly K id;
		private readonly bool recommendable;

		public GenericItem(K id) : this(id, true)
		{
			
		}

		public GenericItem(K id, bool recommendable) 
		{
			if (id == null) 
			{
				throw new ArgumentNullException("id is null");
			}
			this.id = id;
			this.recommendable = recommendable;
		}

		public Object ID
		{
			get {return id;}
		}


        public bool IsRecommendable
		{
			get {return recommendable;}
		}

		public override int GetHashCode() 
		{
			return id.GetHashCode();
		}

		public override bool Equals(Object obj) 
		{
			return (obj is Item) && ((Item) obj).ID.Equals(id);
		}

		public override String ToString() 
		{
			return "Item[id:" + id.ToString() + ']';
		}
		
		public int CompareTo(Item o) 
		{
			return id.CompareTo((K)o.ID);
		}
    }

}	