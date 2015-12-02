using JPB.DataAccess.Configuration;
using JPB.DataAccess.Configuration.Model;
using JPB.DataAccess.ModelsAnotations;
using System;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace JPB.DataAccess
{
#if !DEBUG
    [DebuggerStepThrough]
#endif
    public static class ConfigHelper
    {
        static ConfigHelper()
        {
            ReflecionStore = new Configuration.Config();
        }

        /// <summary>
        /// Anonymous type check by naming convention
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        internal static Boolean IsAnonymousType(this Type type)
        {
            //http://stackoverflow.com/questions/1650681/determining-whether-a-type-is-an-anonymous-type
            //awesome!
            return type.Namespace == null;
            //Boolean hasCompilerGeneratedAttribute = type.GetCustomAttributes().Any(s => s is CompilerGeneratedAttribute);
            //Boolean nameContainsAnonymousType = type.FullName.Contains("AnonymousType");
            //Boolean isAnonymousType = hasCompilerGeneratedAttribute && nameContainsAnonymousType;
            //return nameContainsAnonymousType;
        }

        internal static Configuration.Config ReflecionStore { get; set; }

        internal static ClassInfoCache GetClassInfo(this Type type)
        {
            if (IsAnonymousType(type))
                return new ClassInfoCache(type); //Anonymous types does not have any Attributes

            return ReflecionStore.GetOrCreateClassInfoCache(type);

        }

        internal static Attribute[] GetCustomAttributes(this Type type)
        {
            if (IsAnonymousType(type))
                return new Attribute[0]; //Anonymous types does not have any Attributes

            return ReflecionStore.GetOrCreateClassInfoCache(type).AttributeInfoCaches.ToArray().Select(s => s.Attribute).ToArray();
        }

        internal static Attribute[] GetCustomAttributes(this PropertyInfo type)
        {
            if (IsAnonymousType(type.DeclaringType))
                return new Attribute[0]; //Anonymous types does not have any Attributes

            var deb = ReflecionStore.GetOrCreatePropertyInfoCache(type).AttributeInfoCaches.ToArray().Select(s => s.Attribute).ToArray();

            return deb;
        }

        internal static Attribute[] GetCustomAttributes(this MethodInfo type)
        {
            if (IsAnonymousType(type.DeclaringType))
                return new Attribute[0]; //Anonymous types does not have any Attributes

            var deb = ReflecionStore.GetOrCreateMethodInfoCache(type).AttributeInfoCaches.ToArray().Select(s => s.Attribute).ToArray();

            return deb;
        }

        internal static string GetLocalToDbSchemaMapping(this Type type, string name)
        {
            if (IsAnonymousType(type))
                return name;

            return ReflecionStore.GetOrCreateClassInfoCache(type).SchemaMappingLocalToDatabase(name);
        }

        internal static string GetDbToLocalSchemaMapping(this Type type, string name)
        {
            if (IsAnonymousType(type))
                return name;

            return ReflecionStore.GetOrCreateClassInfoCache(type).SchemaMappingDatabaseToLocal(name);
        }

        internal static PropertyInfo[] GetPropertiesEx(this Type type)
        {
            if (IsAnonymousType(type))
                return type.GetProperties();

            return ReflecionStore.GetOrCreateClassInfoCache(type).PropertyInfoCaches.ToArray().Select(s => s.PropertyInfo).ToArray();
        }

        internal static string[] GetSchemaMapping(this Type type)
        {
            if (IsAnonymousType(type))
                return type.GetPropertiesEx().Select(s => s.Name).ToArray();

            return ReflecionStore.GetOrCreateClassInfoCache(type).LocalToDbSchemaMapping();
        }

        internal static MethodInfo[] GetMethods(this Type type)
        {
            if (IsAnonymousType(type))
                return type.GetMethods();

            return ReflecionStore.GetOrCreateClassInfoCache(type).MethodInfoCaches.ToArray().Select(s => s.MethodInfo).ToArray();
        }

        public static string GetPropertyInfoFromLabda<TSource, TProperty>(
    Expression<Func<TSource, TProperty>> propertyLambda)
        {
            Type type = typeof(TSource);

            MemberExpression member = propertyLambda.Body as MemberExpression;
            if (member == null)
                throw new ArgumentException(string.Format(
                    "Expression '{0}' refers to a method, not a property.",
                    propertyLambda.ToString()));

            PropertyInfo propInfo = member.Member as PropertyInfo;
            if (propInfo == null)
                throw new ArgumentException(string.Format(
                    "Expression '{0}' refers to a field, not a property.",
                    propertyLambda.ToString()));

            return propInfo.Name;
        }

        public static string GetMehtodInfoFromLabda<TSource, TProperty>(
 Expression<Func<TSource, TProperty>> propertyLambda)
        {
            Type type = typeof(TSource);

            MemberExpression member = propertyLambda.Body as MemberExpression;
            if (member != null)
                throw new ArgumentException(string.Format(
                    "Expression '{0}' refers to a property, not a method.",
                    propertyLambda.ToString()));

            PropertyInfo propInfo = member.Member as PropertyInfo;
            if (propInfo == null)
                throw new ArgumentException(string.Format(
                    "Expression '{0}' refers to a field, not a property.",
                    propertyLambda.ToString()));          

            return propInfo.Name;
        }

        //internal static string GetPropertyInfoFromLabda<T>(Expression<Func<T>> exp)
        //{
        //    Type paramType = exp.Parameters[0].Type;  // first parameter of expression
        //    var d = paramType.GetMember((exp.Body as MemberExpression).Member.Name)[0];
        //    return d.Name;
        //}
    }
}