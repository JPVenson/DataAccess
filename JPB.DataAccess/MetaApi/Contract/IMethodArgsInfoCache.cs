using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace JPB.DataAccess.MetaApi.Contract
{
	public interface IMethodArgsInfoCache<TArg> : IComparable<IMethodArgsInfoCache<TArg>>, IEquatable<IMethodArgsInfoCache<TArg>>
		where TArg: IAttributeInfoCache
	{
		/// <summary>
		/// The name of this Param
		/// </summary>
		string ArgumentName { get; }
		/// <summary>
		/// The type of this Param
		/// </summary>
		Type Type { get; }

		/// <summary>
		/// All Attached Attributes
		/// </summary>
		HashSet<TArg> Attributes { get; }

		/// <summary>
		/// Direct reflection
		/// </summary>
		ParameterInfo ParameterInfo { get; }
	}
}
