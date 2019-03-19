#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using JPB.DataAccess.AdoWrapper;
using JPB.DataAccess.Contacts;
using JPB.DataAccess.DbCollection;
using JPB.DataAccess.DbInfoConfig;
using JPB.DataAccess.DbInfoConfig.DbInfo;
using JPB.DataAccess.ModelsAnotations;
using JPB.DataAccess.Query.Operators;

#endregion

namespace JPB.DataAccess.Manager
{
	/// <summary>
	///     Contanins some Helper mehtods for CRUD operation
	/// </summary>
	[DebuggerDisplay(
		"DB={" + nameof(DatabaseStrategy) +
		"}, QueryDebug={Database.LastExecutedQuery ?? Database.LastExecutedQuery.DebuggerQuery}")]
#if !DEBUG
	[DebuggerStepThrough]
#endif
	public partial class DbAccessLayer
	{
		private IDatabase _database;

		static DbAccessLayer()
		{
			SProcedureDbAccessLayer();
			ProviderCollection = new PreDefinedProviderCollection();
			AsyncDefault = true;
		}

		internal DbAccessLayer(DbConfig config = null)
		{
			Config = config ?? new DbConfig(true);
			DefaultLookupPath = AppDomain.CurrentDomain.BaseDirectory;
			LoadCompleteResultBeforeMapping = true;
			CheckFactoryArguments = true;
			UpdateDbAccessLayer();
			Async = AsyncDefault;
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="DbAccessLayer" /> class.
		/// </summary>
		/// <param name="dbAccessType">Type of the database access.</param>
		/// <param name="connection">The connection.</param>
		/// <param name="config">The configuration.</param>
		/// <exception cref="InvalidEnumArgumentException">dbAccessType</exception>
		public DbAccessLayer(DbAccessType dbAccessType, string connection, DbConfig config = null)
			: this(config)
		{
			if (dbAccessType == DbAccessType.Unknown)
			{
				throw new InvalidEnumArgumentException(nameof(dbAccessType), (int) DbAccessType.Unknown,
					typeof(DbAccessType));
			}

			DbAccessType = dbAccessType;
			Database = new DefaultDatabaseAccess(new InstanceConnectionController());
			var database =
				GenerateStrategyFromExternal(ProviderCollection.FirstOrDefault(s => s.Key == dbAccessType).Value,
					connection);
			Database.Attach(database);
			DatabaseStrategy = database;
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="DbAccessLayer" /> class.
		/// </summary>
		/// <param name="fullTypeNameToIDatabaseStrategy">The full type name to database strategy.</param>
		/// <param name="connection">The connection.</param>
		/// <param name="config">The configuration.</param>
		/// <exception cref="System.ArgumentNullException">fullTypeNameToIDatabaseStrategy</exception>
		public DbAccessLayer(string fullTypeNameToIDatabaseStrategy, string connection, DbConfig config = null)
			: this(config)
		{
			if (string.IsNullOrEmpty(fullTypeNameToIDatabaseStrategy))
			{
				throw new ArgumentNullException(nameof(fullTypeNameToIDatabaseStrategy));
			}

			ResolveDbType(fullTypeNameToIDatabaseStrategy);

			var database = GenerateStrategyFromExternal(fullTypeNameToIDatabaseStrategy, connection);

			Database = new DefaultDatabaseAccess(new InstanceConnectionController());
			Database.Attach(database);
			DatabaseStrategy = database;
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="DbAccessLayer" /> class.
		/// </summary>
		/// <param name="database">The database.</param>
		/// <param name="config">The configuration.</param>
		/// <exception cref="System.ArgumentNullException">database</exception>
		public DbAccessLayer(IDatabaseStrategy database, DbConfig config = null)
			: this(config)
		{
			if (database == null)
			{
				throw new ArgumentNullException(nameof(database));
			}

			DbAccessType = database.SourceDatabase;
			Database = new DefaultDatabaseAccess(new InstanceConnectionController());
			Database.Attach(database);
			DatabaseStrategy = database;
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="DbAccessLayer" /> class.
		/// </summary>
		/// <param name="database">The database.</param>
		/// <param name="config">The configuration.</param>
		/// <exception cref="System.ArgumentNullException">database</exception>
		public DbAccessLayer(IDatabase database, DbConfig config = null)
			: this(config)
		{
			if (database == null)
			{
				throw new ArgumentNullException(nameof(database));
			}

			DbAccessType = DbAccessType.Unknown;
			Database = database;
		}

		/// <summary>
		///     Gets the current Config store this instance is attached to.
		/// </summary>
		/// <value>
		///     The configuration.
		/// </value>
		public DbConfig Config { get; }

		/// <summary>
		///     If Enabled the enumeration of Commands will happen in a Thread save way.
		///		It Ensures that only one Operation will be execute at a time.
		/// </summary>
		public bool ThreadSave { get; set; }

		private readonly object _lockRoot = new object();

		/// <summary>
		///     Defines a set of Providers that are inclueded in this DLL or are weak refernced.
		/// </summary>
		public static PreDefinedProviderCollection ProviderCollection { get; }

		/// <summary>
		///     if set the created reader of an read operation will be completely stored in memory then the open connection will be
		///     closed
		///     Default is true
		/// </summary>
		public bool LoadCompleteResultBeforeMapping { get; set; }

		/// <summary>
		///     If set to True a strict check for the Targetdatabase Property on each Factory or provider specific method is done
		///     otherwise this Check is skiped
		/// </summary>
		public bool IsMultiProviderEnvironment { get; set; }

		/// <summary>
		///     For Internal Use only
		/// </summary>
		public IDatabaseStrategy DatabaseStrategy { get; }

		/// <summary>
		///     Selected dbAccessType
		/// </summary>
		public DbAccessType DbAccessType { get; private set; }

		/// <summary>
		///     The default path for loading external Providers via DbAccessType
		/// </summary>
		public string DefaultLookupPath { get; }

		/// <summary>
		///     Current Database
		///     Can be used to write multi statements
		///     Is used for ALL NonStatic statments creators
		/// </summary>
		public IDatabase Database
		{
			get { return _database; }
			set
			{
				if (_database == null)
				{
					_database = value;
				}
				else
				{
					throw new NotSupportedException(
						"Runtime change of Database is not allowed. Create a new DbAccessLayer object");
				}
			}
		}

		/// <summary>
		///     Quick access to the underlying Config store
		/// </summary>
		/// <param name="type">The type.</param>
		/// <returns></returns>
		public DbClassInfoCache GetClassInfo(Type type)
		{
			return Config.GetOrCreateClassInfoCache(type);
		}

		/// <summary>
		///     Creates a Copy that reuses the current connection, type and the given ConnectionHandler
		/// </summary>
		/// <returns></returns>
		public DbAccessLayer Copy()
		{
			return new DbAccessLayer(_database.Clone(), Config);
		}

		internal IDatabaseStrategy GenerateStrategyFromExternal(string fullValidIdentifyer, string connection)
		{
			if (string.IsNullOrEmpty(fullValidIdentifyer))
			{
				throw new ArgumentException("The used type: " + fullValidIdentifyer + " was not found");
			}

			var type = Type.GetType(fullValidIdentifyer);
			if (type == null)
			{
				var parts = fullValidIdentifyer.Split(',')
					.Select(x => x.Trim())
					.ToList();

				var name = parts[0];
				var assembly = parts.Count < 2 ? null : parts[1];
				var version = parts.Count < 3 ? null : parts[2];

				if (version != null && !version.StartsWith("Version="))
				{
					throw new ArgumentException("Invalid version: " + version);
				}

				if (assembly == null)
				{
					throw new ArgumentException("Invalid Assembly declaration, Not existing: " + fullValidIdentifyer);
				}

				try
				{
					var assemblyName = new AssemblyName(fullValidIdentifyer);
					assemblyName.CodeBase = Path.Combine(DefaultLookupPath, assemblyName.CodeBase);
					Assembly.Load(assemblyName);
					type = Type.GetType(fullValidIdentifyer);
					if (type == null)
					{
						throw new DllNotFoundException("Could not load the requested type from loaded Assembly.")
						{
							Data =
							{
								{"Assembly", assemblyName},
								{"Type", name},
								{"fullValidIdentifyer", fullValidIdentifyer}
							}
						};
					}
				}
				catch (Exception ex)
				{
					throw new AggregateException("Could not load the requested DatabaseStrategy from dll", ex);
				}
			}

			//check the type to be a Strategy

			if (!typeof(IDatabaseStrategy).IsAssignableFrom(type))
			{
				throw new ArgumentException("Type was found but does not inhert from IDatabaseStrategy");
			}

			//try constructor injection
			var ctOfType =
				type.GetConstructors()
					.FirstOrDefault(
						s => s.GetParameters().Length == 1 &&
						     s.GetParameters().First().ParameterType == typeof(string));
			if (ctOfType != null)
			{
				return ctOfType.Invoke(new object[] {connection}) as IDatabaseStrategy;
			}

			var instanceOfType = Activator.CreateInstance(type) as IDatabaseStrategy;
			if (instanceOfType == null)
			{
				throw new ArgumentException("Type was found but does not inhert from IDatabaseStrategy");
			}

			instanceOfType.ConnectionString = connection;
			return instanceOfType;
		}

		private void ResolveDbType(string fullTypeNameToIDatabaseStrategy)
		{
			// ReSharper disable once PossibleInvalidOperationException
			var firstOrDefault =
				ProviderCollection.Select(s => (KeyValuePair<DbAccessType, string>?) s)
					.FirstOrDefault(s => s.Value.Value == fullTypeNameToIDatabaseStrategy);
			DbAccessType = firstOrDefault == null ? DbAccessType.Unknown : firstOrDefault.Value.Key;
		}

		/// <summary>
		///     Check for Availability
		/// </summary>
		/// <returns></returns>
		public bool CheckDatabase()
		{
			if (Database == null)
			{
				return false;
			}

			try
			{
				Database.Connect();
				Database.CloseConnection();
			}
			catch (Exception)
			{
				return false;
			}

			return true;
		}

		/// <summary>
		///     Wraps a QueryCommand and its Paramters and then executes it
		/// </summary>
		/// <returns></returns>
		public int ExecuteGenericCommand(string query, IEnumerable<IQueryParameter> values)
		{
			var command = DbAccessLayerHelper.CreateCommand(Database, query);

			if (values != null)
			{
				foreach (var item in values)
				{
					command.Parameters.AddWithValue(item.Name, item.Value, Database);
				}
			}

			Database.LastExecutedQuery?.Refresh();
			return ExecuteGenericCommand(command);
		}

		/// <summary>
		///     Wraps a QueryCommand and its Paramters from Dynamic and then executes it
		/// </summary>
		/// <returns></returns>
		public int ExecuteGenericCommand(string query, dynamic paramenter)
		{
			var parameters = paramenter as IEnumerable<IQueryParameter>;
			if (parameters != null)
			{
				var parm = parameters;
				return ExecuteGenericCommand(query, parm);
			}

			return ExecuteGenericCommand(query,
				(IEnumerable<IQueryParameter>) DbAccessLayerHelper.EnumerateFromUnknownParameter(paramenter));
		}

		/// <summary>
		///     Execute a QueryCommand without Paramters
		/// </summary>
		/// <returns></returns>
		public int ExecuteGenericCommand(IDbCommand query)
		{
			Database.PrepaireRemoteExecution(query);
			RaiseNoResult(this, query);
			return Database.Run(s =>
			{
				try
				{
					if (ThreadSave)
					{
						Monitor.Enter(_lockRoot);
					}

					return s.ExecuteNonQuery(query);
				}
				finally
				{
					if (ThreadSave)
					{
						Monitor.Exit(_lockRoot);
					}
				}
			});
		}

		/// <summary>
		///     Execute a QueryCommand without Paramters
		/// </summary>
		/// <returns></returns>
		public async Task<int> ExecuteGenericCommandAsync(IDbCommand query)
		{
			Database.PrepaireRemoteExecution(query);
			RaiseNoResult(this, query);
			return await Database.RunAsync(async s => { return await s.ExecuteNonQueryAsync(query, Async); });
		}

		/// <summary>
		///     Execute a QueryCommand without Paramters
		/// </summary>
		/// <returns></returns>
		public int ExecuteGenericCommand(string query)
		{
			return ExecuteGenericCommand(DbAccessLayerHelper.CreateCommand(Database, query));
		}

		/// <summary>
		///     Creates a Strong typed query that awaits no Result
		/// </summary>
		/// <returns></returns>
		[MustUseReturnValue("Use the RootQuery to run a query to the database")]
		[Pure]
		public RootQuery Query()
		{
			return new RootQuery(this);
		}

		///// <summary>
		/////     Creates a Strong typed query that awaits a Result
		///// </summary>
		///// <returns></returns>
		//public RootQuery Query(Type targetType)
		//{
		//	return new RootQuery(this, targetType);
		//}

		///// <summary>
		/////     Creates a Strong typed query that awaits a Result
		///// </summary>
		///// <returns></returns>
		//public RootQuery Query<T>()
		//{
		//	return new RootQuery(this, typeof(T));
		//}

		/// <summary>
		///     Creates a new Instance based on possible Ctor's and the given
		///     <paramref name="reader" />
		/// </summary>
		/// <returns></returns>
		public object SetPropertysViaReflection(DbClassInfoCache type, IDataRecord reader)
		{
			return SetPropertysViaReflection(type, reader, DbAccessType);
		}

		/// <summary>
		///     Creates a new Instance based on possible Ctor's and the given
		///     <paramref name="reader" />
		/// </summary>
		/// <returns></returns>
		public object SetPropertysViaReflection(DbClassInfoCache type, IDataRecord reader,
			Dictionary<int, DbPropertyInfoCache> mapping)
		{
			bool created;
			var source = CreateInstance(type, reader, out created);
			if (created)
			{
				return source;
			}

#pragma warning disable 618
			return ReflectionPropertySet(Config, source, type, reader, mapping, DbAccessType);
#pragma warning restore 618
		}

		/// <summary>
		///     Creates an instance based on a Ctor injection or Reflection loading
		///     or when using a MsCoreLib type direct enumeration
		/// </summary>
		/// <returns></returns>
		public static object CreateInstance(DbClassInfoCache classInfo,
			IDataRecord reader)
		{
			bool loaded;
			return CreateInstance(classInfo, reader, out loaded);
		}

		/// <summary>
		///     Creates an instance based on a Ctor injection or Reflection loading
		///     or when using a MsCoreLib type direct enumeration
		/// </summary>
		/// <returns></returns>
		public static object CreateInstance(DbClassInfoCache classInfo,
			IDataRecord reader,
			out bool fullLoaded,
			DbAccessType? accessType = null)
		{
			if (classInfo.IsMsCoreFrameworkType && reader.FieldCount == 1)
			{
				fullLoaded = true;
				var plainValue = reader.GetValue(0);

				return plainValue;
			}

			if (classInfo.WrapNullables != null && !(reader is EgarNullableWrappedRecord) && reader is EgarDataRecord)
			{
				reader = new EgarNullableWrappedRecord(reader, ((EgarDataRecord) reader)._configStore);
			}

			if (classInfo.Factory != null)
			{
				fullLoaded = classInfo.FullFactory;
				var fullObject = classInfo.Factory(reader);
				return fullObject;
			}

			var factories = classInfo.Constructors.Where(s =>
					s.Arguments.Count == 1
					&& s.Arguments.First().Type == typeof(IDataRecord))
				.ToArray();

			var constructor = factories.FirstOrDefault(s =>
				                  s.Attributes.Any(f =>
					                  f.Attribute is ObjectFactoryMethodAttribute
					                  &&
					                  (!accessType.HasValue ||
					                   ((ObjectFactoryMethodAttribute) f.Attribute).TargetDatabase ==
					                   accessType.Value))) ??
			                  factories.FirstOrDefault();

			//maybe single ctor with param

			if (constructor != null)
			{
				if (constructor.Arguments.Count == 1 && constructor.Arguments.First().Type == typeof(IDataRecord))
				{
					classInfo.FullFactory = true;
					classInfo.Factory = s => constructor.Invoke(new object[] {s});
					return CreateInstance(classInfo, reader, out fullLoaded, accessType);
				}
			}
			else
			{
				//check for a Factory method
				var factory =
					classInfo.Mehtods
						.FirstOrDefault(s => s.Attributes.Any(f => f.Attribute is ObjectFactoryMethodAttribute));

				if (factory != null)
				{
					if (factory.MethodInfo.IsStatic)
					{
						var methodInfo = factory.MethodInfo as MethodInfo;
						if (methodInfo != null)
						{
							var returnType = methodInfo.ReturnParameter;

							if (returnType != null && returnType.ParameterType == classInfo.Type)
							{
								if (factory.Arguments.Count == 1 &&
								    factory.Arguments.First().Type == typeof(IDataRecord))
								{
									classInfo.FullFactory = true;
									classInfo.Factory = s => factory.Invoke(new object[] {reader});
									return CreateInstance(classInfo, reader, out fullLoaded, accessType);
								}
							}
						}
					}
				}
			}

			var emptyCtor = classInfo.Constructors.FirstOrDefault(f => !f.Arguments.Any());

			if (emptyCtor == null)
			{
				throw new NotSupportedException(
					"You have to define ether an ObjectFactoryMethod as static or constructor with and IDataReader or an constructor without any arguments");
			}

			classInfo.FullFactory = false;
			classInfo.Factory = s => emptyCtor.Invoke();
			return CreateInstance(classInfo, reader, out fullLoaded, accessType);
		}

		/// <summary>
		///     Loads all propertys from a DataReader into the given Object
		/// </summary>
		[Obsolete("This mehtod is replaced by several FASTER equal ones. " +
		          "It may be replaced, updated or deleted. But it will change that is for sure. " +
		          "legacy or Fallback support only")]
		public static object ReflectionPropertySet(
			DbConfig config,
			object instance,
			DbClassInfoCache info,
			IDataRecord reader,
			Dictionary<int, DbPropertyInfoCache> cache,
			DbAccessType? dbAccessType)
		{
			if (instance == null)
			{
				throw new ArgumentNullException(nameof(instance));
			}

			if (info == null)
			{
				throw new ArgumentNullException(nameof(info));
			}

			if (reader == null)
			{
				return instance;
			}

			//Left c# property name and right the object to read from the reader
			//var listofpropertys = new Dictionary<string, object>();

			var propertys = info.Propertys.ToArray();
			var instanceOfFallbackList = new Dictionary<string, object>();

			if (cache == null)
			{
				cache = new Dictionary<int, DbPropertyInfoCache>();
				for (var i = 0; i < reader.FieldCount; i++)
				{
					info.Propertys.TryGetValue(info.SchemaMappingDatabaseToLocal(reader.GetName(i)), out var val);
					cache.Add(i, val);
				}
			}

			for (var i = 0; i < reader.FieldCount; i++)
			{
				var property = cache[i];
				var value = reader.GetValue(i);

				if (property != null)
				{
					var attributes = property.Attributes;
					var valueConverterAttributeModel =
						attributes.FirstOrDefault(s => s.Attribute is ValueConverterAttribute);

					//Should the SQL value be converted
					if (valueConverterAttributeModel != null)
					{
						var converter = valueConverterAttributeModel.Attribute as ValueConverterAttribute;
						//Create the converter and then convert the value before everything else
						var valueConverter = converter.CreateConverter();
						value = valueConverter.Convert(value, property.PropertyInfo.PropertyType, converter.Parameter,
							CultureInfo.CurrentCulture);
					}

					var xmlAttributeModel =
						attributes.FirstOrDefault(s => s.Attribute is FromXmlAttribute);

					//should the Content be considerd as XML text?
					if (xmlAttributeModel != null)
					{
						//Get the XML text and check if its null or empty
						var xmlStream = value.ToString();
						if (string.IsNullOrEmpty(xmlStream))
						{
							continue;
						}

						//Check for List
						//if this is a list we are expecting other entrys inside
						if (property.CheckForListInterface())
						{
							//target Property is of type list
							//so expect a xml valid list Take the first element and expect the propertys inside this first element
							var record = XmlDataRecord.TryParse(xmlStream,
								property.PropertyInfo.PropertyType.GetGenericArguments().FirstOrDefault(), false,
								config);
							var xmlDataRecords = record.CreateListOfItems();

							var genericArguments =
								config.GetOrCreateClassInfoCache(
									property.PropertyInfo.PropertyType.GetGenericArguments().FirstOrDefault());
							var enumerableOfItems =
								xmlDataRecords.Select(
									s => genericArguments.SetPropertysViaReflection(s, dbAccessType, config)).ToList();
							object castedList;

							if (genericArguments.Type.IsClass &&
							    genericArguments.Type.GetInterface("INotifyPropertyChanged") != null)
							{
								var caster =
									typeof(DbCollection<>).MakeGenericType(genericArguments.Type)
										.GetConstructor(new[] {typeof(IEnumerable)});
								castedList = caster.Invoke(new object[] {enumerableOfItems});
							}
							else
							{
								var caster =
									typeof(NonObservableDbCollection<>).MakeGenericType(genericArguments.Type)
										.GetConstructor(new[] {typeof(IEnumerable)});
								castedList = caster.Invoke(new object[] {enumerableOfItems});
							}

							property.Setter.Invoke(instance, castedList);
						}
						else
						{
							var classInfo = config.GetOrCreateClassInfoCache(property
								.PropertyInfo
								.PropertyType);

							var xmlDataRecord = XmlDataRecord.TryParse(xmlStream, property.PropertyInfo.PropertyType,
								true, config);

							//the t
							var xmlSerilizedProperty = classInfo.SetPropertysViaReflection(xmlDataRecord, dbAccessType,
								config);
							property.Setter.Invoke(instance, xmlSerilizedProperty);
						}
					}
					else if (value is DBNull || value == null)
					{
						property.Setter.Invoke(instance, new object[] {null});
					}
					else
					{
						object changedType;
						if (value.GetType() != property.PropertyInfo.PropertyType)
						{
							changedType = DataConverterExtensions.ChangeType(value, property.PropertyInfo.PropertyType);
						}
						else
						{
							changedType = value;
						}

						property.Setter.Invoke(instance, changedType);
					}
				}
				//This variable is null if we tried to find a property with the LoadNotImplimentedDynamicAttribute but did not found it
				else if (instanceOfFallbackList != null)
				{
					//no property found Look for LoadNotImplimentedDynamicAttribute property to include it

					if (instanceOfFallbackList.Any())
					{
						instanceOfFallbackList.Add(reader.GetName(i), value);
					}
					else
					{
						var maybeFallbackProperty =
							propertys.FirstOrDefault(
								s => s.Value.Attributes.Any(e => e.Attribute is LoadNotImplimentedDynamicAttribute));
						if (maybeFallbackProperty.Value != null)
						{
							instanceOfFallbackList =
								(Dictionary<string, object>) maybeFallbackProperty.Value.Getter.Invoke(instance);
							if (instanceOfFallbackList == null)
							{
								instanceOfFallbackList = new Dictionary<string, object>();
								maybeFallbackProperty.Value.Setter.Invoke(instance, instanceOfFallbackList);
							}

							instanceOfFallbackList.Add(reader.GetName(i), value);
						}
						else
						{
							instanceOfFallbackList = null;
						}
					}
				}
			}

			//foreach (var item in listofpropertys)
			//{
			//	var property = propertys.FirstOrDefault(s => s.PropertyName == item.Key);

			//}
			return instance;
		}

		/// <summary>
		///     Should the ConfigureAwait on all tasks be set
		/// </summary>
		public static bool ConfigureAwait { get; set; }

		/// <summary>
		///     If set the System will never execute a Async method. Set this static flag to effect all NEW DbAccessLayer instances
		/// </summary>
		public static bool AsyncDefault { get; set; }

		/// <summary>
		///     If set the System will never execute a Async method
		/// </summary>
		public bool Async { get; set; }

		internal async Task<IEnumerable<object>> EnumerateDataRecordsAsync(IDbCommand query,
			bool egarLoading,
			DbClassInfoCache type,
			CommandBehavior executionHint = CommandBehavior.Default)
		{
			var resultList = new List<object>();

			if (!egarLoading)
			{
				await EnumerateAsync(query,
					record => { resultList.Add(SetPropertysViaReflection(type, record)); },
					executionHint);
			}
			else
			{
				var recordCache = new List<IDataRecord>();
				await EnumerateAsync(query,
					record => { recordCache.Add(RecordGenerator(record, Config)); }, executionHint);
				resultList.AddRange(recordCache
					.Select(f => SetPropertysViaReflection(type, f))
					.ToArray());
			}

			return resultList;
		}

		internal List<IDataRecord> EnumerateDataRecordsAsync(IDbCommand query)
		{
			return EnumerateMarsDataRecords(query).FirstOrDefault();
		}

		internal async Task EnumerateAsync(IDbCommand query, Action<IDataReader> onRecord,
			CommandBehavior executionHint = CommandBehavior.Default)
		{
			Database.PrepaireRemoteExecution(query);
			try
			{
				if (ThreadSave)
				{
					if (Async)
					{
						throw new InvalidOperationException(
							"You cannot run a command async AND thread save. thats just not possible sory.");
					}

					Monitor.Enter(_lockRoot);
				}

				await Database.RunAsync(
					async s =>
					{
						using (query)
						{
							query.Connection = query.Connection ?? s.GetConnection();
							query.Transaction = query.Transaction ?? s.ConnectionController.Transaction;

							IDataReader dr = null;
							try
							{
								var command = query as DbCommand;
								if (command != null && Async)
								{
									dr = await command.ExecuteReaderAsync(executionHint).ConfigureAwait(ConfigureAwait);
								}
								else
								{
									dr = query.ExecuteReader(executionHint);
								}

								do
								{
									var reader = dr as DbDataReader;
									while (reader != null && Async
										? await reader.ReadAsync().ConfigureAwait(ConfigureAwait)
										: dr.Read())
									{
										onRecord(dr);
									}
								} while (dr.NextResult());
							}
							catch (Exception ex)
							{
								RaiseFailedQuery(this, query, ex);
								throw;
							}
							finally
							{
								dr?.Dispose();
							}
						}

						return new object();
					});
			}
			finally
			{
				if (ThreadSave)
				{
					Monitor.Exit(_lockRoot);
				}
			}
		}

		/// <summary>
		///     Produces an IDataRecord for the given Reader
		/// </summary>
		/// <returns></returns>
		protected virtual IDataRecord RecordGenerator(IDataReader reader, DbConfig config)
		{
			return new EgarDataRecord(reader, Config);
		}

		internal List<List<IDataRecord>> EnumerateMarsDataRecords(
			IDbCommand query)
		{
			Database.PrepaireRemoteExecution(query);
			try
			{
				if (ThreadSave)
				{
					Monitor.Enter(_lockRoot);
				}

				return Database.Run(
					s =>
					{
						var records = new List<List<IDataRecord>>();
						using (query)
						{
							query.Connection = query.Connection ?? s.GetConnection();
							query.Transaction = query.Transaction ?? s.ConnectionController.Transaction;
							using (var dr = query.ExecuteReader())
							{
								try
								{
									do
									{
										var resultSet = new List<IDataRecord>();
										while (dr.Read())
										{
											resultSet.Add(RecordGenerator(dr, Config));
										}

										records.Add(resultSet);
									} while (dr.NextResult());
								}
								catch (Exception ex)
								{
									RaiseFailedQuery(this, query, ex);
									throw;
								}
							}
						}

						return records;
					});
			}
			finally
			{
				if (ThreadSave)
				{
					Monitor.Exit(_lockRoot);
				}
			}
		}

		/// <summary>
		///     Creates a new Instance based on possible Ctor's and the given
		///     <paramref name="reader" />
		/// </summary>
		/// <returns></returns>
		public object SetPropertysViaReflection(DbClassInfoCache type, IDataRecord reader, DbAccessType? accessType)
		{
			return type.SetPropertysViaReflection(reader, DbAccessType, Config);
		}
	}
}