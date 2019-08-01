#region

using System;
using System.Collections;
using System.Collections.Generic;
using JPB.DataAccess.Framework.Contacts;
using JPB.DataAccess.Framework.Helper.LocalDb.Constraints.Contracts;
using JPB.DataAccess.Framework.Helper.LocalDb.Constraints.Defaults;

#endregion

namespace JPB.DataAccess.Framework.Helper.LocalDb.Constraints.Collections
{
	/// <summary>
	/// </summary>
	/// <typeparam name="TEntity">The type of the entity.</typeparam>
	/// <seealso cref="ICheckConstraints{TEntity}" />
	public class CheckConstraints<TEntity> : ICheckConstraints<TEntity>
	{
		/// <summary>
		///     The local database repository
		/// </summary>
		private readonly LocalDbRepository<TEntity> _localDbRepository;

		/// <summary>
		///     The constraints
		/// </summary>
		private HashSet<ILocalDbCheckConstraint<TEntity>> _constraints;

		/// <summary>
		///     Initializes a new instance of the <see cref="CheckConstraints{TEntity}" /> class.
		/// </summary>
		/// <param name="localDbRepository">The local database repository.</param>
		public CheckConstraints(LocalDbRepository<TEntity> localDbRepository)
		{
			_constraints = new HashSet<ILocalDbCheckConstraint<TEntity>>(new ConstraintComparer());
			_localDbRepository = localDbRepository;
		}

		/// <summary>
		///     Returns an enumerator that iterates through the collection.
		/// </summary>
		/// <returns>
		///     An enumerator that can be used to iterate through the collection.
		/// </returns>
		public IEnumerator<ILocalDbCheckConstraint<TEntity>> GetEnumerator()
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
		public void Add(ILocalDbCheckConstraint<TEntity> item)
		{
			if (_localDbRepository.ReposetoryCreated)
			{
				throw new InvalidOperationException("Missing Alter or Setup statement of table");
			}
			_constraints.Add(item);
		}

		/// <summary>
		///     Adds the specified Check Constraint.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="item">The item.</param>
		/// <exception cref="InvalidOperationException">Missing Alter or Setup statement of table</exception>
		public void Add(string name, Func<TEntity, bool> item)
		{
			if (_localDbRepository.ReposetoryCreated)
			{
				throw new InvalidOperationException("Missing Alter or Setup statement of table");
			}
			_constraints.Add(new LocalDbCheckConstraint<TEntity>(name, item));
		}

		/// <summary>
		///     Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1" />.
		/// </summary>
		/// <exception cref="InvalidOperationException">Missing Alter or Setup statement of table</exception>
		public void Clear()
		{
			if (_localDbRepository.ReposetoryCreated)
			{
				throw new InvalidOperationException("Missing Alter or Setup statement of table");
			}
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
		public bool Contains(ILocalDbCheckConstraint<TEntity> item)
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
		public void CopyTo(ILocalDbCheckConstraint<TEntity>[] array, int arrayIndex)
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
		public bool Remove(ILocalDbCheckConstraint<TEntity> item)
		{
			if (_localDbRepository.ReposetoryCreated)
			{
				throw new InvalidOperationException("Missing Alter or Setup statement of table");
			}
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
		///     Enforces all Constraints on the specified item.
		/// </summary>
		/// <param name="item">The item.</param>
		/// <exception cref="ConstraintException"></exception>
		public void Enforce(TEntity item)
		{
			foreach (var localDbCheckConstraint in _constraints)
			{
				if (!localDbCheckConstraint.CheckConstraint(item))
				{
					throw new ConstraintException(string.Format("The Check Constraint '{0}' has detected an invalid object",
					localDbCheckConstraint.Name));
				}
			}
		}
	}
}