using NoQL.CEP.Exceptions;
using System.Collections.Concurrent;
using System.Threading;

namespace NoQL.CEP.Blocks
{
    /// <summary>
    ///     Signal Block collects data until the Signal() method is called
    ///     which sends the collected data down the CEP-tree
    ///     as T[]
    /// </summary>
    /// <typeparam name="T">The type of data expected</typeparam>
    public class SignalBlock<T> : AbstractBlock
    {
        //:TODO: change from lock
        private int DataCountIn;

        private int DataCountOut;
        private int SignalCount;
        private object gateLock = new object();
        private bool gateValue = true;

        public ConcurrentQueue<T> Data { get; set; }

        internal SignalBlock(Processor p)
            : base(p)
        {
            Data = new ConcurrentQueue<T>();
        }

        public override bool OnData(object data)
        {
            if (!(data is T))
            {
                throw new BlockTypeMismatchException(typeof(T), data.GetType(), this);
            }
            lock (gateLock)
            {
                Interlocked.Increment(ref DataCountIn);
                if (gateValue)
                {
                    T sendData;
                    if (Data.TryDequeue(out sendData))
                    {
                        SendToChildren(sendData);
                        Interlocked.Increment(ref DataCountOut);
                        Data.Enqueue((T)data);
                    }
                    else
                    {
                        Interlocked.Increment(ref DataCountOut);
                        SendToChildren(data);
                    }
                }
                else
                {
                    Data.Enqueue((T)data);
                }
                gateValue = false;
                //PrintCount();
            }

            return false;
        }

        public void PrintCount()
        {
            //if (DataCountIn != SignalCount) Console.WriteLine("DataCountIn: {0} DataCountOut: {1} SignalCount: {2}",DataCountIn,DataCountOut,SignalCount);
        }

        public void Signal()
        {
            lock (gateLock)
            {
                Interlocked.Increment(ref SignalCount);

                T sendData;
                if (Data.TryDequeue(out sendData))
                {
                    SendToChildren(sendData);
                    Interlocked.Increment(ref DataCountOut);
                    gateValue = false;
                }
                else
                {
                    gateValue = true;
                }
                PrintCount();
            }
            //T[] localData;
            //lock (this)
            //{
            //    lock (Data)
            //    {
            //        localData = Data.ToArray();
            //        Data = new ConcurrentBag<T>();
            //    }
            //}
            //SendToChildren(localData);
        }

        public override System.Type BlockInputType
        {
            get { throw new System.NotImplementedException(); }
        }

        public override System.Type BlockOutputType
        {
            get { throw new System.NotImplementedException(); }
        }
    }
}