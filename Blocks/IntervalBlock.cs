using NoQL.CEP.Exceptions;
using System;
using System.Collections.Concurrent;
using System.Reactive.Linq;

namespace NoQL.CEP.Blocks
{
    /// <summary>
    ///     Interval Block collects objects and batches them to child blocks (as T[])
    ///     at a fixed-rate time intervals.
    /// </summary>
    /// <typeparam name="T">The type of data expected</typeparam>
    public class IntervalBlock<T> : AbstractBlock
    {
        private IObservable<long> timer;

        public ConcurrentBag<T> Data { get; set; }

        public int Interval { get; private set; }

        internal IntervalBlock(Processor p, int intervalMS)
            : base(p)
        {
            Interval = intervalMS;
            Data = new ConcurrentBag<T>();
            resetTimer();
        }

        public override bool OnData(object data)
        {
            if (!(data is T))
                throw new BlockTypeMismatchException(typeof(T), data.GetType(), this);

            Data.Add((T)data);
            return false;
        }

        private void SendBuffer()
        {
            T[] localData;
            lock (this)
            {
                lock (Data)
                {
                    localData = Data.ToArray();
                    Data = new ConcurrentBag<T>();
                }
            }
            if (localData.Length > 0) SendToChildren(localData);
        }

        private void resetTimer()
        {
            timer = Observable.Interval(TimeSpan.FromMilliseconds(Interval));
            timer.Subscribe(s => SendBuffer());
        }

        public override Type BlockInputType
        {
            get { throw new NotImplementedException(); }
        }

        public override Type BlockOutputType
        {
            get { throw new NotImplementedException(); }
        }
    }
}