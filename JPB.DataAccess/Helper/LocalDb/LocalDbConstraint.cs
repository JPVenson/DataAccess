using System;
using JPB.DataAccess.Contacts;

namespace JPB.DataAccess.Helper.LocalDb
{
	public class LocalDbConstraint : ILocalDbConstraint
	{
		private readonly Func<object, bool> _constraint;

		public LocalDbConstraint(string name, Func<object, bool> constraint)
		{
			Name = name;
			_constraint = constraint;
		}

		public string Name { get; }

		public bool CheckConstraint(object item)
		{
			try
			{
				return _constraint(item);
			}
			catch (Exception)
			{
				return false;
			}
		}
	}
}