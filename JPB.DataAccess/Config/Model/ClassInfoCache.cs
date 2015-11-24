using JPB.DataAccess.Config.Model;
using JPB.DataAccess.ModelsAnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPB.DataAccess.Configuration.Model
{
    internal class ClassInfoCache
    {
        public ClassInfoCache(Type type)
        {
            this.AttributeInfoCaches = new List<AttributeInfoCache>();
            this.PropertyInfoCaches = new List<PropertyInfoCache>();
            this.MethodInfoCaches = new List<MethodInfoCache>();
            this.ConstructorInfoCaches = new List<ConstructorInfoCache>();
            ClassName = type.FullName;
            Type = type;
            if (type != null)
            { 
                this.AttributeInfoCaches = type.GetCustomAttributes(true).Where(s => s is Attribute).Select(s => new AttributeInfoCache(s as Attribute)).ToList();
                this.PropertyInfoCaches = type.GetProperties().Select(s => new PropertyInfoCache(s)).ToList();
                this.MethodInfoCaches = type.GetMethods().Select(s => new MethodInfoCache(s)).ToList();
                this.ConstructorInfoCaches = type.GetConstructors().Select(s => new ConstructorInfoCache(s)).ToList();

                this.CheckForConfig();
            }
        }

        private void CheckForConfig()
        {
            var configMethods = this.MethodInfoCaches.Where(f => f.AttributeInfoCaches.Any(e => e is ConfigMehtodAttribute)).ToArray();
            if (!configMethods.Any())
                return;

            var config = new Config();
            foreach (var item in configMethods)
            {
                item.MethodInfo.Invoke(null, new object[] { config });
            }
        }

        internal PropertyInfoCache GetOrCreatePropertyCache(string info)
        {
            var fod = PropertyInfoCaches.FirstOrDefault(s => s.PropertyName == info);
            if (fod != null)
                return fod;
            fod = PropertyInfoCache.Logical(info);
            PropertyInfoCaches.Add(fod);
            return fod;
        }

        public string ClassName { get; private set; }
        public Type Type { get; private set; }
        public List<PropertyInfoCache> PropertyInfoCaches { get; private set; }
        public List<AttributeInfoCache> AttributeInfoCaches { get; private set; }
        public List<MethodInfoCache> MethodInfoCaches { get; private set; }
        public List<ConstructorInfoCache> ConstructorInfoCaches { get; set; }
    }
}
