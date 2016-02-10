/*
This work is licensed under the Creative Commons Attribution-ShareAlike 4.0 International License. 
To view a copy of this license, visit http://creativecommons.org/licenses/by-sa/4.0/.
Please consider to give some Feedback on CodeProject

http://www.codeproject.com/Articles/818690/Yet-Another-ORM-ADO-NET-Wrapper

*/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using JPB.DataAccess.Contacts.MetaApi;
using JPB.DataAccess.MetaApi.Model;
using JPB.DataAccess.ModelsAnotations;

namespace JPB.DataAccess.DbInfoConfig.DbInfo
{
	/// <summary>
	///     Infos about the Method
	/// </summary>
	public class DbMethodInfoCache : MethodInfoCache<DbAttributeInfoCache, DbMethodArgument>
	{
		internal DbMethodInfoCache(MethodInfo mehtodInfo)
			: base(mehtodInfo)
		{
		}

		internal DbMethodInfoCache(Func<object, object[], object> fakeMehtod, string name = null, params DbAttributeInfoCache[] attributes)
			: base(fakeMehtod, name, attributes)
		{
		}
		/// <summary>
		/// 
		/// </summary>
#if !DEBUG
		[DebuggerHidden]
#endif
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public DbMethodInfoCache()
		{
		}

		/// <summary>
		///		The class that owns this Method
		/// </summary>
		public DbClassInfoCache DeclaringClass { get; protected internal set; }

		public override IMethodInfoCache<DbAttributeInfoCache, DbMethodArgument> Init(MethodBase mehtodInfo)
		{
			Arguments = new HashSet<DbMethodArgument>(Arguments.Where(f => f.Attributes.All(e => !(e.Attribute is IgnoreReflectionAttribute))));

			foreach (var dbMethodArgument in Arguments)
			{
				dbMethodArgument.DeclaringMethod = this;
			}
			return base.Init(mehtodInfo);
		}
	}
}