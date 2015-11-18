using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Threading.Tasks;

namespace JPB.DataAccess
{
    [Serializable]
    public enum CollectionStates
    {
        Added,
        Removed,
        Unchanged,
        Changed
    }

    [Obsolete]
    internal class ReposetoryCollection<T> : ICollection<T>
    {
        internal ReposetoryCollection(IEnumerable enumeration)
        {
            _flatCollectionOfPrimaryKeys = new Dictionary<object, CollectionStates>();

            var foo = (enumeration as IEnumerable<T>);
            if (foo != null)
            {
                _items = foo.ToList();
            }

            _items = new List<T>();

            if (enumeration != null)
            {
                foreach (object item in enumeration)
                    _items.Add((T)item);
            }
        }

        private readonly Dictionary<object, CollectionStates> _flatCollectionOfPrimaryKeys;

        private List<T> _items;

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        public IEnumerator GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        public void CopyTo(Array array, int index)
        {
            if (index > _items.Count)
                return;

            for (int i = index; i < _items.Count; i++)
            {
                var item = _items[i];
                array.SetValue(item, i);
            }
        }

        public void Add(T item)
        {
            this._flatCollectionOfPrimaryKeys.Add(item.GetPK(), CollectionStates.Added);
            _items.Add(item);
        }

        public void Clear()
        {
            foreach (var item in _items)
            {
                this._flatCollectionOfPrimaryKeys.Add(item.GetPK(), CollectionStates.Removed);
            }
            _items.Clear();
        }

        public bool Contains(T item)
        {
            return _items.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _items.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            var pk = item.GetPK();
            if (_items.Remove(item))
            {
                this._flatCollectionOfPrimaryKeys.Add(pk, CollectionStates.Removed);
                return true;
            }
            return false;
        }

        public int Count { get { return _items.Count; } }
        public bool IsReadOnly { get { return false; } }

        private object _sync = new object();
        public object SyncRoot { get { return _sync; } }
        public bool IsSynchronized { get { return false; } }
    }
}