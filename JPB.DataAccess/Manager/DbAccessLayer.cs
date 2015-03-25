using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using JPB.DataAccess.AdoWrapper;
using JPB.DataAccess.DbEventArgs;
using JPB.DataAccess.DebuggerHelper;
using JPB.DataAccess.Helper;
using JPB.DataAccess.ModelsAnotations;
using JPB.DataAccess.QueryFactory;

namespace JPB.DataAccess.Manager
{
    /// <summary>
    /// Contanins some Helper mehtods for CRUD operation
    /// </summary>
    [DebuggerDisplay("DB={dbAccessType}, QueryDebug={Database.LastExecutedQuery.DebuggerQuery}")]
#if !DEBUG
        [DebuggerStepThrough]
#endif

    public partial class DbAccessLayer
    {
        static DbAccessLayer()
        {
            SProcedureDbAccessLayer();
        }

        private IDatabase _database;

        /// <summary>
        /// Defines a set of Providers that are inclueded in this DLL or are weak refernced.
        /// </summary>
        public PreDefinedProviderCollection ProviderCollection { get; private set; }

        internal DbAccessLayer()
        {
            ProviderCollection = new PreDefinedProviderCollection();
            LoadCompleteResultBeforeMapping = true;

            SelectDbAccessLayer();
            UpdateDbAccessLayer();
        }

        /// <summary>
        ///     Create a DbAccessLayer that uses a Predefined type and Connection string
        /// </summary>
        /// <param name="dbAccessType">Can be anything execpt for <code>DbAccessType.Unknown</code></param>
        /// <param name="connection"></param>
        public DbAccessLayer(DbAccessType dbAccessType, string connection)
            : this()
        {
            if (dbAccessType == DbAccessType.Unknown)
            {
                throw new InvalidEnumArgumentException("dbAccessType", (int)DbAccessType.Unknown, typeof(DbAccessType));
            }

            DbAccessType = dbAccessType;
            Database = new DefaultDatabaseAccess();
            Database.Attach(ProviderCollection.FirstOrDefault(s => s.Key == dbAccessType).Value.GenerateStrategy(connection));
        }

        /// <summary>
        /// Create a DbAccessLAyer with exernal Strategy
        /// </summary>
        /// <param name="fullTypeNameToIDatabaseStrategy">Full type name of Strategy class</param>
        /// <param name="connection"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public DbAccessLayer(string fullTypeNameToIDatabaseStrategy, string connection)
            : this()
        {
            if (string.IsNullOrEmpty(fullTypeNameToIDatabaseStrategy))
                throw new ArgumentNullException("fullTypeNameToIDatabaseStrategy");

            ResolveDbType(fullTypeNameToIDatabaseStrategy);

            var type = fullTypeNameToIDatabaseStrategy.GenerateStrategy(connection);

            Database = new DefaultDatabaseAccess();
            Database.Attach(type);
        }

        /// <summary>
        /// Create a DbAccessLayer with a new Database
        /// </summary>
        /// <param name="database"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public DbAccessLayer(IDatabaseStrategy database)
            : this()
        {
            if (database == null)
                throw new ArgumentNullException("database");

            ResolveDbType(database.GetType().FullName);

            Database = new DefaultDatabaseAccess();
            Database.Attach(database);
        }

        /// <summary>
        /// Creates a DbAccessLayer with a new Database
        /// dbAccessType will be Guessed
        /// </summary>
        /// <param name="database"></param>
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
            KeyValuePair<DbAccessType, string>? firstOrDefault =
                ProviderCollection.Select(s => (KeyValuePair<DbAccessType, string>?)s)
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
        /// Selected dbAccessType
        /// </summary>
        public DbAccessType DbAccessType { get; private set; }

        /// <summary>
        /// Current Database
        /// Can be used to write multi statements
        /// Is used for ALL NonStatic statments creators
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
        /// Check for Availability 
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
        /// Check for Availability 
        /// </summary>
        /// <returns></returns>
        public async Task<bool> CheckDatabaseAsync()
        {
            if (Database == null)
                return false;
            Database.Connect();
            Database.CloseConnection();
            return true;
        }

        /// <summary>
        /// Wraps a Query and its Paramters and then executes it
        /// </summary>
        /// <param name="query"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public int ExecuteGenericCommand(string query, IEnumerable<IQueryParameter> values)
        {
            IDbCommand command = DbAccessLayerHelper.CreateCommand(Database, query);

            if (values != null)
                foreach (IQueryParameter item in values)
                    command.Parameters.AddWithValue(item.Name, item.Value, Database);

            return Database.Run(s => s.ExecuteNonQuery(command));
        }

        /// <summary>
        /// Wraps a Query and its Paramters from Dynamic and then executes it
        /// </summary>
        /// <param name="query"></param>
        /// <param name="paramenter"></param>
        /// <returns></returns>
        public int ExecuteGenericCommand(string query, dynamic paramenter)
        {
            return ExecuteGenericCommand(query, (IEnumerable<IQueryParameter>)DbAccessLayerHelper.EnumarateFromDynamics(paramenter));
        }

        /// <summary>
        /// Execute a Query and without Paramters
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public int ExecuteGenericCommand(IDbCommand command)
        {
            return Database.Run(s => s.ExecuteNonQuery(command));
        }


        /// <summary>
        /// if set the created reader of an read operation will be completely stored then the open connection will be closed
        /// Default is true
        /// </summary>
        public bool LoadCompleteResultBeforeMapping { get; set; }

        /// <summary>
        /// Creates a Strong typed query that awaits a Result
        /// </summary>
        /// <param name="targetType"></param>
        /// <returns></returns>
        public QueryBuilder.QueryBuilder Query()
        {
            return new QueryBuilder.QueryBuilder(this.Database);
        }

        /// <summary>
        /// Creates a Strong typed query that awaits a Result
        /// </summary>
        /// <param name="targetType"></param>
        /// <returns></returns>
        public QueryBuilder.QueryBuilder Query(Type targetType)
        {
            return new QueryBuilder.QueryBuilder(this.Database, targetType);
        }

        /// <summary>
        /// Creates a Strong typed query that awaits a Result
        /// </summary>
        /// <returns></returns>
        public QueryBuilder.QueryBuilder Query<T>()
        {
            return new QueryBuilder.QueryBuilder<T>(this.Database);
        }
    }
}