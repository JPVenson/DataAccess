/*
This work is licensed under the Creative Commons Attribution-ShareAlike 4.0 International License. 
To view a copy of this license, visit http://creativecommons.org/licenses/by-sa/4.0/.
Please consider to give some Feedback on CodeProject

http://www.codeproject.com/Articles/818690/Yet-Another-ORM-ADO-NET-Wrapper

*/

using System;
using System.Collections.Generic;

namespace JPB.DataAccess.Contacts.MetaApi
{
	/// <summary>
	/// 
	/// </summary>
	/// <typeparam name="TProp"></typeparam>
	/// <typeparam name="TAttr"></typeparam>
	/// <typeparam name="TMeth"></typeparam>
	/// <typeparam name="TCtor"></typeparam>
	/// <typeparam name="TArg"></typeparam>
	public interface IClassInfoCache<TProp, TAttr, TMeth, TCtor, TArg> : IClassInfoCache
		where TProp : class, IPropertyInfoCache<TAttr>, new()
		where TAttr : class, IAttributeInfoCache, new()
		where TMeth : class, IMethodInfoCache<TAttr, TArg>, new()
		where TCtor : class, IConstructorInfoCache<TAttr, TArg>, new() 
		where TArg : class, IMethodArgsInfoCache<TAttr>, new()
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="type"></param>
		/// <param name="anon"></param>
		/// <returns></returns>
		IClassInfoCache<TProp, TAttr, TMeth, TCtor, TArg> Init(Type type, bool anon = false);
		
		/// <summary>
		///     All Propertys
		/// </summary>
		Dictionary<string, TProp> Propertys { get; }

		/// <summary>
		///     All Attributes on class level
		/// </summary>
		HashSet<TAttr> Attributes { get; }

		/// <summary>
		///     All Mehtods
		/// </summary>
		HashSet<TMeth> Mehtods { get; }

		/// <summary>
		///     All Constructors
		/// </summary>
		HashSet<TCtor> Constructors { get; }
	}

	/// <summary>
	/// Defines the most basic infos about an class
	/// </summary>
	public interface IClassInfoCache : IEquatable<IClassInfoCache>,
		IComparable<IClassInfoCache>,
		IEquatable<Type>
	{
		/// <summary>
		///     The .net ClassName
		/// </summary>
		string Name { get; }

		/// <summary>
		///     The .net Type instance
		/// </summary>
		Type Type { get; } 
	}
}