using System.Collections.Generic;
using JPB.DataAccess.MetaApi.Contract;

namespace JPB.DataAccess.MetaApi.Model
{
	public class MethodInfoCacheEquatableComparer<TAtt> 
		: IComparer<IMethodInfoCache<TAtt>>,
			IEqualityComparer<IMethodInfoCache<TAtt>> 
		where TAtt : class, IAttributeInfoCache, new()
	{
		public int Compare(IMethodInfoCache<TAtt> x, IMethodInfoCache<TAtt> y)
		{
			return System.String.Compare(x.MethodName, y.MethodName, System.StringComparison.Ordinal);
		}

		public bool Equals(IMethodInfoCache<TAtt> x, IMethodInfoCache<TAtt> y)
		{
			if (x.MethodName != y.MethodName)
				return false;
			return x.MethodInfo.Equals(y.MethodInfo);
		}

		public int GetHashCode(IMethodInfoCache<TAtt> obj)
		{
			return obj.GetHashCode();
		}
	}
}