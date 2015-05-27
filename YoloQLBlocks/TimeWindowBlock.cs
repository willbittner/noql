using NoQL.CEP.Blocks;
using System;
using System.Collections.Concurrent;
using System.Reactive.Linq;

namespace NoQL.CEP.YoloQLBlocks
{
    public class TimeWindowBlock<T> : AbstractBlock
    {
        //private IObservable<long> timer;
        private int MAX_SIZE = 10000;

        private TimeSpan WindowSize;
        private ConcurrentDictionary<int, T> dataDictionary;
        private ConcurrentQueue<int> idQueue;

        public TimeWindowBlock(Processor p, TimeSpan windowSize)
            : base(p)
        {
            WindowSize = windowSize;
            idQueue = new ConcurrentQueue<int>();
            dataDictionary = new ConcurrentDictionary<int, T>();
            for (int i = 0; i < MAX_SIZE; i++) idQueue.Enqueue(i);
        }

        public override bool OnData(object data)
        {
            IObservable<long> timer = Observable.Interval(WindowSize);
            var castData = (T)data;
            int nextID;
            if (idQueue.TryDequeue(out nextID))
            {
                dataDictionary[nextID] = castData;
                timer.Subscribe(s => RemoveAfterTimeout(nextID));
            }
            else
            {
                throw new Exception("TimeWindowBlock queue hit max size" + DebugName);
            }
            SendWindow();
            return false;
        }

        private void RemoveAfterTimeout(int id)
        {
            T outval;
            if (dataDictionary.TryRemove(id, out outval))
            {
                idQueue.Enqueue(id);
            }
        }

        public void SendWindow()
        {
            if (dataDictionary.Count > 0) SendToChildren(dataDictionary.Values);
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