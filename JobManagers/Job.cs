using NoQL.CEP.Connections;
using NoQL.CEP.Datastructures;
using System.Runtime.CompilerServices;

namespace NoQL.CEP.JobManagers
{
    public class Job : IPooledObject
    {
        public object Data { get; set; }

        public Connection JobConnection { get; set; }

        [MethodImpl(MethodImplOptions.AggressiveInlining, MethodCodeType = MethodCodeType.IL)]
        public void Accept(object data)
        {
            JobConnection.Emit(data);
            Processor.ObjectPool.PutObject(this);
        }

        #region IPooledObject Members

        public void ResetObject()
        {
            JobConnection = null;
            Data = null;
        }

        #endregion IPooledObject Members
    }
}