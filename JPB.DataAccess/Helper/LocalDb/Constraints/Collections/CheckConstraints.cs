using System;
using System.Collections;
using System.Collections.Generic;
using JPB.DataAccess.Contacts;
using JPB.DataAccess.Helper.LocalDb.Constraints.Defaults;

namespace JPB.DataAccess.Helper.LocalDb.Constraints.Collections
{
	public class CheckConstraints<TEntity> : ICheckConstraints<TEntity>
	{
		private readonly LocalDbRepository<TEntity> _localDbRepository;

		private HashSet<ILocalDbCheckConstraint<TEntity>> _constraints;

		public CheckConstraints(LocalDbRepository<TEntity> localDbRepository)
		{
			_constraints = new HashSet<ILocalDbCheckConstraint<TEntity>>(new ConstraintComparer());
			_localDbRepository = localDbRepository;
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
			if (_localDbRepository.ReposetoryCreated)
				throw new InvalidOperationException("Missing Alter or Setup statement of table");
			_constraints.Add(item);
		}

		public void Add(string name, Func<TEntity, bool> item)
		{
			if (_localDbRepository.ReposetoryCreated)
				throw new InvalidOperationException("Missing Alter or Setup statement of table");
			_constraints.Add(new LocalDbCheckConstraint<TEntity>(name, item));
		}

		public void Clear()
		{
			if (_localDbRepository.ReposetoryCreated)
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
			if (_localDbRepository.ReposetoryCreated)
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