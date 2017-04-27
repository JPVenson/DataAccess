#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using JPB.DataAccess.DbInfoConfig;
using JPB.DataAccess.Helper.LocalDb.Constraints.Contracts;
using JPB.DataAccess.Helper.LocalDb.Constraints.Defaults;

#endregion

namespace JPB.DataAccess.Helper.LocalDb.Constraints.Collections
{
	/// <summary>
	/// </summary>
	/// <typeparam name="TEntity">The type of the entity.</typeparam>
	/// <seealso cref="JPB.DataAccess.Helper.LocalDb.Constraints.Collections.IDefaultConstraints{TEntity}" />
	public class DefaultConstraints<TEntity> : IDefaultConstraints<TEntity>
	{
		/// <summary>
		///     The local database repository
		/// </summary>
		private readonly LocalDbRepository<TEntity> _localDbRepository;

		/// <summary>
		///     The constraints
		/// </summary>
		private HashSet<ILocalDbDefaultConstraint<TEntity>> _constraints;

		/// <summary>
		///     Initializes a new instance of the <see cref="DefaultConstraints{TEntity}" /> class.
		/// </summary>
		/// <param name="localDbRepository">The local database repository.</param>
		public DefaultConstraints(LocalDbRepository<TEntity> localDbRepository)
		{
			_constraints = new HashSet<ILocalDbDefaultConstraint<TEntity>>(new ConstraintComparer());
			_localDbRepository = localDbRepository;
		}

		/// <summary>
		///     Returns an enumerator that iterates through the collection.
		/// </summary>
		/// <returns>
		///     An enumerator that can be used to iterate through the collection.
		/// </returns>
		public IEnumerator<ILocalDbDefaultConstraint<TEntity>> GetEnumerator()
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
		public void Add(ILocalDbDefaultConstraint<TEntity> item)
		{
			if (_localDbRepository.ReposetoryCreated)
				throw new InvalidOperationException("Missing Alter or Setup statement of table");
			_constraints.Add(item);
		}

		/// <summary>
		///     Adds the specified name.
		/// </summary>
		/// <typeparam name="TValue">The type of the value.</typeparam>
		/// <param name="name">The name.</param>
		/// <param name="value">The value.</param>
		/// <param name="setter">The setter.</param>
		/// <exception cref="InvalidOperationException">Missing Alter or Setup statement of table</exception>
		public void Add<TValue>(string name, TValue value, Action<TEntity, TValue> setter)
		{
			if (_localDbRepository.ReposetoryCreated)
				throw new InvalidOperationException("Missing Alter or Setup statement of table");
			_constraints.Add(new LocalDbDefaultConstraint<TEntity, TValue>(name, value, setter));
		}

		/// <summary>
		///     Adds the specified configuration.
		/// </summary>
		/// <typeparam name="TValue">The type of the value.</typeparam>
		/// <param name="config">The configuration.</param>
		/// <param name="name">The name.</param>
		/// <param name="generateValue">The generate value.</param>
		/// <param name="column">The column.</param>
		/// <exception cref="InvalidOperationException">Missing Alter or Setup statement of table</exception>
		public void Add<TValue>(DbConfig config, string name, Func<TValue> generateValue,
			Expression<Func<TEntity, TValue>> column)
		{
			if (_localDbRepository.ReposetoryCreated)
				throw new InvalidOperationException("Missing Alter or Setup statement of table");
			_constraints.Add(new LocalDbDefaultConstraintEx<TEntity, TValue>(config, name, generateValue, column));
		}

		/// <summary>
		///     Adds the specified name.
		/// </summary>
		/// <typeparam name="TValue">The type of the value.</typeparam>
		/// <param name="name">The name.</param>
		/// <param name="generateValue">The generate value.</param>
		/// <param name="column">The column.</param>
		/// <exception cref="InvalidOperationException">Missing Alter or Setup statement of table</exception>
		public void Add<TValue>(string name, Func<TValue> generateValue, Expression<Func<TEntity, TValue>> column)
		{
			if (_localDbRepository.ReposetoryCreated)
				throw new InvalidOperationException("Missing Alter or Setup statement of table");
			_constraints.Add(new LocalDbDefaultConstraintEx<TEntity, TValue>(_localDbRepository.Config, name, generateValue,
				column));
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
		public bool Contains(ILocalDbDefaultConstraint<TEntity> item)
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
		public void CopyTo(ILocalDbDefaultConstraint<TEntity>[] array, int arrayIndex)
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
		public bool Remove(ILocalDbDefaultConstraint<TEntity> item)
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
		///     Enforces all Contrains on the element
		/// </summary>
		/// <param name="elementToAdd">The element to add.</param>
		public void Enforce(TEntity elementToAdd)
		{
			foreach (var localDbDefaultConstraint in _constraints)
				localDbDefaultConstraint.DefaultValue(elementToAdd);
		}
	}
}