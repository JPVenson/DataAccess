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
			return obj.MethodInfo.GetHashCode();
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