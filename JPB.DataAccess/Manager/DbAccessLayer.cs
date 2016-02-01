using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using JPB.DataAccess.AdoWrapper;
using JPB.DataAccess.Contacts;
using JPB.DataAccess.Helper;
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
			return new QueryBuilder.QueryBuilder(Database);
		}

		/// <summary>
		///     Creates a Strong typed query that awaits a Result
		/// </summary>
		/// <returns></returns>
		public QueryBuilder.QueryBuilder Query(Type targetType)
		{
			return new QueryBuilder.QueryBuilder(Database, targetType);
		}

		/// <summary>
		///     Creates a Strong typed query that awaits a Result
		/// </summary>
		/// <returns></returns>
		public QueryBuilder.QueryBuilder Query<T>()
		{
			return new QueryBuilder<T>(Database);
		}
	}
}