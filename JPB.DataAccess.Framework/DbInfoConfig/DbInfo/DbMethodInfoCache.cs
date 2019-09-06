#region

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using JPB.DataAccess.Contacts.MetaApi;
using JPB.DataAccess.MetaApi.Model;
using JPB.DataAccess.ModelsAnotations;

#endregion

namespace JPB.DataAccess.DbInfoConfig.DbInfo
{
	/// <summary>
	///     Infos about the Method
	/// </summary>
	public class DbMethodInfoCache : MethodInfoCache<DbAttributeInfoCache, DbMethodArgument>
	{
		///// <summary>
		/////     Initializes a new instance of the <see cref="DbMethodInfoCache" /> class.
		///// </summary>
		///// <param name="mehtodInfo">The mehtod information.</param>
		//public DbMethodInfoCache(MethodInfo mehtodInfo)
		//	: base(mehtodInfo)
		//{
		//}

		///// <summary>
		/////     Initializes a new instance of the <see cref="DbMethodInfoCache" /> class.
		///// </summary>
		///// <param name="fakeMehtod">The fake mehtod.</param>
		///// <param name="declaringType">Type of the declaring.</param>
		///// <param name="name">The name.</param>
		///// <param name="attributes">The attributes.</param>
		//public DbMethodInfoCache(Func<object, object[], object> fakeMehtod, Type declaringType, string name = null,
		//	params DbAttributeInfoCache[] attributes)
		//	: base(fakeMehtod, declaringType, name, attributes)
		//{
		//}

		/// <summary>
		///     The class that owns this Method
		/// </summary>
		public DbClassInfoCache DeclaringClass { get; protected internal set; }

		/// <summary>
		///     For Internal use Only
		/// </summary>
		/// <param name="mehtodInfo"></param>
		/// <returns></returns>
		public override IMethodInfoCache<DbAttributeInfoCache, DbMethodArgument> Init(MethodBase mehtodInfo)
		{
			Arguments =
				new HashSet<DbMethodArgument>(
					Arguments.Where(f => f.Attributes.All(e => !(e.Attribute is IgnoreReflectionAttribute))));

			foreach (var dbMethodArgument in Arguments)
			{
				dbMethodArgument.DeclaringMethod = this;
			}
			return base.Init(mehtodInfo);
		}
	}
}