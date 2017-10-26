#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using JPB.DataAccess.AdoWrapper;
using JPB.DataAccess.DbInfoConfig;
using JPB.DataAccess.Helper;
using JPB.DataAccess.Manager;

#endregion

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
#if !DEBUG
		[DebuggerHidden]
#endif
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

		/// <summary>
		///     Returns an enumerator that iterates through the collection.
		/// </summary>
		/// <returns>
		///     An enumerator that can be used to iterate through the collection.
		/// </returns>
		public IEnumerator<T> GetEnumerator()
		{
			return _base.GetEnumerator();
		}

		/// <summary>
		///     Returns an enumerator that iterates through a collection.
		/// </summary>
		/// <returns>
		///     An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
		/// </returns>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return _base.GetEnumerator();
		}

		/// <summary>
		///     Creates a DbCollection that contains the XML elements
		/// </summary>
		/// <param name="xml">The XML.</param>
		/// <returns></returns>
		public static NonObservableDbCollection<T> FromXml(string xml)
		{
			return new NonObservableDbCollection<T>(
				XmlDataRecord.TryParse(xml,
						typeof(T), false)
					.CreateListOfItems()
					.Select(item => typeof(T)
						.GetClassInfo()
						.SetPropertysViaReflection(item)));
		}
	}

	/// <summary>
	///     WIP Observes the local collection and allows a Generic save update remove and insert
	/// </summary>
	public class DbCollection<T> : ICollection<T> where T : class, INotifyPropertyChanged
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
		{
			_internalCollection = new Dictionary<T, CollectionStates>(new PocoPkComparer<T>());
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
#if !DEBUG
		[DebuggerHidden]
#endif
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public DbCollection(IEnumerable<T> subset)
		{
			_internalCollection = new Dictionary<T, CollectionStates>(new PocoPkComparer<T>());
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
			_internalCollection.Select(s => s.Value).ToArray().CopyTo(array, arrayIndex);
		}

		public bool Remove(T item)
		{
			item.PropertyChanged -= item_PropertyChanged;
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

		private void Add(T value, CollectionStates state)
		{
			value.PropertyChanged += item_PropertyChanged;
			_internalCollection.Add(value, state);
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
		//	public StateHolder(T value, CollectionStates state)
		//{

		//private class StateHolder : IEquatable<StateHolder>
		//	public CollectionStates State { get; set; }

		//	public override int GetHashCode()
		//	{
		//		int hash = 13;
		//		hash = (hash * 7) + Value.GetHashCode();
		//		hash = (hash * 7) + State.GetHashCode();
		//		return hash;
		//	}

		//	public override bool Equals(object obj)
		//	{
		//		return this.Equals((StateHolder)obj);
		//	}

		//	public bool Equals(StateHolder other)
		//	{
		//		if (ReferenceEquals(other, null))
		//			return false;

		//		if (other.Value == null)
		//			return false;

		//		return other.Value.Equals(Value) && State == other.State;
		//	}

		//	public static bool operator ==(StateHolder that, StateHolder other)
		//	{
		//		if (ReferenceEquals(that, null) && ReferenceEquals(other, null))
		//			return true;

		//		if (!ReferenceEquals(that, null))
		//			return false;

		//		return that.Equals(other);
		//	}

		//	public static bool operator !=(StateHolder that, StateHolder other)
		//	{
		//		return !(other == that);
		//	}
		//}
	}

	/// <summary>
	///     All states that an item inside an DbCollection can be
	/// </summary>
	public enum CollectionStates
	{
		/// <summary>
		///     Element request is not in store
		/// </summary>
		Unknown = 0,

		/// <summary>
		///     Object was created from the Database and has not changed
		/// </summary>
		Unchanged,

		/// <summary>
		///     Object from UserCode
		/// </summary>
		Added,

		/// <summary>
		///     Object was created from the database and has changed since then
		/// </summary>
		Changed,

		/// <summary>
		///     Object was created from the database and should be created
		/// </summary>
		Removed
	}
}