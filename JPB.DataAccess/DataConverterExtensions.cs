#region Jean-Pierre Bachmann

// Erstellt von Jean-Pierre Bachmann am 13:02

#endregion

#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using JetBrains.Annotations;
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
		///     Creates the query debugger.
		/// </summary>
		/// <param name="command">The command.</param>
		/// <param name="source">The source.</param>
		/// <returns></returns>
		public static QueryDebugger CreateQueryDebugger(this IDbCommand command, IDatabase source)
		{
			return new QueryDebugger(command, source);
		}

		/// <summary>
		///     Gets the Value or DB null
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		internal static object GetDataValue(object value)
		{
			return value ?? DBNull.Value;
		}

		/// <summary>
		///     Gets the Value from a Paramter with Conversion if Nessesary
		/// </summary>
		/// <param name="source">The source.</param>
		/// <param name="config">The configuration.</param>
		/// <param name="name">The name.</param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">
		///     source
		///     or
		///     name
		/// </exception>
		internal static object GetParameterValue(this object source, DbConfig config, string name)
		{
			if (source == null)
			{
				throw new ArgumentNullException("source");
			}

			DbPropertyInfoCache propertyInfo;
			config.GetOrCreateClassInfoCache(source.GetType()).Propertys.TryGetValue(name, out propertyInfo);
			if (propertyInfo == null)
			{
				throw new ArgumentNullException("name");
			}
			return propertyInfo.GetConvertedValue(source);
		}

		/// <summary>
		///     Get and Convert the found PK name into Database name
		/// </summary>
		/// <param name="type">The type.</param>
		/// <param name="config">The configuration.</param>
		/// <returns></returns>
		public static string GetPK(this Type type, DbConfig config)
		{
			return config.GetOrCreateClassInfoCache(type).PrimaryKeyProperty.DbName;
		}

		/// <summary>
		///     Returns All forgin keys of the given type
		/// </summary>
		/// <param name="type">The type.</param>
		/// <param name="config">The configuration.</param>
		/// <returns></returns>
		public static DbPropertyInfoCache[] GetFKs(this Type type, DbConfig config)
		{
			return
				config.GetOrCreateClassInfoCache(type)
					.Propertys.Where(f => f.Value.ForginKeyAttribute != null)
					.Select(f => f.Value)
					.ToArray();
		}

		/// <summary>
		///     Gets the first Forgin key that is of type <paramref name="fkType" />
		/// </summary>
		/// <param name="type">The type.</param>
		/// <param name="fkType">Type of the fk.</param>
		/// <param name="config">The configuration.</param>
		/// <returns></returns>
		public static string GetFK(this Type type, Type fkType, DbConfig config)
		{
			var hasFk = config.GetOrCreateClassInfoCache(type)
				.Propertys
				.Select(f => f.Value)
				.Where(f => f.ForginKeyAttribute != null || f.ForginKeyDeclarationAttribute != null)
				.FirstOrDefault(f => f.PropertyType == fkType || f.ForginKeyDeclarationAttribute != null && f.ForginKeyDeclarationAttribute.Attribute.ForeignType == fkType);
			if (hasFk != null)
			{
				return hasFk.DbName;
			}
			return null;
		}

		/// <summary>
		///     Get the forgin key based that contains the
		///     <paramref name="databaseName" />
		/// </summary>
		/// <param name="type">The type.</param>
		/// <param name="databaseName">Name of the database.</param>
		/// <param name="config">The configuration.</param>
		/// <returns></returns>
		public static string GetFK(this Type type, string databaseName, DbConfig config)
		{
			var classInfo = config.GetOrCreateClassInfoCache(type);
			return classInfo.GetDbToLocalSchemaMapping(databaseName);
		}

		/// <summary>
		///     Copies the propertys.
		/// </summary>
		/// <param name="base">The base.</param>
		/// <param name="newObject">The new object.</param>
		/// <param name="config">The configuration.</param>
		/// <returns></returns>
		internal static bool CopyPropertys(object @base, object newObject, DbConfig config)
		{
			var updated = false;
			var propertys = config.GetOrCreateClassInfoCache(@base.GetType()).Propertys.Select(f => f.Value);
			foreach (var propertyInfo in propertys)
			{
				var oldValue = propertyInfo.GetConvertedValue(@base);
				var newValue = propertyInfo.GetConvertedValue(newObject);

				if (newValue == null && oldValue == null ||
				    oldValue != null && (newValue == null || newValue.Equals(oldValue)))
				{
					continue;
				}

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

		/// <summary>
		///     Gets the converted value.
		/// </summary>
		/// <param name="source">The source.</param>
		/// <param name="instance">The instance.</param>
		/// <returns></returns>
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

		internal static object GetPK<T>(this T source, DbConfig config)
		{
			var pk = config.GetOrCreateClassInfoCache(typeof(T)).PrimaryKeyProperty.PropertyName;
			DbPropertyInfoCache val;
			config.GetOrCreateClassInfoCache(typeof(T)).Propertys.TryGetValue(pk, out val);
			if (val == null)
			{
				throw new InvalidOperationException("This Operation requires and Primary Key attribute on the entity to succeed");
			}
			return val.GetConvertedValue(source);
		}

		/// <summary>
		///     Checks the info declaring type to be an List
		/// </summary>
		/// <param name="info">The information.</param>
		/// <returns></returns>
		public static bool CheckForListInterface(this PropertyInfo info)
		{
			if (info.PropertyType == typeof(string))
			{
				return false;
			}
			if (info.PropertyType.GetInterface(typeof(IEnumerable).Name) != null)
			{
				return true;
			}
			return info.PropertyType.GetInterface(typeof(IEnumerable<>).Name) != null;
		}

		/// <summary>
		///     Checks the info declaring type to be an List
		/// </summary>
		/// <param name="info">The information.</param>
		/// <returns></returns>
		public static bool CheckForListInterface(this DbPropertyInfoCache info)
		{
			if (info.PropertyType == typeof(string))
			{
				return false;
			}
			if (info.PropertyType.GetInterface(typeof(IEnumerable).Name) != null)
			{
				return true;
			}
			return info.PropertyType.GetInterface(typeof(IEnumerable<>).Name) != null;
		}

		/// <summary>
		///     Checks the object instance to be an List
		/// </summary>
		/// <param name="info">The information.</param>
		/// <returns></returns>
		internal static bool CheckForListInterface(this object info)
		{
			return !(info is string) &&
			       info.GetType().GetInterface(typeof(IEnumerable).Name) != null &&
			       info.GetType().GetInterface(typeof(IEnumerable<>).Name) != null;
		}

		/// <summary>
		///     returns all propertys that are marked as Forgin keys
		/// </summary>
		/// <param name="type">The type.</param>
		/// <param name="config">The configuration.</param>
		/// <returns></returns>
		public static DbPropertyInfoCache[] GetNavigationProps(this Type type, DbConfig config)
		{
			return
				config.GetOrCreateClassInfoCache(type)
					.Propertys.Where(s => s.Value.ForginKeyAttribute != null)
					.Select(s => s.Value)
					.ToArray();
		}

		/// <summary>
		///     returns all propertys that are marked as Forgin keys
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="config">The configuration.</param>
		/// <returns></returns>
		internal static DbPropertyInfoCache[] GetNavigationProps<T>(DbConfig config)
		{
			return GetNavigationProps(typeof(T), config);
		}

		private static IDictionary<string, Type[]> _conversionEquality = new Dictionary<string, Type[]>()
		{
			{"Number", new[]
			{
				typeof(int),
				typeof(uint),
				typeof(double),
				typeof(decimal),
				typeof(float),
				typeof(short),
				typeof(ushort),
				typeof(byte),
				typeof(sbyte),
				typeof(long),
				typeof(ulong),
			}}
		};

		internal static bool IsNumber(Type type)
		{
			return _conversionEquality["Number"].Any(e => e == type);
		}

		internal static bool ChangeType(ref object value, [NotNull] Type conversion)
		{
			var t = conversion;
			if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>))
			{
				if (value == null)
				{
					return false;
				}

				t = Nullable.GetUnderlyingType(t);
			}

			var valueType = value.GetType();

			if (conversion.IsAssignableFrom(valueType))
			{
				return true;
				//return value;
			}

			if (typeof(Enum).IsAssignableFrom(t))
			{
				// ReSharper disable once UseIsOperator.1
				// ReSharper disable once UseMethodIsInstanceOfType
				if (typeof(long).IsAssignableFrom(valueType))
				{
					value = Enum.ToObject(t, value);
				}
				else if (value is string)
				{
					value = Enum.Parse(t, value as string, true);
				}

				return true;
			}
			else if (typeof(bool).IsAssignableFrom(t))
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

				return true;
			}
			else if (typeof(byte[]).IsAssignableFrom(t))
			{
				if (value is string)
				{
					value = Encoding.Default.GetBytes(value as string);
				}

				return true;
			}
			else if (typeof(DateTimeOffset).IsAssignableFrom(t))
			{
				if (value is string)
				{
					DateTimeOffset val;
					if (DateTimeOffset.TryParse(value.ToString(), out val))
					{
						value = val;
						return true;
						//return val;
					}
				}

				return false;
			}
			else if (typeof(DateTime).IsAssignableFrom(t))
			{
				if (value is string)
				{
					DateTime val;
					if (DateTime.TryParse(value.ToString(), out val))
					{
						value = val;
						return true;
						//return val;
					}
				}

				return false;
				//return value;
			}
			else if (IsNumber(t))
			{
				value = Convert.ChangeType(value, t);
				return true;
			}
			else if (t == typeof(string))
			{
				value = value.ToString();
				return true;
			}
			return false;

			//return Convert.ChangeType(value, t);
		}


		/// <summary>
		///     Returns all Cached Propertys from a <paramref name="type" />
		/// </summary>
		/// <param name="type">The type.</param>
		/// <param name="ignore">The ignore.</param>
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