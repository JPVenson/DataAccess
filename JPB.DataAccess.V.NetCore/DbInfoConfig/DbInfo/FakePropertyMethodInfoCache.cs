using System;
using System.Collections.Generic;
using System.Linq;
using JPB.DataAccess.Contacts.MetaApi;
using JPB.DataAccess.MetaApi.Model;

namespace JPB.DataAccess.DbInfoConfig.DbInfo
{
	internal class FakePropertyMethodInfoCache<TAtt, TArg> : MethodInfoCache<TAtt, TArg>
		where TAtt : class, IAttributeInfoCache, new()
		where TArg : class, IMethodArgsInfoCache<TAtt>, new()
	{
		private readonly DbPropertyInfoCache _property;

		public FakePropertyMethodInfoCache(DbPropertyInfoCache property, Func<object, object[], object> fakeMehtod)
			: base(fakeMehtod)
		{
			_property = property;
		}

		public override object Invoke(object target, params object[] param)
		{
			var argAdd = new List<object>();
			argAdd.Add(target);
			argAdd.AddRange(new object[] { param.ToArray() });
			var argList = argAdd.ToArray();
			return base.Invoke(_property, argList);
		}
	}
}