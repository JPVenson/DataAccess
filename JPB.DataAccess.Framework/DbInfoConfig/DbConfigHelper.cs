#if !DEBUG
using System.Diagnostics;
#endif

#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JPB.DataAccess.DbInfoConfig.DbInfo;
using JPB.DataAccess.MetaApi;

#endregion

namespace JPB.DataAccess.DbInfoConfig
{

	/// <summary>
	/// Defines a Helper for Reflection on DbAccessLayer level
	/// </summary>
#if !DEBUG
	[DebuggerStepThrough]
#endif
	public static class DbConfigHelper
	{
		static DbConfigHelper()
		{
			ReflecionStore = new DbConfig(true);
		}

		internal static DbConfig ReflecionStore { get; set; }

		/// <summary>
		///     Anonymous type check by naming convention
		/// </summary>
		/// <returns></returns>
		internal static bool IsAnonymousType(this DbClassInfoCache type)
		{
			//http://stackoverflow.com/questions/1650681/determining-whether-a-type-is-an-anonymous-type
			return type.Type.Namespace == null;
		}

		/// <summary>
		///     Get the ClassInfoCache object for the type
		/// </summary>
		/// <returns></returns>
		public static DbClassInfoCache GetClassInfo(this Type type)
		{
			if (type.IsAnonymousType())
			{
				return new DbClassInfoCache(type, true); //Anonymous types does not have any Attributes
			}

			return ReflecionStore.GetOrCreateClassInfoCache(type);
		}

		internal static string GetLocalToDbSchemaMapping(this DbClassInfoCache type, string name)
		{
			if (IsAnonymousType(type))
			{
				return name;
			}

			return type.SchemaMappingLocalToDatabase(name);
		}

		internal static string GetDbToLocalSchemaMapping(this DbClassInfoCache type, string name)
		{
			if (IsAnonymousType(type))
			{
				return name;
			}

			return type.SchemaMappingDatabaseToLocal(name);
		}

		internal static IEnumerable<PropertyInfo> GetPropertiesEx(this DbClassInfoCache type)
		{
			if (IsAnonymousType(type))
			{
				return type.Type.GetProperties();
			}

			return type
				.Propertys
				.Select(s => s.Value.PropertyInfo);
		}

		internal static string[] GetSchemaMapping(this DbClassInfoCache type)
		{
			if (IsAnonymousType(type))
			{
				return type.GetPropertiesEx().Select(s => s.Name).ToArray();
			}

			return type.LocalToDbSchemaMapping();
		}

		//}
		//    return d.Name;
		//    var d = paramType.GetMember((exp.Body as MemberExpression).Member.Name)[0];
		//    Type paramType = exp.Parameters[0].Type;  // first parameter of expression
		//{

		//internal static string GetPropertyInfoFromLabda<T>(Expression<Func<T>> exp)
	}
}