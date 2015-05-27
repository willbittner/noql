using NoQL.CEP.Attributes;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace NoQL.CEP
{
    public interface IRamDB
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void Add<T>(T obj);

        string CreateIndex<T>(Func<T, object> keyFunction);

        void CreateIndex<T>(string indexName, Func<T, object> keyFunction);

        void Delete<T>(T delObj);

        /// <summary>
        ///     NEEDS A _LOT_ of revisiting
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="deletionSet"></param>
        void Delete<T>(IEnumerable<T> deletionSet);

        ArrayList GetEnumerable();

        IEnumerable<T> GetEnumerable<T>();

        void Update<T>(T obj, UpdatePolicy policy);

        IEnumerable<T> GetEnumerable<T>(string ixName, object ixValue);

        void Init<T>();
    }

    public class RamDB : IRamDB
    {
        private readonly ConcurrentDictionary<Type, dynamic> Tuples = new ConcurrentDictionary<Type, dynamic>();
        private ConcurrentDictionary<Type, List<string>> SetIndexNames = new ConcurrentDictionary<Type, List<string>>();

        private DistributedRamDB distributedRamDb;

        public RamDB(int preAllocSize = 0) //Need to get rid of this, not used anymore
        {
            //distributedRamDb = new DistributedRamDB("localhost");
        }

        public RamDB()
        {
            //distributedRamDb = new DistributedRamDB("localhost");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add<T>(T obj)
        {
            Type t = typeof(T);
            if (!Tuples.ContainsKey(t))
                Init<T>();

            Debug.Assert(Tuples[t] is YOLOtuple<T>);

            var tuple = Tuples[t] as YOLOtuple<T>;
            //distributedRamDb.Add(obj, null);
            foreach (string key in tuple.Indexes.Keys)
            {
                Debug.Assert(tuple.Indexes[key] is YOLOtuple<T>.IndexContainer<T>);

                var ixContainer = tuple.Indexes[key] as YOLOtuple<T>.IndexContainer<T>;

                object ixVal = ixContainer.IndexFunction(obj);

                if (ixVal == null) throw new Exception("Cannot Add Object to RamDB - value from Index function null - Object Type: " + typeof(T).Name + " Object: " + obj.ToString());
                //if ((!ixContainer.Data.ContainsKey(ixVal)) || ixContainer.Data[ixVal] == null)
                //{
                //    ixContainer.Data[ixVal] = new LockingList<T>();
                ////}

                ixContainer.Data[ixVal] = obj;
            }
        }

        public static string GetRedisKey<T>(T obj, object key)
        {
            return typeof(T).Name + key.ToString();
        }

        public string CreateIndex<T>(Func<T, object> keyFunction)
        {
            //distributedRamDb.AddIndex<T>(null);
            string ret = Guid.NewGuid().ToString();
            CreateIndex(ret, keyFunction);
            return ret;
        }

        public void CreateIndex<T>(string indexName, Func<T, object> keyFunction)
        {
            var ixContainer = new YOLOtuple<T>.IndexContainer<T>();
            ixContainer.IndexName = indexName;
            ixContainer.IndexFunction = keyFunction;
            //distributedRamDb.AddIndex<T>(null);
            if (ixContainer.Data == null) ixContainer.Data = new ConcurrentDictionary<object, T>();
            if (!Tuples.ContainsKey(typeof(T))) Init<T>();

            Debug.Assert(Tuples[typeof(T)] is YOLOtuple<T>);

            (Tuples[typeof(T)] as YOLOtuple<T>).Indexes[indexName] = ixContainer;
            Func<object, object> newKeyFunc = o => (object)keyFunction((T)o);
            if (!SetIndexNames.ContainsKey(typeof(T))) SetIndexNames[typeof(T)] = new List<string>();
            SetIndexNames[typeof(T)].Add(indexName);
            Reindex<T>();
        }

        public void Delete<T>(T delObj)
        {
            if (!Tuples.ContainsKey(typeof(T)))
                return;

            var tuple = Tuples[typeof(T)] as YOLOtuple<T>;
            Debug.Assert(tuple != null);

            foreach (object o in tuple.Indexes.Values)
            {
                var index = o as YOLOtuple<T>.IndexContainer<T>;

                Debug.Assert(index != null);
                T outobj;
                index.Data.TryRemove(o, out outobj);
            }
        }

        /// <summary>
        ///     NEEDS A _LOT_ of revisiting
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="deletionSet"></param>
        public void Delete<T>(IEnumerable<T> deletionSet)
        {
            //var tuple = Tuples[typeof (T)] as YOLOtuple<T>;
            //Debug.Assert(tuple != null);

            //foreach (object o in tuple.Indexes.Values)
            //{
            //    var index = o as YOLOtuple<T>.IndexContainer<T>;

            //    //Debug.Assert(index != null);

            //    List<T> deletion = deletionSet.ToList();
            //    foreach (T deletable in deletion)
            //    {
            //        foreach (var list in index.Data.Values)
            //        {
            //            list.Remove(deletable);
            //        }
            //    }
            //}
            int count = 0;
            for (int i = 0; i < deletionSet.Count(); i++)
            {
                Delete(deletionSet.ElementAt(i));
                count++;
            }
        }

        public IEnumerable<T> GetEnumerable<T>()
        {
            List<T> ret = new List<T>();

            var tuple = Tuples[typeof(T)] as YOLOtuple<T>;
            Debug.Assert(tuple != null);

            foreach (object o in tuple.Indexes.Values)
            {
                var index = (YOLOtuple<T>.IndexContainer<T>)o;

                //Debug.Assert(index != null);

                lock (index.Data)
                {
                    foreach (var list in index.Data.Values)
                    {
                        ret.Add(list);
                    }
                }
            }
            return ret.Distinct().ToList();
        }

        public IEnumerable<T> GetEnumerableAnd<T>(IList<LookupSpecification> Specifiers)
        {
            IEnumerable<T> output = null;
            foreach (LookupSpecification spec in Specifiers)
            {
                if (output == null)
                    output = GetEnumerable<T>(spec.IndexName, spec.IndexValue).ToList();
                else
                    output = output.Intersect(GetEnumerable<T>(spec.IndexName, spec.IndexValue)).ToList();
            }
            return output ?? new List<T>();
        }

        public IEnumerable<T> GetEnumerableOr<T>(IList<LookupSpecification> Specifiers)
        {
            IEnumerable<T> output = null;
            foreach (LookupSpecification spec in Specifiers)
            {
                if (output == null)
                    output = GetEnumerable<T>(spec.IndexName, spec.IndexValue).ToList();
                else
                    output = output.Union(GetEnumerable<T>(spec.IndexName, spec.IndexValue)).ToList();
            }
            return output ?? new List<T>();
        }

        public void Update<T>(T obj, UpdatePolicy policy)
        {
            //YOLOtuple<T> tuple = Tuples[typeof(T)];

            //bool removed = false;

            //var indexContainer = (tuple.Indexes as IEnumerable<YOLOtuple<T>.IndexContainer<T>>);
            //if (indexContainer != null)
            //{
            //    var containers = indexContainer.ToList();

            //    foreach (var container in containers)
            //    {
            //        var ixVal = container.IndexFunction(obj);

            //    }

            //}
            //if (!removed && policy == UpdatePolicy.UPDATE_ONLY)
            //    return;
            Add(obj);
        }

        public IEnumerable<T> GetEnumerable<T>(string ixName, object ixValue)
        {
            IEnumerable<T> ret = null;

            var tuple = Tuples[typeof(T)] as YOLOtuple<T>;
            Debug.Assert(tuple != null);

            if (!tuple.Indexes.ContainsKey(ixName))
            {
                throw new Exception("Invalid index name: " + ixName + " Type: " + typeof(T).Name);
            }

            var index = tuple.Indexes[ixName] as YOLOtuple<T>.IndexContainer<T>;
            if (!index.Data.ContainsKey(ixValue))
                throw new Exception("RamDB does not have any values for key: " + ixName + " KeyValue: " + ixValue + " Type: " + typeof(T));
            if (index.Data[ixValue] == null) return new T[0];
            return ToEnum(index.Data[ixValue]);
        }

        private IEnumerable<T> ToEnum<T>(T val)
        {
            T[] ar = new T[1];
            ar[0] = val;
            return ar;
        }

        public T GetFirstValue<T>(string indexName, object indexValue = null)
        {
            var tuple = Tuples[typeof(T)] as YOLOtuple<T>;

            Debug.Assert(tuple != null);

            if (!tuple.Indexes.ContainsKey(indexName))
            {
                throw new ArgumentException("Invalid index", "indexName");
            }

            var index = tuple.Indexes[indexName] as YOLOtuple<T>.IndexContainer<T>;
            return index.Data[indexValue ?? index.Data.Keys.First()];
        }

        public Func<T, object> GetIndexFunction<T>(string indexName)
        {
            var tuple = Tuples[typeof(T)] as YOLOtuple<T>;
            Debug.Assert(tuple != null);

            if (!tuple.Indexes.ContainsKey(indexName))
            {
                throw new ArgumentException("Invalid index name", indexName);
            }

            var index = tuple.Indexes[indexName] as YOLOtuple<T>.IndexContainer<T>;

            return index.IndexFunction;
        }

        public void Init<T>()
        {
            Type t = typeof(T);
            if (Tuples.ContainsKey(t))
                return; //This type is already inited

            var tuple = new YOLOtuple<T>();

            int indexCount = 0;
            foreach (PropertyInfo property in t.GetProperties())
            {
                object[] matchingProps = property.GetCustomAttributes(typeof(PrimaryKeyAttribute), true);
                if (!matchingProps.Any())
                    continue;

                Func<T, object> get = o => property.GetMethod.Invoke(o, null);
                var ixContainer = new YOLOtuple<T>.IndexContainer<T> { IndexName = property.Name, IndexFunction = get };
                tuple.Indexes[property.Name] = ixContainer;
                indexCount++;
            }

            if (indexCount == 0) //All objects need at least one index
            {
                var ixContainer = new YOLOtuple<T>.IndexContainer<T> { IndexName = "_default_", IndexFunction = x => x };
                tuple.Indexes["_default_"] = ixContainer;
            }

            Tuples[t] = tuple;
        }

        private void Reindex<T>()
        {
            List<T> list = GetEnumerable<T>().ToList();
            lock (this)
            {
                var tuple = Tuples[typeof(T)] as YOLOtuple<T>;

                foreach (string ixKey in tuple.Indexes.Keys)
                {
                    var ix = tuple.Indexes[ixKey] as YOLOtuple<T>.IndexContainer<T>;
                    ix.Data.Clear();
                }

                foreach (T item in list)
                {
                    Add(item);
                }
            }
        }

        #region Nested type: YOLOtuple

        internal class YOLOtuple<T> : IEnumerable
        {
            //IndexName -> IndexContainer
            internal ConcurrentDictionary<string, object> Indexes = new ConcurrentDictionary<string, object>();

            internal Type DataType
            {
                get { return typeof(T); }
            }

            #region Nested type: IndexContainer

            internal class IndexContainer<T> : IEnumerable
            {
                internal ConcurrentDictionary<object, T> Data = new ConcurrentDictionary<object, T>();

                internal Func<T, object> IndexFunction { get; set; }

                internal string IndexName { get; set; }

                public IEnumerator GetEnumerator()
                {
                    ArrayList ret = new ArrayList();

                    foreach (var collect in Data.Values)
                    {
                        ret.Add(collect);
                    }
                    return ret.GetEnumerator();
                }
            }

            #endregion Nested type: IndexContainer

            public IEnumerator GetEnumerator()
            {
                var ret = new ArrayList();
                foreach (IEnumerable ix in Indexes.Values)
                {
                    foreach (var item in ix)
                    {
                        if (!ret.Contains(item))
                        {
                            //No union operators on untyped collections
                            //gotta do it the hard way
                            ret.Add(item);
                        }
                    }
                }
                return ret.GetEnumerator();
            }
        }

        #endregion Nested type: YOLOtuple

        public ArrayList GetEnumerable()
        {
            var ret = new ArrayList();
            foreach (IEnumerable ix in Tuples.Values)
            {
                foreach (var item in ix)
                {
                    if (!ret.Contains(item))
                    {
                        //No union operators on untyped collections
                        //gotta do it the hard way
                        ret.Add(item);
                    }
                }
            }
            return ret;
        }
    }
}