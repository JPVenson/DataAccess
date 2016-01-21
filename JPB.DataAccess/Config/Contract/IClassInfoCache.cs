using System;
using System.Collections.Generic;

namespace JPB.DataAccess.Config.Contract
{
	/// <summary>
	/// 
	/// </summary>
	/// <typeparam name="TProp"></typeparam>
	/// <typeparam name="TAttr"></typeparam>
	/// <typeparam name="TMeth"></typeparam>
	/// <typeparam name="TCtor"></typeparam>
	public interface IClassInfoCache<TProp, TAttr, TMeth, TCtor>
		where TProp : class, IPropertyInfoCache, new()
		where TAttr : class, IAttributeInfoCache, new()
		where TMeth : class, IMethodInfoCache, new()
		where TCtor : class, IConstructorInfoCache, new()
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="type"></param>
		/// <param name="anon"></param>
		/// <returns></returns>
		IClassInfoCache<TProp, TAttr, TMeth, TCtor> Init(Type type, bool anon = false);

		/// <summary>
		///     The .net ClassName
		/// </summary>
		string ClassName { get; }

		/// <summary>
		///     The .net Type instance
		/// </summary>
		Type Type { get; }

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
}