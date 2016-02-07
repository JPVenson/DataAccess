/*
This work is licensed under the Creative Commons Attribution-ShareAlike 4.0 International License. 
To view a copy of this license, visit http://creativecommons.org/licenses/by-sa/4.0/.
Please consider to give some Feedback on CodeProject

http://www.codeproject.com/Articles/818690/Yet-Another-ORM-ADO-NET-Wrapper

*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace JPB.DataAccess.MetaApi.Contract
{
	public interface IMethodArgsInfoCache<TArg> 
		: IComparable<IMethodArgsInfoCache<TArg>>, IEquatable<IMethodArgsInfoCache<TArg>>
		where TArg: class, IAttributeInfoCache, new() 
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

		IMethodArgsInfoCache<TArg> Init(ParameterInfo info);
	}
}
