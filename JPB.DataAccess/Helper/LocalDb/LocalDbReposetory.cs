//using System;
//using System.CodeDom;
//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using JPB.DataAccess.Contacts;
//using JPB.DataAccess.DbInfoConfig;
//using JPB.DataAccess.Helper.LocalDb.Constraints.Collections;
//using JPB.DataAccess.Helper.LocalDb.Trigger;
//using JPB.DataAccess.Manager;

//namespace JPB.DataAccess.Helper.LocalDb
//{
//	/// <summary>
//	/// Provides an wrapper for the non Generic LocalDbReposetory 
//	/// Thread Save as on every call to the enumerator a new collection is created
//	/// </summary>
//	/// <typeparam name="T"></typeparam>
//	public class LocalDbReposetory<T> : LocalDbReposetoryBase<T>, ICollection<T>
//	{
//		/// <summary>
//		/// Creates a new LocalDB Repro by using <typeparamref name="T"/>
//		/// </summary>
//		public LocalDbReposetory(DbConfig config)
//			: base(typeof(T), null, config, true)
//		{
//		}
//		/// <summary>
//		/// Creates a new LocalDB Repro by using <typeparamref name="T"/> that uses the DbAccessLayer as fallback if the requested item was not found localy
//		/// </summary>
//		public LocalDbReposetory(DbAccessLayer db)
//			: base(db, typeof(T))
//		{
//		}
//		/// <summary>
//		/// Creates a new LocalDB Repro by using <typeparamref name="T"/> and uses the KeyProvider to generate Primarykeys
//		/// </summary>
//		public LocalDbReposetory(DbConfig config, ILocalDbPrimaryKeyConstraint keyProvider)
//			: base(typeof(T), keyProvider, config, true)
//		{
//		}

//		/// <summary>
//		/// Creates a new LocalDB Repro by using <typeparamref name="T"/>
//		/// </summary>
//		/// <param name="config"></param>
//		/// <param name="useOrignalObjectInMemory">If set to true the original object is used otherwise a copy will be created</param>
//		/// <param name="keyGenerator"></param>
//		public LocalDbReposetory(DbConfig config, bool useOrignalObjectInMemory, ILocalDbPrimaryKeyConstraint keyGenerator)
//			: base()
//		{
//			base.Init(typeof(T), keyGenerator, config, useOrignalObjectInMemory, new TriggerForTableCollection<T>(this));
//		}

//		public new TriggerForTableCollection<T> Triggers
//		{
//			get { return (TriggerForTableCollection<T>)base.Triggers; }
//		}

//		//public new ConstraintCollection<T> Constraints
//		//{
//		//	get { return (ConstraintCollection<T>)base.Constraints; }
//		//}

//		/// <summary>
//		/// Adds a new Item to the Table
//		/// </summary>
//		/// <param name="item"></param>
//		public void Add(T item)
//		{
//			base.Add(item);
//		}

//		/// <summary>
//		/// Checks if the item ref is ether localy stored or on database
//		/// </summary>
//		/// <param name="item"></param>
//		/// <returns></returns>
//		public bool Contains(T item)
//		{
//			return base.Contains(item);
//		}

//		/// <summary>
//		/// Thread save
//		/// </summary>
//		/// <returns></returns>
//		public new T[] ToArray()
//		{
//			lock (SyncRoot)
//			{
//				return Base.Values.Cast<T>().ToArray();
//			}
//		}

//		/// <summary>
//		/// Copys the current collection the an Array
//		/// </summary>
//		/// <param name="array"></param>
//		/// <param name="arrayIndex"></param>
//		public void CopyTo(T[] array, int arrayIndex)
//		{
//			base.CopyTo(array, arrayIndex);
//		}

//		public bool Update(T item)
//		{
//			return base.Update(item);
//		}

//		/// <summary>
//		/// Removes the item from this collection
//		/// </summary>
//		/// <param name="item"></param>
//		/// <returns></returns>
//		public bool Remove(T item)
//		{
//			return base.Remove(item);
//		}

//		/// <summary>
//		/// Returns an object with the given Primarykey
//		/// </summary>
//		/// <param name="primaryKey"></param>
//		/// <returns></returns>
//		public new T this[object primaryKey]
//		{
//			get { return (T)base[primaryKey]; }
//		}

//		/// <summary>
//		/// Gets an enumerator
//		/// </summary>
//		/// <returns></returns>
//		public new IEnumerator<T> GetEnumerator()
//		{
//			return Base.Values.Cast<T>().GetEnumerator();
//		}

//		public Contacts.Pager.IDataPager<T> CreatePager()
//		{
//			return new LocalDataPager<T>(this);
//		}
//	}
//}
