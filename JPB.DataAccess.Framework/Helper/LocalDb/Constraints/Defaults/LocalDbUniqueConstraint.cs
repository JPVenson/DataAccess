#region

using System;
using System.Collections.Generic;
using JPB.DataAccess.Framework.Helper.LocalDb.Constraints.Contracts;

#endregion

namespace JPB.DataAccess.Framework.Helper.LocalDb.Constraints.Defaults
{
	/// <summary>
	/// </summary>
	/// <typeparam name="TEntity">The type of the entity.</typeparam>
	/// <typeparam name="TKey">The type of the key.</typeparam>
	/// <seealso cref="ILocalDbUniqueConstraint{TEntity}" />
	public class LocalDbUniqueConstraint<TEntity, TKey> : ILocalDbUniqueConstraint<TEntity>
	{
		/// <summary>
		///     The get key
		/// </summary>
		private readonly Func<TEntity, TKey> _getKey;

		/// <summary>
		///     The index
		/// </summary>
		private readonly HashSet<TKey> _index;

		/// <summary>
		///     The lock root
		/// </summary>
		private readonly object LockRoot;

		/// <summary>
		///     Initializes a new instance of the <see cref="LocalDbUniqueConstraint{TEntity, TKey}" /> class.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="getKey">The get key.</param>
		/// <param name="elementComparer">The element comparer.</param>
		/// <exception cref="ArgumentNullException">
		///     name
		///     or
		///     getKey
		/// </exception>
		public LocalDbUniqueConstraint(
			string name,
			Func<TEntity, TKey> getKey,
			IEqualityComparer<TKey> elementComparer = null)
		{
			LockRoot = new object();
			if (name == null)
			{
				throw new ArgumentNullException(nameof(name));
			}
			if (getKey == null)
			{
				throw new ArgumentNullException(nameof(getKey));
			}
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

		/// <summary>
		///     The name of this Constraint
		/// </summary>
		public string Name { get; private set; }

		/// <summary>
		///     The function that checks if the certain constraint is fulfilled
		/// </summary>
		/// <param name="item"></param>
		/// <returns>
		///     True if success false if failed
		/// </returns>
		public bool CheckConstraint(TEntity item)
		{
			lock (LockRoot)
			{
				if (_index.Contains(_getKey(item)))
				{
					return false;
				}
			}
			return true;
		}

		/// <summary>
		///     Adds the specified item to the Unique Index.
		/// </summary>
		/// <param name="item">The item.</param>
		public void Add(TEntity item)
		{
			lock (LockRoot)
			{
				_index.Add(_getKey(item));
			}
		}

		/// <summary>
		///     Deletes the specified item from the Unique Index.
		/// </summary>
		/// <param name="item">The item.</param>
		public void Delete(TEntity item)
		{
			lock (LockRoot)
			{
				_index.Remove(_getKey(item));
			}
		}

		/// <summary>
		///     Updates the specified item in the Unique Index.
		/// </summary>
		/// <param name="item">The item.</param>
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