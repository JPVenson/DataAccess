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

        public ClassInfoCache GetOrCreatePropertyInfoCache(Type type)
        {
            var element = SClassInfoCaches.FirstOrDefault(s => s.ClassName == type.FullName);

            if (element == null)
            {
                SClassInfoCaches.Add(element = new ClassInfoCache(type));
            }

            return element;
        }

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

        public ClassInfoCache GetOrCreateClassInfoCache(Type type)
        {
            var element = SClassInfoCaches.FirstOrDefault(s => s.ClassName == type.FullName);

            if (element == null)
            {
                SClassInfoCaches.Add(element = new ClassInfoCache(type));
            }

            return element;
        }

        public static ConcurrentBag<ClassInfoCache> SClassInfoCaches { get; set; }

        public class AttributeInfoCache
        {
            public Attribute Attribute { get; set; }
            public object AttributeName { get; set; }

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
                this.AttributeInfoCaches =
                    propertyInfo.GetCustomAttributes(true).Where(s => s is Attribute).Select(s => new AttributeInfoCache(s as Attribute)).ToArray();
            }

            public PropertyInfo PropertyInfo { get; set; }
            public string PropertyName { get; set; }
            public AttributeInfoCache[] AttributeInfoCaches { get; set; }
        }

        public class ClassInfoCache
        {
            public ClassInfoCache(Type type)
            {
                ClassName = type.FullName;
                Type = type;
                this.AttributeInfoCaches = type.GetCustomAttributes(true).Where(s => s is Attribute).Select(s => new AttributeInfoCache(s as Attribute)).ToArray();
                this.PropertyInfoCaches = type.GetProperties().Select(s => new PropertyInfoCache(s)).ToArray();
            }

            public string ClassName { get; set; }
            public Type Type { get; set; }
            public PropertyInfoCache[] PropertyInfoCaches { get; set; }
            public AttributeInfoCache[] AttributeInfoCaches { get; set; }
        }

    }
}