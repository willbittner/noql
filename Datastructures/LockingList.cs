using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace NoQL.CEP.Datastructures
{
    public class LockingList<T> : IList<T>
    {
        private List<T> _list = new List<T>();

        #region IList<T> Members

        [MethodImpl(MethodImplOptions.Synchronized)]
        public int IndexOf(T item)
        {
            return _list.IndexOf(item);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Insert(int index, T item)
        {
            _list.Insert(index, item);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void RemoveAt(int index)
        {
            _list.RemoveAt(index);
        }

        public T this[int index]
        {
            get { return _list[index]; }
            set { _list[index] = value; }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Add(T item)
        {
            _list.Add(item);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Clear()
        {
            _list.Clear();
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool Contains(T item)
        {
            return _list.Contains(item);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void CopyTo(T[] array, int arrayIndex)
        {
            _list.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return _list.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool Remove(T item)
        {
            return _list.Remove(item);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public IEnumerator<T> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        IEnumerator IEnumerable.GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        #endregion IList<T> Members
    }
}