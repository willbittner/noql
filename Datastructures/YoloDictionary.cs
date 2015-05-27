using System;
using System.Collections;
using System.Collections.Generic;

namespace Testbed
{
    internal class YoloDictionary<Key, Value> : IDictionary<Key, Value>
    {
        [ThreadStatic]
        private static Dictionary<Key, Value> localDict;

        #region IDictionary<Key,Value> Members

        public void Add(Key key, Value value)
        {
            throw new NotImplementedException();
        }

        public bool ContainsKey(Key key)
        {
            throw new NotImplementedException();
        }

        public ICollection<Key> Keys
        {
            get { throw new NotImplementedException(); }
        }

        public bool Remove(Key key)
        {
            return localDict.Remove(key);
        }

        public bool TryGetValue(Key key, out Value value)
        {
            throw new NotImplementedException();
        }

        public ICollection<Value> Values
        {
            get { throw new NotImplementedException(); }
        }

        public Value this[Key key]
        {
            get
            {
                if (localDict == null) localDict = new Dictionary<Key, Value>();
                return localDict[key];
            }
            set
            {
                if (localDict == null) localDict = new Dictionary<Key, Value>();
                localDict[key] = value;
            }
        }

        public void Add(KeyValuePair<Key, Value> item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(KeyValuePair<Key, Value> item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(KeyValuePair<Key, Value>[] array, int arrayIndex)
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

        public bool Remove(KeyValuePair<Key, Value> item)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<KeyValuePair<Key, Value>> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        #endregion IDictionary<Key,Value> Members
    }
}