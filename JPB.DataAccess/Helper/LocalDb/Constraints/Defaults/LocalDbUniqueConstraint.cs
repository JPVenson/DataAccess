using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using JPB.DataAccess.DbInfoConfig;
using JPB.DataAccess.DbInfoConfig.DbInfo;
using JPB.DataAccess.Helper.LocalDb.Constraints.Contracts;

namespace JPB.DataAccess.Helper.LocalDb.Constraints.Defaults
{
	public class LocalDbUniqueConstraint : ILocalDbUniqueConstraint
	{
		private readonly Func<object, object> _getKey;

		public LocalDbUniqueConstraint(
			string name,
			Func<object, object> getKey,
			IEqualityComparer<object> elementComparer = null)
		{
			if (name == null) throw new ArgumentNullException("name");
			if (getKey == null) throw new ArgumentNullException("getKey");
			Name = name;
			_getKey = getKey;

			if (elementComparer != null)
			{
				_index = new HashSet<object>(elementComparer);
			}
			else
			{
				_index = new HashSet<object>();
			}
		}

		private HashSet<object> _index;

		public string Name { get; private set; }
		public bool CheckConstraint(object item)
		{
			if (_index.Contains(_getKey(item)))
				return false;
			return true;
		}

		public void Add(object item)
		{
			_index.Add(_getKey(item));
		}

		public void Delete(object item)
		{
			_index.Remove(_getKey(item));
		}

		public void Update(object item)
		{
			Delete(item);
			Add(item);
		}
	}
}