using System;
using System.Collections;
using System.Collections.Generic;
using JPB.DataAccess.Contacts;
using JPB.DataAccess.Helper.LocalDb.Constraints.Contracts;

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

	public class UniqueConstrains : ICollection<ILocalDbUniqueConstraint>
	{
		private readonly LocalDbReposetoryBase _localDbReposetoryBase;
		private HashSet<ILocalDbUniqueConstraint> _constraints;

		public UniqueConstrains(LocalDbReposetoryBase localDbReposetoryBase)
		{
			_constraints = new HashSet<ILocalDbUniqueConstraint>(new ConstraintComparer());
			_localDbReposetoryBase = localDbReposetoryBase;
		}

		public IEnumerator<ILocalDbUniqueConstraint> GetEnumerator()
		{
			return _constraints.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable)_constraints).GetEnumerator();
		}

		public void Add(ILocalDbUniqueConstraint item)
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

		public bool Contains(ILocalDbUniqueConstraint item)
		{
			return _constraints.Contains(item);
		}

		public void CopyTo(ILocalDbUniqueConstraint[] array, int arrayIndex)
		{
			_constraints.CopyTo(array, arrayIndex);
		}

		public bool Remove(ILocalDbUniqueConstraint item)
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

		public void ItemAdded(object item)
		{
			foreach (var localDbUniqueConstraint in _constraints)
			{
				localDbUniqueConstraint.Add(item);
			}
		}

		public void ItemRemoved(object item)
		{
			foreach (var localDbUniqueConstraint in _constraints)
			{
				localDbUniqueConstraint.Delete(item);
			}
		}

		public void ItemUpdated(object item)
		{
			foreach (var localDbUniqueConstraint in _constraints)
			{
				localDbUniqueConstraint.Update(item);
			}
		}
		
		public void Enforce(object item)
		{
			foreach (var localDbUniqueConstraint in _constraints)
			{
				if (!localDbUniqueConstraint.CheckConstraint(item))
					throw new ConstraintException(string.Format("The Unique Constraint '{0}' has detected an invalid object", localDbUniqueConstraint.Name));
			}
		}
	}
}