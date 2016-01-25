using System;
using JPB.DataAccess.MetaApi.Model;

namespace JPB.DataAccess.MetaApi.Contract
{
	/// <summary>
	/// Contains all Infos about an Attribute
	/// </summary>
	public interface IAttributeInfoCache
		: IEquatable<IAttributeInfoCache>, 
		IComparable<AttributeInfoCache>
	{	
		/// <summary>
		///		Direct Reflection
		/// </summary>
		Attribute Attribute { get; }
		/// <summary>
		///		Uniqe ID for the Attribute [ToBeSupported]
		/// </summary>
		object AttributeName { get; }
		/// <summary>
		/// Internal use Only
		/// </summary>
		/// <param name="attribute"></param>
		/// <returns></returns>
		IAttributeInfoCache Init(Attribute attribute);
	}
}