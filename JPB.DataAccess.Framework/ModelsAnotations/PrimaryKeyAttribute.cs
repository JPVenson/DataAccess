using System;

namespace JPB.DataAccess.Framework.ModelsAnotations
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