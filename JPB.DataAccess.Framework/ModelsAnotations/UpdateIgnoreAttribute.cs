using System;

namespace JPB.DataAccess.Framework.ModelsAnotations
{
	/// <summary>
	///     Ignores this Property when creating an Update statement
	/// </summary>
	[AttributeUsage(AttributeTargets.Property)]
	public class UpdateIgnoreAttribute : DbAccessTypeAttribute
	{
	}
}