using System;

namespace JPB.DataAccess.ModelsAnotations
{
	/// <summary>
	///     Marks the property that will be used to hold all non existing Columns
	///     Must be of Type <code>IDictionary string Object</code>
	///     Only for Automatik Loading
	/// </summary>
	[AttributeUsage(AttributeTargets.Property)]
	public sealed class LoadNotImplimentedDynamicAttribute : IgnoreReflectionAttribute
	{
	}
}