/*
This work is licensed under the Creative Commons Attribution-ShareAlike 4.0 International License. 
To view a copy of this license, visit http://creativecommons.org/licenses/by-sa/4.0/.
Please consider to give some Feedback on CodeProject

http://www.codeproject.com/Articles/818690/Yet-Another-ORM-ADO-NET-Wrapper

*/

using System;
using System.Collections.Generic;
using JPB.DataAccess.MetaApi.Contract;

namespace JPB.DataAccess.MetaApi.Model.Equatable
{
	internal class ClassInfoEquatableComparer
		: IEqualityComparer<IClassInfoCache>,
		IEqualityComparer<Type>, 
		IComparer<IClassInfoCache>
	{
		public bool Equals(IClassInfoCache x, IClassInfoCache y)
		{
			if (x == null && y == null)
				return true;
			if (x == null || y == null)
				return false;
			if (x.ClassName != y.ClassName)
				return false;
			if (x.Type == y.Type)
				return true;
			return true;
		}

		public int GetHashCode(IClassInfoCache obj)
		{
			return obj.Type.GetHashCode();
		}

		public int Compare(IClassInfoCache x, IClassInfoCache y)
		{
			if (x == null)
				return -1;
			if (y == null)
				return +1;
			return System.String.Compare(x.ClassName, y.ClassName, System.StringComparison.Ordinal);
		}

		public bool Equals(Type x, Type y)
		{
			if (x == null && y == null)
				return true;
			if (x == null || y == null)
				return false;
			if (x.FullName != y.FullName)
				return false;
			if (x == y)
				return true;
			if (x.IsEquivalentTo(y))
				return true;
			return false;
		}

		public int GetHashCode(Type obj)
		{
			return obj.GetHashCode();
		}
	}
}