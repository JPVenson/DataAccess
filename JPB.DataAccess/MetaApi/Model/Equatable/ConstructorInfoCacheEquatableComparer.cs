/*
This work is licensed under the Creative Commons Attribution-ShareAlike 4.0 International License. 
To view a copy of this license, visit http://creativecommons.org/licenses/by-sa/4.0/.
Please consider to give some Feedback on CodeProject

http://www.codeproject.com/Articles/818690/Yet-Another-ORM-ADO-NET-Wrapper

*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JPB.DataAccess.Contacts.MetaApi;

namespace JPB.DataAccess.MetaApi.Model.Equatable
{
	internal class ConstructorInfoCacheEquatableComparer<TAtt, TArg>
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
			return obj.MethodInfo.GetHashCode();
		}

		public int Compare(IConstructorInfoCache<TAtt, TArg> x, IConstructorInfoCache<TAtt, TArg> y)
		{
			if (x == null)
				return -1;
			if (y == null)
				return +1;
			return System.String.Compare(x.MethodName, y.MethodName, System.StringComparison.Ordinal);
		}
	}
}
