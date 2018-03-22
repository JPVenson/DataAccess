using System;

namespace JPB.DataAccess.ModelsAnotations
{
	/// <summary>
	///     Indicates that this property is a Primary key
	///     Requert for Selection over PK
	/// </summary>
	[AttributeUsage(AttributeTargets.Property)]
	public class PrimaryKeyAttribute : DataAccessAttribute
	{
	}
}