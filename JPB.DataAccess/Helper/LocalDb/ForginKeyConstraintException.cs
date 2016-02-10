using System;

namespace JPB.DataAccess.Helper.LocalDb
{
	public class ForginKeyConstraintException : Exception
	{
		public ForginKeyConstraintException(string changedTableName, string constraintTableName, object constraintValue)
			: base(string.Format(
			"The attempt to change the value '{2}' on the table '{0}' would violate an constraint on table '{1}'",
			changedTableName, 
			constraintTableName, 
			constraintValue))
		{

		}
	}
}
