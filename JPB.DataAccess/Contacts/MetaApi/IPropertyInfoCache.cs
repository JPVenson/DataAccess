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

namespace JPB.DataAccess.Contacts.MetaApi
{
	/// <summary>
	/// </summary>
	public interface IPropertyInfoCache<TAtt>
		: IComparable<IPropertyInfoCache<TAtt>>,
			IEquatable<IPropertyInfoCache<TAtt>> where TAtt : class, IAttributeInfoCache, new()
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
		IMethodInfoCache<TAtt, MethodArgsInfoCache<TAtt>> Setter { get; }

		/// <summary>
		///     The Getter Method can be null
		/// </summary>
		IMethodInfoCache<TAtt, MethodArgsInfoCache<TAtt>> Getter { get; }

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
		HashSet<TAtt> Attributes { get; }

		/// <summary>
		///     Sets all propertys on this instance
		/// </summary>
		/// <param name="propertyInfo"></param>
		/// <param name="anon"></param>
		/// <returns></returns>
		IPropertyInfoCache<TAtt> Init(PropertyInfo propertyInfo, bool anon);
	}
}