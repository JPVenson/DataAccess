/*
This work is licensed under the Creative Commons Attribution-ShareAlike 4.0 International License. 
To view a copy of this license, visit http://creativecommons.org/licenses/by-sa/4.0/.
Please consider to give some Feedback on CodeProject

http://www.codeproject.com/Articles/818690/Yet-Another-ORM-ADO-NET-Wrapper

*/

using System.Collections.Generic;
using JPB.DataAccess.Contacts.MetaApi;

namespace JPB.DataAccess.MetaApi.Model.Equatable
{
	/// <summary>
	///     Defines methods for compareing two Property Cache instances on its Name and location
	/// </summary>
	public class PropertyEquatableComparer<TAtt>
		: IEqualityComparer<IPropertyInfoCache<TAtt>>,
			IComparer<IPropertyInfoCache<TAtt>>
		where TAtt : class, IAttributeInfoCache, new()
	{
		public int Compare(IPropertyInfoCache<TAtt> x, IPropertyInfoCache<TAtt> y)
		{
			if (x == null)
				return -1;
			if (y == null)
				return +1;

			return x.GetHashCode() - y.GetHashCode();
		}

		public bool Equals(IPropertyInfoCache<TAtt> x, IPropertyInfoCache<TAtt> y)
		{
			if (x == null && y == null)
				return true;
			if (x == null || y == null)
				return false;

			if ((x.PropertyInfo == null && y.PropertyInfo != null) || (y.PropertyInfo == null && x.PropertyInfo != null))
				return false;
			if (x.PropertyInfo.DeclaringType.FullName != y.PropertyInfo.DeclaringType.FullName)
				return false;
			if (x.PropertyName != y.PropertyName)
				return false;
			return true;
		}

		public int GetHashCode(IPropertyInfoCache<TAtt> obj)
		{
			return obj.PropertyInfo.GetHashCode();
		}
	}
}