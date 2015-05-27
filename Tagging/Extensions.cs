using ServiceStack.Net30.Collections.Concurrent;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NoQL.CEP.Tagging
{
    public static class Extensions
    {
        private static IDictionary<int, ConcurrentDictionary<WeakReference, ConcurrentDictionary<string, object>>> TagDictionary =
            new ConcurrentDictionary<int, ConcurrentDictionary<WeakReference, ConcurrentDictionary<string, object>>>();

        public static IDictionary<string, object> GetAllTags(this object obj)
        {
            var hash = obj.GetHashCode();
            if (!TagDictionary.ContainsKey(hash))
                return null;

            //Grab the inner dictionary
            var innerDict = TagDictionary[hash];

            var wr = innerDict.Keys.FirstOrDefault(r => r.Target == obj);

            if (wr == null || !wr.IsAlive)
            {
                if (wr != null)
                {
                    ConcurrentDictionary<string, object> temp;
                    innerDict.TryRemove(wr, out temp);
                }

                return null;
            }

            return innerDict[wr];
        }

        public static object GetTag(this object obj, string key)
        {
            IDictionary<string, object> tags;
            return (tags = GetAllTags(obj)) != null && tags.ContainsKey(key) ? tags[key] : null;
        }

        public static object GetTag<T>(this object obj, string key) where T : class
        {
            IDictionary<string, object> tags;
            return (tags = GetAllTags(obj)) != null ? (T)(tags[key]) : null;
        }

        public static void SetTag(this object obj, string key, object value)
        {
            var hash = obj.GetHashCode();
            IDictionary<WeakReference, ConcurrentDictionary<string, object>> innerDict;
            if (!TagDictionary.ContainsKey(hash) || TagDictionary[hash] == null)
            {
                innerDict = TagDictionary[hash] = new ConcurrentDictionary<WeakReference, ConcurrentDictionary<string, object>>();
            }
            else
            {
                innerDict = TagDictionary[hash];
            }

            var wr = innerDict.Keys.FirstOrDefault(r => r.Target == obj);
            if (wr == null)
            {
                wr = new WeakReference(obj);
                innerDict[wr] = new ConcurrentDictionary<string, object>();
            }

            innerDict[wr][key] = value;
        }
    }
}