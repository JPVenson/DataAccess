using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using JPB.DataAccess.ModelsAnotations;

namespace JPB.DataAccess.Config.Model
{
	public class ClassInfoCache
	{
		public ClassInfoCache(Type type)
		{
			this.AttributeInfoCaches = new List<AttributeInfoCache>();
			this.PropertyInfoCaches = new List<PropertyInfoCache>();
			this.MethodInfoCaches = new List<MethodInfoCache>();
			this.ConstructorInfoCaches = new List<ConstructorInfoCache>();
			ClassName = type.FullName;
			Type = type;

			this.AttributeInfoCaches = type
				.GetCustomAttributes(true)
				.Where(s => s is Attribute)
				.Select(s => new AttributeInfoCache(s as Attribute))
				.ToList();
			this.PropertyInfoCaches = type
				.GetProperties()
				.Select(s => new PropertyInfoCache(s))
				.ToList();
			this.MethodInfoCaches = type
				.GetMethods()
				.Select(s => new MethodInfoCache(s))
				.ToList();
			this.ConstructorInfoCaches = type
				.GetConstructors()
				.Select(s => new ConstructorInfoCache(s))
				.ToList();
		}

		/// <summary>
		/// When alternating the Configuration you have to call this method to renew the property enumerations. This also happens after the usage of the config attribute
		/// </summary>
		/// <param name="withSubProperty"></param>
		public void RenumeratePropertys(bool withSubProperty)
		{
			if (withSubProperty)
				foreach (var propertyInfoCach in PropertyInfoCaches)
				{
					propertyInfoCach.RenumeratePropertys();
				}

			this.ForModel = this.AttributeInfoCaches.FirstOrDefault(s => s.Attribute is ForModel);
			this.SelectFactory = this.AttributeInfoCaches.FirstOrDefault(s => s.Attribute is SelectFactoryAttribute);
			this.HasRelations = this.AttributeInfoCaches.Any(s => s.Attribute is ForeignKeyAttribute);
		}

		public void CheckForConfig()
		{
			var configMethods = MethodInfoCaches
				.Where(f => f.AttributeInfoCaches.Any(e => e.Attribute is ConfigMehtodAttribute))
				.ToArray();
			if (!configMethods.Any())
				return;

			var config = new Config();
			foreach (var item in configMethods)
			{
				item.MethodInfo.Invoke(null, new object[] { config });
			}

			RenumeratePropertys(true);
		}

		public string ClassName { get; private set; }
		public Type Type { get; private set; }

		public Func<IDataRecord, object> Factory { get; set; }
		public bool FullFactory { get; set; }

		public AttributeInfoCache ForModel { get; private set; }
		public AttributeInfoCache SelectFactory { get; private set; }

		/// <summary>
		/// Key is C# Property name and Value is DB Eqivalent
		/// </summary>
		public Dictionary<string, string> SchemaMappingValues { get; private set; }
		public List<PropertyInfoCache> PropertyInfoCaches { get; private set; }
		public List<AttributeInfoCache> AttributeInfoCaches { get; private set; }
		public List<MethodInfoCache> MethodInfoCaches { get; private set; }
		public List<ConstructorInfoCache> ConstructorInfoCaches { get; set; }
		public bool HasRelations { get; private set; }

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
			var mappings = SchemaMappingValues.FirstOrDefault(s => s.Value.Equals(databaseName, StringComparison.InvariantCulture));
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
