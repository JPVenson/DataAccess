using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JPB.DataAccess.DbInfoConfig;
using JPB.DataAccess.DbInfoConfig.DbInfo;
using JPB.DataAccess.Manager;

namespace JPB.DataAccess.Helper
{
	public class LocalDbReposetory<T>
		: ICollection<T> where T : class
	{
		// ReSharper disable StaticFieldInGenericType
		private static readonly object LockRoot = new object();
		private static readonly DbClassInfoCache TypeInfo;
		internal static long IdCounter;
		private readonly DbAccessLayer _db;
		private readonly Dictionary<long, T> _base;
		// ReSharper restore StaticFieldInGenericType

		static LocalDbReposetory()
		{
			TypeInfo = new DbConfig().GetOrCreateClassInfoCache(typeof(T));
		}

		public LocalDbReposetory()
		{
			_base = new Dictionary<long, T>();
			IdCounter = 0;
		}

		public LocalDbReposetory(DbAccessLayer db)
		{
			_db = db;
			_base = new Dictionary<long, T>();
			IdCounter = 0;
		}

		public IEnumerator<T> GetEnumerator()
		{
			if (_db != null)
				return _db.Select<T>().Cast<T>().GetEnumerator();

			return _base.Values.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		private static long SetId(object item)
		{
			lock (LockRoot)
			{
				var newId = IdCounter++;
				TypeInfo.PrimaryKeyProperty.Setter.Invoke(item, newId);
				return newId;
			}
		}

		private static long GetId(object item)
		{
			return (long)TypeInfo.PrimaryKeyProperty.Getter.Invoke(item);
		}

		public void Add(T item)
		{
			if (_db != null)
			{
				_db.Insert(item);
			}
			else
			{
				if (!Contains(item))
				{
					_base.Add(SetId(item), item);
				}
			}
		}

		public void Clear()
		{
			_base.Clear();
		}

		public bool Contains(T item)
		{
			var local = _base.ContainsValue(item);
			if (!local && _db != null)
			{
				var pk = GetId(item);
				return _db.Select<T>(pk) != null;
			}

			return local;
		}

		public bool Contains(long item)
		{
			var local = _base.ContainsKey(item);
			if (!local && _db != null)
			{
				return _db.Select<T>(item) != null;
			}
			return local;
		}

		public void CopyTo(T[] array, int arrayIndex)
		{
			throw new NotImplementedException();
		}

		public bool Remove(T item)
		{
			var id = GetId(item);
			var success = _base.Remove(id);
			if (!success)
			{
				_db.Delete(item);
				success = true;
			}
			return success;
		}

		public int Count
		{
			get { return _base.Count; }
		}

		public bool IsReadOnly { get { return false; } }
	}
}
