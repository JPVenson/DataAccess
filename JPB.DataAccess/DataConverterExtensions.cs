#region Jean-Pierre Bachmann

// Erstellt von Jean-Pierre Bachmann am 13:02

#endregion

#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using JPB.DataAccess.AdoWrapper;
using JPB.DataAccess.Helper;
using JPB.DataAccess.Manager;
using JPB.DataAccess.ModelsAnotations;

#endregion

namespace JPB.DataAccess
{
    internal static class DataHelperExtensions
    {
        internal static void AddWithValue(this IDataParameterCollection source, string name, object parameter,
            IDatabase db)
        {
            source.Add(db.CreateParameter(name, parameter));
        }
    }

    internal class StaticHelper
    {
        internal static IEnumerable<T> CastToEnumerable<T>(object o) where T : class
        {
            var foo = (o as IEnumerable<T>);
            if (foo != null)
                return foo;

            var basicEnumerable = o as IEnumerable;
            var castedEmumerable = new List<T>();

            if (basicEnumerable != null)
            {
                foreach (object VARIABLE in basicEnumerable)
                    castedEmumerable.Add((T)VARIABLE);
            }

            return castedEmumerable.AsReadOnly();
        }
    }
#if !DEBUG
        [DebuggerStepThrough]
#endif
    internal static class DataConverterExtensions
    {
        internal static string GetTableName<T>()
        {
            var forModel = typeof(T).GetCustomAttributes(false).FirstOrDefault(s => s is ForModel) as ForModel;
            if (forModel != null)
                return forModel.AlternatingName;
            return typeof(T).Name;
        }

        internal static string GetTableName(this Type type)
        {
            var forModel = type.GetCustomAttributes(false).FirstOrDefault(s => s is ForModel) as ForModel;
            if (forModel != null)
                return forModel.AlternatingName;
            return type.Name;
        }

        internal static object GetParamaterValue(this object source, string name)
        {
            return GetParamater(source, name).GetValue(source, null);
        }

        internal static PropertyInfo GetParamater(this object source, string name)
        {
            return source.GetType().GetProperties().FirstOrDefault(s => s.Name == name);
        }

        internal static bool CheckForPK(this PropertyInfo info)
        {
            return info.GetCustomAttributes(false).Any(s => s is PrimaryKeyAttribute) || (info.Name.EndsWith("_ID"));
        }

        internal static bool CheckForFK(this PropertyInfo info, string name)
        {
            if (info.Name != name)
                return false;
            return info.GetCustomAttributes(false).Any(s => s is PrimaryKeyAttribute);
        }

        internal static string GetPKPropertyName(this Type type)
        {
            PropertyInfo name = type.GetProperties().FirstOrDefault(CheckForPK);
            return name == null ? null : name.Name;
        }

        /// <summary>
        /// Get and Convert the found PK into Database name
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        internal static string GetPK(this Type type)
        {
            PropertyInfo name = type.GetProperties().FirstOrDefault(CheckForPK);
            return MapEntiysPropToSchema(type, name == null ? null : name.Name);
        }

        internal static string GetFK(this Type type, string name)
        {
            name = type.ReMapSchemaToEntiysProp(name);
            PropertyInfo prop = type.GetProperties().FirstOrDefault(info => CheckForFK(info, name));
            return prop == null ? null : prop.Name;
        }

        /// <summary>
        /// Gets the PK value of the Object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        internal static long GetPK<T>(this T source)
        {
            return GetPK<T, long>(source);
        }

        internal static E GetPK<T, E>(this T source)
        {
            string pk = source.GetType().GetPKPropertyName();
            return (E)source.GetType().GetProperty(pk).GetValue(source, null);
        }

        internal static E GetFK<E>(this object source, string name)
        {
            Type type = source.GetType();
            string pk = type.GetFK(name);
            return (E)type.GetProperty(pk).GetValue(source, null);
        }

        internal static E GetFK<T, E>(this T source, string name)
        {
            string pk = typeof(T).GetFK(name);
            return (E)typeof(T).GetProperty(pk).GetValue(source, null);
        }

        internal static IEnumerable<string> MapEntiyToSchema(Type type, string[] ignore)
        {
            return from propertyInfo in type.GetProperties().Where(s => !s.GetGetMethod().IsVirtual)
                   where !ignore.Contains(propertyInfo.Name)
                   let formodle = propertyInfo.GetCustomAttributes(false).FirstOrDefault(s => s is ForModel) as ForModel
                   select formodle != null ? formodle.AlternatingName : propertyInfo.Name;
        }

        internal static IEnumerable<string> MapEntiyToSchema<T>(string[] ignore)
        {
            return MapEntiyToSchema(typeof(T), ignore);
        }

        internal static string MapEntiysPropToSchema(this Type source, string prop)
        {
            PropertyInfo[] propertys = source.GetProperties();
            return (from propertyInfo in propertys
                    where propertyInfo.Name == prop
                    let formodle =
                        propertyInfo.GetCustomAttributes(false).FirstOrDefault(s => s is ForModel) as ForModel
                    select formodle != null ? formodle.AlternatingName : propertyInfo.Name).FirstOrDefault();
        }

        internal static string MapEntiysPropToSchema<T>(string prop)
        {
            return MapEntiysPropToSchema(typeof(T), prop);
        }

        internal static string ReMapSchemaToEntiysProp<T>(string prop)
        {
            return ReMapSchemaToEntiysProp(typeof(T), prop);
        }

        internal static string ReMapSchemaToEntiysProp(this Type source, string prop)
        {
            foreach (PropertyInfo propertyInfo in from propertyInfo in source.GetProperties()
                                                  let customAttributes =
                                                      propertyInfo.GetCustomAttributes(false)
                                                          .FirstOrDefault(s => s is ForModel) as ForModel
                                                  where
                                                      customAttributes != null &&
                                                      customAttributes.AlternatingName == prop
                                                  select propertyInfo)
                return propertyInfo.Name;
            return prop;
        }

        internal static bool CheckForListInterface(this PropertyInfo info)
        {
            if (info.PropertyType == typeof(string))
                return false;
            if (info.PropertyType.GetInterface(typeof(IEnumerable).Name) != null)
                return true;
            if (info.PropertyType.GetInterface(typeof(IEnumerable<>).Name) != null)
                return true;
            return false;
        }

        internal static bool CheckForListInterface(this object info)
        {
            return !(info is string) &&
                   info.GetType().GetInterface(typeof(IEnumerable).Name) != null &&
                   info.GetType().GetInterface(typeof(IEnumerable<>).Name) != null;
        }

        internal static T LoadNavigationProps<T>(this T source, IDatabase accessLayer)
        {
            return (T)LoadNavigationProps(source as object, accessLayer);
        }

        internal static object LoadNavigationProps(this object source, IDatabase accessLayer)
        {
            Type type = source.GetType();
            PropertyInfo[] props = source.GetType().GetProperties().ToArray();
            PropertyInfo[] virtualProps = props.Where(s => s.GetGetMethod(false).IsVirtual).ToArray();
            Type targetType = null;
            foreach (PropertyInfo propertyInfo in virtualProps)
            {
                //var firstOrDefault = source.GetFK<long>(propertyInfo.Name);
                IDbCommand sqlCommand;

                var firstOrDefault =
                    propertyInfo.GetCustomAttributes(false).FirstOrDefault(s => s is ForeignKeyAttribute) as
                        ForeignKeyAttribute;
                if (firstOrDefault == null)
                    continue;
                if (CheckForListInterface(propertyInfo))
                {
                    long pk = source.GetPK();
                    targetType = propertyInfo.PropertyType.GetGenericArguments().FirstOrDefault();
                    sqlCommand = DbAccessLayer.CreateSelect(targetType, accessLayer,
                        " WHERE " + firstOrDefault.KeyName + " = @pk", new List<IQueryParameter>
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
                    var castMethod =
                        typeof(StaticHelper).GetMethod("CastToEnumerable").MakeGenericMethod(targetType);
                    dynamic castedObject = castMethod.Invoke(null, new object[] { orDefault });

                    propertyInfo.SetValue(source, castedObject, null);
                    foreach (object item in orDefault)
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

        internal static T SetPropertysViaRefection<T>(IDataRecord reader)
            where T : class
        {
            return (T)SetPropertysViaRefection(typeof(T), reader);
        }

        public static EgarDataRecord CreateEgarRecord(this IDataRecord rec)
        {
            return new EgarDataRecord(rec);
        }

        internal static dynamic SetPropertysViaRefection(Type type, IDataRecord reader)
        {
            var constructorInfos = type.GetConstructors();
            dynamic source = null;

            var constructor =
                constructorInfos
                    .FirstOrDefault(s => s.GetCustomAttributes().Any(e => e is ObjectFactoryMehtodAttribute)) ??
                constructorInfos.FirstOrDefault(s =>
                {
                    var parameterInfos = s.GetParameters();
                    return parameterInfos.Length == 1 && parameterInfos.First().ParameterType == typeof(IDataRecord);
                });

            //maybe single ctor with param

            if (constructor != null)
            {
                var parameterInfos = constructor.GetParameters();
                if (parameterInfos.Length == 1 && parameterInfos.First().ParameterType == typeof(IDataRecord))
                {
                    return Activator.CreateInstance(type, reader);
                }
            }
            else
            {
                //check for a Factory mehtod
                var factory =
                    type.GetMethods()
                        .FirstOrDefault(s => s.GetCustomAttributes().Any(f => f is ObjectFactoryMehtodAttribute));

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
                                return factory.Invoke(null, new object[] { reader });
                            }
                        }
                    }
                }
            }

            //well letzs do it by our self and create an instance and then load the propertys

            source = Activator.CreateInstance(type);

            var listofpropertys = new Dictionary<string, object>();

            for (var i = 0; i < reader.FieldCount; i++)
                listofpropertys.Add(ReMapSchemaToEntiysProp(type, reader.GetName(i)), reader.GetValue(i));

            foreach (var item in listofpropertys)
            {
                PropertyInfo property = type.GetProperty(item.Key);
                if (property != null)
                {
                    if (item.Value is DBNull)
                        property.SetValue(source, null, null);
                    else
                    {
                        if (property.PropertyType.IsGenericTypeDefinition &&
                            property.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                        {
                            object convertedValue = Convert.ChangeType(item.Value,
                                Nullable.GetUnderlyingType(
                                    property.PropertyType));
                            property.SetValue(source, convertedValue, null);
                        }
                        else
                            property.SetValue(source, item.Value, null);
                    }
                }
            }

            if (reader is IDisposable)
            {
                (reader as IDisposable).Dispose();
            }

            return source;
        }

        internal static IEnumerable<string> GetPropertysViaRefection(this Type t, params string[] ignore)
        {
            return t.GetProperties().Select(s => s.Name).Except(ignore);
        }
    }
}