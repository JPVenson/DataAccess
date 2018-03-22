using System;

namespace JPB.DataAccess.ModelsAnotations
{
	/// <summary>
	///     Marks a property to be ignored by the complete searching logic
	///     Experimental
	/// </summary>
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property)]
	public class IgnoreReflectionAttribute : DataAccessAttribute
	{
	}
}