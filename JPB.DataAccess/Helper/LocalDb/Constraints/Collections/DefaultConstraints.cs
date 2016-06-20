using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using JPB.DataAccess.DbInfoConfig;
using JPB.DataAccess.Helper.LocalDb.Constraints.Contracts;
using JPB.DataAccess.Helper.LocalDb.Constraints.Defaults;

namespace JPB.DataAccess.Helper.LocalDb.Constraints.Collections
{
	public class DefaultConstraints<TEntity> : IDefaultConstraints<TEntity>
	{
		private readonly LocalDbReposetory<TEntity> _localDbReposetory;
		private HashSet<ILocalDbDefaultConstraint<TEntity>> _constraints;

		public DefaultConstraints(LocalDbReposetory<TEntity> localDbReposetory)
		{
			_constraints = new HashSet<ILocalDbDefaultConstraint<TEntity>>(new ConstraintComparer());
			_localDbReposetory = localDbReposetory;
		}

		public IEnumerator<ILocalDbDefaultConstraint<TEntity>> GetEnumerator()
		{
			return _constraints.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable)_constraints).GetEnumerator();
		}

		public void Add(ILocalDbDefaultConstraint<TEntity> item)
		{
			if (_localDbReposetory.ReposetoryCreated)
				throw new InvalidOperationException("Missing Alter or Setup statement of table");
			_constraints.Add(item);
		}

		public void Add<TValue>(string name, TValue value, Action<TEntity, TValue> setter)
		{
			if (_localDbReposetory.ReposetoryCreated)
				throw new InvalidOperationException("Missing Alter or Setup statement of table");
			_constraints.Add(new LocalDbDefaultConstraint<TEntity, TValue>(name, value, setter));
		}

		public void Add<TValue>(DbConfig config, string name, Func<TValue> generateValue, Expression<Func<TEntity, TValue>> column)
		{
			if (_localDbReposetory.ReposetoryCreated)
				throw new InvalidOperationException("Missing Alter or Setup statement of table");
			_constraints.Add(new LocalDbDefaultConstraintEx<TEntity, TValue>(config, name, generateValue, column));
		}

		public void Add<TValue>(string name, Func<TValue> generateValue, Expression<Func<TEntity, TValue>> column)
		{
			if (_localDbReposetory.ReposetoryCreated)
				throw new InvalidOperationException("Missing Alter or Setup statement of table");
			_constraints.Add(new LocalDbDefaultConstraintEx<TEntity, TValue>(_localDbReposetory.Config, name, generateValue, column));
		}

		public void Clear()
		{
			if (_localDbReposetory.ReposetoryCreated)
				throw new InvalidOperationException("Missing Alter or Setup statement of table");
			_constraints.Clear();
		}

		public bool Contains(ILocalDbDefaultConstraint<TEntity> item)
		{
			return _constraints.Contains(item);
		}

		public void CopyTo(ILocalDbDefaultConstraint<TEntity>[] array, int arrayIndex)
		{
			_constraints.CopyTo(array, arrayIndex);
		}

		public bool Remove(ILocalDbDefaultConstraint<TEntity> item)
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

		public void Enforce(TEntity elementToAdd)
		{
			foreach (var localDbDefaultConstraint in _constraints)
			{
				localDbDefaultConstraint.DefaultValue(elementToAdd);
			}
		}
	}
}