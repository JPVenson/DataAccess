using System;
using JPB.DataAccess.Helper.LocalDb.Constraints.Contracts;

namespace JPB.DataAccess.Helper.LocalDb.Constraints.Defaults
{
	public class LocalDbDefaultConstraint<TEntity, TValue> : ILocalDbDefaultConstraint<TEntity>
	{
		private readonly TValue _value;
		private Action<object, object> _set;

		public LocalDbDefaultConstraint(string name, TValue value, Action<object, object> set)
		{
			if (name == null) throw new ArgumentNullException("name");
			if (value == null) throw new ArgumentNullException("value");
			if (set == null) throw new ArgumentNullException("set");
			_value = value;
			_set = set;
			Name = name;
		}

		public string Name { get; private set; }
		public void DefaultValue(TEntity item)
		{
			_set(item, _value);
		}
	}
}