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

namespace Taste.Transforms
{
	using System;
	using System.Collections.Generic;


    /// <summary>
    /// <p>A simple, fast utility class that maps keys to counts.</p>
    /// author Sean Owen
    /// </summary>
    /// <typeparam name="T"></typeparam>
	internal class Counters<T> 
	{	
		private readonly Dictionary<T, MutableInteger> counts = new Dictionary<T, MutableInteger>(1009);

		public void Increment(T key) 
		{
			MutableInteger count = get(key);
			if (count == null) 
			{
				MutableInteger newCount = new MutableInteger();
				newCount.Value = 1;
				counts.Add(key, newCount);
			} else {
				count.Increment();
			}
		}

		public int GetCount(T key) 
		{
			MutableInteger count = get(key);
			return count == null ? 0 : count.Value;
		}

        private MutableInteger get(T Key)
        {
            MutableInteger value;
            if (counts.TryGetValue(Key, out value))
                return value;
            return null;
        }

		public int Count 
		{
			get {return counts.Count;}
		}

        public Dictionary<T, MutableInteger> Map
        {
            get { return counts; }
        }

        public int this[T key]
        {
            get 
            {
                return GetCount(key);
            }

            set
            {
                MutableInteger mi = get(key);
                if (mi == null)
                {
                    mi = new MutableInteger();
                    counts.Add(key, mi);
                };
                mi.Value = value;
            }
        }
     
		public override String ToString() 
		{
			return "Counters[" + counts + ']';
		}
	}

    internal class MutableInteger
    {
        int _value;

        public override String ToString()
        {
            return "MutableInteger[" + _value + "]";
        }
        public int Value
        {
            get { return _value; }
            set { _value = value; }
        }

        public void Increment(int delta)
        {
            _value += delta;
        }

        public void Increment()
        {
            _value++;
        }

        public void Decrement()
        {
            _value--;
        }

        public void Decrement(int delta)
        {
            _value -= delta;
        }

    }


}	