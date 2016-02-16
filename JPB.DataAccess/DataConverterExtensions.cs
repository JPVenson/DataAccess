/*
This work is licensed under the Creative Commons Attribution-ShareAlike 4.0 International License. 
To view a copy of this license, visit http://creativecommons.org/licenses/by-sa/4.0/.
Please consider to give some Feedback on CodeProject

http://www.codeproject.com/Articles/818690/Yet-Another-ORM-ADO-NET-Wrapper

*/

#region Jean-Pierre Bachmann

// Erstellt von Jean-Pierre Bachmann am 13:02

#endregion

#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Text;
using JPB.DataAccess.AdoWrapper;
using JPB.DataAccess.Contacts;
using JPB.DataAccess.DbCollection;
using JPB.DataAccess.DbInfoConfig;
using JPB.DataAccess.DbInfoConfig.DbInfo;
using JPB.DataAccess.DebuggerHelper;
using JPB.DataAccess.Helper;
using JPB.DataAccess.Manager;
using JPB.DataAccess.MetaApi.Model;
using JPB.DataAccess.ModelsAnotations;

#endregion

namespace JPB.DataAccess
{
	/// <summary>
	///     Helper Extentions for Maintaining Value
	/// </summary>
#if !DEBUG
	[DebuggerStepThrough]
#endif
	public static class DataConverterExtensions
	{
		/// <summary>
		/// </summary>
		/// <returns></returns>
		internal static QueryDebugger CreateQueryDebuggerAuto(this IDbCommand command, IDatabase source)
		{
			if (DbAccessLayer.Debugger)
			{
				return new QueryDebugger(command, source);
			}
			return null;
		}

		/// <summary>
		/// </summary>
		/// <returns></returns>
		public static QueryDebugger CreateQueryDebugger(this IDbCommand command, IDatabase source = null)
		{
			return new QueryDebugger(command, source);
		}

		/// <summary>
		///     Checks
		///     <paramref name="t" />
		///     for Generics
		///     This would indicate that the call of the proc could return some data
		/// </summary>
		/// <returns></returns>
		public static bool CheckForResultProcedure(Type t)
		{
			var attStatus = t.GetGenericArguments();
			return attStatus.Any();
		}

		/// <summary>
		///     Gets the Value or DB null
		/// </summary>
		/// <returns></returns>
		public static object GetDataValue(object value)
		{
			return value ?? DBNull.Value;
		}

		/// <summary>
		///     Gets the Value from a Paramter with Conversion if Nessesary
		/// </summary>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException"></exception>
		public static object GetParamaterValue(this object source, string name)
		{
			if (source == null)
				throw new ArgumentNullException("source");
			var propertyInfo = GetParamater(source, name);
			if (propertyInfo == null)
				throw new ArgumentNullException("name");
			return propertyInfo.GetConvertedValue(source);
		}

		/// <summary>
		///     retuns the Cashed Property info from Refection Cash
		/// </summary>
		/// <returns></returns>
		public static DbPropertyInfoCache GetParamater(this object source, string name)
		{
			DbPropertyInfoCache val;
			source.GetType().GetClassInfo().Propertys.TryGetValue(name, out val);
			return val;
		}

		/// <summary>
		///     Checks a
		///     <paramref name="info" />
		///     to be a Primary Key
		/// </summary>
		/// <returns></returns>
		public static bool CheckForPK(this PropertyInfo info)
		{
			return info.GetCustomAttributes().Any(s => s is PrimaryKeyAttribute) || (info.Name.EndsWith("_ID"));
		}

		/// <summary>
		///     Checks a
		///     <paramref name="info" />
		///     to be a Primary Key
		/// </summary>
		/// <returns></returns>
		public static bool CheckForFK(this PropertyInfo info, string name)
		{
			if (info.Name != name)
				return false;
			return info.GetCustomAttributes().Any(s => s is PrimaryKeyAttribute);
		}

		/// <summary>
		///     Checks a Property to BE handled as a Forgine Key from an Other class
		///     (Checks for PrimaryKey)
		/// </summary>
		/// <returns></returns>
		public static bool CheckForFK(this PropertyInfo info)
		{
			return info.GetCustomAttributes().Any(s => s is PrimaryKeyAttribute);
		}

		/// <summary>
		///     Returns the Primarykey name (Converted) if exists
		/// </summary>
		/// <returns></returns>
		public static string GetPKPropertyName(this Type type)
		{
			return type.GetClassInfo().PrimaryKeyProperty.PropertyName;
		}

		/// <summary>
		///     Get and Convert the found PK name into Database name
		/// </summary>
		/// <returns></returns>
		public static string GetPK(this Type type)
		{
			return type.GetClassInfo().PrimaryKeyProperty.DbName;
		}

		/// <summary>
		///     Returns All forgin keys of the given type
		/// </summary>
		/// <returns></returns>
		public static DbPropertyInfoCache[] GetFKs(this Type type)
		{
			return type.GetClassInfo().Propertys.Where(f => f.Value.ForginKeyAttribute != null).Select(f => f.Value).ToArray();
		}

		/// <summary>
		///     Gets the first Forgin key that is of type <paramref name="fkType" />
		/// </summary>
		/// <returns></returns>
		public static string GetFK(this Type type, Type fkType)
		{
			var hasFk = type.GetClassInfo()
				.Propertys
				.Select(f => f.Value)
				.Where(f => f.ForginKeyAttribute != null)
				.FirstOrDefault(f => f.PropertyType == fkType);
			if (hasFk != null)
				return hasFk.PropertyName;
			return null;

			//var prop = type.GetPropertiesEx().FirstOrDefault(info =>
			//{
			//	if (!info.GetGetMethod().IsVirtual)
			//	{
			//		return false;
			//	}

			//	if (info.PropertyType == fkType)
			//		return true;
			//	return false;
			//});
			//return prop == null ? null : prop.Name;
		}

		/// <summary>
		///     Get the forgin key based that contains the
		///     <paramref name="databaseName" />
		/// </summary>
		/// <returns></returns>
		public static string GetFK(this Type type, string databaseName)
		{
			var classInfo = type.GetClassInfo();
			return classInfo.GetDbToLocalSchemaMapping(databaseName);
		}

		/// <summary>
		///     retruns the Value of
		///     <paramref name="databaseName" />
		///     in the type of
		///     <paramref name="source" />
		/// </summary>
		/// <typeparam name="E"></typeparam>
		/// <returns></returns>
		public static E GetFK<E>(this object source, string databaseName)
		{
			var type = source.GetType();
			string pk = type.GetFK(databaseName);
			DbPropertyInfoCache val;
			type.GetClassInfo().Propertys.TryGetValue(pk, out val);
			return (E)val.GetConvertedValue(source);
		}

		/// <summary>
		///     retruns the Value of
		///     <paramref name="databaseName" />
		///     in the type of
		///     <typeparamref name="T"></typeparamref>
		/// </summary>
		/// <typeparam name="TE"></typeparam>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static TE GetFK<T, TE>(this T source, string databaseName)
		{
			string pk = typeof(T).GetFK(databaseName);
			DbPropertyInfoCache val;
			typeof(T).GetClassInfo().Propertys.TryGetValue(pk, out val);
			return (TE)val.GetConvertedValue(source);
		}

		internal static object GetConvertedValue(this DbPropertyInfoCache source, object instance)
		{
			var converterAttributeModel =
				source.Attributes.FirstOrDefault(s => s.Attribute is ValueConverterAttribute);

			if (converterAttributeModel != null)
			{
				var converterAtt = (converterAttributeModel.Attribute as ValueConverterAttribute);
				var valueConverter = converterAtt.CreateConverter();
				return valueConverter.ConvertBack(source.Getter.Invoke(instance), null, converterAtt.Parameter,
					CultureInfo.CurrentCulture);
			}
			return source.Getter.Invoke(instance);
		}

		/// <summary>
		///     Gets the PK value of the Object
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static object GetPK<T>(this T source)
		{
			return GetPK<T, object>(source);
		}

		/// <summary>
		///     Gets the PK value of the Object
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static Type GetPKType<T>(this T source)
		{
			string pk = source.GetType().GetPKPropertyName();
			return source.GetType().GetProperty(pk).PropertyType;
		}

		/// <summary>
		///     Gets the Primary key of
		///     <typeparamref name="T"></typeparamref>
		///     and convert it the
		///     <typeparamref name="E"></typeparamref>
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="E"></typeparam>
		/// <returns></returns>
		public static E GetPK<T, E>(this T source)
		{
			string pk = typeof(T).GetPKPropertyName();
			DbPropertyInfoCache val;
			typeof(T).GetClassInfo().Propertys.TryGetValue(pk, out val);
			return (E)val.GetConvertedValue(source);
		}

		/// <summary>
		///     Checks the info declaring type to be an List
		/// </summary>
		/// <returns></returns>
		public static bool CheckForListInterface(this PropertyInfo info)
		{
			if (info.PropertyType == typeof(string))
				return false;
			if (info.PropertyType.GetInterface(typeof(IEnumerable).Name) != null)
				return true;
			return info.PropertyType.GetInterface(typeof(IEnumerable<>).Name) != null;
		}

		/// <summary>
		///     Checks the info declaring type to be an List
		/// </summary>
		/// <returns></returns>
		public static bool CheckForListInterface(this DbPropertyInfoCache info)
		{
			if (info.PropertyType == typeof(string))
				return false;
			if (info.PropertyType.GetInterface(typeof(IEnumerable).Name) != null)
				return true;
			return info.PropertyType.GetInterface(typeof(IEnumerable<>).Name) != null;
		}

		/// <summary>
		///     Checks the object instance to be an List
		/// </summary>
		/// <returns></returns>
		public static bool CheckForListInterface(this object info)
		{
			return !(info is string) &&
				   info.GetType().GetInterface(typeof(IEnumerable).Name) != null &&
				   info.GetType().GetInterface(typeof(IEnumerable<>).Name) != null;
		}

		/// <summary>
		///     returns all propertys that are marked as Forgin keys
		/// </summary>
		/// <returns></returns>
		public static DbPropertyInfoCache[] GetNavigationProps(this Type type)
		{
			return type.GetClassInfo().Propertys.Where(s => s.Value.ForginKeyAttribute != null).Select(s => s.Value).ToArray();
		}

		/// <summary>
		///     returns all propertys that are marked as Forgin keys
		/// </summary>
		/// <returns></returns>
		public static DbPropertyInfoCache[] GetNavigationProps<T>()
		{
			return GetNavigationProps(typeof(T));
		}			

		/// <summary>
		///     Sets the infomations from the
		///     <paramref name="reader" />
		///     into the given object
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		//public static T SetPropertysViaReflection<T>(DbClassInfoCache info, IDataRecord reader)
		//	where T : class
		//{
		//	return (T)info.SetPropertysViaReflection(reader);
		//}

		/// <summary>
		///     Factory
		///     Will enumerate the
		///     <paramref name="rec" />
		///     and wrapps all infos into a Egar record
		/// </summary>
		/// <returns></returns>
		public static EgarDataRecord CreateEgarRecord(this IDataRecord rec)
		{
			return new EgarDataRecord(rec);
		}	

			

		/// <summary>
		///     Returns all Cached Propertys from a <paramref name="type" />
		/// </summary>
		/// <returns></returns>
		public static IEnumerable<string> GetPropertysViaRefection(this DbClassInfoCache type, params string[] ignore)
		{
			return
				type
					.Propertys.Select(f => f.Value)
					.Where(f => !ignore.Contains(f.DbName))
					.Select(s => s.PropertyName);
		}
	}
}