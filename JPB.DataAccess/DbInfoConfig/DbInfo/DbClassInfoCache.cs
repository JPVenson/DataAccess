/*
This work is licensed under the Creative Commons Attribution-ShareAlike 4.0 International License. 
To view a copy of this license, visit http://creativecommons.org/licenses/by-sa/4.0/.
Please consider to give some Feedback on CodeProject

http://www.codeproject.com/Articles/818690/Yet-Another-ORM-ADO-NET-Wrapper

*/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using JPB.DataAccess.Contacts.MetaApi;
using JPB.DataAccess.MetaApi.Model;
using JPB.DataAccess.ModelsAnotations;

namespace JPB.DataAccess.DbInfoConfig.DbInfo
{
	/// <summary>
	///     for internal use only
	/// </summary>
	public class DbClassInfoCache : ClassInfoCache<DbPropertyInfoCache, DbAttributeInfoCache, DbMethodInfoCache, DbConstructorInfoCache, DbMethodArgument>
	{
		/// <summary>
		/// 
		/// </summary>
#if !DEBUG
		[DebuggerHidden]
#endif
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public DbClassInfoCache()
		{
			SchemaMappingValues = new Dictionary<string, string>();
		}

		private Dictionary<string, string> _invertedSchema;

		internal DbClassInfoCache(Type type, bool anon = false)
			: base(type, anon)
		{
			SchemaMappingValues = new Dictionary<string, string>();
			Refresh(true);
		}

		/// <summary>
		/// For interal use Only
		/// </summary>
		/// <param name="type"></param>
		/// <param name="anon"></param>
		/// <returns></returns>
		public override IClassInfoCache<DbPropertyInfoCache, DbAttributeInfoCache, DbMethodInfoCache, DbConstructorInfoCache, DbMethodArgument> Init(Type type, bool anon = false)
		{
			var item = base.Init(type, anon);
			//.ToDictionary(s => s.Key);
			PropertyInfoCaches = new Dictionary<string, DbPropertyInfoCache>(PropertyInfoCaches
				.Where(f => f.Value.AttributeInfoCaches.All(e => !(e.Attribute is IgnoreReflectionAttribute))).ToDictionary(s => s.Key, f => f.Value));
			MethodInfoCaches = new HashSet<DbMethodInfoCache>(MethodInfoCaches.Where(f => f.AttributeInfoCaches.All(d => !(d.Attribute is IgnoreReflectionAttribute))));
			ConstructorInfoCaches = new HashSet<DbConstructorInfoCache>(ConstructorInfoCaches.Where(f => f.AttributeInfoCaches.All(e => !(e.Attribute is IgnoreReflectionAttribute))));
			foreach (var dbPropertyInfoCach in PropertyInfoCaches)
			{
				dbPropertyInfoCach.Value.DeclaringClass = this;
			}
			foreach (var dbPropertyInfoCach in MethodInfoCaches)
			{
				dbPropertyInfoCach.DeclaringClass = this;
			}
			foreach (var dbPropertyInfoCach in ConstructorInfoCaches)
			{
				dbPropertyInfoCach.DeclaringClass = this;
			}

			Refresh(true);
			return item;
		}

		/// <summary>
		///     If enumerated a method that creats an Instance and then fills all propertys
		/// </summary>
		public Func<IDataRecord, object> Factory { get; set; }

		/// <summary>
		///     Internal Use only
		/// </summary>
#if !DEBUG
		[DebuggerHidden]
#endif
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool FullFactory { get; set; }

		/// <summary>
		///     If known the ForModelAttribute attribute
		/// </summary>
		public DbAttributeInfoCache<ForModelAttribute> ForModel { get; private set; }

		/// <summary>
		///     If known the SelectFactory Attribute
		/// </summary>
		public DbAttributeInfoCache<SelectFactoryAttribute> SelectFactory { get; private set; }

		/// <summary>
		///     Key is C# Property name and Value is DB Eqivalent
		/// </summary>
		public Dictionary<string, string> SchemaMappingValues { get; private set; }

		/// <summary>
		///		Easy access to the SQL Table name
		/// </summary>
		public string TableName
		{
			get
			{
				if (ForModel == null)
					return ClassName;
				return ForModel.Attribute.AlternatingName;
			}
		}

		/// <summary>
		///     Internal use Only
		/// </summary>
#if !DEBUG
		[DebuggerHidden]
#endif
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool HasRelations { get; private set; }

		/// <summary>
		///		If known the Property with an RowVersion Attribute
		/// </summary>
		public DbPropertyInfoCache RowVersionProperty { get; private set; }

		/// <summary>
		///		If knwon the Property with an PrimaryKey Attribute
		/// </summary>
		public DbPropertyInfoCache PrimaryKeyProperty { get; private set; }

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

			ForModel = DbAttributeInfoCache<ForModelAttribute>.WrapperOrNull(AttributeInfoCaches.FirstOrDefault(s => s.Attribute is ForModelAttribute));
			SelectFactory = DbAttributeInfoCache<SelectFactoryAttribute>.WrapperOrNull(AttributeInfoCaches.FirstOrDefault(s => s.Attribute is SelectFactoryAttribute));
			HasRelations = AttributeInfoCaches.Any(s => s.Attribute is ForeignKeyAttribute);

			RowVersionProperty = PropertyInfoCaches.FirstOrDefault(s => s.Value.RowVersionAttribute != null).Value;
			PrimaryKeyProperty = PropertyInfoCaches.FirstOrDefault(s => s.Value.PrimaryKeyAttribute != null).Value;

			CreateSchemaMapping();
		}

		internal void CheckCtor()
		{
			var hasAutoGeneratorAttribute = AttributeInfoCaches.Any(f => f.Attribute is AutoGenerateCtorAttribute);
			if (hasAutoGeneratorAttribute && Factory == null)
			{
				CreateFactory();
			}
		}

		internal void CheckForConfig()
		{
			var configMethods = MethodInfoCaches
				.Where(f => f.AttributeInfoCaches.Any(e => e.Attribute is ConfigMehtodAttribute))
				.ToArray();
			if (configMethods.Any())
			{
				var config = new DbConfig();
				foreach (var item in configMethods)
				{
					item.MethodInfo.Invoke(null, new object[] { config });
				}

				Refresh(true);
				CheckCtor();
			}
		}

		internal void CreateFactory()
		{
			if (!FullFactory)
				Factory = FactoryHelper.CreateFactory(Type, DbConfig.ConstructorSettings);
			FullFactory = true;
		}

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
			string mappings;
			if (!_invertedSchema.TryGetValue(databaseName, out mappings))
			{
				return databaseName;
			}
			return mappings;
		}

		internal void CreateSchemaMapping()
		{
			SchemaMappingValues = new Dictionary<string, string>();
			_invertedSchema = new Dictionary<string, string>();
			foreach (var item in PropertyInfoCaches)
			{
				if (item.Value.IgnoreAnyAttribute != null)
				{
					continue;
				}
				SchemaMappingValues.Add(item.Value.PropertyName, item.Value.DbName);
				_invertedSchema.Add(item.Value.DbName, item.Value.PropertyName);
			}
		}

		internal string[] LocalToDbSchemaMapping()
		{
			return SchemaMappingValues.Values.ToArray();
		}
	}
}