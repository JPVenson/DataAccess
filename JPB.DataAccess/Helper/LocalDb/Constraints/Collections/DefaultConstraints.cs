using System;
using System.Collections;
using System.Collections.Generic;
using JPB.DataAccess.Helper.LocalDb.Constraints.Contracts;

namespace JPB.DataAccess.Helper.LocalDb.Constraints.Collections
{
	public class DefaultConstraints : ICollection<ILocalDbDefaultConstraint>
	{
		private readonly LocalDbReposetoryBase _localDbReposetoryBase;
		private HashSet<ILocalDbDefaultConstraint> _constraints;

		public DefaultConstraints(LocalDbReposetoryBase localDbReposetoryBase)
		{
			_constraints = new HashSet<ILocalDbDefaultConstraint>(new ConstraintComparer());
			_localDbReposetoryBase = localDbReposetoryBase;
		}

		public IEnumerator<ILocalDbDefaultConstraint> GetEnumerator()
		{
			return _constraints.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable) _constraints).GetEnumerator();
		}

		public void Add(ILocalDbDefaultConstraint item)
		{
			if (_localDbReposetoryBase.ReposetoryCreated)
				throw new InvalidOperationException("Missing Alter or Setup statement of table");
			_constraints.Add(item);
		}

		public void Clear()
		{
			if (_localDbReposetoryBase.ReposetoryCreated)
				throw new InvalidOperationException("Missing Alter or Setup statement of table");
			_constraints.Clear();
		}

		public bool Contains(ILocalDbDefaultConstraint item)
		{
			return _constraints.Contains(item);
		}

		public void CopyTo(ILocalDbDefaultConstraint[] array, int arrayIndex)
		{
			_constraints.CopyTo(array, arrayIndex);
		}

		public bool Remove(ILocalDbDefaultConstraint item)
		{
			if (_localDbReposetoryBase.ReposetoryCreated)
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

		public void Enforce(object elementToAdd)
		{
			foreach (var localDbDefaultConstraint in _constraints)
			{
				localDbDefaultConstraint.DefaultValue(elementToAdd);
			}
		}
	}
}