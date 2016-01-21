using System;
using System.Collections.Generic;
using System.Reflection;
using JPB.DataAccess.Config.Model;

namespace JPB.DataAccess.Config.Contract
{
	/// <summary>
	/// </summary>
	public interface IPropertyInfoCache : IComparable<PropertyInfoCache>
	{
		/// <summary>
		///     the type of the Setter delegate
		/// </summary>
		Type SetterDelegate { get; }

		/// <summary>
		///     the type of the Getter delegate
		/// </summary>
		Type GetterDelegate { get; }

		/// <summary>
		///     The Setter mehtod can be null
		/// </summary>
		MethodInfoCache Setter { get; }

		/// <summary>
		///     The Getter Method can be null
		/// </summary>
		MethodInfoCache Getter { get; }

		/// <summary>
		///     The return type of the property
		/// </summary>
		Type PropertyType { get; }

		/// <summary>
		///     Direct Reflection
		/// </summary>
		PropertyInfo PropertyInfo { get; }

		/// <summary>
		///     The name of the Property
		/// </summary>
		string PropertyName { get; }

		/// <summary>
		///     All Attributes on this Property
		/// </summary>
		HashSet<AttributeInfoCache> AttributeInfoCaches { get; }

		IPropertyInfoCache Init(PropertyInfo propertyInfo, bool anon);

		int GetHashCode();
	}
}