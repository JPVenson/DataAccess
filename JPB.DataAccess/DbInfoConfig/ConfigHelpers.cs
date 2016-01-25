#if !DEBUG
using System.Diagnostics;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JPB.DataAccess.DbInfoConfig.DbInfo;
using JPB.DataAccess.MetaApi;

namespace JPB.DataAccess.DbInfoConfig
{
#if !DEBUG
	[DebuggerStepThrough]
#endif
	/// <summary>
	/// </summary>
	public static class DbConfigHelper
	{
		static DbConfigHelper()
		{
			ReflecionStore = new DbConfig();
		}

		internal static DbConfig ReflecionStore { get; set; }

		/// <summary>
		///     Get the ClassInfoCache object for the type
		/// </summary>
		/// <returns></returns>
		public static DbClassInfoCache GetClassInfo(this Type type)
		{
			if (MetaInfoStoreExtentions.IsAnonymousType(type))
				return new DbClassInfoCache(type, true); //Anonymous types does not have any Attributes

			return ReflecionStore.GetOrCreateClassInfoCache(type);
		}

		internal static IEnumerable<Attribute> GetCustomAttributes(this Type type)
		{
			if (MetaInfoStoreExtentions.IsAnonymousType(type))
				return new Attribute[0]; //Anonymous types does not have any Attributes

			return ReflecionStore.GetOrCreateClassInfoCache(type).AttributeInfoCaches.Select(s => s.Attribute);
		}

		internal static IEnumerable<Attribute> GetCustomAttributes(this PropertyInfo type)
		{
			if (MetaInfoStoreExtentions.IsAnonymousType(type.DeclaringType))
				return new Attribute[0]; //Anonymous types does not have any Attributes

			var deb =
				ReflecionStore.GetOrCreatePropertyInfoCache(type).AttributeInfoCaches.Select(s => s.Attribute);

			return deb;
		}

		internal static IEnumerable<Attribute> GetCustomAttributes(this MethodInfo type)
		{
			if (MetaInfoStoreExtentions.IsAnonymousType(type.DeclaringType))
				return new Attribute[0]; //Anonymous types does not have any Attributes

			var deb = ReflecionStore.GetOrCreateMethodInfoCache(type).AttributeInfoCaches.Select(s => s.Attribute);

			return deb;
		}

		internal static string GetLocalToDbSchemaMapping(this Type type, string name)
		{
			if (MetaInfoStoreExtentions.IsAnonymousType(type))
				return name;

			return ReflecionStore.GetOrCreateClassInfoCache(type).SchemaMappingLocalToDatabase(name);
		}

		internal static string GetDbToLocalSchemaMapping(this Type type, string name)
		{
			if (MetaInfoStoreExtentions.IsAnonymousType(type))
				return name;

			return ReflecionStore.GetOrCreateClassInfoCache(type).SchemaMappingDatabaseToLocal(name);
		}

		internal static IEnumerable<PropertyInfo> GetPropertiesEx(this Type type)
		{
			if (MetaInfoStoreExtentions.IsAnonymousType(type))
				return type.GetProperties();

			return ReflecionStore
				.GetOrCreateClassInfoCache(type)
				.PropertyInfoCaches
				.Select(s => s.Value.PropertyInfo);
		}

		internal static string[] GetSchemaMapping(this Type type)
		{
			if (MetaInfoStoreExtentions.IsAnonymousType(type))
				return type.GetPropertiesEx().Select(s => s.Name).ToArray();

			return ReflecionStore.GetOrCreateClassInfoCache(type).LocalToDbSchemaMapping();
		}

		internal static IEnumerable<MethodInfo> GetMethods(this Type type)
		{
			if (MetaInfoStoreExtentions.IsAnonymousType(type))
				return type.GetMethods();

			return ReflecionStore.GetOrCreateClassInfoCache(type).MethodInfoCaches.ToArray().Select(s => s.MethodInfo);
		}

		

		//internal static string GetPropertyInfoFromLabda<T>(Expression<Func<T>> exp)
		//{
		//    Type paramType = exp.Parameters[0].Type;  // first parameter of expression
		//    var d = paramType.GetMember((exp.Body as MemberExpression).Member.Name)[0];
		//    return d.Name;
		//}
	}
}