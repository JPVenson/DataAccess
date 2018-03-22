using System;

namespace JPB.DataAccess.ModelsAnotations
{
	/// <summary>
	///     Should the DbAccessLayer wrap DbNull values to C# nullables
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public class WrapDbNullablesAttribute : DataAccessAttribute
	{
	}
}