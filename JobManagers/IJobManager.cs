using System;

namespace NoQL.CEP.JobManagers
{
    public interface IJobManager
    {
        int Size { get; }

        void AddWeight(Type t);

        void AddWeight(Type t, int weightPts);

        Job Next();

        void RemoveWeight(Type t);

        void RemoveWeight(Type t, int weightPts);

        void Schedule(Job j);
    }
}