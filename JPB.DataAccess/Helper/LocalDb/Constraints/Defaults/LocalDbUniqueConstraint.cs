using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using JPB.DataAccess.DbInfoConfig;
using JPB.DataAccess.DbInfoConfig.DbInfo;
using JPB.DataAccess.Helper.LocalDb.Constraints.Contracts;

namespace JPB.DataAccess.Helper.LocalDb.Constraints.Defaults
{
	public class LocalDbUniqueConstraint<TEntity, TKey> : ILocalDbUniqueConstraint<TEntity>
	{
		private readonly Func<TEntity, TKey> _getKey;

		public LocalDbUniqueConstraint(
			string name,
			Func<TEntity, TKey> getKey,
			IEqualityComparer<TKey> elementComparer = null)
		{
			LockRoot = new object();
			if (name == null) throw new ArgumentNullException("name");
			if (getKey == null) throw new ArgumentNullException("getKey");
			Name = name;
			_getKey = getKey;

			if (elementComparer != null)
			{
				_index = new HashSet<TKey>(elementComparer);
			}
			else
			{
				_index = new HashSet<TKey>();
			}
		}

		private readonly HashSet<TKey> _index;
		private readonly object LockRoot;

		public string Name { get; private set; }
		public bool CheckConstraint(TEntity item)
		{
			lock (LockRoot)
			{
				if (_index.Contains(_getKey(item)))
					return false;
			}
			return true;
		}

		public void Add(TEntity item)
		{
			lock (LockRoot)
			{
				_index.Add(_getKey(item));
			}
		}

		public void Delete(TEntity item)
		{
			lock (LockRoot)
			{
				_index.Remove(_getKey(item));
			}
		}

		public void Update(TEntity item)
		{
			lock (LockRoot)
			{
				Delete(item);
				Add(item);
			}
		}
	}
}