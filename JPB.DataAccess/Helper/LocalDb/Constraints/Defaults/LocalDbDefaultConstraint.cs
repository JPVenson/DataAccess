using System;
using JPB.DataAccess.Helper.LocalDb.Constraints.Contracts;

namespace JPB.DataAccess.Helper.LocalDb.Constraints.Defaults
{
	public class LocalDbDefaultConstraint : ILocalDbDefaultConstraint
	{
		private readonly object _value;
		private readonly Action<object> _set;

		public LocalDbDefaultConstraint(string name, object value, Action<object> set)
		{
			if (name == null) throw new ArgumentNullException("name");
			if (value == null) throw new ArgumentNullException("value");
			if (set == null) throw new ArgumentNullException("set");
			_value = value;
			_set = set;
			Name = name;
		}

		public string Name { get; private set; }
		public void DefaultValue()
		{
			_set(_value);
		}
	}
}