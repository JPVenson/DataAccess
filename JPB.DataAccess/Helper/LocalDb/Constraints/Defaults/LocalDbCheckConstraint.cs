using System;
using System.Linq.Expressions;
using JPB.DataAccess.Contacts;
using JPB.DataAccess.DbInfoConfig.DbInfo;

namespace JPB.DataAccess.Helper.LocalDb.Constraints.Defaults
{
	public class LocalDbCheckConstraint : ILocalDbCheckConstraint
	{
		public LocalDbCheckConstraint(string name, Func<object, bool> constraint)
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
