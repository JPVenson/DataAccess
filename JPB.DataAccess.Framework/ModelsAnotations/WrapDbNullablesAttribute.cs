using System;

namespace JPB.DataAccess.Framework.ModelsAnotations
{
	/// <summary>
	///     Should the DbAccessLayer wrap DbNull values to C# nullables
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	[Obsolete("This attribute is obsolete because null wrapping now happens by default", true)]
	public class WrapDbNullablesAttribute : DataAccessAttribute
	{
	}
}