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
            this.AttributeInfoCaches = type.GetCustomAttributes(true).Where(s => s is Attribute).Select(s => new AttributeInfoCache(s as Attribute)).ToList();
            this.PropertyInfoCaches = type.GetProperties().Select(s => new PropertyInfoCache(s)).ToList();
            this.MethodInfoCaches = type.GetMethods().Select(s => new MethodInfoCache(s)).ToList();
            this.ConstructorInfoCaches = type.GetConstructors().Select(s => new ConstructorInfoCache(s)).ToList();
        }

        public void CheckForConfig()
        {
            var configMethods = MethodInfoCaches.Where(f => f.AttributeInfoCaches.Any(e => e.Attribute is ConfigMehtodAttribute)).ToArray();
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
        /// <summary>
        /// Key is C# Property name and Value is DB Eqivalent
        /// </summary>
        public Dictionary<string, string> SchemaMappingValues { get; private set; }
        public List<PropertyInfoCache> PropertyInfoCaches { get; private set; }
        public List<AttributeInfoCache> AttributeInfoCaches { get; private set; }
        public List<MethodInfoCache> MethodInfoCaches { get; private set; }
        public List<ConstructorInfoCache> ConstructorInfoCaches { get; set; }

        internal string SchemaMappingLocalToDatabase(string cSharpName)
        {
            CreateSchemaMapping();
            var mappings = SchemaMappingValues.FirstOrDefault(s => s.Key.Equals(cSharpName));
            if (mappings.Equals(default(KeyValuePair<string, string>)))
            {
                return cSharpName;
            }
            return mappings.Value;
        }

        internal string SchemaMappingDatabaseToLocal(string databaseName)
        {
            CreateSchemaMapping();
            var mappings = SchemaMappingValues.FirstOrDefault(s => s.Value.Equals(databaseName));
            if (mappings.Equals(default(KeyValuePair<string, string>)))
            {
                return databaseName;
            }
            return mappings.Key;
        }

        internal void CreateSchemaMapping()
        {
            if (SchemaMappingValues == null)
            {
                SchemaMappingValues = new Dictionary<string, string>();
                foreach (var item in PropertyInfoCaches)
                {
                    var forModel = item.AttributeInfoCaches.FirstOrDefault(s => s.Attribute is ForModel);
                    if (forModel != null)
                    {
                        SchemaMappingValues.Add(item.PropertyName, (forModel.Attribute as ForModel).AlternatingName);
                    }
                    else
                    {
                        SchemaMappingValues.Add(item.PropertyName, item.PropertyName);
                    }
                }
            }
        }

        internal string[] LocalToDbSchemaMapping()
        {
            CreateSchemaMapping();
            return SchemaMappingValues.Values.ToArray();
        }

        internal string[] DbToLocalSchemaMapping()
        {
            CreateSchemaMapping();
            if (SchemaMappingValues == null)
            {
                SchemaMappingValues = new Dictionary<string, string>();
                foreach (var item in PropertyInfoCaches)
                {
                    var forModel = item.AttributeInfoCaches.FirstOrDefault(s => s.Attribute is ForModel);
                    if (forModel != null)
                    {
                        SchemaMappingValues.Add(item.PropertyName, (forModel.Attribute as ForModel).AlternatingName);
                    }
                    else
                    {
                        SchemaMappingValues.Add(item.PropertyName, item.PropertyName);
                    }
                }
            }

            return SchemaMappingValues.Values.ToArray();
        }
    }
}
