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
        public static QueryDebugger CreateQueryDebugger(this IDbCommand command)
        {
            return new QueryDebugger(command);
        }

        public static string GetTableName<T>()
        {
            var forModel = typeof(T).GetCustomAttributes().FirstOrDefault(s => s is ForModel) as ForModel;
            if (forModel != null)
                return forModel.AlternatingName;
            return typeof(T).Name;
        }

        public static string GetTableName(this Type type)
        {
            var forModel = type.GetCustomAttributes().FirstOrDefault(s => s is ForModel) as ForModel;
            if (forModel != null)
                return forModel.AlternatingName;
            return type.Name;
        }

        public static object GetParamaterValue(this object source, string name)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            var propertyInfo = GetParamater(source, name);
            if (propertyInfo == null)
                throw new ArgumentNullException("name");
            return propertyInfo.GetConvertedValue(source);
        }

        public static PropertyInfo GetParamater(this object source, string name)
        {
            return ReflectionHelpers.GetProperties(source.GetType()).FirstOrDefault(s => s.Name == name);
        }

        public static bool CheckForPK(this PropertyInfo info)
        {
            return info.GetCustomAttributes().Any(s => s is PrimaryKeyAttribute) || (info.Name.EndsWith("_ID"));
        }

        public static bool CheckForFK(this PropertyInfo info, string name)
        {
            if (info.Name != name)
                return false;
            return info.GetCustomAttributes().Any(s => s is PrimaryKeyAttribute);
        }

        public static bool CheckForFK(this PropertyInfo info)
        {
            return info.GetCustomAttributes().Any(s => s is PrimaryKeyAttribute);
        }

        public static string GetPKPropertyName(this Type type)
        {
            PropertyInfo name = ReflectionHelpers.GetProperties(type).FirstOrDefault(CheckForPK);
            return name == null ? null : name.Name;
        }

        /// <summary>
        ///     Get and Convert the found PK into Database name
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetPK(this Type type)
        {
            PropertyInfo name = ReflectionHelpers.GetProperties(type).FirstOrDefault(CheckForPK);
            return MapEntiysPropToSchema(type, name == null ? null : name.Name);
        }

        public static PropertyInfo[] GetFKs(this Type type)
        {
            return ReflectionHelpers.GetProperties(type).Where(CheckForFK).ToArray();
        }

        public static string GetFK(this Type type, string name)
        {
            name = type.ReMapSchemaToEntiysProp(name);
            PropertyInfo prop = ReflectionHelpers.GetProperties(type).FirstOrDefault(info => CheckForFK(info, name));
            return prop == null ? null : prop.Name;
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
        public static long GetPK<T>(this T source)
        {
            return GetPK<T, long>(source);
        }

        public static E GetPK<T, E>(this T source)
        {
            string pk = source.GetType().GetPKPropertyName();
            return (E)source.GetType().GetProperty(pk).GetConvertedValue(source);
        }

        public static E GetFK<E>(this object source, string name)
        {
            Type type = source.GetType();
            string pk = type.GetFK(name);
            return (E)type.GetProperty(pk).GetConvertedValue(source);
        }

        public static E GetFK<T, E>(this T source, string name)
        {
            string pk = typeof(T).GetFK(name);
            return (E)typeof(T).GetProperty(pk).GetConvertedValue(source);
        }

        public static IEnumerable<string> MapEntiyToSchema(Type type, string[] ignore)
        {
            foreach (var s1 in ReflectionHelpers.GetProperties(type))
            {
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
                    if (!ignore.Contains(s1.Name))
                    {
                        yield return (s1.GetCustomAttributes().First(s => s is FromXmlAttribute) as FromXmlAttribute).FieldName;
                    }
                }
                else if (!s1.GetGetMethod().IsVirtual && !s1.GetCustomAttributes().Any(s => s is IgnoreReflectionAttribute))
                {
                    if (!ignore.Contains(s1.Name))
                    {
                        var formodle = s1.GetCustomAttributes().FirstOrDefault(s => s is ForModel) as ForModel;
                        var s2 = formodle != null ? formodle.AlternatingName : s1.Name;
                        yield return s2;
                    }
                }
            }
        }

        public static IEnumerable<string> MapEntiyToSchema<T>(string[] ignore)
        {
            return MapEntiyToSchema(typeof(T), ignore);
        }

        public static string MapEntiysPropToSchema(this Type type, string prop)
        {
            PropertyInfo[] propertys = ReflectionHelpers.GetProperties(type);
            return (from propertyInfo in propertys
                    where propertyInfo.Name == prop
                    let formodle =
                            propertyInfo.GetCustomAttributes().FirstOrDefault(s => s is ForModel) as ForModel
                    select formodle != null ? formodle.AlternatingName : propertyInfo.Name).FirstOrDefault();
        }

        public static string MapEntiysPropToSchema<T>(string prop)
        {
            return MapEntiysPropToSchema(typeof(T), prop);
        }

        public static string ReMapSchemaToEntiysProp<T>(string prop)
        {
            return ReMapSchemaToEntiysProp(typeof(T), prop);
        }

        public static string ReMapSchemaToEntiysProp(this Type type, string prop)
        {
            foreach (PropertyInfo propertyInfo in from propertyInfo in ReflectionHelpers.GetProperties(type)
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

        public static bool CheckForListInterface(this PropertyInfo info)
        {
            if (info.PropertyType == typeof(string))
                return false;
            if (info.PropertyType.GetInterface(typeof(IEnumerable).Name) != null)
                return true;
            if (info.PropertyType.GetInterface(typeof(IEnumerable<>).Name) != null)
                return true;
            return false;
        }

        public static bool CheckForListInterface(this object info)
        {
            return !(info is string) &&
                   info.GetType().GetInterface(typeof(IEnumerable).Name) != null &&
                   info.GetType().GetInterface(typeof(IEnumerable<>).Name) != null;
        }

        public static PropertyInfo[] GetNavigationProps(this Type type)
        {
            return ReflectionHelpers.GetProperties(type).Where(s => s.GetGetMethod(false).IsVirtual).ToArray();
        }

        public static PropertyInfo[] GetNavigationProps<T>()
        {
            return GetNavigationProps(typeof(T));
        }

        public static T LoadNavigationProps<T>(this T source, IDatabase accessLayer)
        {
            return (T)LoadNavigationProps(source as object, accessLayer);
        }

        public static object LoadNavigationProps(this object source, IDatabase accessLayer)
        {
            Type type = source.GetType();
            PropertyInfo[] props = ReflectionHelpers.GetProperties(type).ToArray();
            PropertyInfo[] virtualProps = GetNavigationProps(type);
            Type targetType = null;
            foreach (PropertyInfo propertyInfo in virtualProps)
            {
                //var firstOrDefault = source.GetFK<long>(propertyInfo.ClassName);
                IDbCommand sqlCommand;

                var firstOrDefault =
                    propertyInfo.GetCustomAttributes().FirstOrDefault(s => s is ForeignKeyAttribute) as
                        ForeignKeyAttribute;
                if (firstOrDefault == null)
                    continue;
                if (CheckForListInterface(propertyInfo))
                {
                    long pk = source.GetPK();
                    var targetName = firstOrDefault.KeyName;
                    targetType = propertyInfo.PropertyType.GetGenericArguments().FirstOrDefault();

                    if (string.IsNullOrEmpty(targetName))
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
                    sqlCommand =
                        DbAccessLayer.CreateSelect(targetType, accessLayer, (long)fkproperty);
                }

                List<object> orDefault = DbAccessLayer.RunSelect(targetType, accessLayer, sqlCommand);

                if (CheckForListInterface(orDefault) && CheckForListInterface(propertyInfo))
                {
                    //MethodInfo castMethod =
                    //    typeof(StaticHelper).GetMethod("CastToEnumerable").MakeGenericMethod(targetType);

                    var constructorInfo = typeof(ReposetoryCollection<>).MakeGenericType(targetType).GetConstructor(new[] { typeof(IEnumerable) });

                    Debug.Assert(constructorInfo != null, "constructorInfo != null");
                    var reproCollection = constructorInfo.Invoke(new object[] { orDefault });
                    propertyInfo.SetValue(source, reproCollection, null);
                    foreach (var item in orDefault)
                        item.LoadNavigationProps(accessLayer);
                }
                if (!CheckForListInterface(propertyInfo))
                {
                    object @default = orDefault.FirstOrDefault();
                    propertyInfo.SetValue(source, @default, null);
                    @default.LoadNavigationProps(accessLayer);
                }
            }

            return source;
        }

        public static T SetPropertysViaReflection<T>(IDataRecord reader)
            where T : class
        {
            return (T)SetPropertysViaReflection(typeof(T), reader);
        }

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
            var listofpropertys = new Dictionary<string, object>();
            var type = instance.GetType();

            PropertyInfo[] propertys = type.GetProperties();
            var instanceOfFallbackList = new Dictionary<string, object>();

            for (int i = 0; i < reader.FieldCount; i++)
                listofpropertys.Add(ReMapSchemaToEntiysProp(type, reader.GetName(i)), reader.GetValue(i));

            foreach (var item in listofpropertys)
            {
                var property = propertys.FirstOrDefault(s => s.Name == item.Key);
                if (property != null)
                {
                    var value = item.Value;

                    var any = property.GetCustomAttributes().FirstOrDefault(s => s is ValueConverterAttribute) as ValueConverterAttribute;

                    if (any != null)
                    {
                        var valueConverter = any.CreateConverter();

                        value = valueConverter.Convert(value, property.PropertyType, any.Parameter,
                            CultureInfo.CurrentCulture);
                    }

                    var isXmlField =
                        property.GetCustomAttributes().FirstOrDefault(s => s is FromXmlAttribute) as FromXmlAttribute;

                    if (isXmlField != null)
                    {
                        var xmlStream = value.ToString();

                        //Check for List

                        if (CheckForListInterface(property))
                        {
                            //target Property is of type list
                            //so expect a xml valid list Take the first element and expect the propertys inside this first element
                            var record = new XmlDataRecord(xmlStream, property.PropertyType.GetGenericArguments().FirstOrDefault());
                            var xmlDataRecords = record.CreateListOfItems();

                            var genericArguments = property.PropertyType.GetGenericArguments().FirstOrDefault();
                            var enumerableOfItems = xmlDataRecords.Select(xmlDataRecord => SetPropertysViaReflection(genericArguments, xmlDataRecord)).ToList();

                            var caster = typeof(ReposetoryCollection<>).MakeGenericType(genericArguments).GetConstructor(new[] { typeof(IEnumerable) });

                            var castedList = caster.Invoke(new object[] { enumerableOfItems });
                            property.SetValue(instance, castedList);
                        }
                        else
                        {
                            var xmlSerilizedProperty = SetPropertysViaReflection(property.PropertyType, new XmlDataRecord(xmlStream, property.PropertyType));

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
            ConstructorInfo[] constructorInfos = type.GetConstructors();
            object source = null;

            ConstructorInfo constructor =
                constructorInfos
                    .FirstOrDefault(s => s.GetCustomAttributes().Any(e => e is ObjectFactoryMethodAttribute)) ??
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
                    return Activator.CreateInstance(type, reader);
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
                        ParameterInfo[] returnParameter = factory.GetParameters();
                        ParameterInfo returnType = factory.ReturnParameter;

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
            return Activator.CreateInstance(type);
        }

        /// <summary>
        /// Creates a new Instance based on possible Ctor's and the given <param name="reader"></param>
        /// </summary>
        /// <param name="type"></param>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static object SetPropertysViaReflection(Type type, IDataRecord reader)
        {
            bool created;
            var source = type.CreateInstance(reader, out created);
            if (created)
                return source;

            return SetPropertysViaReflection(source, reader);
        }

        public static IEnumerable<string> GetPropertysViaRefection(this Type type, params string[] ignore)
        {
            return ReflectionHelpers.GetProperties(type).Select(s => s.Name).Except(ignore);
        }
    }
}