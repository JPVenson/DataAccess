namespace JPB.DataAccess.Helper.LocalDb.Constraints
{
	public class ForginKeyConstraintException : ConstraintException
	{
		public ForginKeyConstraintException(
			string constraintName,
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
			"would violate the constraint '{5}' on table " +
			"'{1}' " +
			"on the property" +
			"'{4}'" +
			"\r\nAll transactions will be rolled back",
			changedTableName, 
			constraintTableName, 
			constraintValue,
			pkName,
			fkName, 
			constraintName))
		{

		}
	}
}
