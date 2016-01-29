using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JPB.DataAccess.MetaApi.Contract;

namespace JPB.DataAccess.MetaApi.Model.Equatable
{
	/// <summary>
	/// Defines methods for compareing two Property Cache instances on its Name and location
	/// </summary>
	public class PropertyEquatableComparer<TAtt> 
		: IEqualityComparer<IPropertyInfoCache<TAtt>>, 
		IComparer<IPropertyInfoCache<TAtt>>
		where TAtt : class, IAttributeInfoCache, new()
	{
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
			return obj.GetHashCode();
		}

		public int Compare(IPropertyInfoCache<TAtt> x, IPropertyInfoCache<TAtt> y)
		{
			if (x == null)
				return -1;
			if (y == null)
				return +1;

			return x.GetHashCode() - y.GetHashCode();
		}
	}
}
