using System;

namespace JPB.DataAccess.ModelsAnotations
{
	/// <summary>
	///     Adds a namespace to the generated class
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
	public sealed class AutoGenerateCtorNamespaceAttribute : DataAccessAttribute
	{
		// See the attribute guidelines at
		//  http://go.microsoft.com/fwlink/?LinkId=85236
		internal readonly string UsedNamespace;

		/// <summary>
		///     Creates a new Attribute that is used for CodeGeneration
		///     This Attributes tell the factory to include certain namespaces.
		/// </summary>
		/// <param name="usedNamespace"></param>
		public AutoGenerateCtorNamespaceAttribute(string usedNamespace)
		{
			UsedNamespace = usedNamespace;
		}
	}
}