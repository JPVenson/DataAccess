/*
This work is licensed under the Creative Commons Attribution-ShareAlike 4.0 International License. 
To view a copy of this license, visit http://creativecommons.org/licenses/by-sa/4.0/.
Please consider to give some Feedback on CodeProject

http://www.codeproject.com/Articles/818690/Yet-Another-ORM-ADO-NET-Wrapper

*/
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using JPB.DataAccess.AdoWrapper;
using JPB.DataAccess.Contacts;
using JPB.DataAccess.DbCollection;
using JPB.DataAccess.DbInfoConfig;
using JPB.DataAccess.DbInfoConfig.DbInfo;
using JPB.DataAccess.Helper;
using JPB.DataAccess.ModelsAnotations;
using JPB.DataAccess.QueryBuilder;

namespace JPB.DataAccess.Manager
{
	/// <summary>
	///     Contanins some Helper mehtods for CRUD operation
	/// </summary>
	[DebuggerDisplay("DB={dbAccessType}, QueryDebug={Database.LastExecutedQuery.DebuggerQuery}")]
#if !DEBUG
		[DebuggerStepThrough]
#endif
	public partial class DbAccessLayer
	{
		static DbAccessLayer()
		{
			Debugger = false;
			SProcedureDbAccessLayer();
			ProviderCollection = new PreDefinedProviderCollection();
		}

		private IDatabase _database;

		/// <summary>
		/// Object that is used globaly for each Equallity Comparsion if no other is specifyed ether for the type or the instance. This field overrides 
		/// </summary>
		public static object DefaultAssertionObject;

		/// <summary>
		/// When specifying an Long as DefaultAssertionObject the PocoPkComparer will use instedt the value casted as int when the property is int instedt of Long and vice versa (more Rewrite operations may follow)
		/// </summary>
		public static bool DefaultAssertionObjectRewrite { get; set; }

		/// <summary>
		///     Enables the automatic creation of QueryDebugger objects on each created IDbCommand
		/// </summary>
		public static bool Debugger { get; set; }

		/// <summary>
		///		If set to True a strict check for the Targetdatabase Property on each Factory or provider specific method is done otherwise this Check is skiped 
		/// </summary>
		public bool IsMultiProviderEnvironment { get; set; }

		/// <summary>
		///     Defines a set of Providers that are inclueded in this DLL or are weak refernced.
		/// </summary>
		public static PreDefinedProviderCollection ProviderCollection { get; private set; }

		internal DbAccessLayer()
		{
			LoadCompleteResultBeforeMapping = true;

			SelectDbAccessLayer();
			UpdateDbAccessLayer();
		}

		/// <summary>
		///     Create a DbAccessLayer that uses a Predefined type and Connection string
		/// </summary>
		public DbAccessLayer(DbAccessType dbAccessType, string connection)
			: this()
		{
			if (dbAccessType == DbAccessType.Unknown)
			{
				throw new InvalidEnumArgumentException("dbAccessType", (int) DbAccessType.Unknown, typeof (DbAccessType));
			}

			DbAccessType = dbAccessType;
			Database = new DefaultDatabaseAccess();
			var database =
				ProviderCollection.FirstOrDefault(s => s.Key == dbAccessType).Value.GenerateStrategy(connection);
			Database.Attach(database);
			DatabaseStrategy = database;
		}

		/// <summary>
		///     Create a DbAccessLAyer with exernal Strategy
		/// </summary>
		/// <exception cref="ArgumentNullException"></exception>
		public DbAccessLayer(string fullTypeNameToIDatabaseStrategy, string connection)
			: this()
		{
			if (string.IsNullOrEmpty(fullTypeNameToIDatabaseStrategy))
				throw new ArgumentNullException("fullTypeNameToIDatabaseStrategy");

			ResolveDbType(fullTypeNameToIDatabaseStrategy);

			var database = fullTypeNameToIDatabaseStrategy.GenerateStrategy(connection);

			Database = new DefaultDatabaseAccess();
			Database.Attach(database);
			DatabaseStrategy = database;
		}

		/// <summary>
		///     Create a DbAccessLayer with a new Database
		/// </summary>
		/// <exception cref="ArgumentNullException"></exception>
		public DbAccessLayer(IDatabaseStrategy database)
			: this()
		{
			if (database == null)
				throw new ArgumentNullException("database");

			ResolveDbType(database.GetType().FullName);

			Database = new DefaultDatabaseAccess();
			Database.Attach(database);
			DatabaseStrategy = database;
		}

		/// <summary>
		///     Creates a DbAccessLayer with a new Database
		///     dbAccessType will be Guessed
		/// </summary>
		public DbAccessLayer(IDatabase database)
			: this()
		{
			if (database == null)
				throw new ArgumentNullException("database");

			DbAccessType = DbAccessType.Unknown;
			Database = database;
		}

		private void ResolveDbType(string fullTypeNameToIDatabaseStrategy)
		{
			// ReSharper disable once PossibleInvalidOperationException
			var firstOrDefault =
				ProviderCollection.Select(s => (KeyValuePair<DbAccessType, string>?) s)
					.FirstOrDefault(s => s.Value.Value == fullTypeNameToIDatabaseStrategy);
			if (firstOrDefault == null)
			{
				DbAccessType = DbAccessType.Unknown;
			}
			else
			{
				DbAccessType = firstOrDefault.Value.Key;
			}
		}

		/// <summary>
		///     For Internal Use only
		/// </summary>
		public IDatabaseStrategy DatabaseStrategy { get; private set; }

		/// <summary>
		///     Selected dbAccessType
		/// </summary>
		public DbAccessType DbAccessType { get; private set; }

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
		///     Check for Availability
		/// </summary>
		/// <returns></returns>
		public bool CheckDatabase()
		{
			if (Database == null)
				return false;
			Database.Connect();
			Database.CloseConnection();
			return true;
		}

		/// <summary>
		///     Wraps a Query and its Paramters and then executes it
		/// </summary>
		/// <returns></returns>
		public int ExecuteGenericCommand(string query, IEnumerable<IQueryParameter> values)
		{
			var command = DbAccessLayerHelper.CreateCommand(Database, query);

			if (values != null)
				foreach (IQueryParameter item in values)
					command.Parameters.AddWithValue(item.Name, item.Value, Database);

			return Database.Run(s => s.ExecuteNonQuery(command));
		}

		/// <summary>
		///     Wraps a Query and its Paramters from Dynamic and then executes it
		/// </summary>
		/// <returns></returns>
		public int ExecuteGenericCommand(string query, dynamic paramenter)
		{
			return ExecuteGenericCommand(query,
				(IEnumerable<IQueryParameter>) DbAccessLayerHelper.EnumarateFromDynamics(paramenter));
		}

		/// <summary>
		///     Execute a Query and without Paramters
		/// </summary>
		/// <returns></returns>
		public int ExecuteGenericCommand(IDbCommand command)
		{
			return Database.Run(s => s.ExecuteNonQuery(command));
		}


		/// <summary>
		///     if set the created reader of an read operation will be completely stored then the open connection will be closed
		///     Default is true
		/// </summary>
		public bool LoadCompleteResultBeforeMapping { get; set; }

		/// <summary>
		///     Creates a Strong typed query that awaits a Result
		/// </summary>
		/// <returns></returns>
		public QueryBuilder.QueryBuilder Query()
		{
			return new QueryBuilder.QueryBuilder(this);
		}

		/// <summary>
		///     Creates a Strong typed query that awaits a Result
		/// </summary>
		/// <returns></returns>
		public QueryBuilder.QueryBuilder Query(Type targetType)
		{
			return new QueryBuilder.QueryBuilder(this, targetType);
		}

		/// <summary>
		///     Creates a Strong typed query that awaits a Result
		/// </summary>
		/// <returns></returns>
		public QueryBuilder.QueryBuilder Query<T>()
		{
			return new QueryBuilder<T>(this);
		}

		/// <summary>
		///     Creates a new Instance based on possible Ctor's and the given
		///     <paramref name="reader" />
		/// </summary>
		/// <returns></returns>
		public object SetPropertysViaReflection(DbClassInfoCache type, IDataRecord reader)
		{
			bool created;
			object source = CreateInstance(type, reader, out created);
			if (created)
				return source;

			return SetPropertysViaReflection(source, type, reader, null);
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
			object source = CreateInstance(type, reader, out created);
			if (created)
				return source;

			return SetPropertysViaReflection(source, type, reader, mapping);
		}

		/// <summary>
		///     Creates an instance based on a Ctor injection or Reflection loading
		/// </summary>
		/// <returns></returns>
		public object CreateInstance(DbClassInfoCache classInfo, IDataRecord reader, out bool fullLoaded)
		{
			if (classInfo.Factory != null)
			{
				fullLoaded = classInfo.FullFactory;
				return classInfo.Factory(reader);
			}

			var objectFactorys = classInfo.Constructors.Where(s => s.Arguments.Count == 1 
			&& s.Arguments.First().Type == typeof(IDataRecord));

			var constructor = objectFactorys.FirstOrDefault(s =>
			s.Attributes.Any(f =>
				f.Attribute is ObjectFactoryMethodAttribute
				&& (!IsMultiProviderEnvironment || (f.Attribute as ObjectFactoryMethodAttribute).TargetDatabase == Database.TargetDatabase)));

			if (constructor == null)
				constructor = objectFactorys.FirstOrDefault();

			//maybe single ctor with param

			if (constructor != null)
			{
				if (constructor.Arguments.Count == 1 && constructor.Arguments.First().Type == typeof(IDataRecord))
				{
					classInfo.FullFactory = true;
					classInfo.Factory = s => constructor.Invoke(new object[] { s });
					return CreateInstance(classInfo, reader, out fullLoaded);
				}
			}
			else
			{
				//check for a Factory mehtod
				var factory =
					classInfo.Mehtods
						.FirstOrDefault(s => s.Attributes.Any(f => f.Attribute is ObjectFactoryMethodAttribute));

				if (factory != null)
				{
					if (factory.MethodInfo.IsStatic)
					{
						var returnType = (factory.MethodInfo as MethodInfo).ReturnParameter;

						if (returnType != null && returnType.ParameterType == classInfo.Type)
						{
							if (factory.Arguments.Count == 1 &&
								factory.Arguments.First().Type == typeof(IDataRecord))
							{
								classInfo.FullFactory = true;
								classInfo.Factory = s => factory.Invoke(new object[] { reader });
								return CreateInstance(classInfo, reader, out fullLoaded);
							}
						}
					}
				}
			}

			var emptyCtor = classInfo.Constructors.FirstOrDefault(f => !f.Arguments.Any());

			if (emptyCtor == null)
			{
				throw new NotSupportedException("You have to define ether an ObjectFactoryMethod as static or constructor or any constructor without any arguments");
			}

			classInfo.FullFactory = false;
			classInfo.Factory = s => emptyCtor.Invoke(new object[0]);
			return CreateInstance(classInfo, reader, out fullLoaded);
		}
		/// <summary>
		///     Loads all propertys from a DataReader into the given Object
		/// </summary>
		[Obsolete("This mehtod is replaced by several FASTER equal ones. It may be replaced, updated or delted. But it will change that is for sure. legacy support only")]
		public object SetPropertysViaReflection(
			object instance,
			DbClassInfoCache info,
			IDataRecord reader,
			Dictionary<int, DbPropertyInfoCache> cache)
		{
			if (reader == null)
				return instance;

			//Left c# property name and right the object to read from the reader
			//var listofpropertys = new Dictionary<string, object>();

			var propertys = info.Propertys.ToArray();
			var instanceOfFallbackList = new Dictionary<string, object>();

			if (cache == null)
			{
				cache = new Dictionary<int, DbPropertyInfoCache>();
				for (var i = 0; i < reader.FieldCount; i++)
				{
					DbPropertyInfoCache val = null;
					info.Propertys.TryGetValue(info.SchemaMappingDatabaseToLocal(reader.GetName(i)), out val);
					cache.Add(i, val);
				}
			}

			//for (int i = 0; i < reader.FieldCount; i++)
			//{
			//	var dbName = info.SchemaMappingValues.ElementAt(i).Key;
			//	listofpropertys.Append(info.SchemaMappingDatabaseToLocal(dbName), reader.GetValue(i));
			//}

			//foreach (var schemaMappingValue in info.SchemaMappingValues)
			//{
			//}

			//for (int i = 0; i < reader.FieldCount; i++)
			//	listofpropertys.Append(type.GetDbToLocalSchemaMapping(reader.GetName(i)), reader.GetValue(i));


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
						if (DataConverterExtensions.CheckForListInterface(property))
						{
							//target Property is of type list
							//so expect a xml valid list Take the first element and expect the propertys inside this first element
							var record = XmlDataRecord.TryParse(xmlStream,
								property.PropertyInfo.PropertyType.GetGenericArguments().FirstOrDefault());
							var xmlDataRecords = record.CreateListOfItems();

							var genericArguments =
								property.PropertyInfo.PropertyType.GetGenericArguments().FirstOrDefault().GetClassInfo();
							List<object> enumerableOfItems = xmlDataRecords.Select(s => SetPropertysViaReflection(genericArguments, s)).ToList();
							object castedList;

							if (genericArguments.Type.IsClass && genericArguments.Type.GetInterface("INotifyPropertyChanged") != null)
							{
								var caster =
									typeof(DbCollection<>).MakeGenericType(genericArguments.Type).GetConstructor(new[] { typeof(IEnumerable) });

								Debug.Assert(caster != null, "caster != null");

								castedList = caster.Invoke(new object[] { enumerableOfItems });
							}
							else
							{
								var caster =
									typeof(NonObservableDbCollection<>).MakeGenericType(genericArguments.Type)
										.GetConstructor(new[] { typeof(IEnumerable) });

								Debug.Assert(caster != null, "caster != null");

								castedList = caster.Invoke(new object[] { enumerableOfItems });
							}

							property.Setter.Invoke(instance, castedList);
						}
						else
						{
							//the t
							object xmlSerilizedProperty = 
								SetPropertysViaReflection(property
								.PropertyInfo
								.PropertyType
								.GetClassInfo(), XmlDataRecord.TryParse(xmlStream, property.PropertyInfo.PropertyType));

							property.Setter.Invoke(instance, xmlSerilizedProperty);
						}
					}
					else if (value is DBNull || value == null)
					{
						property.Setter.Invoke(instance, new object[] { null });
					}
					else
					{
						object changedType;
						if (value.GetType() != property.PropertyInfo.PropertyType)
						{
							changedType = ChangeType(value, property.PropertyInfo.PropertyType);
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
							instanceOfFallbackList = (Dictionary<string, object>)maybeFallbackProperty.Value.Getter.Invoke(instance);
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

			if (reader is EgarDataRecord)
			{
				(reader as EgarDataRecord).Dispose();
			}

			return instance;
		}

		internal static object ChangeType(object value, Type conversion)
		{
			var t = conversion;

			if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>))
			{
				if (value == null)
				{
					return null;
				}

				t = Nullable.GetUnderlyingType(t);
			}

			if (typeof(Enum).IsAssignableFrom(t))
			{
				// ReSharper disable once UseIsOperator.1
				// ReSharper disable once UseMethodIsInstanceOfType
				if (typeof(long).IsAssignableFrom(value.GetType()))
				{
					value = Enum.ToObject(t, value);
				}
				else if (value is string)
				{
					value = Enum.Parse(t, value as string, true);
				}
			}
			else if (typeof(bool).IsAssignableFrom(t))
			{
				if (value is int)
				{
					value = value.Equals(1);
				}
				else if (value is string)
				{
					value = value.Equals("1");
				}
				else if (value is bool)
				{
					value = (bool)value;
				}
			}
			else if (typeof(byte[]).IsAssignableFrom(t))
			{
				if (value is string)
				{
					value = Encoding.Default.GetBytes(value as string);
				}
			}

			return Convert.ChangeType(value, t);
		}

		internal List<IDataRecord> EnumerateDataRecords(IDbCommand query, bool egarLoading)
		{
			return EnumerateMarsDataRecords(query, egarLoading).FirstOrDefault();
		}

		internal List<List<IDataRecord>> EnumerateMarsDataRecords(
			IDbCommand query,
			bool egarLoading = true)
		{
			return Database.Run(
				s =>
				{
					//Skip enumeration and parsing and make a Direct loading
					//This increeses Performance

					var records = new List<List<IDataRecord>>();

					using (var dr = query.ExecuteReader())
					{
						try
						{
							do
							{
								var resultSet = new List<IDataRecord>();
								while (dr.Read())
								{
									resultSet.Add(dr.CreateEgarRecord());
								}
								records.Add(resultSet);
							} while (dr.NextResult());
						}
						finally
						{
							dr.Close();
						}
					}
					return records;
				});
		}

		internal IEnumerable EnumerateDirectDataRecords(IDbCommand query,
			DbClassInfoCache info)
		{
			return Database.Run(
				s =>
				{
					//Skip enumeration and parsing and make a Direct loading
					//This increeses Performance

					var records = new ArrayList();

					using (var dr = query.ExecuteReader())
					{
						try
						{
							do
							{
								while (dr.Read())
								{
									records.Add(SetPropertysViaReflection(info, dr));
								}
							} while (dr.NextResult());
						}
						finally
						{
							dr.Close();
						}
					}
					return records;
				});
		}
	}
}