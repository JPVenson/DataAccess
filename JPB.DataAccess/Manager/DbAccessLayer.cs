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
    [DebuggerDisplay("DB={DbType}, QueryDebug={Database.LastExecutedQuery.DebuggerQuery}")]
#if !DEBUG
        [DebuggerStepThrough]
#endif

    public partial class DbAccessLayer
    {
        private IDatabase _database;

        /// <summary>
        /// Defines a set of Providers that are inclueded in this DLL or are weak refernced.
        /// </summary>
        public PreDefinedProviderCollection ProviderCollection { get; private set; }
        
        internal DbAccessLayer()
        {
            ProviderCollection = new PreDefinedProviderCollection();
            LoadCompleteResultBeforeMapping = true;
        }

        /// <summary>
        ///     Create a DbAccessLayer that uses a Predefined type and Connection string
        /// </summary>
        /// <param name="dbType">Can be anything execpt for <code>DbTypes.Unknown</code></param>
        /// <param name="connection"></param>
        public DbAccessLayer(DbTypes dbType, string connection)
            : this()
        {
            DbType = dbType;
            Database = new Database();

            if (dbType == DbTypes.Unknown)
            {
                throw new InvalidEnumArgumentException("dbType", (int)DbTypes.Unknown, typeof(DbTypes));
            }

            Database.Attach(ProviderCollection.FirstOrDefault(s => s.Key == dbType).Value.GenerateStrategy(connection));
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

            IDatabaseStrategy type = fullTypeNameToIDatabaseStrategy.GenerateStrategy(connection);

            SelectDbAccessLayer();
            UpdateDbAccessLayer();

            Database = new Database();
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
            SelectDbAccessLayer();
            UpdateDbAccessLayer();

            ResolveDbType(database.GetType().FullName);

            Database = new Database();
            Database.Attach(database);
        }

        /// <summary>
        /// Creates a DbAccessLayer with a new Database
        /// DbType will be Guessed
        /// </summary>
        /// <param name="database"></param>
        public DbAccessLayer(IDatabase database)
            : this()
        {
            if (database == null)
                throw new ArgumentNullException("database");
            SelectDbAccessLayer();
            UpdateDbAccessLayer();

            DbType = DbTypes.Unknown;
            Database = database;
        }

        private void ResolveDbType(string fullTypeNameToIDatabaseStrategy)
        {
            // ReSharper disable once PossibleInvalidOperationException
            KeyValuePair<DbTypes, string>? firstOrDefault =
                ProviderCollection.Select(s => (KeyValuePair<DbTypes, string>?)s)
                    .FirstOrDefault(s => s.Value.Value == fullTypeNameToIDatabaseStrategy);
            if (firstOrDefault == null)
            {
                DbType = DbTypes.Unknown;
            }
            else
            {
                DbType = firstOrDefault.Value.Key;
            }
        }

       

        /// <summary>
        /// Selected DbType
        /// </summary>
        public DbTypes DbType { get; private set; }

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
                    _database = value;
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
            Database.Connect(false);
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
    }
}