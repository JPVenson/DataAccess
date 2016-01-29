using System.Collections.Generic;
using JPB.DataAccess.MetaApi.Contract;

namespace JPB.DataAccess.MetaApi.Model
{
	public class MethodInfoCacheEquatableComparer<TAtt, TArg>
		: IComparer<IMethodInfoCache<TAtt, TArg>>,
			IEqualityComparer<IMethodInfoCache<TAtt, TArg>> 
		where TAtt : class, IAttributeInfoCache, new() where TArg : class, IMethodArgsInfoCache<TAtt>, new()
	{
		public bool Equals(IMethodInfoCache<TAtt, TArg> x, IMethodInfoCache<TAtt, TArg> y)
		{
			if (x == null && y == null)
				return true;
			if (x == null || y == null)
				return false;
			if (x.MethodName != y.MethodName)
				return false;
			return x.MethodInfo.Equals(y.MethodInfo);
		}

		public int GetHashCode(IMethodInfoCache<TAtt, TArg> obj)
		{
			return obj.GetHashCode();
		}

		public int Compare(IMethodInfoCache<TAtt, TArg> x, IMethodInfoCache<TAtt, TArg> y)
		{
			if (x == null)
				return -1;
			if (y == null)
				return +1;
			return System.String.Compare(x.MethodName, y.MethodName, System.StringComparison.Ordinal);
		}
	}
}