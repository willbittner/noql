using System;
using System.Collections.Concurrent;

namespace NoQL.CEP.JobManagers
{
    public class SimpleJobManager : IJobManager
    {
        private ConcurrentQueue<Job> jobs = new ConcurrentQueue<Job>();

        public int Size
        {
            get { return jobs.Count; }
        }

        public void AddWeight(Type t)
        {
            throw new NotImplementedException();
        }

        public void AddWeight(Type t, int weightPts)
        {
            throw new NotImplementedException();
        }

        public Job Next()
        {
            Job j = null;
            jobs.TryDequeue(out j);
            return j;
        }

        public void RemoveWeight(Type t)
        {
            throw new NotImplementedException();
        }

        public void RemoveWeight(Type t, int weightPts)
        {
            throw new NotImplementedException();
        }

        public void Schedule(Job j)
        {
            jobs.Enqueue(j);
        }
    }
}