using System;
using System.Threading;


namespace Taste.Common
{
    [Serializable]
    public class AtomicInteger 
    {
        // Fields
        private int value;

        public AtomicInteger()
        {
        }

        public AtomicInteger(int initialValue)
        {
            //AtomicInteger integer = this;
            this.value = initialValue;
        }

        public int AddAndGet(int delta)
        {
            int num;
            int num2;
            do
            {
                num = this.Value;
                num2 = num + delta;
            }
            while (!this.CompareAndSet(num, num2));
            return num2;
        }

        public bool CompareAndSet(int i1, int i2)
        {
            return (Interlocked.CompareExchange(ref this.value, i2, i1) == i1);
        }

        public int DecrementAndGet()
        {
            return Interlocked.Decrement(ref this.value);
        }

        public double DoubleValue
        {
            get { return (double)this.Value; }
        }

        public float FloatValue
        {
            get { return (float)this.Value; }
        }

        public int Get()
        {
            return this.value;
        }

        public int Value
        {
            get { return this.Get(); }
        }

        public int GetAndAdd(int delta)
        {
            int num;
            int num2;
            do
            {
                num = this.Value;
                num2 = num + delta;
            }
            while (!this.CompareAndSet(num, num2));
            return num;
        }

        public int GetAndDecrement()
        {
            return (this.DecrementAndGet() + 1);
        }

        public int GetAndIncrement()
        {
            return (this.IncrementAndGet() - 1);
        }

        public int GetAndSet(int i)
        {
            return Interlocked.Exchange(ref this.value, i);
        }

        public int IncrementAndGet()
        {
            return Interlocked.Increment(ref this.value);
        }


        public int IntValue
        {
            get {return this.Get();}
        }

        public void LazySet(int newValue)
        {
            this.value = newValue;
        }

        public long LongValue
        {
            get { return (long)this.Get(); }
        }

        public void Set(int newValue)
        {
            this.value = newValue;
        }

        public override string ToString()
        {
            return this.Get().ToString();
        }

        public bool WeakCompareAndSet(int expect, int update)
        {
            return this.CompareAndSet(expect, update);
        }

        public static implicit operator int(AtomicInteger i)
        {
            return i.Get();
        }
    }         

}
