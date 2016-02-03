﻿using System;
using System.Collections.Generic;

namespace JPB.DataAccess.MetaApi.Contract
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
		Dictionary<string, TProp> PropertyInfoCaches { get; }

		/// <summary>
		///     All Attributes on class level
		/// </summary>
		HashSet<TAttr> AttributeInfoCaches { get; }

		/// <summary>
		///     All Mehtods
		/// </summary>
		HashSet<TMeth> MethodInfoCaches { get; }

		/// <summary>
		///     All Constructors
		/// </summary>
		HashSet<TCtor> ConstructorInfoCaches { get; }
	}

	/// <summary>
	/// Defines the most basic infos about an class
	/// </summary>
	public interface IClassInfoCache : IEquatable<IClassInfoCache>, IComparable<IClassInfoCache>
	{
		/// <summary>
		///     The .net ClassName
		/// </summary>
		string ClassName { get; }

		/// <summary>
		///     The .net Type instance
		/// </summary>
		Type Type { get; } 
	}
}