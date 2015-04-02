using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JPB.DataAccess.Manager;

namespace JPB.DataAccess.DbCollection
{
    /// <summary>
    /// WIP Observes the local collection and allows a Generic save update remove and insert
    /// </summary>
    public class DbCollection<T> : IList<T> where T : class, INotifyPropertyChanged
    {
        class StateHolder
        {
            public StateHolder(T value, CollectionStates state)
            {
                Value = value;
                State = state;
            }

            public T Value { get; set; }
            public CollectionStates State { get; set; }
        }

        private void Add(T value, CollectionStates state)
        {
            this._internalCollection.Add(new StateHolder(value, state));
        }

        private readonly DbAccessLayer _layer;

        internal DbCollection(DbAccessLayer layer, IEnumerable<T> subset)
        {
            _layer = layer;
            _internalCollection = new List<StateHolder>();
            _changeTracker = new Dictionary<T, List<string>>();

            if (subset is IOrderedEnumerable<T>)
            {
                throw new NotImplementedException("This Collection has a Bag behavior and does not support a IOrderedEnumerable");
            }

            foreach (var item in subset)
            {
                Add(item, CollectionStates.Unchanged);
                item.PropertyChanged += item_PropertyChanged;
            }
        }

        void item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var listEntry = new List<string>();
            var trackerEntry = _changeTracker.FirstOrDefault(s => s.Key == sender as T);
            if (trackerEntry.Equals(default(KeyValuePair<T, List<string>>)))
            {
                _changeTracker.Add(sender as T, listEntry);
            }
            else
            {
                listEntry = trackerEntry.Value;
            }
            if (!listEntry.Contains(e.PropertyName))
                listEntry.Add(e.PropertyName);
        }

        private readonly IDictionary<T, List<string>> _changeTracker;

        private readonly List<StateHolder> _internalCollection;

        public IEnumerator<T> GetEnumerator()
        {
            return _internalCollection
                .Where(s => s.State != CollectionStates.Removed)
                .Select(s => s.Value)
                .GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_internalCollection).GetEnumerator();
        }

        public void Add(T item)
        {
            Add(item, CollectionStates.Added);
        }

        public void Clear()
        {
            foreach (var pair in _internalCollection)
            {
                Remove(pair.Value);
            }
        }

        private bool ChangeState(T item, CollectionStates state)
        {
            var fod = _internalCollection.FirstOrDefault(s => s.Value == item);

            if (fod == null)
                return false;

            fod.State = state;
            return true;
        }

        public CollectionStates GetEntryState(T item)
        {
            return _internalCollection.FirstOrDefault(s => s.Value == item).State;
        }

        public bool Contains(T item)
        {
            return _internalCollection.Any(s => s.Value.Equals(item));
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _internalCollection.Select(s => s.Value).ToArray().CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            item.PropertyChanged -= item_PropertyChanged;
            _changeTracker.Remove(item);
            return ChangeState(item, CollectionStates.Removed);
        }

        public int Count
        {
            get { return _internalCollection.Count(s => s.State != CollectionStates.Removed); }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public int IndexOf(T item)
        {
            throw new NotImplementedException("This Collection has a Bag behavior and does not support this Action");
            //return _internalCollection.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            throw new NotImplementedException("This Collection has a Bag behavior and does not support this Action");
            //_internalCollection.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException("This Collection has a Bag behavior and does not support this Action");
            //_internalCollection.RemoveAt(index);
        }

        /// <summary>
        /// Sync the Changes to this Collection to the Database
        /// </summary>
        public void SaveChanges()
        {
            var bulk = _layer.Database.CreateCommand("");

            foreach (var pair in _internalCollection)
            {
                IDbCommand tempCommand;
                switch (pair.State)
                {
                    case CollectionStates.Added:
                        tempCommand = DbAccessLayer.CreateInsertWithSelectCommand(typeof(T), pair.Value, _layer.Database);
                        break;
                    case CollectionStates.Removed:
                        tempCommand = DbAccessLayer.CreateDelete(pair.Value, _layer.Database);
                        break;
                    case CollectionStates.Unchanged:
                        tempCommand = null;
                        break;
                    case CollectionStates.Changed:
                        tempCommand = DbAccessLayer.CreateUpdate(pair.Value, _layer.Database);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                if (tempCommand != null)
                {
                    bulk = DbAccessLayer.MergeCommands(_layer.Database, bulk, tempCommand, true);
                }
            }

            var results = _layer.ExecuteMARS(bulk, typeof(T)).SelectMany(s => s).Cast<T>().ToArray();
            //Added 
            var added = _internalCollection.Where(s => s.State == CollectionStates.Added).ToArray();
            for (int i = 0; i < added.Length; i++)
            {
                var addedOne = added[i];
                var newId = results[i];
                DbAccessLayer.CopyPropertys(addedOne, newId);
            }

            foreach (var collectionStatese in _internalCollection.ToArray())
            {
                this.ChangeState(collectionStatese.Value, CollectionStates.Unchanged);
            }
        }

        public T this[int index]
        {
            get { return _internalCollection.ElementAt(index).Value; }
            set { this.Insert(index, value); }
        }
    }
}
