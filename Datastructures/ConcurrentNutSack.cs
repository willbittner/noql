using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace NoQL.CEP.JobManagers
{
    // yup
    public class ConcurrentNutSack<T>
    {
        [ThreadStatic]
        private static Queue<T> localQueue;

        private bool debug = true;
        private ConcurrentQueue<T> globalQueue = new ConcurrentQueue<T>();

        public void Enqueue(T obj)
        {
            if (localQueue == null)
            {
                globalQueue.Enqueue(obj);
            }
            else
            {
                localQueue.Enqueue(obj);
            }
        }

        public void RegisterWorkerThread()
        {
            localQueue = new Queue<T>();
        }

        public int Size()
        {
            if (localQueue == null) return 0;
            return globalQueue.Count;
        }

        public bool TryDequeue(out T obj)
        {
            if (localQueue == null) throw new Exception("Non worker thread trying to dequeue");
            if (localQueue.Count > 0)
            {
                obj = localQueue.Dequeue();
                return true;
            }
            return globalQueue.TryDequeue(out obj);
        }
    }
}