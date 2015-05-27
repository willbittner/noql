using ServiceStack.Redis;
using ServiceStack.Redis.Generic;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace NoQL.CEP
{
    public class YoloList<T> : ObservableCollection<T>
    {
        public YoloList(IEnumerable<T> oldcollection)
            : base(oldcollection)
        {
        }

        public void AddUniqute(T item)
        {
            if (!this.Contains(item)) this.Add(item);
        }
    }

    public class DistributedRamDB
    {
        private ConcurrentDictionary<Type, ObservableCollection<object>> redisLists = new ConcurrentDictionary<Type, ObservableCollection<object>>();

        private RedisClient mainClient;

        public string Hostname { get; set; }

        public DistributedRamDB(string hostname)
        {
            Hostname = hostname;
            try
            {
                mainClient = new RedisClient(Hostname);
            }
            catch (Exception e)
            {
                throw new Exception("RamDB Redis exception: " + e.InnerException);
                mainClient = null;
            }
        }

        public YoloList<T> GetYoloList<T>()
        {
            return new YoloList<T>(GetList<T>());
        }

        public void AddIndex<T>(object key)
        {
            var typelist = GetList<Type>();
            if (typelist.Contains(typeof(T))) return;
            typelist.Add(typeof(T));
        }

        public IList<Type> GetTypeList()
        {
            return GetList<Type>();
        }

        public void ClearList(Type type)
        {
            mainClient.RemoveAllFromList(GetStringListID(type));
        }

        private static string GetStringListID<T>()
        {
            return GetStringListID(typeof(T));
        }

        private static string GetStringListID(Type type)
        {
            return "urn:" + type.Name + ":list";
        }

        public IList<object> GetList(Type type)
        {
            string listString = GetStringListID(type);
            //var list = mainClient.Lists[listString];
            var obj2 = mainClient.As<object>();
            var list22 = obj2.Lists[listString];
            return list22;
        }

        public IList<T> GetList<T>()
        {
            IRedisTypedClient<T> typedClient = mainClient.As<T>();

            IRedisList<T> list = typedClient.Lists[GetStringListID<T>()];

            return list;
        }

        private bool IsConnected()
        {
            if (mainClient == null) return false;

            return true;
        }

        public void Add<T>(T obj, object key)
        {
            if (!IsConnected()) return;

            IList<T> list = GetList<T>();
            list.Add(obj);
            mainClient.PublishMessage("update", "ItemAdded");
        }

        public void Update<T>(T obj, object key, NoQL.CEP.UpdatePolicy policy)
        {
            if (!IsConnected()) return;

            IList<T> list = GetList<T>();
            lock (list)
            {
                if (list.Contains(obj))
                {
                    list.Remove(obj);
                    list.Add(obj);
                }
                else
                {
                    if (policy == UpdatePolicy.UPDATE_OR_INSERT)
                        list.Add(obj);
                }
            }

            mainClient.PublishMessage("update", "ItemAdded");
        }

        public IEnumerable<T> Get<T>()
        {
            return GetList<T>();
        }
    }
}