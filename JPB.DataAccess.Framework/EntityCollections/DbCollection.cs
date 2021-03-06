﻿#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using JPB.DataAccess.Helper;
using JPB.DataAccess.Manager;

#endregion

namespace JPB.DataAccess.EntityCollections
{
	internal interface IDbCollection
	{

	}

	/// <summary>
	///     WIP Observes the local collection and allows a Generic save update remove and insert
	/// </summary>
	public class DbCollection<T> : ICollection<T>, IDbCollection where T : class
	{
		private readonly IDictionary<T, List<string>> _changeTracker;

		private readonly Dictionary<T, CollectionStates> _internalCollection;

		/// <summary>
		///     Internal use only
		/// </summary>
#if !DEBUG
		[DebuggerHidden]
#endif
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public DbCollection(IEnumerable subset) 
			: this(subset.OfType<T>())
		{

		}

		/// <summary>
		///		You can include this Property in your Join(expression) expression to join all
		///	items of this foreign key together.
		///		Calling this Property from code will throw an exception.
		/// </summary>
		public T Type
		{
			get
			{
				throw new Exception(
					"This Property is not intended to be called from your code. " +
					"It should only be used in a Join Statement");
			}
		}

		/// <summary>
		///     Internal use only
		/// </summary>
#if !DEBUG
		[DebuggerHidden]
#endif
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public DbCollection(IEnumerable<T> subset) : this()
		{
			foreach (var item in subset)
			{
				Add(item, CollectionStates.Unchanged);

				if (item is INotifyPropertyChanged notifiableItem)
				{
					notifiableItem.PropertyChanged += item_PropertyChanged;
				}
			}
		}

		public DbCollection()
		{
			_internalCollection = new Dictionary<T, CollectionStates>(new PocoPkComparer<T>());
			_changeTracker = new Dictionary<T, List<string>>();
		}

		/// <summary>
		/// </summary>
		/// <exception cref="NotSupportedException"></exception>
		public T this[int index]
		{
			get { return _internalCollection.ElementAt(index).Key; }
		}

		private bool ChangeState(T item, CollectionStates state)
		{
			var fod = _internalCollection.ContainsKey(item);

			if (!fod)
			{
				return false;
			}

			_internalCollection[item] = state;
			return true;
		}

		/// <summary>
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public CollectionStates GetEntryState(T item)
		{
			CollectionStates entry;

			if (!_internalCollection.TryGetValue(item, out entry))
			{
				return CollectionStates.Unknown;
			}
			return entry;
		}

		/// <summary>
		///     Sync the Changes to this Collection to the Database
		/// </summary>
		public void SaveChanges(DbAccessLayer layer)
		{
			var bulk = layer.Database.CreateCommand("");
			var removed = new List<T>();

			foreach (var pair in _internalCollection)
			{
				IDbCommand tempCommand;
				switch (pair.Value)
				{
					case CollectionStates.Added:
						tempCommand = layer.CreateInsertWithSelectCommand(typeof(T), pair.Key);
						break;
					case CollectionStates.Removed:
						tempCommand = DbAccessLayer.CreateDelete(layer.Database, layer.GetClassInfo(typeof(T)), pair.Key);
						removed.Add(pair.Key);
						break;
					case CollectionStates.Unchanged:
						tempCommand = null;
						break;
					case CollectionStates.Changed:
						tempCommand = layer.CreateUpdate(pair.Key, layer.Database);
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}

				if (tempCommand != null)
				{
					bulk = layer.Database.MergeCommands(bulk, tempCommand, true);
				}
			}

			var results = layer.ExecuteMARS(bulk, typeof(T)).SelectMany(s => s).Cast<T>().ToArray();
			//Added
			var added = _internalCollection.Where(s => s.Value == CollectionStates.Added).ToArray();
			for (var i = 0; i < added.Length; i++)
			{
				var addedOne = added[i];
				var newId = results[i];
				DataConverterExtensions.CopyPropertys(addedOne.Value, newId, layer.Config);
			}

			//Removed
			foreach (var item in removed)
			{
				_internalCollection.Remove(item);
			}

			foreach (var collectionStatese in _internalCollection.Keys.ToArray())
			{
				ChangeState(collectionStatese, CollectionStates.Unchanged);
			}
		}

		//	public T Value { get; set; }
		//	}
		//		State = state;
		//		Value = value;
		//	{

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
		public IEnumerator<T> GetEnumerator()
		{
			return _internalCollection
				.Where(s => s.Value != CollectionStates.Removed)
				.Select(s => s.Key)
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
			foreach (var pair in _internalCollection.ToArray())
			{
				Remove(pair.Key);
			}
		}

		public bool Contains(T item)
		{
			return _internalCollection.ContainsKey(item);
		}

		public void CopyTo(T[] array, int arrayIndex)
		{
			_internalCollection.Select(s => s.Key).ToArray().CopyTo(array, arrayIndex);

		}

		public bool Remove(T item)
		{
			if (item is INotifyPropertyChanged notifiableItem)
			{
				notifiableItem.PropertyChanged -= item_PropertyChanged;
			}
			var currentState = GetEntryState(item);

			if (currentState == CollectionStates.Added)
			{
				_changeTracker.Remove(item);
				return _internalCollection.Remove(item);
			}
			_changeTracker.Remove(item);
			return ChangeState(item, CollectionStates.Removed);
		}

		public int Count
		{
			get { return _internalCollection.Count(s => s.Value != CollectionStates.Removed); }
		}

		public bool IsReadOnly
		{
			get { return false; }
		}

		private void Add(T item, CollectionStates state)
		{
			if (item is INotifyPropertyChanged notifiableItem)
			{
				notifiableItem.PropertyChanged += item_PropertyChanged;
			}
			_internalCollection.Add(item, state);
		}

		private void item_PropertyChanged(object sender, PropertyChangedEventArgs e)
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
			{
				listEntry.Add(e.PropertyName);
			}
			if (GetEntryState(sender as T) == CollectionStates.Unchanged)
			{
				ChangeState(sender as T, CollectionStates.Changed);
			}
		}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
	}
}