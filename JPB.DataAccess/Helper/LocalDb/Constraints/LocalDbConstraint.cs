using System;
using JPB.DataAccess.Contacts;

namespace JPB.DataAccess.Helper.LocalDb.Constraints
{
	public class LocalDbConstraint : ILocalDbConstraint
	{
		public LocalDbConstraint(string name, Func<object,bool> constraint)
		{
			_name = name;
			_constraint = constraint;
		}

		private string _name;
		private Func<object, bool> _constraint;

		public string Name
		{
			get
			{
				return _name;
			}
		}

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
