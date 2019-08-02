#region

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using JPB.DataAccess.AdoWrapper;
using JPB.DataAccess.Contacts.MetaApi;
using JPB.DataAccess.MetaApi.Model;
using JPB.DataAccess.ModelsAnotations;

#endregion

namespace JPB.DataAccess.DbInfoConfig.DbInfo
{
	/// <summary>
	///     for internal use only
	/// </summary>
	public class DbClassInfoCache :
			ClassInfoCache
			<DbPropertyInfoCache, DbAttributeInfoCache, DbMethodInfoCache, DbConstructorInfoCache, DbMethodArgument>
	{
		/// <summary>
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

		internal DbClassInfoCache(Type type, bool anon = false)
			: base(type, anon)
		{
			SchemaMappingValues = new Dictionary<string, string>();
			Refresh(true);
		}

		private Dictionary<string, string> _invertedSchema;

		/// <summary>
		///     If enumerated a method that creats an Instance and then fills all propertys
		/// </summary>
		public Func<EagarDataRecord, object> Factory { get; set; }

		/// <summary>
		///     Internal Use only
		/// </summary>
#if !DEBUG
		[DebuggerHidden]
#endif
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool FullFactory { get; internal set; }

		/// <summary>
		///     If known the WrapDbNullablesAttribute attribute
		/// </summary>
		[Obsolete("This attribute is obsolete because null wrapping now happens by default", true)]
		public DbAttributeInfoCache<WrapDbNullablesAttribute> WrapNullables { get; set; }

		/// <summary>
		///     If known the ForModelAttribute attribute
		/// </summary>
		public DbAttributeInfoCache<ForModelAttribute> ForModel { get; private set; }

		/// <summary>
		///     If known the SelectFactory Attribute
		/// </summary>
		public DbAttributeInfoCache<SelectFactoryAttribute> SelectFactory { get; private set; }

		/// <summary>
		///     If knwon the MethodProxyAttribute Attribute
		/// </summary>
		public DbAttributeInfoCache<MethodProxyAttribute> MethodProxyAttribute { get; private set; }

		/// <summary>
		///     Key is C# Property name and Value is DB Eqivalent
		/// </summary>
		public Dictionary<string, string> SchemaMappingValues { get; private set; }

		/// <summary>
		///     Easy access to the SQL Table name
		/// </summary>
		public string TableName
		{
			get
			{
				if (ForModel == null)
				{
					return Name.Split('.').Last();
				}
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
		///     If known the Property with an RowVersion Attribute
		/// </summary>
		public DbPropertyInfoCache RowVersionProperty { get; private set; }

		/// <summary>
		///     If knwon the Property with an PrimaryKey Attribute
		/// </summary>
		public DbPropertyInfoCache PrimaryKeyProperty { get; private set; }

		/// <summary>
		///     For interal use Only
		/// </summary>
		/// <param name="type"></param>
		/// <param name="anon"></param>
		/// <returns></returns>
		public override
				IClassInfoCache
				<DbPropertyInfoCache, DbAttributeInfoCache, DbMethodInfoCache, DbConstructorInfoCache, DbMethodArgument> Init(
					Type type, bool anon = false)
		{
			var item = base.Init(type, anon);
			//.ToDictionary(s => s.Key);
			Propertys = new Dictionary<string, DbPropertyInfoCache>(Propertys
					.Where(f => f.Value.Attributes.All(e => !(e.Attribute is IgnoreReflectionAttribute)))
					.ToDictionary(s => s.Key, f => f.Value));
			Mehtods =
					new HashSet<DbMethodInfoCache>(
					Mehtods.Where(f => f.Attributes.All(d => !(d.Attribute is IgnoreReflectionAttribute))));
			Constructors =
					new HashSet<DbConstructorInfoCache>(
					Constructors.Where(f => f.Attributes.All(e => !(e.Attribute is IgnoreReflectionAttribute))));
			foreach (var dbPropertyInfoCach in Propertys)
			{
				dbPropertyInfoCach.Value.DeclaringClass = this;
			}
			foreach (var dbPropertyInfoCach in Mehtods)
			{
				dbPropertyInfoCach.DeclaringClass = this;
			}
			foreach (var dbPropertyInfoCach in Constructors)
			{
				dbPropertyInfoCach.DeclaringClass = this;
			}

			Refresh(true);
			return item;
		}

		/// <summary>
		///     When alternating the Configuration you have to call this method to renew the property enumerations.
		///     This also happens after the usage of the config attribute
		/// </summary>
		public void Refresh(bool withSubProperty)
		{
			if (withSubProperty)
			{
				foreach (var propertyInfoCach in Propertys)
				{
					propertyInfoCach.Value.Refresh();
				}
			}

			ForModel = DbAttributeInfoCache<ForModelAttribute>.WrapperOrNull(Attributes);
			SelectFactory = DbAttributeInfoCache<SelectFactoryAttribute>.WrapperOrNull(Attributes);
			var preConfig = MethodProxyAttribute == null;

			MethodProxyAttribute = DbAttributeInfoCache<MethodProxyAttribute>.WrapperOrNull(Attributes);

			HasRelations = Attributes.Any(s => s.Attribute is ForeignKeyAttribute);

			RowVersionProperty = Propertys.FirstOrDefault(s => s.Value.RowVersionAttribute != null).Value;
			PrimaryKeyProperty = Propertys.FirstOrDefault(s => s.Value.PrimaryKeyAttribute != null).Value;

			CreateSchemaMapping();
		}

		internal void CheckCtor(DbConfig config)
		{
			var hasAutoGeneratorAttribute = Attributes.Any(f => f.Attribute is AutoGenerateCtorAttribute);
			if (hasAutoGeneratorAttribute && Factory == null)
			{
				CreateFactory(config);
			}
		}

		internal void CheckForConfig(DbConfig config)
		{
			var targetType = this;

			if (MethodProxyAttribute != null)
			{
				targetType = config.GetOrCreateClassInfoCache(MethodProxyAttribute.Attribute.MethodProxy);
			}

			var configMethods =
					targetType.Mehtods.Where(f => f.MethodInfo.IsStatic && f.Attributes.Any(e => e.Attribute is ConfigMehtodAttribute)).ToArray();
			if (configMethods.Any())
			{
				foreach (
				var item in configMethods.Where(f => f.Arguments.Count == 1 && f.Arguments.First().Type == typeof(DbConfig)))
				{
					item.MethodInfo.Invoke(null, new object[] {config});
				}

				var fod = config.GetOrCreateClassInfoCache(typeof(ConfigurationResolver<>)
						                .MakeGenericType(Type))
				                .Constructors
				                .First();

				var resolver = fod.Invoke(null, config, this);
				foreach (var item in configMethods.Where(f => f.Arguments.Count == 1 && f.Arguments.First().Type == typeof(ConfigurationResolver<>).MakeGenericType(Type)))
				{
					item.MethodInfo.Invoke(null, new[] {resolver});
				}
				Refresh(true);
			}
			CheckCtor(config);
		}

		internal void CreateFactory(DbConfig config)
		{
			if (!FullFactory)
			{
				Factory = FactoryHelper.CreateFactory(this, config.ConstructorSettings);
			}
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
			foreach (var item in Propertys)
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