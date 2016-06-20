using System;
using System.Collections;
using System.Collections.Generic;
using JPB.DataAccess.Contacts;
using JPB.DataAccess.Helper.LocalDb.Constraints.Contracts;
using JPB.DataAccess.Helper.LocalDb.Constraints.Defaults;

namespace JPB.DataAccess.Helper.LocalDb.Constraints.Collections
{
	public class ConstraintComparer : IEqualityComparer<ILocalDbConstraint>
	{
		public bool Equals(ILocalDbConstraint x, ILocalDbConstraint y)
		{
			return x.Name == y.Name;
		}

		public int GetHashCode(ILocalDbConstraint obj)
		{
			return obj.Name.GetHashCode();
		}
	}

	public class UniqueConstrains<TEntity> : IUniqueConstrains<TEntity>
	{
		private readonly LocalDbReposetory<TEntity> _localDbReposetory;
		private HashSet<ILocalDbUniqueConstraint<TEntity>> _constraints;

		public UniqueConstrains(LocalDbReposetory<TEntity> localDbReposetory)
		{
			_constraints = new HashSet<ILocalDbUniqueConstraint<TEntity>>();
			_localDbReposetory = localDbReposetory;
		}

		public IEnumerator<ILocalDbUniqueConstraint<TEntity>> GetEnumerator()
		{
			return _constraints.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable)_constraints).GetEnumerator();
		}

		public void Add(ILocalDbUniqueConstraint<TEntity> item)
		{
			if (_localDbReposetory.ReposetoryCreated)
				throw new InvalidOperationException("Missing Alter or Setup statement of table");
			_constraints.Add(item);
		}

		public void Add<TValue>(string name, Func<TEntity, TValue> item)
		{
			if (_localDbReposetory.ReposetoryCreated)
				throw new InvalidOperationException("Missing Alter or Setup statement of table");
			_constraints.Add(new LocalDbUniqueConstraint<TEntity, TValue>(name, item));
		}

		public void Clear()
		{
			if (_localDbReposetory.ReposetoryCreated)
				throw new InvalidOperationException("Missing Alter or Setup statement of table");
			_constraints.Clear();
		}

		public bool Contains(ILocalDbUniqueConstraint<TEntity> item)
		{
			return _constraints.Contains(item);
		}

		public void CopyTo(ILocalDbUniqueConstraint<TEntity>[] array, int arrayIndex)
		{
			_constraints.CopyTo(array, arrayIndex);
		}

		public bool Remove(ILocalDbUniqueConstraint<TEntity> item)
		{
			if (_localDbReposetory.ReposetoryCreated)
				throw new InvalidOperationException("Missing Alter or Setup statement of table");
			return _constraints.Remove(item);
		}

		public int Count
		{
			get { return _constraints.Count; }
		}

		public bool IsReadOnly
		{
			get { return false; }
		}

		public void ItemAdded(TEntity item)
		{
			foreach (var localDbUniqueConstraint in _constraints)
			{
				localDbUniqueConstraint.Add(item);
			}
		}

		public void ItemRemoved(TEntity item)
		{
			foreach (var localDbUniqueConstraint in _constraints)
			{
				localDbUniqueConstraint.Delete(item);
			}
		}

		public void ItemUpdated(TEntity item)
		{
			foreach (var localDbUniqueConstraint in _constraints)
			{
				localDbUniqueConstraint.Update(item);
			}
		}
		
		public void Enforce(TEntity item)
		{
			foreach (var localDbUniqueConstraint in _constraints)
			{
				if (!localDbUniqueConstraint.CheckConstraint(item))
					throw new ConstraintException(string.Format("The Unique Constraint '{0}' has detected an invalid object", localDbUniqueConstraint.Name));
			}
		}
	}
}