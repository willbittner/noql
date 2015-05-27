using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace NoQL.CEP.JobManagers
{
    public class DefaultJobManager : IJobManager
    {
        private List<Type> Cycle = new List<Type> { typeof(object) };
        private ConcurrentDictionary<Type, ConcurrentQueue<Job>> Queues = new ConcurrentDictionary<Type, ConcurrentQueue<Job>>();
        private Processor processor;
        private int Count = 0;

        private int cyclePosition = 0;

        private Type GetCycleType(bool incrememnt = true)
        {
            int pos = cyclePosition;
            if (incrememnt) Interlocked.Increment(ref cyclePosition);
            return Cycle[pos % Cycle.Count];
        }

        public DefaultJobManager(Processor p)
        {
            processor = p;
            Queues[typeof(object)] = new ConcurrentQueue<Job>();
        }

        #region IJobManager Members

        public Job Next()
        {
            return GetJobFor(GetCycleType()) ?? BackupPlan();
        }

        private Job BackupPlan()
        {
            var ret = (Cycle.Select(GetJobFor).FirstOrDefault(job => job != null));
            Interlocked.Decrement(ref Count);
            return ret;
        }

        private Job GetJobFor(Type type)
        {
            if (!Queues.ContainsKey(type) || Queues[type] == null)
                return null;

            var list = Queues[type];
            Job ret;
            if (!list.TryDequeue(out ret))
                return null;

            Interlocked.Decrement(ref Count);
            return ret;
        }

        public void Schedule(Job j)
        {
            var type = j.Data.GetType();
            if (!Queues.ContainsKey(type) || Queues[type] == null)
                Queues[typeof(object)].Enqueue(j);
            else
                Queues[type].Enqueue(j);
            Interlocked.Increment(ref Count);
        }

        public void AddWeight(Type t)
        {
            if (t == typeof(object))
                throw new Exception("Can not perform weight operations on object");
            lock (Cycle)
            {
                if (!Queues.ContainsKey(t))
                {
                    Queues[t] = new ConcurrentQueue<Job>();
                }
                Cycle.Add(t);
            }
        }

        public void AddWeight(Type t, int weightPts)
        {
            if (t == typeof(object))
                throw new Exception("Can not perform weight operations on object");
            if (weightPts < 0)
                throw new Exception("Weight Pts must be positive");

            lock (Cycle)
            {
                if (!Queues.ContainsKey(t))
                {
                    Queues[t] = new ConcurrentQueue<Job>();
                }
                for (int i = 0; i < weightPts; i++)
                    Cycle.Add(t);
            }
        }

        public void RemoveWeight(Type t)
        {
            if (t == typeof(object))
                throw new Exception("Can not perform weight operations on object");

            lock (Cycle)
            {
                var cntOfType = Cycle.Count(tx => tx == t);
                if (cntOfType == 0)
                    return;
                else if (cntOfType == 1 && (Queues[t] != null))
                {
                    List<Job> jobsToMove = Queues[t].ToList();
                    foreach (Job j in jobsToMove)
                        Queues[typeof(object)].Enqueue(j);
                }
                Cycle.RemoveAt(Cycle.IndexOf(t));
            }
        }

        public void RemoveWeight(Type t, int weightPts)
        {
            if (t == typeof(object))
                throw new Exception("Can not perform weight operations on object");
            if (weightPts < 0)
                throw new Exception("Weight Pts must be positive");

            lock (Cycle)
            {
                for (int i = 0; i < weightPts; i++)
                    RemoveWeight(t);
            }
        }

        public int Size
        {
            get
            {
                return Count;
            }
        }

        #endregion IJobManager Members
    }
}