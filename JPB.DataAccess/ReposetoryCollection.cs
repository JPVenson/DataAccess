using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Threading.Tasks;

namespace JPB.DataAccess
{
    internal class ReposetoryCollection<TE> : ICollection<TE>
    {
        //static ReposetoryCollection()
        //{
        //    ctor = typeof(ReposetoryCollection<>).GetConstructor(new[] { typeof(IEnumerable) });
        //}

        //internal static readonly ConstructorInfo ctor;

        public ReposetoryCollection(IEnumerable enumeration)
        {
            _flatCollectionOfPrimaryKeys = new Dictionary<long, CollectionStates>();

            var foo = (enumeration as IEnumerable<TE>);
            if (foo != null)
            {
                _items = foo.ToList();
            }

            _items = new List<TE>();

            if (enumeration != null)
            {
                foreach (object item in enumeration)
                    _items.Add((TE)item);
            }
        }

        internal enum CollectionStates
        {
            Added,
            Removed,
        }

        private readonly Dictionary<long, CollectionStates> _flatCollectionOfPrimaryKeys;

        private List<TE> _items;

        IEnumerator<TE> IEnumerable<TE>.GetEnumerator()
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

        public void Add(TE item)
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

        public bool Contains(TE item)
        {
            return _items.Contains(item);
        }

        public void CopyTo(TE[] array, int arrayIndex)
        {
            _items.CopyTo(array, arrayIndex);
        }

        public bool Remove(TE item)
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