using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace NoQL.CEP.JobManagers
{
    internal enum OperationEnum
    {
        Delete = 1,
        Insert = 2
    }

    internal class NutBagOperation<T>
    {
        public T data;
        public OperationEnum op;
        public int threadid;

        public NutBagOperation(OperationEnum opin, T datain, int forThread)
        {
            op = opin;
            data = datain;
            threadid = forThread;
        }
    }

    public class ConcurrentNutBag<T> : IList<T>
    {
        [ThreadStatic]
        private int _threadID;

        [ThreadStatic]
        private List<T> _threadLocalList;

        [ThreadStatic]
        private int _workerThreadIsInit;

        private ConcurrentBag<T> globalBag = new ConcurrentBag<T>();

        private int nextThreadID;
        private ConcurrentQueue<NutBagOperation<T>> workerThreadUpdates = new ConcurrentQueue<NutBagOperation<T>>();

        private void CheckForUpdates()
        {
            int tempID = _threadID;
            CheckNewWorkerThread();
            //if (GetMyOpB == null) return;
            if (workerThreadUpdates.Count > 0)
            {
                NutBagOperation<T> opout;
                while (workerThreadUpdates.TryDequeue(out opout))
                {
                    if (opout.threadid == _threadID)
                    {
                        UpdateMyThread(opout);
                    }
                    else
                    {
                        workerThreadUpdates.Enqueue(opout);
                    }
                }
            }
        }

        private void CheckNewWorkerThread()
        {
            while (_workerThreadIsInit == 0)
            {
                if (Interlocked.CompareExchange(ref _workerThreadIsInit, 1, 0) == 0)
                {
                    _threadLocalList = new List<T>();
                    _threadID = Interlocked.Increment(ref nextThreadID);
                }
            }
        }

        public void ConcurrentNugBag()
        {
        }

        //ConcurrentBag<NutBagOperation<T>> GetMyOpBag()
        //{
        //    CheckNewWorkerThread();
        //    if (workerThreadUpdates.Count() >= _threadID + 1) return workerThreadUpdates.ElementAt(_threadID);
        //    return null;
        //}

        private void DoDelete(T data)
        {
            CheckNewWorkerThread();
            CheckForUpdates();
            for (int i = 0; i < nextThreadID; i++)
            {
                if (i == _threadID) continue;
                var op = new NutBagOperation<T>(OperationEnum.Delete, data, i);
                workerThreadUpdates.Enqueue(op);
            }
        }

        private void DoInsert(T data)
        {
            CheckNewWorkerThread();
            CheckForUpdates();
            for (int i = 0; i < nextThreadID; i++)
            {
                if (i == _threadID) continue;
                var op = new NutBagOperation<T>(OperationEnum.Insert, data, i);
                workerThreadUpdates.Enqueue(op);
            }
        }

        private void UpdateMyThread(NutBagOperation<T> op)
        {
            if (op.op == OperationEnum.Delete)
            {
                _threadLocalList.Remove(op.data);
            }
            if (op.op == OperationEnum.Insert)
            {
                _threadLocalList.Add(op.data);
            }
        }

        #region IList<T> Members

        public int IndexOf(T item)
        {
            throw new NotImplementedException();
        }

        public void Insert(int index, T item)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        public T this[int index]
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public void Add(T item)
        {
            DoInsert(item);
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(T item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public int Count
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsReadOnly
        {
            get { throw new NotImplementedException(); }
        }

        public bool Remove(T item)
        {
            DoDelete(item);
            return true;
        }

        public IEnumerator<T> GetEnumerator()
        {
            CheckNewWorkerThread();
            CheckForUpdates();
            foreach (T data in _threadLocalList)
            {
                yield return data;
            }
            for (int i = 0; i < workerThreadUpdates.Count; i++)
            {
                NutBagOperation<T> op;
                if (workerThreadUpdates.TryDequeue(out op))
                {
                    if (op.op == OperationEnum.Insert)
                    {
                        yield return op.data;
                    }
                }
                else break;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion IList<T> Members
    }
}