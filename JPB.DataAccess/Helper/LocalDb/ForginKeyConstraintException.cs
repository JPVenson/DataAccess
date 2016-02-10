using System;

namespace JPB.DataAccess.Helper.LocalDb
{
	public class ForginKeyConstraintException : Exception
	{
		public ForginKeyConstraintException(string changedTableName, string constraintTableName, object constraintValue)
			: base(string.Format("The attempt to change the value '{0}' on the table '{1}' would violate an constraint on table '{2}'", changedTableName, constraintTableName, constraintValue))
		{

		}
	}
}
