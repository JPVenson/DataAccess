using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JPB.DataAccess.MetaApi.Contract;

namespace JPB.DataAccess.MetaApi.Model.Equatable
{
	public class ConstructorInfoCacheEquatableComparer<TAtt, TArg>
		: IEqualityComparer<IConstructorInfoCache<TAtt, TArg>>,
		IComparer<IConstructorInfoCache<TAtt, TArg>>
		where TAtt : class, IAttributeInfoCache, new() 
		where TArg : class, IMethodArgsInfoCache<TAtt>, new()
	{
		public bool Equals(IConstructorInfoCache<TAtt, TArg> x, IConstructorInfoCache<TAtt, TArg> y)
		{
			if (x == null && y == null)
				return true;
			if (x == null || y == null)
				return false;
			return x.MethodName != y.MethodName;
		}

		public int GetHashCode(IConstructorInfoCache<TAtt, TArg> obj)
		{
			return obj.GetHashCode();
		}

		public int Compare(IConstructorInfoCache<TAtt, TArg> x, IConstructorInfoCache<TAtt, TArg> y)
		{
			if (x == null)
				return -1;
			if (y == null)
				return +1;
			return x.CompareTo(y);
		}
	}
}
