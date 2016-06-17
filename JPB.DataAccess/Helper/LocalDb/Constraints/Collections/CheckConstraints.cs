using System;
using System.Collections;
using System.Collections.Generic;
using JPB.DataAccess.Contacts;

namespace JPB.DataAccess.Helper.LocalDb.Constraints.Collections
{
	public class CheckConstraints : ICollection<ILocalDbCheckConstraint>
	{
		private readonly LocalDbReposetoryBase _localDbReposetoryBase;

		private HashSet<ILocalDbCheckConstraint> _constraints;

		public CheckConstraints(LocalDbReposetoryBase localDbReposetoryBase)
		{
			_constraints = new HashSet<ILocalDbCheckConstraint>(new ConstraintComparer());
			_localDbReposetoryBase = localDbReposetoryBase;
		}

		public IEnumerator<ILocalDbCheckConstraint> GetEnumerator()
		{
			return _constraints.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable) _constraints).GetEnumerator();
		}

		public void Add(ILocalDbCheckConstraint item)
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

		public bool Contains(ILocalDbCheckConstraint item)
		{
			return _constraints.Contains(item);
		}

		public void CopyTo(ILocalDbCheckConstraint[] array, int arrayIndex)
		{
			_constraints.CopyTo(array, arrayIndex);
		}

		public bool Remove(ILocalDbCheckConstraint item)
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

		public void Enforce(object item)
		{
			foreach (var localDbCheckConstraint in _constraints)
			{
				if(!localDbCheckConstraint.CheckConstraint(item))
					throw new ConstraintException(string.Format("The Check Constraint '{0}' has detected an invalid object", localDbCheckConstraint.Name));
			}
		}
	}
}