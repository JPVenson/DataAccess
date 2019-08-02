using System;

namespace JPB.DataAccess.ModelsAnotations
{
	/// <summary>
	///     Marks a ctor or a Method as an Factory method
	///     The ctor must have only one param that is of type IDataRecord
	///     The Method must have only one param that is of type IDataRecord and returns a new Instance
	///     The Method must be static
	/// </summary>
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor, Inherited = false)]
	public sealed class ObjectFactoryMethodAttribute : DbAccessTypeAttribute
	{
	}
}