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

		/// <summary>
		///     Anonymous type check by naming convention
		/// </summary>
		/// <returns></returns>
		internal static bool IsAnonymousType(this DbClassInfoCache type)
		{
			//http://stackoverflow.com/questions/1650681/determining-whether-a-type-is-an-anonymous-type
			return type.Type.Namespace == null;
		}

		internal static DbConfig ReflecionStore { get; set; }

		/// <summary>
		///     Get the ClassInfoCache object for the type
		/// </summary>
		/// <returns></returns>
		public static DbClassInfoCache GetClassInfo(this Type type)
		{
			if (type.IsAnonymousType())
				return new DbClassInfoCache(type, true); //Anonymous types does not have any Attributes

			return ReflecionStore.GetOrCreateClassInfoCache(type);
		}

		internal static IEnumerable<Attribute> GetCustomAttributes(this Type type)
		{
			if (type.IsAnonymousType())
				return new Attribute[0]; //Anonymous types does not have any Attributes

			return ReflecionStore.GetOrCreateClassInfoCache(type).AttributeInfoCaches.Select(s => s.Attribute);
		}

		internal static IEnumerable<Attribute> GetCustomAttributes(this DbPropertyInfoCache type)
		{
			if (IsAnonymousType(type.DeclaringClass))
				return new Attribute[0]; //Anonymous types does not have any Attributes

			return type.AttributeInfoCaches.Select(s => s.Attribute);
		}

		internal static IEnumerable<Attribute> GetCustomAttributes(this DbMethodInfoCache type)
		{
			if (IsAnonymousType(type.DeclaringClass))
				return new Attribute[0]; //Anonymous types does not have any Attributes

			return type.AttributeInfoCaches.Select(s => s.Attribute);
		}

		internal static string GetLocalToDbSchemaMapping(this DbClassInfoCache type, string name)
		{
			if (IsAnonymousType(type))
				return name;

			return type.SchemaMappingLocalToDatabase(name);
		}

		internal static string GetDbToLocalSchemaMapping(this DbClassInfoCache type, string name)
		{
			if (IsAnonymousType(type))
				return name;

			return type.SchemaMappingDatabaseToLocal(name);
		}

		internal static IEnumerable<PropertyInfo> GetPropertiesEx(this DbClassInfoCache type)
		{
			if (IsAnonymousType(type))
				return type.Type.GetProperties();

			return type
				.PropertyInfoCaches
				.Select(s => s.Value.PropertyInfo);
		}

		internal static string[] GetSchemaMapping(this DbClassInfoCache type)
		{
			if (IsAnonymousType(type))
				return type.GetPropertiesEx().Select(s => s.Name).ToArray();

			return type.LocalToDbSchemaMapping();
		}

		internal static IEnumerable<MethodInfo> GetMethods(this DbClassInfoCache type)
		{
			if (IsAnonymousType(type))
				return type.Type.GetMethods();

			return type.MethodInfoCaches.ToArray().Select(s => s.MethodInfo);
		}

		//internal static string GetPropertyInfoFromLabda<T>(Expression<Func<T>> exp)
		//{
		//    Type paramType = exp.Parameters[0].Type;  // first parameter of expression
		//    var d = paramType.GetMember((exp.Body as MemberExpression).Member.Name)[0];
		//    return d.Name;
		//}
	}
}