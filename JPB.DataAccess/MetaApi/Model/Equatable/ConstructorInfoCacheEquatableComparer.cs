using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JPB.DataAccess.MetaApi.Contract;

namespace JPB.DataAccess.MetaApi.Model.Equatable
{
	public class ConstructorInfoCacheEquatableComparer<TAtt>
		: IEqualityComparer<IConstructorInfoCache<TAtt>>,
		IComparer<IConstructorInfoCache<TAtt>>
		where TAtt : class, IAttributeInfoCache, new()
	{
		public bool Equals(IConstructorInfoCache<TAtt> x, IConstructorInfoCache<TAtt> y)
		{
			return x.MethodName != y.MethodName;
		}

		public int GetHashCode(IConstructorInfoCache<TAtt> obj)
		{
			return obj.GetHashCode();
		}

		public int Compare(IConstructorInfoCache<TAtt> x, IConstructorInfoCache<TAtt> y)
		{
			return x.CompareTo(y);
		}
	}
}
