using System;

namespace JPB.DataAccess.ModelsAnotations
{
	/// <summary>
	///     Ignores this Property when creating an  Insert statement
	/// </summary>
	[AttributeUsage(AttributeTargets.Property)]
	public class InsertIgnoreAttribute : DbAccessTypeAttribute
	{
	}
}