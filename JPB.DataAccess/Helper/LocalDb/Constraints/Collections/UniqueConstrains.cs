#region

using System;
using System.Collections;
using System.Collections.Generic;
using JPB.DataAccess.Contacts;
using JPB.DataAccess.Helper.LocalDb.Constraints.Contracts;
using JPB.DataAccess.Helper.LocalDb.Constraints.Defaults;

#endregion

namespace JPB.DataAccess.Helper.LocalDb.Constraints.Collections
{
	/// <summary>
	/// </summary>
	/// <seealso cref="ILocalDbConstraint" />
	public class ConstraintComparer : IEqualityComparer<ILocalDbConstraint>
	{
		/// <summary>
		///     Determines whether the specified objects are equal.
		/// </summary>
		/// <param name="x">The first object of type <see cref="ILocalDbConstraint" /> to compare.</param>
		/// <param name="y">The second object of type <see cref="ILocalDbConstraint" /> to compare.</param>
		/// <returns>
		///     true if the specified objects are equal; otherwise, false.
		/// </returns>
		public bool Equals(ILocalDbConstraint x, ILocalDbConstraint y)
		{
			return x.Name == y.Name;
		}

		/// <summary>
		///     Returns a hash code for this instance.
		/// </summary>
		/// <param name="obj">The object.</param>
		/// <returns>
		///     A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
		/// </returns>
		public int GetHashCode(ILocalDbConstraint obj)
		{
			return obj.Name.GetHashCode();
		}
	}

	/// <summary>
	/// </summary>
	/// <typeparam name="TEntity">The type of the entity.</typeparam>
	/// <seealso cref="JPB.DataAccess.Helper.LocalDb.Constraints.Collections.IUniqueConstrains{TEntity}" />
	public class UniqueConstrains<TEntity> : IUniqueConstrains<TEntity>
	{
		/// <summary>
		///     The local database repository
		/// </summary>
		private readonly LocalDbRepository<TEntity> _localDbRepository;

		/// <summary>
		///     The constraints
		/// </summary>
		private HashSet<ILocalDbUniqueConstraint<TEntity>> _constraints;

		/// <summary>
		///     Initializes a new instance of the <see cref="UniqueConstrains{TEntity}" /> class.
		/// </summary>
		/// <param name="localDbRepository">The local database repository.</param>
		public UniqueConstrains(LocalDbRepository<TEntity> localDbRepository)
		{
			_constraints = new HashSet<ILocalDbUniqueConstraint<TEntity>>();
			_localDbRepository = localDbRepository;
		}

		/// <summary>
		///     Returns an enumerator that iterates through the collection.
		/// </summary>
		/// <returns>
		///     An enumerator that can be used to iterate through the collection.
		/// </returns>
		public IEnumerator<ILocalDbUniqueConstraint<TEntity>> GetEnumerator()
		{
			return _constraints.GetEnumerator();
		}

		/// <summary>
		///     Returns an enumerator that iterates through a collection.
		/// </summary>
		/// <returns>
		///     An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
		/// </returns>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable) _constraints).GetEnumerator();
		}

		/// <summary>
		///     Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1" />.
		/// </summary>
		/// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
		/// <exception cref="InvalidOperationException">Missing Alter or Setup statement of table</exception>
		public void Add(ILocalDbUniqueConstraint<TEntity> item)
		{
			if (_localDbRepository.ReposetoryCreated)
				throw new InvalidOperationException("Missing Alter or Setup statement of table");
			_constraints.Add(item);
		}

		/// <summary>
		///     Adds the specified Unique key generator.
		/// </summary>
		/// <typeparam name="TValue">The type of the value.</typeparam>
		/// <param name="name">The name.</param>
		/// <param name="item">The item.</param>
		/// <exception cref="InvalidOperationException">Missing Alter or Setup statement of table</exception>
		public void Add<TValue>(string name, Func<TEntity, TValue> item)
		{
			if (_localDbRepository.ReposetoryCreated)
				throw new InvalidOperationException("Missing Alter or Setup statement of table");
			_constraints.Add(new LocalDbUniqueConstraint<TEntity, TValue>(name, item));
		}

		/// <summary>
		///     Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1" />.
		/// </summary>
		/// <exception cref="InvalidOperationException">Missing Alter or Setup statement of table</exception>
		public void Clear()
		{
			if (_localDbRepository.ReposetoryCreated)
				throw new InvalidOperationException("Missing Alter or Setup statement of table");
			_constraints.Clear();
		}

		/// <summary>
		///     Determines whether the <see cref="T:System.Collections.Generic.ICollection`1" /> contains a specific value.
		/// </summary>
		/// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
		/// <returns>
		///     true if <paramref name="item" /> is found in the <see cref="T:System.Collections.Generic.ICollection`1" />;
		///     otherwise, false.
		/// </returns>
		public bool Contains(ILocalDbUniqueConstraint<TEntity> item)
		{
			return _constraints.Contains(item);
		}

		/// <summary>
		///     Copies the elements of the <see cref="T:System.Collections.Generic.ICollection`1" /> to an
		///     <see cref="T:System.Array" />, starting at a particular <see cref="T:System.Array" /> index.
		/// </summary>
		/// <param name="array">
		///     The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied
		///     from <see cref="T:System.Collections.Generic.ICollection`1" />. The <see cref="T:System.Array" /> must have
		///     zero-based indexing.
		/// </param>
		/// <param name="arrayIndex">The zero-based index in <paramref name="array" /> at which copying begins.</param>
		public void CopyTo(ILocalDbUniqueConstraint<TEntity>[] array, int arrayIndex)
		{
			_constraints.CopyTo(array, arrayIndex);
		}

		/// <summary>
		///     Removes the first occurrence of a specific object from the
		///     <see cref="T:System.Collections.Generic.ICollection`1" />.
		/// </summary>
		/// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
		/// <returns>
		///     true if <paramref name="item" /> was successfully removed from the
		///     <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false. This method also returns false if
		///     <paramref name="item" /> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1" />.
		/// </returns>
		/// <exception cref="InvalidOperationException">Missing Alter or Setup statement of table</exception>
		public bool Remove(ILocalDbUniqueConstraint<TEntity> item)
		{
			if (_localDbRepository.ReposetoryCreated)
				throw new InvalidOperationException("Missing Alter or Setup statement of table");
			return _constraints.Remove(item);
		}

		/// <summary>
		///     Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.
		/// </summary>
		public int Count
		{
			get { return _constraints.Count; }
		}

		/// <summary>
		///     Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.
		/// </summary>
		public bool IsReadOnly
		{
			get { return false; }
		}

		/// <summary>
		///     Add this item to the Unique constraint.
		/// </summary>
		/// <param name="item">The item.</param>
		public void ItemAdded(TEntity item)
		{
			foreach (var localDbUniqueConstraint in _constraints)
				localDbUniqueConstraint.Add(item);
		}

		/// <summary>
		///     Remove this item from the Unique constraint
		/// </summary>
		/// <param name="item">The item.</param>
		public void ItemRemoved(TEntity item)
		{
			foreach (var localDbUniqueConstraint in _constraints)
				localDbUniqueConstraint.Delete(item);
		}

		/// <summary>
		///     Update this item in the Unique constraint
		/// </summary>
		/// <param name="item">The item.</param>
		public void ItemUpdated(TEntity item)
		{
			foreach (var localDbUniqueConstraint in _constraints)
				localDbUniqueConstraint.Update(item);
		}

		/// <summary>
		///     Gets the initializer for this Constraint.
		///     It can be used to store the internal state
		/// </summary>
		/// <returns></returns>
		public object GetInitializer()
		{
			return _constraints;
		}

		/// <summary>
		///     Gets the initializer for this Constraint.
		///     It can be used to restore the internal state
		/// </summary>
		/// <returns></returns>
		public void SetInitializer(object initializerValue)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		///     Enforces the specified item.
		/// </summary>
		/// <param name="item">The item.</param>
		/// <exception cref="ConstraintException"></exception>
		public void Enforce(TEntity item)
		{
			foreach (var localDbUniqueConstraint in _constraints)
				if (!localDbUniqueConstraint.CheckConstraint(item))
					throw new ConstraintException(string.Format("The Unique Constraint '{0}' has detected an invalid object",
						localDbUniqueConstraint.Name));
		}
	}
}