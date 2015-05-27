using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace NoQL.CEP.Datastructures
{
    public interface IPooledObject
    {
        void ResetObject();
    }

    /// <summary>
    ///     Object Pool for Pre-Allocated Objects
    /// </summary>
    public class ObjectPool
    {
        private IDictionary<Type, object> pool = new ConcurrentDictionary<Type, object>();

        public int PoolSizes { get; set; }

        /// <summary>
        ///     Gets an object out of the object pool or if there
        ///     are no more objects in the pool, creates
        ///     a new one
        /// </summary>
        /// <typeparam name="ObjectType">
        ///     The object type
        ///     to get out of the pool
        /// </typeparam>
        /// <returns></returns>
        public ObjectType GetObject<ObjectType>() where ObjectType : new()
        {
            if (!pool.ContainsKey(typeof(ObjectType)))
                return new ObjectType();

            var stack = (ConcurrentStack<ObjectType>)pool[typeof(ObjectType)];

            ObjectType ret;
            if (!stack.TryPop(out ret))
                return new ObjectType();

            return ret;
        }

        /// <summary>
        ///     Call this when you'd like to initialize an object pool for
        ///     a given type
        /// </summary>
        /// <typeparam name="ObjectType">The type to initialize in the pool</typeparam>
        public void InitializePool<ObjectType>() where ObjectType : IPooledObject, new()
        {
            if (!pool.ContainsKey(typeof(ObjectType)))
            {
                pool[typeof(ObjectType)] = new ConcurrentStack<ObjectType>();
            }

            var bag = (ConcurrentStack<ObjectType>)pool[typeof(ObjectType)];

            for (int i = 0; i < PoolSizes; i++)
            {
                bag.Push(new ObjectType());
            }
        }

        /// <summary>
        ///     Puts an object back in the pool after it's been used.
        /// </summary>
        /// <typeparam name="ObjectType">The object type being placed back in the pool</typeparam>
        /// <param name="obj">The object type being placed back in the pool</param>
        public void PutObject<ObjectType>(ObjectType obj)
        {
            if (!pool.ContainsKey(typeof(ObjectType)))
                return;

            var stack = (ConcurrentStack<ObjectType>)pool[typeof(ObjectType)];

            if (stack.Count < PoolSizes)
                stack.Push(obj);
            else
                GC.ReRegisterForFinalize(obj); //Make sure this object is registered in GC
        }
    }
}