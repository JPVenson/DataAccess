namespace JPB.DataAccess.Helper.LocalDb.Constraints
{
	/// <summary>
	///     An exception that will be thrown if an Invalid data insert/update/delete is detected
	/// </summary>
	/// <seealso cref="ConstraintException" />
	public class ForginKeyConstraintException : ConstraintException
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="ForginKeyConstraintException" /> class.
		/// </summary>
		/// <param name="constraintName">Name of the constraint.</param>
		/// <param name="changedTableName">Name of the changed table.</param>
		/// <param name="constraintTableName">Name of the constraint table.</param>
		/// <param name="constraintValue">The constraint value.</param>
		/// <param name="pkName">Name of the pk.</param>
		/// <param name="fkName">Name of the fk.</param>
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