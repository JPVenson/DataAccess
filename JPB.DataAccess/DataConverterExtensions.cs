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
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using JPB.DataAccess.AdoWrapper;
using JPB.DataAccess.Contacts;
using JPB.DataAccess.DbInfoConfig;
using JPB.DataAccess.DbInfoConfig.DbInfo;
using JPB.DataAccess.DebuggerHelper;
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
			//if (DbAccessLayer.Debugger)
			//{
			//	return new QueryDebugger(command, source);
			//}
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
		///     Gets the Value or DB null
		/// </summary>
		/// <returns></returns>
		internal static object GetDataValue(object value)
		{
			return value ?? DBNull.Value;
		}

		/// <summary>
		///     Gets the Value from a Paramter with Conversion if Nessesary
		/// </summary>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException"></exception>
		internal static object GetParamaterValue(this object source, string name)
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
		internal static DbPropertyInfoCache GetParamater(this object source, string name)
		{
			DbPropertyInfoCache val;
			source.GetType().GetClassInfo().Propertys.TryGetValue(name, out val);
			return val;
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

		internal static bool CopyPropertys(object @base, object newObject)
		{
			var updated = false;
			var propertys = @base.GetType().GetClassInfo().Propertys.Select(f => f.Value);
			foreach (var propertyInfo in propertys)
			{
				var oldValue = propertyInfo.GetConvertedValue(@base);
				var newValue = propertyInfo.GetConvertedValue(newObject);

				if (newValue == null && oldValue == null ||
				    (oldValue != null && (newValue == null || newValue.Equals(oldValue))))
					continue;

				propertyInfo.Setter.Invoke(@base, newValue);
				updated = true;
			}
			return updated;
		}

		internal static object GetDefault(Type type)
		{
			if (type.IsValueType)
			{
				return Activator.CreateInstance(type);
			}
			return null;
		}

		internal static object GetConvertedValue(this DbPropertyInfoCache source, object instance)
		{
			var converterAttributeModel =
				source.Attributes.FirstOrDefault(s => s.Attribute is ValueConverterAttribute);

			if (converterAttributeModel != null)
			{
				var converterAtt = converterAttributeModel.Attribute as ValueConverterAttribute;
				var valueConverter = converterAtt.CreateConverter();
				return valueConverter.ConvertBack(source.Getter.Invoke(instance), null, converterAtt.Parameter,
					CultureInfo.CurrentCulture);
			}
			return source.Getter.Invoke(instance);
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
		internal static object GetPK<T>(this T source)
		{
			var pk = typeof (T).GetClassInfo().PrimaryKeyProperty.PropertyName;
			DbPropertyInfoCache val;
			typeof (T).GetClassInfo().Propertys.TryGetValue(pk, out val);
			return val.GetConvertedValue(source);
		}

		/// <summary>
		///     Checks the info declaring type to be an List
		/// </summary>
		/// <returns></returns>
		public static bool CheckForListInterface(this PropertyInfo info)
		{
			if (info.PropertyType == typeof (string))
				return false;
			if (info.PropertyType.GetInterface(typeof (IEnumerable).Name) != null)
				return true;
			return info.PropertyType.GetInterface(typeof (IEnumerable<>).Name) != null;
		}

		/// <summary>
		///     Checks the info declaring type to be an List
		/// </summary>
		/// <returns></returns>
		public static bool CheckForListInterface(this DbPropertyInfoCache info)
		{
			if (info.PropertyType == typeof (string))
				return false;
			if (info.PropertyType.GetInterface(typeof (IEnumerable).Name) != null)
				return true;
			return info.PropertyType.GetInterface(typeof (IEnumerable<>).Name) != null;
		}

		/// <summary>
		///     Checks the object instance to be an List
		/// </summary>
		/// <returns></returns>
		internal static bool CheckForListInterface(this object info)
		{
			return !(info is string) &&
			       info.GetType().GetInterface(typeof (IEnumerable).Name) != null &&
			       info.GetType().GetInterface(typeof (IEnumerable<>).Name) != null;
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
		internal static DbPropertyInfoCache[] GetNavigationProps<T>()
		{
			return GetNavigationProps(typeof (T));
		}

		/// <summary>
		///     Sets the infomations from the
		///     <paramref name="reader" />
		///     into the given object
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
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

		internal static object ChangeType(object value, Type conversion)
		{
			var t = conversion;

			if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof (Nullable<>))
			{
				if (value == null)
				{
					return null;
				}

				t = Nullable.GetUnderlyingType(t);
			}

			if (typeof (Enum).IsAssignableFrom(t))
			{
				// ReSharper disable once UseIsOperator.1
				// ReSharper disable once UseMethodIsInstanceOfType
				if (typeof (long).IsAssignableFrom(value.GetType()))
				{
					value = Enum.ToObject(t, value);
				}
				else if (value is string)
				{
					value = Enum.Parse(t, value as string, true);
				}
			}
			else if (typeof (bool).IsAssignableFrom(t))
			{
				if (value is int)
				{
					value = value.Equals(1);
				}
				else if (value is string)
				{
					value = value.Equals("1");
				}
				else if (value is bool)
				{
					value = (bool) value;
				}
			}
			else if (typeof (byte[]).IsAssignableFrom(t))
			{
				if (value is string)
				{
					value = Encoding.Default.GetBytes(value as string);
				}
			}

			return Convert.ChangeType(value, t);
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