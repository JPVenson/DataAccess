﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using JPB.DataAccess.ModelsAnotations;

namespace JPB.DataAccess.Config.Model
{
	/// <summary>
	///     for internal use only
	/// </summary>
	public class ClassInfoCache
	{
		private Dictionary<string, string> _invertedSchema;

		internal ClassInfoCache(Type type, bool anon = false)
		{
			ClassName = type.FullName;
			Type = type;
			AttributeInfoCaches = new HashSet<AttributeInfoCache>(type
				.GetCustomAttributes(true)
				.Where(s => s is Attribute)
				.Select(s => new AttributeInfoCache(s as Attribute)));
			PropertyInfoCaches = new Dictionary<string, PropertyInfoCache>(type
				.GetProperties()
				.Select(s => new PropertyInfoCache(s, anon))
				.ToDictionary(s => s.PropertyName, s => s));
			MethodInfoCaches = new HashSet<MethodInfoCache>(type
				.GetMethods()
				.Select(s => new MethodInfoCache(s)));
			ConstructorInfoCaches = new HashSet<ConstructorInfoCache>(type
				.GetConstructors()
				.Select(s => new ConstructorInfoCache(s)));
			CreateSchemaMapping();
		}

		/// <summary>
		///     The .net ClassName
		/// </summary>
		public string ClassName { get; private set; }

		/// <summary>
		///     The .net Type instance
		/// </summary>
		public Type Type { get; private set; }

		/// <summary>
		///     If enumerated a method that creats an Instance and then fills all propertys
		/// </summary>
		public Func<IDataRecord, object> Factory { get; set; }

		/// <summary>
		///     Internal Use only
		/// </summary>
		[DebuggerHidden]
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool FullFactory { get; set; }

		/// <summary>
		///     If known the ForModel attribute
		/// </summary>
		public AttributeInfoCache ForModel { get; private set; }

		/// <summary>
		///     If known the SelectFactory Attribute
		/// </summary>
		public AttributeInfoCache SelectFactory { get; private set; }

		/// <summary>
		///     Key is C# Property name and Value is DB Eqivalent
		/// </summary>
		public Dictionary<string, string> SchemaMappingValues { get; private set; }

		/// <summary>
		///     All Propertys
		/// </summary>
		public Dictionary<string, PropertyInfoCache> PropertyInfoCaches { get; private set; }

		/// <summary>
		///     All Attributes on class level
		/// </summary>
		public HashSet<AttributeInfoCache> AttributeInfoCaches { get; private set; }

		/// <summary>
		///     All Mehtods
		/// </summary>
		public HashSet<MethodInfoCache> MethodInfoCaches { get; private set; }

		/// <summary>
		///     All Constructors
		/// </summary>
		public HashSet<ConstructorInfoCache> ConstructorInfoCaches { get; private set; }

		/// <summary>
		///     Internal use Only
		/// </summary>
		[DebuggerHidden]
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool HasRelations { get; private set; }

		/// <summary>
		///     When alternating the Configuration you have to call this method to renew the property enumerations.
		///     This also happens after the usage of the config attribute
		/// </summary>
		public void Refresh(bool withSubProperty)
		{
			if (withSubProperty)
				foreach (var propertyInfoCach in PropertyInfoCaches)
				{
					propertyInfoCach.Value.Refresh();
				}

			ForModel = AttributeInfoCaches.FirstOrDefault(s => s.Attribute is ForModel);
			SelectFactory = AttributeInfoCaches.FirstOrDefault(s => s.Attribute is SelectFactoryAttribute);
			HasRelations = AttributeInfoCaches.Any(s => s.Attribute is ForeignKeyAttribute);
			CreateSchemaMapping();
			CheckCtor();
		}

		internal void CheckCtor()
		{
			bool hasAutoGeneratorAttribute = AttributeInfoCaches.Any(f => f.Attribute is AutoGenerateCtorAttribute);
			if (hasAutoGeneratorAttribute && Factory == null)
			{
				CreateFactory();
			}
		}

		internal void CheckForConfig()
		{
			MethodInfoCache[] configMethods = MethodInfoCaches
				.Where(f => f.AttributeInfoCaches.Any(e => e.Attribute is ConfigMehtodAttribute))
				.ToArray();
			if (configMethods.Any())
			{
				var config = new Config();
				foreach (MethodInfoCache item in configMethods)
				{
					item.MethodInfo.Invoke(null, new object[] {config});
				}

				Refresh(true);
			}
		}

		internal void CreateFactory()
		{
			if (!FullFactory)
				Factory = FactoryHelper.CreateFactory(Type, Config.ConstructorSettings);
			FullFactory = true;
		}

		internal string SchemaMappingLocalToDatabase(string cSharpName)
		{
			KeyValuePair<string, string> mappings = SchemaMappingValues.FirstOrDefault(s => s.Key.Equals(cSharpName));
			if (mappings.Equals(default(KeyValuePair<string, string>)))
			{
				return cSharpName;
			}
			return mappings.Value;
		}

		internal string SchemaMappingDatabaseToLocal(string databaseName)
		{
			string mappings;
			if (!_invertedSchema.TryGetValue(databaseName, out mappings))
			{
				return databaseName;
			}
			return mappings;


			//var mappings = SchemaMappingValues.FirstOrDefault(s => s.Value.Equals(databaseName, StringComparison.InvariantCulture));
			//if (mappings.Equals(default(KeyValuePair<string, string>)))
			//{
			//	return databaseName;
			//}
			//return mappings.Key;
		}

		internal void CreateSchemaMapping()
		{
			SchemaMappingValues = new Dictionary<string, string>();
			_invertedSchema = new Dictionary<string, string>();
			foreach (var item in PropertyInfoCaches)
			{
				SchemaMappingValues.Add(item.Value.PropertyName, item.Value.DbName);
				_invertedSchema.Add(item.Value.DbName, item.Value.PropertyName);
			}
		}

		internal string[] LocalToDbSchemaMapping()
		{
			return SchemaMappingValues.Values.ToArray();
		}

		//internal string[] DbToLocalSchemaMapping()
		//{
		//	if (SchemaMappingValues == null)
		//	{
		//		SchemaMappingValues = new Dictionary<string, string>();
		//		foreach (var item in PropertyInfoCaches)
		//		{
		//			SchemaMappingValues.Append(item.Value.PropertyName, item.Value.DbName);
		//		}
		//	}

		//	return SchemaMappingValues.Values.ToArray();
		//}
	}
}