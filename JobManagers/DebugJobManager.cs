using System;
using System.Collections.Generic;
using System.Threading;

namespace NoQL.CEP.JobManagers
{
    public class DebugJobManager : IJobManager
    {
        private readonly object _syncRoot = new object();
        private Queue<Job> jobs = new Queue<Job>();

        #region IJobManager Members

        public int Size
        {
            get { return jobs.Count; }
        }

        public void AddWeight(Type t)
        {
        }

        public void AddWeight(Type t, int weightPts)
        {
        }

        public Job Next()
        {
            lock (_syncRoot)
            {
                while (jobs.Count == 0)
                    Monitor.Wait(_syncRoot);
            }

            lock (jobs)
            {
                return jobs.Count > 0 ? jobs.Dequeue() : null;
            }
        }

        public void RemoveWeight(Type t)
        {
        }

        public void RemoveWeight(Type t, int weightPts)
        {
        }

        public void Schedule(Job j)
        {
            lock (_syncRoot)
            {
                jobs.Enqueue(j);

                Monitor.Pulse(_syncRoot);
            }
        }

        #endregion IJobManager Members
    }
}