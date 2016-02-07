/*
This work is licensed under the Creative Commons Attribution-ShareAlike 4.0 International License. 
To view a copy of this license, visit http://creativecommons.org/licenses/by-sa/4.0/.
Please consider to give some Feedback on CodeProject

http://www.codeproject.com/Articles/818690/Yet-Another-ORM-ADO-NET-Wrapper

*/
using System;
using System.Collections.Generic;
using System.Reflection;
using JPB.DataAccess.MetaApi.Model;

namespace JPB.DataAccess.MetaApi.Contract
{
	/// <summary>
	/// Holts all infos about an Constructor
	/// </summary>
	/// <typeparam name="TAtt"></typeparam>
	public interface IConstructorInfoCache<TAtt, TArg> :
		IMethodInfoCache<TAtt, TArg>,
		IComparable<IConstructorInfoCache<TAtt, TArg>>,
		IEquatable<IConstructorInfoCache<TAtt, TArg>>
		where TAtt : class, IAttributeInfoCache, new()
		where TArg : class, IMethodArgsInfoCache<TAtt>, new()
	{

	}
}