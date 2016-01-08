using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using JPB.DataAccess.Manager;

namespace JPB.DataAccess.DbCollection
{
	/// <summary>
	///     For internal use only
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class NonObservableDbCollection<T> : IEnumerable<T>
	{
		private readonly List<T> _base;

		/// <summary>
		///     Internal use only
		/// </summary>
		[DebuggerHidden]
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public NonObservableDbCollection(IEnumerable enumerable)
		{
			_base = new List<T>();
			foreach (T item in enumerable)
			{
				_base.Add(item);
			}
		}

		public IEnumerator<T> GetEnumerator()
		{
			return _base.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _base.GetEnumerator();
		}
	}

	/// <summary>
	///     WIP Observes the local collection and allows a Generic save update remove and insert
	/// </summary>
	public class DbCollection<T> : ICollection<T> where T : class, INotifyPropertyChanged
	{
		private readonly IDictionary<T, List<string>> _changeTracker;

		private readonly List<StateHolder> _internalCollection;

		/// <summary>
		///     Internal use only
		/// </summary>
		[DebuggerHidden]
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public DbCollection(IEnumerable subset)
		{
			_internalCollection = new List<StateHolder>();
			_changeTracker = new Dictionary<T, List<string>>();

			if (subset is IOrderedEnumerable<T>)
			{
				throw new NotImplementedException("This Collection has a Bag behavior and does not support a IOrderedEnumerable");
			}

			foreach (T item in subset)
			{
				Add(item, CollectionStates.Unchanged);
				item.PropertyChanged += item_PropertyChanged;
			}
		}

		/// <summary>
		///     Internal use only
		/// </summary>
		[DebuggerHidden]
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public DbCollection(IEnumerable<T> subset)
		{
			_internalCollection = new List<StateHolder>();
			_changeTracker = new Dictionary<T, List<string>>();

			if (subset is IOrderedEnumerable<T>)
			{
				throw new NotImplementedException("This Collection has a Bag behavior and does not support a IOrderedEnumerable");
			}

			foreach (T item in subset)
			{
				Add(item, CollectionStates.Unchanged);
				item.PropertyChanged += item_PropertyChanged;
			}
		}

		/// <summary>
		/// </summary>
		/// <exception cref="NotSupportedException"></exception>
		public T this[int index]
		{
			get { return _internalCollection.ElementAt(index).Value; }
			set
			{
				throw new NotSupportedException("Collection has a Bag behavior and does not support a Set on a specific position");
			}
		}

		public IEnumerator<T> GetEnumerator()
		{
			return _internalCollection
				.Where(s => s.State != CollectionStates.Removed)
				.Select(s => s.Value)
				.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable) _internalCollection).GetEnumerator();
		}

		public void Add(T item)
		{
			Add(item, CollectionStates.Added);
		}

		public void Clear()
		{
			foreach (StateHolder pair in _internalCollection)
			{
				Remove(pair.Value);
			}
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

		private void Add(T value, CollectionStates state)
		{
			_internalCollection.Add(new StateHolder(value, state));
		}

		private void item_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			var listEntry = new List<string>();
			KeyValuePair<T, List<string>> trackerEntry = _changeTracker.FirstOrDefault(s => s.Key == sender as T);
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

		private bool ChangeState(T item, CollectionStates state)
		{
			StateHolder fod = _internalCollection.FirstOrDefault(s => s.Value == item);

			if (fod == null)
				return false;

			fod.State = state;
			return true;
		}

		public CollectionStates GetEntryState(T item)
		{
			return _internalCollection.FirstOrDefault(s => s.Value == item).State;
		}

		/// <summary>
		///     Sync the Changes to this Collection to the Database
		/// </summary>
		public void SaveChanges(DbAccessLayer _layer)
		{
			IDbCommand bulk = _layer.Database.CreateCommand("");
			var removed = new List<T>();

			foreach (StateHolder pair in _internalCollection)
			{
				IDbCommand tempCommand;
				switch (pair.State)
				{
					case CollectionStates.Added:
						tempCommand = DbAccessLayer.CreateInsertWithSelectCommand(typeof (T), pair.Value, _layer.Database);
						break;
					case CollectionStates.Removed:
						tempCommand = DbAccessLayer.CreateDelete(pair.Value, _layer.Database);
						removed.Add(pair.Value);
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
					bulk = _layer.Database.MergeCommands(bulk, tempCommand, true);
				}
			}

			T[] results = _layer.ExecuteMARS(bulk, typeof (T)).SelectMany(s => s).Cast<T>().ToArray();
			//Added 
			StateHolder[] added = _internalCollection.Where(s => s.State == CollectionStates.Added).ToArray();
			for (int i = 0; i < added.Length; i++)
			{
				StateHolder addedOne = added[i];
				T newId = results[i];
				DbAccessLayer.CopyPropertys(addedOne.Value, newId);
			}

			//Removed
			foreach (T item in removed)
			{
				StateHolder fod = _internalCollection.First(s => s.Value == item);
				_internalCollection.Remove(fod);
			}

			foreach (StateHolder collectionStatese in _internalCollection.ToArray())
			{
				ChangeState(collectionStatese.Value, CollectionStates.Unchanged);
			}
		}

		private class StateHolder
		{
			public StateHolder(T value, CollectionStates state)
			{
				Value = value;
				State = state;
			}

			public T Value { get; set; }
			public CollectionStates State { get; set; }
		}
	}

	public enum CollectionStates
	{
		Unchanged,
		Added,
		Changed,
		Removed
	}
}