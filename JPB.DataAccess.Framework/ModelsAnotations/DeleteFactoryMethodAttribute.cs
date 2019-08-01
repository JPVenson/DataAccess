using System;

namespace JPB.DataAccess.Framework.ModelsAnotations
{
	/// <summary>
	///     Marks a Method as an Factory mehtod
	///     The method must return a <code>string</code> or <code>IQueryFactoryResult</code>
	/// </summary>
	[AttributeUsage(AttributeTargets.Method, Inherited = false)]
	public class DeleteFactoryMethodAttribute : DbAccessTypeAttribute
	{
	}
}