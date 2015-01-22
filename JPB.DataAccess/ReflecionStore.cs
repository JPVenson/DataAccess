using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using JPB.DataAccess.ModelsAnotations;

namespace JPB.DataAccess
{

#if !DEBUG
    [DebuggerStepThrough]
#endif
    public class ReflecionStore
    {
        static ReflecionStore()
        {
            SClassInfoCaches = new ConcurrentBag<ClassInfoCache>();
        }

        /// <summary>
        /// Gets an Cache object if exists or creats one
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public ClassInfoCache GetOrCreatePropertyInfoCache(Type type)
        {
            var element = SClassInfoCaches.FirstOrDefault(s => s.ClassName == type.FullName);

            if (element == null)
            {
                SClassInfoCaches.Add(element = new ClassInfoCache(type));
            }

            return element;
        }
        /// <summary>
        /// Gets an Cache object if exists or creats one
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public PropertyInfoCache GetOrCreatePropertyInfoCache(PropertyInfo type)
        {
            var declareingType = type.ReflectedType;
            var name = type.Name;
            var element = SClassInfoCaches.FirstOrDefault(s => s.Type == declareingType && s.PropertyInfoCaches.Any(e => e.PropertyName == name));

            if (element == null)
            {
                var declaringType = type.ReflectedType;
                SClassInfoCaches.Add(element = new ClassInfoCache(declaringType));
            }

            return element.PropertyInfoCaches.FirstOrDefault(s => s.PropertyName == type.Name);
        }
        /// <summary>
        /// Gets an Cache object if exists or creats one
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public ClassInfoCache GetOrCreateClassInfoCache(Type type)
        {
            var element = SClassInfoCaches.FirstOrDefault(s => s.ClassName == type.FullName);

            if (element == null)
            {
                SClassInfoCaches.Add(element = new ClassInfoCache(type));
            }

            return element;
        }
        /// <summary>
        /// Gets an Cache object if exists or creats one
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public MethodInfoCache GetOrCreateMethodInfoCache(MethodInfo type)
        {
            var declareingType = type.ReflectedType;
            var name = type.Name;
            var element = SClassInfoCaches.FirstOrDefault(s => s.Type == declareingType && s.MethodInfoCaches.Any(e => e.MethodName == name));

            if (element == null)
            {
                var declaringType = type.ReflectedType;
                SClassInfoCaches.Add(element = new ClassInfoCache(declaringType));
            }

            return element.MethodInfoCaches.FirstOrDefault(s => s.MethodName == type.Name);
        }

        public static ConcurrentBag<ClassInfoCache> SClassInfoCaches { get; private set; }

        public class MethodInfoCache
        {
            public MethodInfoCache(MethodInfo mehtodInfo)
            {
                MethodInfo = mehtodInfo;
                MethodName = mehtodInfo.Name;
                this.AttributeInfoCaches =
                    mehtodInfo.GetCustomAttributes(true).Where(s => s is Attribute).Select(s => new AttributeInfoCache(s as Attribute)).ToArray();
            }

            public MethodInfo MethodInfo { get; private set; }
            public string MethodName { get; private set; }
            public AttributeInfoCache[] AttributeInfoCaches { get; private set; }
        }

        public class AttributeInfoCache
        {
            public Attribute Attribute { get; private set; }
            public object AttributeName { get; private set; }

            public AttributeInfoCache(Attribute attribute)
            {
                Attribute = attribute;
                AttributeName = attribute.TypeId;
            }
        }

        public class PropertyInfoCache
        {
            public PropertyInfoCache(PropertyInfo propertyInfo)
            {
                PropertyInfo = propertyInfo;
                PropertyName = propertyInfo.Name;
                this.AttributeInfoCaches = propertyInfo.GetCustomAttributes(true).Where(s => s is Attribute).Select(s => new AttributeInfoCache(s as Attribute)).ToArray();
            }

            public PropertyInfo PropertyInfo { get; private set; }
            public string PropertyName { get; private set; }
            public AttributeInfoCache[] AttributeInfoCaches { get; private set; }
        }

        public class ClassInfoCache
        {
            public ClassInfoCache(Type type)
            {
                ClassName = type.FullName;
                Type = type;
                this.AttributeInfoCaches = type.GetCustomAttributes(true).Where(s => s is Attribute).Select(s => new AttributeInfoCache(s as Attribute)).ToArray();
                this.PropertyInfoCaches = type.GetProperties().Select(s => new PropertyInfoCache(s)).ToArray();
                this.MethodInfoCaches = type.GetMethods().Select(s => new MethodInfoCache(s)).ToArray();
            }

            public string ClassName { get; private set; }
            public Type Type { get; private set; }
            public PropertyInfoCache[] PropertyInfoCaches { get; private set; }
            public AttributeInfoCache[] AttributeInfoCaches { get; private set; }
            public MethodInfoCache[] MethodInfoCaches { get; private set; }
        }

    }
}