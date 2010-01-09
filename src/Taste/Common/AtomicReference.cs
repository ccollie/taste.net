using System;
using System.Threading;

namespace Taste.Common
{
    public class AtomicReference<V> where V : new()
    {
        // Fields
        private V value;
        private object objVal;

       
        public AtomicReference()
        {
        }

        public AtomicReference(V initialValue)
        {
            //AtomicReference<V> reference = this;
            this.value = initialValue;
            this.objVal = (object)value;
        }

        public bool CompareAndSet(object obj1, object obj2)
        {
            return (Interlocked.CompareExchange(ref this.objVal, obj2, obj1) == obj1);
        }

        public bool WeakCompareAndSet(V expect, V update)
        {
            return this.CompareAndSet(expect, update);
        }

        public V Get()
        {
            return this.value;
        }

        public V Value
        {
            get { return Get(); }
            set { Set(value); }
        }

        public V GetAndSet(V newValue)
        {
            V obj2;
            do
            {
                obj2 = this.Get();
            }
            while (!this.CompareAndSet(obj2, newValue));
            return obj2;
        }

        public void LazySet(V newValue)
        {
            this.value = newValue;
        }


        public void Set(V newValue)
        {
            this.value = newValue;
            this.objVal = (object)value;
        }

        public override string ToString()
        {
            return this.Get().ToString();
        }

    }
 
}
