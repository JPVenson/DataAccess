/*
This work is licensed under the Creative Commons Attribution-ShareAlike 4.0 International License. 
To view a copy of this license, visit http://creativecommons.org/licenses/by-sa/4.0/.
Please consider to give some Feedback on CodeProject

http://www.codeproject.com/Articles/818690/Yet-Another-ORM-ADO-NET-Wrapper

*/

using System;
using JPB.DataAccess.MetaApi.Model;

namespace JPB.DataAccess.Contacts.MetaApi
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