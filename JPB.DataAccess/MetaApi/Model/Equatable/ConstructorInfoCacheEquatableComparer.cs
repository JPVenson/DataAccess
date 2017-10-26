#region

using System;
using System.Collections.Generic;
using JPB.DataAccess.Contacts.MetaApi;

#endregion

namespace JPB.DataAccess.MetaApi.Model.Equatable
{
	internal class ConstructorInfoCacheEquatableComparer<TAtt, TArg>
		: IEqualityComparer<IConstructorInfoCache<TAtt, TArg>>,
			IComparer<IConstructorInfoCache<TAtt, TArg>>
		where TAtt : class, IAttributeInfoCache, new()
		where TArg : class, IMethodArgsInfoCache<TAtt>, new()
	{
		public int Compare(IConstructorInfoCache<TAtt, TArg> x, IConstructorInfoCache<TAtt, TArg> y)
		{
			if (x == null)
			{
				return -1;
			}
			if (y == null)
			{
				return +1;
			}
			return string.Compare(x.MethodName, y.MethodName, StringComparison.Ordinal);
		}

		public bool Equals(IConstructorInfoCache<TAtt, TArg> x, IConstructorInfoCache<TAtt, TArg> y)
		{
			if (x == null && y == null)
			{
				return true;
			}
			if (x == null || y == null)
			{
				return false;
			}
			return x.MethodName != y.MethodName;
		}

		public int GetHashCode(IConstructorInfoCache<TAtt, TArg> obj)
		{
			return obj.MethodInfo.GetHashCode();
		}
	}
}