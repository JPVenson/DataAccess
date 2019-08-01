using System;

namespace JPB.DataAccess.Framework.ModelsAnotations
{
	/// <summary>
	///     Ignores this Property when creating an  Insert statement
	/// </summary>
	[AttributeUsage(AttributeTargets.Property)]
	public class InsertIgnoreAttribute : DbAccessTypeAttribute
	{
	}
}