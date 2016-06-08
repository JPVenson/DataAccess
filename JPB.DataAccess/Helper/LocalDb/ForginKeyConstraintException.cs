using System;

namespace JPB.DataAccess.Helper.LocalDb
{
	internal class ForginKeyConstraintException : ConstraintException
	{
		public ForginKeyConstraintException(
			string changedTableName, 
			string constraintTableName, 
			object constraintValue,
			string pkName,
			string fkName)
			: base(string.Format(
			"The attempt to change/or add an POCO with the value " +
			"'{2}' " +
			"on the property" +
			"'{3}'" +
			"on the table " +
			"'{0}' " +
			"would violate an constraint on table " +
			"'{1}' " +
			"on the property" +
			"'{4}'",
			changedTableName, 
			constraintTableName, 
			constraintValue,
			pkName,
			fkName))
		{

		}
	}

	internal class ConstraintException : Exception
	{
		public ConstraintException(string message) : base(message)
		{

		}
	}
}
