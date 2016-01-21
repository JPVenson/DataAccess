using System.Collections.Generic;
using System.Reflection;
using JPB.DataAccess.Config.Model;

namespace JPB.DataAccess.Config.Contract
{
	public interface IConstructorInfoCache
	{
		/// <summary>
		///     Direct Reflection
		/// </summary>
		ConstructorInfo MethodInfo { get; }

		/// <summary>
		///     The name of the constructor
		/// </summary>
		string MethodName { get; }

		/// <summary>
		///     All Attributes
		/// </summary>
		HashSet<AttributeInfoCache> AttributeInfoCaches { get; }

		IConstructorInfoCache Init(ConstructorInfo ctorInfo);
	}
}