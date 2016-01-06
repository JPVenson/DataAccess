using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using JPB.DataAccess.ModelsAnotations;

namespace JPB.DataAccess.Config.Model
{
	/// <summary>
	/// for internal use only
	/// </summary>
	public class ClassInfoCache
	{
		internal ClassInfoCache(Type type, bool anon = false)
		{
			this.AttributeInfoCaches = new HashSet<AttributeInfoCache>();
			this.PropertyInfoCaches = new HashSet<PropertyInfoCache>();
			this.MethodInfoCaches = new HashSet<MethodInfoCache>();
			this.ConstructorInfoCaches = new HashSet<ConstructorInfoCache>();
			ClassName = type.FullName;
			Type = type;
			this.AttributeInfoCaches = new HashSet<AttributeInfoCache>(type
					.GetCustomAttributes(true)
					.Where(s => s is Attribute)
					.Select(s => new AttributeInfoCache(s as Attribute)));
			this.PropertyInfoCaches = new HashSet<PropertyInfoCache>(type
				.GetProperties()
				.Select(s => new PropertyInfoCache(s, anon)));
			this.MethodInfoCaches = new HashSet<MethodInfoCache>(type
				.GetMethods()
				.Select(s => new MethodInfoCache(s)));
			this.ConstructorInfoCaches = new HashSet<ConstructorInfoCache>(type
				.GetConstructors()
				.Select(s => new ConstructorInfoCache(s)));
			CreateSchemaMapping();
		}

		/// <summary>
		/// When alternating the Configuration you have to call this method to renew the property enumerations. This also happens after the usage of the config attribute
		/// </summary>
		/// <param name="withSubProperty"></param>
		public void Refresh(bool withSubProperty)
		{
			if (withSubProperty)
				foreach (var propertyInfoCach in PropertyInfoCaches)
				{
					propertyInfoCach.Refresh();
				}

			this.ForModel = this.AttributeInfoCaches.FirstOrDefault(s => s.Attribute is ForModel);
			this.SelectFactory = this.AttributeInfoCaches.FirstOrDefault(s => s.Attribute is SelectFactoryAttribute);
			this.HasRelations = this.AttributeInfoCaches.Any(s => s.Attribute is ForeignKeyAttribute);
			CreateSchemaMapping();
		}

		internal void CheckForConfig()
		{
			var configMethods = MethodInfoCaches
				.Where(f => f.AttributeInfoCaches.Any(e => e.Attribute is ConfigMehtodAttribute))
				.ToArray();
			if (configMethods.Any())
			{
				var config = new Config();
				foreach (var item in configMethods)
				{
					item.MethodInfo.Invoke(null, new object[] { config });
				}

				Refresh(true);
			}
			var hasAutoGeneratorAttribute = this.AttributeInfoCaches.Any(f => f.Attribute is AutoGenerateCtorAttribute);
			if (hasAutoGeneratorAttribute)
			{
				CreateFactory();
			}
		}

		internal void CreateFactory()
		{
			if (!FullFactory)
				this.Factory = FactoryHelper.CreateFactory(Type, Config.ConstructorSettings);
			this.FullFactory = true;
		}

		/// <summary>
		/// The .net ClassName
		/// </summary>
		public string ClassName { get; private set; }
		/// <summary>
		/// The .net Type instance 
		/// </summary>
		public Type Type { get; private set; }

		/// <summary>
		/// If enumerated a method that creats an Instance and then fills all propertys
		/// </summary>
		public Func<IDataRecord, object> Factory { get; set; }

		/// <summary>
		/// Internal Use only
		/// </summary>
		public bool FullFactory { get; set; }

		/// <summary>
		/// If known the ForModel attribute
		/// </summary>
		public AttributeInfoCache ForModel { get; private set; }

		/// <summary>
		/// If known the SelectFactory Attribute
		/// </summary>
		public AttributeInfoCache SelectFactory { get; private set; }

		/// <summary>
		/// Key is C# Property name and Value is DB Eqivalent
		/// </summary>
		public Dictionary<string, string> SchemaMappingValues { get; private set; }

		/// <summary>
		/// All Propertys
		/// </summary>
		public HashSet<PropertyInfoCache> PropertyInfoCaches { get; private set; }

		/// <summary>
		/// All Attributes on class level
		/// </summary>
		public HashSet<AttributeInfoCache> AttributeInfoCaches { get; private set; }

		/// <summary>
		/// All Mehtods
		/// </summary>
		public HashSet<MethodInfoCache> MethodInfoCaches { get; private set; }

		/// <summary>
		/// All Constructors
		/// </summary>
		public HashSet<ConstructorInfoCache> ConstructorInfoCaches { get; private set; }

		/// <summary>
		/// Internal use Only
		/// </summary>
		public bool HasRelations { get; private set; }

		internal string SchemaMappingLocalToDatabase(string cSharpName)
		{
			var mappings = SchemaMappingValues.FirstOrDefault(s => s.Key.Equals(cSharpName));
			if (mappings.Equals(default(KeyValuePair<string, string>)))
			{
				return cSharpName;
			}
			return mappings.Value;
		}

		internal string SchemaMappingDatabaseToLocal(string databaseName)
		{
			var mappings = SchemaMappingValues.FirstOrDefault(s => s.Value.Equals(databaseName, StringComparison.InvariantCulture));
			if (mappings.Equals(default(KeyValuePair<string, string>)))
			{
				return databaseName;
			}
			return mappings.Key;
		}

		internal void CreateSchemaMapping()
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

		internal string[] LocalToDbSchemaMapping()
		{
			return SchemaMappingValues.Values.ToArray();
		}

		internal string[] DbToLocalSchemaMapping()
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

			return SchemaMappingValues.Values.ToArray();
		}
	}
}
