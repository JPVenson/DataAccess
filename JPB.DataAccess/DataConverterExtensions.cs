#region Jean-Pierre Bachmann

// Erstellt von Jean-Pierre Bachmann am 13:02

#endregion

#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using JPB.DataAccess.AdoWrapper;
using JPB.DataAccess.DebuggerHelper;
using JPB.DataAccess.Helper;
using JPB.DataAccess.Manager;
using JPB.DataAccess.ModelsAnotations;
using JPB.DataAccess.DbCollection;
using System.Data.SqlTypes;

#endregion

namespace JPB.DataAccess
{
    /// <summary>
    /// Helper Extentions for Maintaining Value
    /// </summary>
#if !DEBUG
    [DebuggerStepThrough]
#endif
    public static class DataConverterExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public static QueryDebugger CreateQueryDebugger(this IDbCommand command, IDatabase source = null)
        {
            return new QueryDebugger(command, source);
        }

        /// <summary>
        /// Checks <param name="t"></param> for Generics
        /// This would indicate that the call of the proc could return some data
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static bool CheckForResultProcedure(Type t)
        {
            var attStatus = t.GetGenericArguments();
            return attStatus.Any();
        }

        /// <summary>
        /// Gets the Value or DB null
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static object GetDataValue(object value)
        {
            return value ?? DBNull.Value;
        }

        /// <summary>
        /// Gets the table name from an Entity
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>The class name or if contains the ForModel name</returns>
        public static string GetTableName<T>()
        {
            return typeof(T).GetTableName();
        }

        /// <summary>
        /// Gets the table name from an Entity
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>The class name or if contains the ForModel name</returns>
        public static string GetTableName(this Type type)
        {
            var forModel = type.GetCustomAttributes().FirstOrDefault(s => s is ForModel) as ForModel;
            if (forModel != null)
                return forModel.AlternatingName;
            return type.Name;
        }

        /// <summary>
        /// Gets the Value from a Paramter with Conversion if Nessesary
        /// </summary>
        /// <param name="source"></param>
        /// <param name="name"></param>
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
        /// retuns the Cashed Property info from Refection Cash
        /// </summary>
        /// <param name="source"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static PropertyInfo GetParamater(this object source, string name)
        {
            return ConfigHelper.GetPropertiesEx(source.GetType()).FirstOrDefault(s => s.Name == name);
        }

        /// <summary>
        /// Checks a <param name="info"></param> to be a Primary Key
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public static bool CheckForPK(this PropertyInfo info)
        {
            return info.GetCustomAttributes().Any(s => s is PrimaryKeyAttribute) || (info.Name.EndsWith("_ID"));
        }
        /// <summary>
        /// Checks a <param name="info"></param> to be a Primary Key
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public static bool CheckForFK(this PropertyInfo info, string name)
        {
            if (info.Name != name)
                return false;
            return info.GetCustomAttributes().Any(s => s is PrimaryKeyAttribute);
        }

        /// <summary>
        /// Checks a Property to BE handled as a Forgine Key from an Other class
        /// (Checks for PrimaryKey)
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public static bool CheckForFK(this PropertyInfo info)
        {
            return info.GetCustomAttributes().Any(s => s is PrimaryKeyAttribute);
        }

        /// <summary>
        /// Returns the Primarykey name (Converted) if exists
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetPKPropertyName(this Type type)
        {
            PropertyInfo name = ConfigHelper.GetPropertiesEx(type).FirstOrDefault(CheckForPK);
            return name == null ? null : name.Name;
        }

        /// <summary>
        ///     Get and Convert the found PK name into Database name
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetPK(this Type type)
        {
            PropertyInfo name = ConfigHelper.GetPropertiesEx(type).FirstOrDefault(CheckForPK);
            return MapEntiysPropToSchema(type, name == null ? null : name.Name);
        }

        /// <summary>
        /// Returns All forgin keys of the given type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static PropertyInfo[] GetFKs(this Type type)
        {
            return ConfigHelper.GetPropertiesEx(type).Where(CheckForFK).ToArray();
        }

        /// <summary>
        /// Get the forgin key based that contains the <param name="name"></param>
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetFK(this Type type, Type fkType)
        {
            PropertyInfo prop = ConfigHelper.GetPropertiesEx(type).FirstOrDefault(info =>
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
        /// Get the forgin key based that contains the <param name="name"></param>
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetFK(this Type type, string name)
        {
            name = type.ReMapSchemaToEntiysProp(name);
            PropertyInfo prop = ConfigHelper.GetPropertiesEx(type).FirstOrDefault(info => CheckForFK(info, name));
            return prop == null ? null : prop.Name;
        }

        /// <summary>
        /// retruns the Value of <param name="name"></param> in the type of <param name="source"></param>
        /// </summary>
        /// <param name="source"></param>
        /// <param name="name"></param>
        /// <typeparam name="E"></typeparam>
        /// <returns></returns>
        public static E GetFK<E>(this object source, string name)
        {
            Type type = source.GetType();
            string pk = type.GetFK(name);
            return (E)type.GetProperty(pk).GetConvertedValue(source);
        }

        /// <summary>
        /// retruns the Value of <param name="name"></param> in the type of <typeparam name="T"></typeparam>
        /// </summary>
        /// <param name="source"></param>
        /// <param name="name"></param>
        /// <typeparam name="TE"></typeparam>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static TE GetFK<T, TE>(this T source, string name)
        {
            string pk = typeof(T).GetFK(name);
            return (TE)typeof(T).GetProperty(pk).GetConvertedValue(source);
        }

        internal static object GetConvertedValue(this PropertyInfo source, object instance)
        {
            var converter = source.GetCustomAttributes().FirstOrDefault(s => s is ValueConverterAttribute) as ValueConverterAttribute;

            if (converter != null)
            {
                var valueConverter = converter.CreateConverter();
                return valueConverter.ConvertBack(source.GetValue(instance), null, converter.Parameter, CultureInfo.CurrentCulture);
            }
            return source.GetValue(instance);
        }

        /// <summary>
        ///     Gets the PK value of the Object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static object GetPK<T>(this T source)
        {
            return GetPK<T, object>(source);
        }

        /// <summary>
        ///     Gets the PK value of the Object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static Type GetPKType<T>(this T source)
        {
            string pk = source.GetType().GetPKPropertyName();
            return source.GetType().GetProperty(pk).PropertyType;
        }


        /// <summary>
        /// Gets the Primary key of <typeparam name="T"></typeparam> and convert it the <typeparam name="E"></typeparam>
        /// </summary>
        /// <param name="source"></param>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="E"></typeparam>
        /// <returns></returns>
        public static E GetPK<T, E>(this T source)
        {
            string pk = typeof(T).GetPKPropertyName();
            return (E)ConfigHelper.GetPropertiesEx(typeof(T)).First(s => s.Name == pk).GetConvertedValue(source);
        }



        /// <summary>
        /// Returns an Orderd list of all Converted names that <param name="type"></param> contains, exept for all Propertynames that are defined in <param name="ignore"></param>
        /// </summary>
        /// <param name="type"></param>
        /// <param name="ignore"></param>
        /// <returns></returns>
        public static IEnumerable<string> MapEntiyToSchema(Type type, string[] ignore)
        {
            foreach (var s1 in ConfigHelper.GetPropertiesEx(type))
            {
                if (ignore.Contains(s1.Name))
                    continue;

                if (s1.GetGetMethod().IsVirtual && s1.GetCustomAttributes().Any(s =>
                {
                    var isAttr = s is FromXmlAttribute;

                    if (!isAttr)
                        return false;

                    var att = s as FromXmlAttribute;
                    if (att.LoadStrategy == LoadStrategy.IncludeInSelect)
                        return true;
                    return false;
                }))
                {
                    yield return ((FromXmlAttribute)s1.GetCustomAttributes().First(s => s is FromXmlAttribute)).FieldName;
                }
                else if (!s1.GetGetMethod().IsVirtual && !s1.GetCustomAttributes().Any(s => s is IgnoreReflectionAttribute))
                {
                    var formodle = s1.GetCustomAttributes().FirstOrDefault(s => s is ForModel) as ForModel;
                    yield return formodle != null ? formodle.AlternatingName : s1.Name;
                }
            }
        }

        /// <summary>
        /// Returns an Orderd list of all Converted names that <typeparam name="T"></typeparam> contains, exept for all Propertynames that are defined in <param name="ignore"></param>
        /// </summary>
        /// <param name="ignore"></param>
        /// <returns></returns>
        public static IEnumerable<string> MapEntiyToSchema<T>(string[] ignore)
        {
            return MapEntiyToSchema(typeof(T), ignore);
        }

        /// <summary>
        /// Maps one propertyname of <param name="type"></param> into the corresponding DbName that is defined by the object
        /// If you want to convert multible names call MapEntiyToSchema
        /// </summary>
        /// <param name="type"></param>
        /// <param name="prop"></param>
        /// <returns></returns>
        public static string MapEntiysPropToSchema(this Type type, string prop)
        {
            return (from propertyInfo in ConfigHelper.GetPropertiesEx(type)
                    where propertyInfo.Name == prop
                    let formodle =
                            propertyInfo.GetCustomAttributes().FirstOrDefault(s => s is ForModel) as ForModel
                    select formodle != null ? formodle.AlternatingName : propertyInfo.Name).FirstOrDefault();
        }

        /// <summary>
        /// Maps one propertyname of <param name="type"></param> into the corresponding DbName that is defined by the object
        /// If you want to convert multible names call MapEntiyToSchema
        /// </summary>
        /// <param name="prop"></param>
        /// <returns></returns>
        public static string MapEntiysPropToSchema<T>(string prop)
        {
            return MapEntiysPropToSchema(typeof(T), prop);
        }

        /// <summary>
        /// Maps a DbName into the corresponding C# property or class
        /// </summary>
        /// <param name="prop"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static string ReMapSchemaToEntiysProp<T>(string prop)
        {
            return ReMapSchemaToEntiysProp(typeof(T), prop);
        }

        /// <summary>
        /// Maps a DbName into the corresponding C# property or class
        /// </summary>
        /// <param name="type"></param>
        /// <param name="prop"></param>
        /// <returns></returns>
        public static string ReMapSchemaToEntiysProp(this Type type, string prop)
        {
            foreach (var propertyInfo in from propertyInfo in ConfigHelper.GetPropertiesEx(type)
                                         let customAttributes =
                                                 propertyInfo.GetCustomAttributes()
                                                 .FirstOrDefault(s => s is ForModel) as ForModel
                                         where
                                             customAttributes != null &&
                                             customAttributes.AlternatingName == prop
                                         select propertyInfo)
                return propertyInfo.Name;
            return prop;
        }

        /// <summary>
        /// Checks the info declaring type to be an List
        /// </summary>
        /// <param name="info"></param>
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
        /// Checks the object instance to be an List
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public static bool CheckForListInterface(this object info)
        {
            return !(info is string) &&
                   info.GetType().GetInterface(typeof(IEnumerable).Name) != null &&
                   info.GetType().GetInterface(typeof(IEnumerable<>).Name) != null;
        }

        /// <summary>
        /// returns all propertys that are marked as Forgin keys
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static PropertyInfo[] GetNavigationProps(this Type type)
        {
            return ConfigHelper.GetPropertiesEx(type).Where(s => s.GetGetMethod(false).IsVirtual).ToArray();
        }

        /// <summary>
        /// returns all propertys that are marked as Forgin keys
        /// </summary>
        /// <returns></returns>
        public static PropertyInfo[] GetNavigationProps<T>()
        {
            return GetNavigationProps(typeof(T));
        }

        /// <summary>
        /// ToBeSupplied
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="accessLayer"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T LoadNavigationProps<T>(this T source, IDatabase accessLayer)
        {
            return (T)LoadNavigationProps(source as object, accessLayer);
        }

        /// <summary>
        /// ToBeSupplied
        /// </summary>
        /// <param name="source"></param>
        /// <param name="accessLayer"></param>
        /// <returns></returns>
        public static object LoadNavigationProps(this object source, IDatabase accessLayer)
        {

            //Get nav Propertys

            foreach (var propertyInfo in GetNavigationProps(source.GetType()))
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
                    var targetName = firstOrDefault.KeyName;
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
                    sqlCommand = DbAccessLayer.CreateSelect(targetType, accessLayer, (object)fkproperty);
                }

                var orDefault = DbAccessLayer.RunSelect(targetType, accessLayer, sqlCommand);

                //result is list and property is list
                if (CheckForListInterface(orDefault) && CheckForListInterface(propertyInfo))
                {
                    var constructorInfo = typeof(DbCollection<>).MakeGenericType(targetType).GetConstructor(new[] { typeof(IEnumerable) });

                    Debug.Assert(constructorInfo != null, "constructorInfo != null");
                    var reproCollection = constructorInfo.Invoke(new object[] { orDefault });
                    propertyInfo.SetValue(source, reproCollection, null);
                    foreach (var item in orDefault)
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
        /// Sets the infomations from the <param name="reader"></param> into the given object
        /// </summary>
        /// <param name="reader"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T SetPropertysViaReflection<T>(IDataRecord reader)
            where T : class
        {
            return (T)SetPropertysViaReflection(typeof(T), reader);
        }

        /// <summary>
        /// Factory
        /// Will enumerate the <param name="rec"></param> and wrapps all infos into a Egar record
        /// </summary>
        /// <param name="rec"></param>
        /// <returns></returns>
        public static EgarDataRecord CreateEgarRecord(this IDataRecord rec)
        {
            return new EgarDataRecord(rec);
        }

        /// <summary>
        /// Loads all propertys from a DataReader into the given Object
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="reader"></param>
        public static object SetPropertysViaReflection(object instance, IDataRecord reader)
        {
            if (reader == null)
                return instance;

            var listofpropertys = new Dictionary<string, object>();
            var type = instance.GetType();

            var propertys = ConfigHelper.GetPropertiesEx(type);
            var instanceOfFallbackList = new Dictionary<string, object>();

            for (int i = 0; i < reader.FieldCount; i++)
                listofpropertys.Add(ReMapSchemaToEntiysProp(type, reader.GetName(i)), reader.GetValue(i));

            foreach (var item in listofpropertys)
            {
                var property = propertys.FirstOrDefault(s => s.Name == item.Key);
                if (property != null)
                {
                    var value = item.Value;
                    var attributes = property.GetCustomAttributes();
                    var any = attributes.FirstOrDefault(s => s is ValueConverterAttribute) as ValueConverterAttribute;

                    //Should the SQL value be converted
                    if (any != null)
                    {
                        //Create the converter and then convert the value before everything else
                        var valueConverter = any.CreateConverter();
                        value = valueConverter.Convert(value, property.PropertyType, any.Parameter,
                            CultureInfo.CurrentCulture);
                    }                 

                    var isXmlField =
                        attributes.FirstOrDefault(s => s is FromXmlAttribute) as FromXmlAttribute;

                    //should the Content be considerd as XML text?
                    if (isXmlField != null)
                    {
                        //Get the XML text and check if its null or empty
                        var xmlStream = value.ToString();
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
                            var record = XmlDataRecord.TryParse(xmlStream, property.PropertyType.GetGenericArguments().FirstOrDefault());
                            var xmlDataRecords = record.CreateListOfItems();

                            var genericArguments = property.PropertyType.GetGenericArguments().FirstOrDefault();
                            var enumerableOfItems = xmlDataRecords.Select(xmlDataRecord => SetPropertysViaReflection(genericArguments, xmlDataRecord)).ToList();
                            object castedList;

                            if (genericArguments.IsClass && genericArguments.GetInterface("INotifyPropertyChanged") != null)
                            {
                                var caster = typeof(DbCollection<>).MakeGenericType(genericArguments).GetConstructor(new[] { typeof(IEnumerable) });

                                Debug.Assert(caster != null, "caster != null");

                                castedList = caster.Invoke(new object[] { enumerableOfItems });
                            }
                            else
                            {
                                var caster = typeof(NonObservableDbCollection<>).MakeGenericType(genericArguments).GetConstructor(new[] { typeof(IEnumerable) });

                                Debug.Assert(caster != null, "caster != null");

                                castedList = caster.Invoke(new object[] { enumerableOfItems });
                            }                                    

                            property.SetValue(instance, castedList);
                        }
                        else
                        {
                            //the t
                            var xmlSerilizedProperty = SetPropertysViaReflection(property.PropertyType, XmlDataRecord.TryParse(xmlStream, property.PropertyType));

                            property.SetValue(instance, xmlSerilizedProperty);
                        }
                    }
                    else if (value is DBNull || value == null)
                    {
                        property.SetValue(instance, null, null);
                    }
                    else
                    {
                        var changedType = ChangeType(value, property.PropertyType);
                        property.SetValue(instance, changedType, null);
                    }
                }
                //This variable is null if we tried to find a property with the LoadNotImplimentedDynamicAttribute but did not found it
                else if (instanceOfFallbackList != null)
                {
                    //no property found Look for LoadNotImplimentedDynamicAttribute property to include it

                    if (instanceOfFallbackList.Any())
                    {
                        instanceOfFallbackList.Add(item.Key, item.Value);
                    }
                    else
                    {
                        PropertyInfo maybeFallbackProperty = propertys.FirstOrDefault(s => s.GetCustomAttributes().Any(e => e is LoadNotImplimentedDynamicAttribute));
                        if (maybeFallbackProperty != null)
                        {
                            instanceOfFallbackList = (Dictionary<string, object>)maybeFallbackProperty.GetValue(instance);
                            if (instanceOfFallbackList == null)
                            {
                                instanceOfFallbackList = new Dictionary<string, object>();
                                maybeFallbackProperty.SetValue(instance, instanceOfFallbackList);
                            }
                            instanceOfFallbackList.Add(item.Key, item.Value);
                        }
                        else
                        {
                            instanceOfFallbackList = null;
                        }
                    }
                }
            }

            if (reader is IDisposable)
            {
                (reader as IDisposable).Dispose();
            }

            return instance;
        }

        internal static object ChangeType(object value, Type conversion)
        {
            var t = conversion;

            if (t.IsGenericType && t.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {
                if (value == null)
                {
                    return null;
                }

                t = Nullable.GetUnderlyingType(t);
            }

            if (typeof(Enum).IsAssignableFrom(t))
            {
                // ReSharper disable once UseIsOperator.1
                // ReSharper disable once UseMethodIsInstanceOfType
                if (typeof(long).IsAssignableFrom(value.GetType()))
                {
                    value = Enum.ToObject(t, value);
                }
                else if (value is string)
                {
                    value = Enum.Parse(t, value as string, true);
                }
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
                    value = (bool)value;
                }
            }
            else if (typeof(byte[]).IsAssignableFrom(t))
            {
                if (value is string)
                {
                    value = Encoding.Default.GetBytes(value as string);
                }
            }

            return Convert.ChangeType(value, t);
        }

        /// <summary>
        /// Creates an instance based on a Ctor injection or Reflection loading
        /// </summary>
        /// <param name="type"></param>
        /// <param name="reader"></param>
        /// <param name="fullLoaded">Is loaded by a Ctor</param>
        /// <returns></returns>
        public static object CreateInstance(this Type type, IDataRecord reader, out bool fullLoaded)
        {            
            var constructorInfos = ConfigHelper.ReflecionStore.GetOrCreateClassInfoCache(type).ConstructorInfoCaches.Select(f => f.MethodInfo);

            var constructor = constructorInfos.FirstOrDefault(s => s.GetCustomAttributes().Any(e => e is ObjectFactoryMethodAttribute)) ??
                constructorInfos.FirstOrDefault(s =>
                {
                    ParameterInfo[] parameterInfos = s.GetParameters();
                    return parameterInfos.Length == 1 && parameterInfos.First().ParameterType == typeof(IDataRecord);
                });

            //maybe single ctor with param

            if (constructor != null)
            {
                ParameterInfo[] parameterInfos = constructor.GetParameters();
                if (parameterInfos.Length == 1 && parameterInfos.First().ParameterType == typeof(IDataRecord))
                {
                    fullLoaded = true;
                    //todo add delegate to cache
                    return constructor.Invoke(new object[] { reader });
                    //return Activator.CreateInstance(type, reader);
                }
            }
            else
            {
                //check for a Factory mehtod
                var factory =
                    type.GetMethods()
                        .FirstOrDefault(s => s.GetCustomAttributes().Any(f => f is ObjectFactoryMethodAttribute));

                if (factory != null)
                {
                    if (factory.IsStatic)
                    {
                        var returnParameter = factory.GetParameters();
                        var returnType = factory.ReturnParameter;

                        if (returnType != null && returnType.ParameterType == type)
                        {
                            if (returnParameter.Length == 1 &&
                                returnParameter.First().ParameterType == typeof(IDataRecord))
                            {
                                fullLoaded = true;
                                return factory.Invoke(null, new object[] { reader });
                            }
                        }
                    }
                }
            }

            //well letzs do it by our self and create an instance and then load the propertys
            fullLoaded = false;
            //todo add delegate to cache
            return constructorInfos.First().Invoke(new object[0]);
            //return Activator.CreateInstance(type);
        }

        /// <summary>
        /// Creates a new Instance based on possible Ctor's and the given <param name="reader"></param>
        /// </summary>
        /// <param name="type"></param>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static object SetPropertysViaReflection(this Type type, IDataRecord reader)
        {
            bool created;
            var source = type.CreateInstance(reader, out created);
            if (created)
                return source;

            return SetPropertysViaReflection(source, reader);
        }

        /// <summary>
        /// Returns all Cached Propertys from a <paramref name="type"/>
        /// </summary>
        /// <param name="type"></param>
        /// <param name="ignore"></param>
        /// <returns></returns>
        public static IEnumerable<string> GetPropertysViaRefection(this Type type, params string[] ignore)
        {
            return ConfigHelper.GetPropertiesEx(type).Select(s => s.Name).Except(ignore);
        }
    }
}