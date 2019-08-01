using System;

namespace JPB.DataAccess.Framework.ModelsAnotations
{
	/// <summary>
	///     Marks a class as a StoredPrecedure wrapper
	///     if the marked class contains a Generic Arguement
	///     The result stream from the Select Statement will be parsed into the generic arguement
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, Inherited = false)]
	public class StoredProcedureAttribute : InsertIgnoreAttribute
	{
	}
}