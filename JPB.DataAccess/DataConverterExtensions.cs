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
using System.Text;
using JPB.DataAccess.AdoWrapper;
using JPB.DataAccess.Config;
using JPB.DataAccess.Config.Model;
using JPB.DataAccess.Contacts;
using JPB.DataAccess.DbCollection;
using JPB.DataAccess.DebuggerHelper;
using JPB.DataAccess.Helper;
using JPB.DataAccess.Manager;
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
		///     <paramref name="t"/>
		///     for Generics
		///     This would indicate that the call of the proc could return some data
		/// </summary>
		/// <returns></returns>
		public static bool CheckForResultProcedure(Type t)
		{
			Type[] attStatus = t.GetGenericArguments();
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
		///     Gets the table name from an Entity
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns>The class name or if contains the ForModel name</returns>
		public static string GetTableName<T>()
		{
			return typeof (T).GetTableName();
		}

		/// <summary>
		///     Gets the table name from an Entity
		/// </summary>
		/// <returns>The class name or if contains the ForModel name</returns>
		public static string GetTableName(this Type type)
		{
			var forModel = type.GetCustomAttributes().FirstOrDefault(s => s is ForModel) as ForModel;
			if (forModel != null)
				return forModel.AlternatingName;
			return type.Name;
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
			PropertyInfoCache propertyInfo = GetParamater(source, name);
			if (propertyInfo == null)
				throw new ArgumentNullException("name");
			return propertyInfo.GetConvertedValue(source);
		}

		/// <summary>
		///     retuns the Cashed Property info from Refection Cash
		/// </summary>
		/// <returns></returns>
		public static PropertyInfoCache GetParamater(this object source, string name)
		{
			PropertyInfoCache val;
			source.GetType().GetClassInfo().PropertyInfoCaches.TryGetValue(name, out val);
			return val;
		}

		/// <summary>
		///     Checks a
		///     <paramref name="info"/>
		///     to be a Primary Key
		/// </summary>
		/// <returns></returns>
		public static bool CheckForPK(this PropertyInfo info)
		{
			return info.GetCustomAttributes().Any(s => s is PrimaryKeyAttribute) || (info.Name.EndsWith("_ID"));
		}

		/// <summary>
		///     Checks a
		///     <paramref name="info"/>
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
			PropertyInfo name = type.GetPropertiesEx().FirstOrDefault(CheckForPK);
			return name == null ? null : name.Name;
		}

		/// <summary>
		///     Get and Convert the found PK name into Database name
		/// </summary>
		/// <returns></returns>
		public static string GetPK(this Type type)
		{
			PropertyInfo name = type.GetPropertiesEx().FirstOrDefault(CheckForPK);
			return type.GetLocalToDbSchemaMapping(name == null ? null : name.Name);
		}

		/// <summary>
		///     Returns All forgin keys of the given type
		/// </summary>
		/// <returns></returns>
		public static PropertyInfo[] GetFKs(this Type type)
		{
			return type.GetPropertiesEx().Where(CheckForFK).ToArray();
		}

		/// <summary>
		///     Gets the first Forgin key that is of type <paramref name="fkType"/>
		/// </summary>
		/// <returns></returns>
		public static string GetFK(this Type type, Type fkType)
		{
			PropertyInfo prop = type.GetPropertiesEx().FirstOrDefault(info =>
			{
				if (!info.GetGetMethod().IsVirtual)
				{
					return false;
				}

				if (info.PropertyType == fkType)
					return true;
				return false;
			});
			return prop == null ? null : prop.Name;
		}

		/// <summary>
		///     Get the forgin key based that contains the
		///     <paramref name="name"/>
		/// </summary>
		/// <returns></returns>
		public static string GetFK(this Type type, string name)
		{
			name = type.GetDbToLocalSchemaMapping(name);
			PropertyInfo prop = type.GetPropertiesEx().FirstOrDefault(info => CheckForFK(info, name));
			return prop == null ? null : prop.Name;
		}

		/// <summary>
		///     retruns the Value of
		///     <paramref name="name"/>
		///     in the type of
		///     <paramref name="source"/>
		/// </summary>
		/// <typeparam name="E"></typeparam>
		/// <returns></returns>
		public static E GetFK<E>(this object source, string name)
		{
			Type type = source.GetType();
			string pk = type.GetFK(name);
			PropertyInfoCache val;
			type.GetClassInfo().PropertyInfoCaches.TryGetValue(pk, out val);
			return (E) val.GetConvertedValue(source);
		}

		/// <summary>
		///     retruns the Value of
		///     <paramref name="name"/>
		///     in the type of
		///     <typeparamref name="T"></typeparamref>
		/// </summary>
		/// <typeparam name="TE"></typeparam>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static TE GetFK<T, TE>(this T source, string name)
		{
			string pk = typeof (T).GetFK(name);
			PropertyInfoCache val;
			typeof (T).GetClassInfo().PropertyInfoCaches.TryGetValue(pk, out val);
			return (TE) val.GetConvertedValue(source);
		}

		internal static object GetConvertedValue(this PropertyInfoCache source, object instance)
		{
			AttributeInfoCache converterAttributeModel =
				source.AttributeInfoCaches.FirstOrDefault(s => s.Attribute is ValueConverterAttribute);

			if (converterAttributeModel != null)
			{
				var converterAtt = (converterAttributeModel.Attribute as ValueConverterAttribute);
				IValueConverter valueConverter = converterAtt.CreateConverter();
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
			string pk = typeof (T).GetPKPropertyName();
			PropertyInfoCache val;
			typeof (T).GetClassInfo().PropertyInfoCaches.TryGetValue(pk, out val);
			return (E) val
				.GetConvertedValue(source);
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
		public static bool CheckForListInterface(this PropertyInfoCache info)
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
		public static bool CheckForListInterface(this object info)
		{
			return !(info is string) &&
			       info.GetType().GetInterface(typeof (IEnumerable).Name) != null &&
			       info.GetType().GetInterface(typeof (IEnumerable<>).Name) != null;
		}

		/// <summary>
		///     returns all propertys that are marked as Forgin keys
		/// </summary>
		/// <returns></returns>
		public static PropertyInfo[] GetNavigationProps(this Type type)
		{
			return type.GetPropertiesEx().Where(s => s.GetGetMethod(false).IsVirtual).ToArray();
		}

		/// <summary>
		///     returns all propertys that are marked as Forgin keys
		/// </summary>
		/// <returns></returns>
		public static PropertyInfo[] GetNavigationProps<T>()
		{
			return GetNavigationProps(typeof (T));
		}

		/// <summary>
		///     ToBeSupported
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static T LoadNavigationProps<T>(this T source, IDatabase accessLayer, bool egarloading = false)
		{
			return (T) LoadNavigationProps(source as object, accessLayer, egarloading);
		}

		/// <summary>
		///     ToBeSupported
		/// </summary>
		/// <returns></returns>
		public static object LoadNavigationProps(this object source, IDatabase accessLayer, bool egarLoading)
		{
			//Get nav Propertys
			foreach (PropertyInfo propertyInfo in GetNavigationProps(source.GetType()))
			{
				//var firstOrDefault = source.GetFK<long>(propertyInfo.ClassName);
				IDbCommand sqlCommand;

				var firstOrDefault =
					propertyInfo.GetCustomAttributes().FirstOrDefault(s => s is ForeignKeyAttribute) as
						ForeignKeyAttribute;
				if (firstOrDefault == null)
					continue;
				Type targetType = null;
				if (CheckForListInterface(propertyInfo))
				{
					object pk = source.GetPK();
					string targetName = firstOrDefault.KeyName;
					targetType = propertyInfo.PropertyType.GetGenericArguments().FirstOrDefault();

					if (String.IsNullOrEmpty(targetName))
					{
						targetName = targetType.GetPK();
					}

					sqlCommand = DbAccessLayer.CreateSelect(targetType, accessLayer,
						" WHERE " + targetName + " = @pk", new List<IQueryParameter>
						{
							new QueryParameter
							{
								Name = "@pk",
								Value = pk
							}
						});
				}
				else
				{
					object fkproperty = source.GetParamaterValue(firstOrDefault.KeyName);

					if (fkproperty == null)
						continue;

					targetType = propertyInfo.PropertyType;
					sqlCommand = DbAccessLayer.CreateSelect(targetType, accessLayer, fkproperty);
				}

				object[] orDefault = DbAccessLayer.RunSelect(targetType, accessLayer, sqlCommand, egarLoading);

				//result is list and property is list
				if (CheckForListInterface(orDefault) && CheckForListInterface(propertyInfo))
				{
					ConstructorInfo constructorInfo =
						typeof (DbCollection<>).MakeGenericType(targetType).GetConstructor(new[] {typeof (IEnumerable)});

					Debug.Assert(constructorInfo != null, "constructorInfo != null");
					object reproCollection = constructorInfo.Invoke(new object[] {orDefault});
					propertyInfo.SetValue(source, reproCollection, null);
					foreach (object item in orDefault)
						item.LoadNavigationProps(accessLayer);
				}
				if (CheckForListInterface(propertyInfo))
					continue;

				object @default = orDefault.FirstOrDefault();
				propertyInfo.SetValue(source, @default, null);
				@default.LoadNavigationProps(accessLayer);
			}

			return source;
		}

		/// <summary>
		///     Sets the infomations from the
		///     <paramref name="reader"/>
		///     into the given object
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static T SetPropertysViaReflection<T>(ClassInfoCache info, IDataRecord reader)
			where T : class
		{
			return (T) info.SetPropertysViaReflection(reader);
		}

		/// <summary>
		///     Factory
		///     Will enumerate the
		///     <paramref name="rec"/>
		///     and wrapps all infos into a Egar record
		/// </summary>
		/// <returns></returns>
		public static EgarDataRecord CreateEgarRecord(this IDataRecord rec)
		{
			return new EgarDataRecord(rec);
		}

		/// <summary>
		///     Loads all propertys from a DataReader into the given Object
		/// </summary>
		public static object SetPropertysViaReflection(
			object instance,
			ClassInfoCache info,
			IDataRecord reader,
			Dictionary<int, PropertyInfoCache> cache)
		{
			if (reader == null)
				return instance;

			//Left c# property name and right the object to read from the reader
			//var listofpropertys = new Dictionary<string, object>();

			KeyValuePair<string, PropertyInfoCache>[] propertys = info.PropertyInfoCaches.ToArray();
			var instanceOfFallbackList = new Dictionary<string, object>();

			if (cache == null)
			{
				cache = new Dictionary<int, PropertyInfoCache>();
				for (int i = 0; i < reader.FieldCount; i++)
				{
					PropertyInfoCache val = null;
					info.PropertyInfoCaches.TryGetValue(info.SchemaMappingDatabaseToLocal(reader.GetName(i)), out val);
					cache.Add(i, val);
				}
			}

			//for (int i = 0; i < reader.FieldCount; i++)
			//{
			//	var dbName = info.SchemaMappingValues.ElementAt(i).Key;
			//	listofpropertys.Append(info.SchemaMappingDatabaseToLocal(dbName), reader.GetValue(i));
			//}

			//foreach (var schemaMappingValue in info.SchemaMappingValues)
			//{
			//}

			//for (int i = 0; i < reader.FieldCount; i++)
			//	listofpropertys.Append(type.GetDbToLocalSchemaMapping(reader.GetName(i)), reader.GetValue(i));


			for (int i = 0; i < reader.FieldCount; i++)
			{
				PropertyInfoCache property = cache[i];
				object value = reader.GetValue(i);

				if (property != null)
				{
					HashSet<AttributeInfoCache> attributes = property.AttributeInfoCaches;
					AttributeInfoCache valueConverterAttributeModel =
						attributes.FirstOrDefault(s => s.Attribute is ValueConverterAttribute);

					//Should the SQL value be converted
					if (valueConverterAttributeModel != null)
					{
						var converter = valueConverterAttributeModel.Attribute as ValueConverterAttribute;
						//Create the converter and then convert the value before everything else
						IValueConverter valueConverter = converter.CreateConverter();
						value = valueConverter.Convert(value, property.PropertyInfo.PropertyType, converter.Parameter,
							CultureInfo.CurrentCulture);
					}

					AttributeInfoCache xmlAttributeModel =
						attributes.FirstOrDefault(s => s.Attribute is FromXmlAttribute);

					//should the Content be considerd as XML text?
					if (xmlAttributeModel != null)
					{
						//Get the XML text and check if its null or empty
						string xmlStream = value.ToString();
						if (String.IsNullOrEmpty(xmlStream))
						{
							continue;
						}

						//Check for List
						//if this is a list we are expecting other entrys inside
						if (CheckForListInterface(property))
						{
							//target Property is of type list
							//so expect a xml valid list Take the first element and expect the propertys inside this first element
							XmlDataRecord record = XmlDataRecord.TryParse(xmlStream,
								property.PropertyInfo.PropertyType.GetGenericArguments().FirstOrDefault());
							IEnumerable<XmlDataRecord> xmlDataRecords = record.CreateListOfItems();

							ClassInfoCache genericArguments =
								property.PropertyInfo.PropertyType.GetGenericArguments().FirstOrDefault().GetClassInfo();
							List<object> enumerableOfItems = xmlDataRecords.Select(genericArguments.SetPropertysViaReflection).ToList();
							object castedList;

							if (genericArguments.Type.IsClass && genericArguments.Type.GetInterface("INotifyPropertyChanged") != null)
							{
								ConstructorInfo caster =
									typeof (DbCollection<>).MakeGenericType(genericArguments.Type).GetConstructor(new[] {typeof (IEnumerable)});

								Debug.Assert(caster != null, "caster != null");

								castedList = caster.Invoke(new object[] {enumerableOfItems});
							}
							else
							{
								ConstructorInfo caster =
									typeof (NonObservableDbCollection<>).MakeGenericType(genericArguments.Type)
										.GetConstructor(new[] {typeof (IEnumerable)});

								Debug.Assert(caster != null, "caster != null");

								castedList = caster.Invoke(new object[] {enumerableOfItems});
							}

							property.Setter.Invoke(instance, castedList);
						}
						else
						{
							//the t
							object xmlSerilizedProperty = property
								.PropertyInfo
								.PropertyType
								.GetClassInfo()
								.SetPropertysViaReflection(XmlDataRecord.TryParse(xmlStream, property.PropertyInfo.PropertyType));

							property.Setter.Invoke(instance, xmlSerilizedProperty);
						}
					}
					else if (value is DBNull || value == null)
					{
						property.Setter.Invoke(instance, new object[] {null});
					}
					else
					{
						object changedType;
						if (value.GetType() != property.PropertyInfo.PropertyType)
						{
							changedType = ChangeType(value, property.PropertyInfo.PropertyType);
						}
						else
						{
							changedType = value;
						}

						property.Setter.Invoke(instance, changedType);
					}
				}
					//This variable is null if we tried to find a property with the LoadNotImplimentedDynamicAttribute but did not found it
				else if (instanceOfFallbackList != null)
				{
					//no property found Look for LoadNotImplimentedDynamicAttribute property to include it

					if (instanceOfFallbackList.Any())
					{
						instanceOfFallbackList.Add(reader.GetName(i), value);
					}
					else
					{
						KeyValuePair<string, PropertyInfoCache> maybeFallbackProperty =
							propertys.FirstOrDefault(
								s => s.Value.AttributeInfoCaches.Any(e => e.Attribute is LoadNotImplimentedDynamicAttribute));
						if (!maybeFallbackProperty.Equals(default(KeyValuePair<string, PropertyInfoCache>)))
						{
							instanceOfFallbackList = (Dictionary<string, object>) maybeFallbackProperty.Value.Getter.Invoke(instance);
							if (instanceOfFallbackList == null)
							{
								instanceOfFallbackList = new Dictionary<string, object>();
								maybeFallbackProperty.Value.Setter.Invoke(instance, instanceOfFallbackList);
							}
							instanceOfFallbackList.Add(reader.GetName(i), value);
						}
						else
						{
							instanceOfFallbackList = null;
						}
					}
				}
			}


			//foreach (var item in listofpropertys)
			//{
			//	var property = propertys.FirstOrDefault(s => s.PropertyName == item.Key);

			//}

			if (reader is EgarDataRecord)
			{
				(reader as EgarDataRecord).Dispose();
			}

			return instance;
		}

		internal static object ChangeType(object value, Type conversion)
		{
			Type t = conversion;

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
		///     Creates an instance based on a Ctor injection or Reflection loading
		/// </summary>
		/// <returns></returns>
		public static object CreateInstance(this ClassInfoCache classInfo, IDataRecord reader, out bool fullLoaded)
		{
			if (classInfo.Factory != null)
			{
				fullLoaded = classInfo.FullFactory;
				return classInfo.Factory(reader);
			}

			ConstructorInfo[] constructorInfos = classInfo.ConstructorInfoCaches.Select(f => f.MethodInfo).ToArray();

			ConstructorInfo constructor =
				constructorInfos.FirstOrDefault(s => s.GetCustomAttributes().Any(e => e is ObjectFactoryMethodAttribute)) ??
				constructorInfos.FirstOrDefault(s =>
				{
					ParameterInfo[] parameterInfos = s.GetParameters();
					return parameterInfos.Length == 1 && parameterInfos.First().ParameterType == typeof (IDataRecord);
				});

			//maybe single ctor with param

			if (constructor != null)
			{
				ParameterInfo[] parameterInfos = constructor.GetParameters();
				if (parameterInfos.Length == 1 && parameterInfos.First().ParameterType == typeof (IDataRecord))
				{
					classInfo.FullFactory = true;
					classInfo.Factory = s => constructor.Invoke(new object[] {s});
					return classInfo.CreateInstance(reader, out fullLoaded);
				}
			}
			else
			{
				//check for a Factory mehtod
				MethodInfoCache factory =
					classInfo.MethodInfoCaches
						.FirstOrDefault(s => s.AttributeInfoCaches.Any(f => f.Attribute is ObjectFactoryMethodAttribute));

				if (factory != null)
				{
					MethodInfo method = factory.MethodInfo;
					if (method.IsStatic)
					{
						ParameterInfo[] returnParameter = method.GetParameters();
						ParameterInfo returnType = method.ReturnParameter;

						if (returnType != null && returnType.ParameterType == classInfo.Type)
						{
							if (returnParameter.Length == 1 &&
							    returnParameter.First().ParameterType == typeof (IDataRecord))
							{
								classInfo.FullFactory = true;
								classInfo.Factory = s => method.Invoke(null, new object[] {reader});
								return classInfo.CreateInstance(reader, out fullLoaded);
							}
						}
					}
				}
			}

			classInfo.FullFactory = false;
			classInfo.Factory = s => constructorInfos.First().Invoke(new object[0]);
			return classInfo.CreateInstance(reader, out fullLoaded);
		}

		/// <summary>
		///     Creates a new Instance based on possible Ctor's and the given
		///     <paramref name="reader"/>
		/// </summary>
		/// <returns></returns>
		public static object SetPropertysViaReflection(this ClassInfoCache type, IDataRecord reader)
		{
			bool created;
			object source = type.CreateInstance(reader, out created);
			if (created)
				return source;

			return SetPropertysViaReflection(source, type, reader, null);
		}


		/// <summary>
		///     Creates a new Instance based on possible Ctor's and the given
		///     <paramref name="reader"/>
		/// </summary>
		/// <returns></returns>
		public static object SetPropertysViaReflection(this ClassInfoCache type, IDataRecord reader,
			Dictionary<int, PropertyInfoCache> mapping)
		{
			bool created;
			object source = type.CreateInstance(reader, out created);
			if (created)
				return source;

			return SetPropertysViaReflection(source, type, reader, mapping);
		}

		/// <summary>
		///     Returns all Cached Propertys from a <paramref name="type" />
		/// </summary>
		/// <returns></returns>
		public static IEnumerable<string> GetPropertysViaRefection(this Type type, params string[] ignore)
		{
			return
				type.GetClassInfo()
					.PropertyInfoCaches.Select(f => f.Value)
					.Where(f => !ignore.Contains(f.DbName))
					.Select(s => s.PropertyName);
		}
	}
}