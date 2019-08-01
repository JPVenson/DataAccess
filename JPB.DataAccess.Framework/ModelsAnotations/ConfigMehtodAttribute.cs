using System;

namespace JPB.DataAccess.Framework.ModelsAnotations
{
	/// <summary>
	///     When a methode is marked with this attribute it can be used to configurate the current class. Must be public static
	///     void
	/// </summary>
	[AttributeUsage(AttributeTargets.Method, Inherited = false)]
	public sealed class ConfigMehtodAttribute : DataAccessAttribute
	{
	}
}