using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using JPB.DataAccess.DbInfoConfig.DbInfo;
using JPB.DataAccess.Helper.LocalDb.Constraints.Defaults;

namespace JPB.DataAccess.Helper.LocalDb.Index
{
	/// <summary>
	/// Simple Index for Indexed searches
	/// </summary>
	/// <typeparam name="TEntity"></typeparam>
	/// <typeparam name="TKey"></typeparam>
	public class DbIndex<TEntity, TKey> : IDbIndex<TEntity>
	{
		private readonly IDictionary<TKey, TEntity> _index;

		/// <summary>
		///     The lock root
		/// </summary>
		private readonly object _lockRoot;

		private readonly DbPropertyInfoCache _indexer;

		/// <summary>
		///     Initializes a new instance of the <see cref="LocalDbUniqueConstraint{TEntity,TKey}" /> class.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="column">The index Column.</param>
		/// <param name="elementComparer">The element comparer.</param>
		/// <exception cref="ArgumentNullException">
		///     name
		///     or
		///     getKey
		/// </exception>
		public DbIndex(
			string name,
			DbPropertyInfoCache column,
			IEqualityComparer<TKey> elementComparer = null)
		{
			_lockRoot = new object();
			if (name == null)
			{
				throw new ArgumentNullException(nameof(name));
			}
			Name = name;
			_indexer = column;
			if (elementComparer != null)
			{
				_index = new ConcurrentDictionary<TKey, TEntity>(elementComparer);
			}
			else
			{
				_index = new ConcurrentDictionary<TKey, TEntity>();
			}
		}

		/// <summary>
		///     The name of this Constraint
		/// </summary>
		public string Name { get; private set; }

		private TKey GetKey(TEntity item)
		{
			var invoke = _indexer.Getter.Invoke(item);
			return (TKey)invoke;
		}

		/// <summary>
		///     Adds the specified item to the Unique Index.
		/// </summary>
		/// <param name="item">The item.</param>
		public void Add(TEntity item)
		{
			lock (_lockRoot)
			{
				_index.Add(GetKey(item), item);
			}
		}

		/// <summary>
		///     Deletes the specified item from the Unique Index.
		/// </summary>
		/// <param name="item">The item.</param>
		public void Delete(TEntity item)
		{
			lock (_lockRoot)
			{
				_index.Remove(GetKey(item));
			}
		}

		/// <summary>
		///     Updates the specified item in the Unique Index.
		/// </summary>
		/// <param name="item">The item.</param>
		public void Update(TEntity item)
		{
			lock (_lockRoot)
			{
				Delete(item);
				Add(item);
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		/// <inheritdoc />
		public IEnumerator<TEntity> GetEnumerator()
		{
			lock (_lockRoot)
			{
				return _index.Select(f => f.Value).GetEnumerator();
			}
		}
	}
}
