using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using JPB.DataAccess.ModelsAnotations;
using JPB.DataAccess.Configuration.Model;
using JPB.DataAccess.Config;

namespace JPB.DataAccess.Configuration
{
    /// <summary>
    /// Class info Storage
    /// </summary>
#if !DEBUG
    [DebuggerStepThrough]
#endif
    public class Config
    {
        static Config()
        {
            SClassInfoCaches = new ConcurrentBag<ClassInfoCache>();
        }

        /// <summary>
        /// Creates a new Instance for configuration
        /// </summary>
        /// <param name="enableReflection">If set reflection will be used to enumerate all used class instances</param>
        public Config(bool enableReflection = true)
        {
            this.UseReflection = enableReflection;
        }

        public void SetConfig<T>(Action<ConfigurationResolver<T>> validator)
        {
            validator(new ConfigurationResolver<T>(this));
        }

        /// <summary>
        /// Indicates the usage of Reflection
        /// </summary>
        public bool UseReflection { get; private set; }
             
        /// <summary>
        /// Gets an Cache object if exists or creats one
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        internal PropertyInfoCache GetOrCreatePropertyInfoCache(PropertyInfo type)
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
        internal ClassInfoCache GetOrCreateClassInfoCache(Type type)
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
        internal MethodInfoCache GetOrCreateMethodInfoCache(MethodInfo type)
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

        internal static ConcurrentBag<ClassInfoCache> SClassInfoCaches { get; private set; }
    }
}