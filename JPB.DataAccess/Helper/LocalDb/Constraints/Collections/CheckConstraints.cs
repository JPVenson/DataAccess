using System;
using System.Collections;
using System.Collections.Generic;
using JPB.DataAccess.Contacts;

namespace JPB.DataAccess.Helper.LocalDb.Constraints.Collections
{
	public class CheckConstraints<TEntity> : ICheckConstraints<TEntity>
	{
		private readonly LocalDbReposetory<TEntity> _localDbReposetory;

		private HashSet<ILocalDbCheckConstraint<TEntity>> _constraints;

		public CheckConstraints(LocalDbReposetory<TEntity> localDbReposetory)
		{
			_constraints = new HashSet<ILocalDbCheckConstraint<TEntity>>(new ConstraintComparer());
			_localDbReposetory = localDbReposetory;
		}

		public IEnumerator<ILocalDbCheckConstraint<TEntity>> GetEnumerator()
		{
			return _constraints.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable) _constraints).GetEnumerator();
		}

		public void Add(ILocalDbCheckConstraint<TEntity> item)
		{
			if (_localDbReposetory.ReposetoryCreated)
				throw new InvalidOperationException("Missing Alter or Setup statement of table");
			_constraints.Add(item);
		}

		public void Clear()
		{
			if (_localDbReposetory.ReposetoryCreated)
				throw new InvalidOperationException("Missing Alter or Setup statement of table");
			_constraints.Clear();
		}

		public bool Contains(ILocalDbCheckConstraint<TEntity> item)
		{
			return _constraints.Contains(item);
		}

		public void CopyTo(ILocalDbCheckConstraint<TEntity>[] array, int arrayIndex)
		{
			_constraints.CopyTo(array, arrayIndex);
		}

		public bool Remove(ILocalDbCheckConstraint<TEntity> item)
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

		public void Enforce(TEntity item)
		{
			foreach (var localDbCheckConstraint in _constraints)
			{
				if(!localDbCheckConstraint.CheckConstraint(item))
					throw new ConstraintException(string.Format("The Check Constraint '{0}' has detected an invalid object", localDbCheckConstraint.Name));
			}
		}
	}
}