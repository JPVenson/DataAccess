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

        public PreDefinedProviderCollection ProviderCollection { get; set; }
        
        /// <summary>
        ///     Must set the Database Property
        ///     Just for Intigration
        /// </summary>
        [Obsolete("Will maybe removed in future", false)]
        public DbAccessLayer()
        {
            ProviderCollection = new PreDefinedProviderCollection();
        }

        /// <summary>
        ///     Create a DbAccessLayer that uses a Predefined type and Connection string
        /// </summary>
        /// <param name="dbType"></param>
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

            Database.Attach(GenerateStrategy(ProviderCollection.FirstOrDefault(s => s.Key == dbType).Value, connection));
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

            IDatabaseStrategy type = GenerateStrategy(fullTypeNameToIDatabaseStrategy, connection);

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
            DbType = DbTypes.Unknown;

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

        private static IDatabaseStrategy GenerateStrategy(string fullValidIdentifyer, string connection)
        {
            if (string.IsNullOrEmpty(fullValidIdentifyer))
                throw new ArgumentException("Type was not found");

            Type type = Type.GetType(fullValidIdentifyer);
            if (type == null)
            {
                IEnumerable<string> parallelQuery = Directory.EnumerateFiles(Directory.GetCurrentDirectory(), "*.dll",
                    SearchOption.TopDirectoryOnly);

                Assembly assam = null;

                Parallel.ForEach(parallelQuery, (s, e) =>
                {
                    Assembly loadFile = Assembly.LoadFile(s);
                    Type resolve = loadFile.GetType(fullValidIdentifyer);
                    if (resolve != null)
                    {
                        type = resolve;
                        assam = loadFile;
                        e.Break();
                    }
                });

                if (type == null)
                    throw new ArgumentException("Type was not found");
            }

            //check the type to be a Strategy

            if (!type.GetInterfaces().Contains(typeof(IDatabaseStrategy)))
            {
                throw new ArgumentException("Type was found but does not inhert from IDatabaseStrategy");
            }

            //try constructor injection
            ConstructorInfo ctOfType =
                type.GetConstructors()
                    .FirstOrDefault(
                        s => s.GetParameters().Length == 1 && s.GetParameters().First().ParameterType == typeof(string));
            if (ctOfType != null)
            {
                return ctOfType.Invoke(new object[] { connection }) as IDatabaseStrategy;
            }
            var instanceOfType = Activator.CreateInstance(type) as IDatabaseStrategy;
            if (instanceOfType == null)
                throw new ArgumentException("Type was found but does not inhert from IDatabaseStrategy");

            instanceOfType.ConnectionString = connection;

            return instanceOfType;
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

        private static IDbCommand CheckInstanceForAttriute<TE>(Type type,object entry, IDatabase db,
    Func<object, IDatabase, IDbCommand> fallback, params object[] param)
    where TE : DataAccessAttribute
        {
            //try to get a Factory mehtod
            //var methods =
            //    type.GetMethods()
            //        .FirstOrDefault(s => s.GetCustomAttributes(false).Any(e => e is TE /*&& (e as TE).DbQuery.HasFlag(DbType)*/));
            
            MethodInfo[] methods =
                type.GetMethods().Where(s => s.GetCustomAttributes(false).Any(e => e is TE)).ToArray();

            if (methods.Any())
            {
                MethodInfo[] searchMethodWithFittingParams = methods.Where(s =>
                {
                    ParameterInfo[] parameterInfos = s.GetParameters();

                    if (parameterInfos.Length != param.Length)
                    {
                        return false;
                    }

                    for (int i = 0; i < parameterInfos.Length; i++)
                    {
                        ParameterInfo para = parameterInfos[i];
                        object tryParam = param[i];
                        if (tryParam == null)
                            return false;
                        if (!(para.ParameterType == para.GetType()))
                        {
                            return false;
                        }
                    }
                    return true;
                }).ToArray();

                if (searchMethodWithFittingParams.Length != 1)
                {
                    return fallback(entry, db);
                }

                MethodInfo method = searchMethodWithFittingParams.First();

                //must be public static
                if (!method.IsStatic)
                {
                    object[] cleanParams = param != null && param.Any() ? param : null;
                    object invoke = method.Invoke(entry, cleanParams);
                    if (invoke != null)
                    {
                        if (invoke is string && !string.IsNullOrEmpty(invoke as string))
                        {
                            return CreateCommand(db, invoke as string);
                        }
                        if (invoke is IQueryFactoryResult)
                        {
                            var result = invoke as IQueryFactoryResult;
                            return CreateCommandWithParameterValues(result.Query, db, result.Parameters);
                        }
                    }
                }
            }
            return fallback(entry, db);
        }

        private static IDbCommand CheckInstanceForAttriute<T, TE>(Type type, T entry, IDatabase db,
            Func<T, IDatabase, IDbCommand> fallback, params object[] param)
            where TE : DataAccessAttribute
        {
            return CheckInstanceForAttriute<TE>(type, entry, db, (o, database) => fallback((T)o, database), param);
        }

        /// <summary>
        /// Wraps a Query and its Paramters and then executes it
        /// </summary>
        /// <param name="query"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public int ExecuteGenericCommand(string query, IEnumerable<IQueryParameter> values)
        {
            IDbCommand command = CreateCommand(Database, query);

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
            return ExecuteGenericCommand(query, (IEnumerable<IQueryParameter>)EnumarateFromDynamics(paramenter));
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
        /// Execute a Query on a given Database
        /// </summary>
        /// <param name="command"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        public static int ExecuteGenericCommand(IDbCommand command, IDatabase db)
        {
            return db.Run(s => s.ExecuteNonQuery(command));
        }

        /// <summary>
        /// Runs a Command on a given Database and Converts the Output into <typeparam name="T"></typeparam>
        /// </summary>
        /// <param name="command"></param>
        /// <param name="db"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static List<T> ExecuteGenericCreateModelsCommand<T>(IDbCommand command, IDatabase db)
            where T : class, new()
        {
            return db.Run(
                s =>
                    s.GetEntitiesList(command, DataConverterExtensions.SetPropertysViaRefection<T>)
                        .ToList());
        }

        /// <summary>
        /// Gets the Value or DB null
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static object GetDataValue(object value)
        {
            return value ?? DBNull.Value;
        }

        /// <summary>
        /// Wraps a Parameterless string into a Command for the given DB
        /// </summary>
        /// <param name="db"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public static IDbCommand CreateCommand(IDatabase db, string query)
        {
            return db.CreateCommand(query);
        }

        /// <summary>
        /// Wraps a <param name="query"></param> on a given <param name="type"></param> by including <param name="entry"></param>'s 
        /// propertys that are defined in <param name="propertyInfos"></param>
        /// </summary>
        /// <param name="type"></param>
        /// <param name="query"></param>
        /// <param name="propertyInfos"></param>
        /// <param name="entry"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        public static IDbCommand CreateCommandWithParameterValues(Type type, string query, string[] propertyInfos, object entry, IDatabase db)
        {
            object[] propertyvalues =
                propertyInfos.Select(
                    propertyInfo =>
                    {
                        PropertyInfo property =
                            type.GetProperty(DataConverterExtensions.ReMapSchemaToEntiysProp(type, propertyInfo));
                        object dataValue = GetDataValue(property.GetConvertedValue(entry));
                        return dataValue;
                    }).ToArray();
            return CreateCommandWithParameterValues(query, db, propertyvalues);
        }

        /// <summary>
        /// Wraps a <param name="query"></param> on a given typeof(T) by including <param name="entry"></param>'s 
        /// propertys that are defined in <param name="propertyInfos"></param>
        /// </summary>
        /// <param name="query"></param>
        /// <param name="propertyInfos"></param>
        /// <param name="entry"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        public static IDbCommand CreateCommandWithParameterValues<T>(string query, string[] propertyInfos, T entry, IDatabase db)
        {
            return CreateCommandWithParameterValues(typeof(T), query, propertyInfos, entry, db);
        }

        /// <summary>
        /// Wraps <param name="query"></param> into a Command and adds the values
        /// values are added by Index
        /// </summary>
        /// <param name="query"></param>
        /// <param name="db"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public static IDbCommand CreateCommandWithParameterValues(string query, IDatabase db, object[] values)
        {
            var listofQueryParamter = new List<IQueryParameter>();
            for (int i = 0; i < values.Count(); i++)
                listofQueryParamter.Add(new QueryParameter { Name = i.ToString(CultureInfo.InvariantCulture), Value = values[i] });
            return CreateCommandWithParameterValues(query, db, listofQueryParamter);
        }

        /// <summary>
        /// Wraps <param name="query"></param> into a Command and adds the values
        /// values are added by Name of IQueryParamter
        /// If item of <param name="values"></param> contains a name that does not contains @ it will be added
        /// </summary>
        /// <param name="query"></param>
        /// <param name="db"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public static IDbCommand CreateCommandWithParameterValues(string query, IDatabase db, IEnumerable<IQueryParameter> values)
        {
            IDbCommand cmd = CreateCommand(db, query);
            if (values != null)
                foreach (IQueryParameter queryParameter in values)
                {
                    IDbDataParameter dbDataParameter = cmd.CreateParameter();
                    dbDataParameter.Value = queryParameter.Value;
                    dbDataParameter.ParameterName = !queryParameter.Name.StartsWith("@")
                        ? "@" + queryParameter.Name
                        : queryParameter.Name;
                    cmd.Parameters.Add(dbDataParameter);
                }
            return cmd;
        }

        private static IEnumerable<IQueryParameter> EnumarateFromDynamics(dynamic parameter)
        {
            var list = new List<IQueryParameter>();

            PropertyInfo[] propertys = ((Type)parameter.GetType()).GetProperties();

            for (int i = 0; i < propertys.Length; i++)
            {
                PropertyInfo element = propertys[i];
                dynamic value = DataConverterExtensions.GetParamaterValue(parameter, element.Name);
                list.Add(new QueryParameter { Name = "@" + element.Name, Value = value });
            }

            return list;
        }

        /// <summary>
        /// Returns all Propertys that can be loaded due reflection
        /// </summary>
        /// <param name="type"></param>
        /// <param name="ignorePk"></param>
        /// <returns></returns>
        public static string CreatePropertyCSV(Type type, bool ignorePk = false)
        {
            return CreatePropertyNames(type, ignorePk).Aggregate((e, f) => e + ", " + f);
        }

        /// <summary>
        /// Returns all Propertys that can be loaded due reflection
        /// </summary>
        /// <param name="ignorePk"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static string CreatePropertyCSV<T>(bool ignorePk = false)
        {
            return CreatePropertyCSV(typeof(T), ignorePk);
        }

        /// <summary>
        /// Returns all Propertys that can be loaded due reflection and excludes all propertys in ignore
        /// </summary>
        /// <param name="type"></param>
        /// <param name="ignore"></param>
        /// <returns></returns>
        protected static string CreatePropertyCSV(Type type, params string[] ignore)
        {
            IEnumerable<string> propertyNames = CreatePropertyNames(type, ignore);
            return propertyNames.Aggregate((e, f) => e + ", " + f);
        }

        /// <summary>
        /// Returns all Propertys that can be loaded due reflection and excludes all propertys in ignore
        /// </summary>
        /// <param name="ignore"></param>
        /// <returns></returns>
        protected static string CreatePropertyCSV<T>(params string[] ignore)
        {
            return CreatePropertyCSV(typeof(T), ignore);
        }

        /// <summary>
        /// Maps all propertys of <param name="type"></param> into the Db columns
        /// </summary>
        /// <param name="type"></param>
        /// <param name="ignore"></param>
        /// <returns></returns>
        protected static IEnumerable<string> CreatePropertyNames(Type type, params string[] ignore)
        {
            return DataConverterExtensions.MapEntiyToSchema(type, ignore).ToList();
        }

        /// <summary>
        /// Maps all propertys of typeof(T) into the Db columns
        /// </summary>
        /// <param name="ignore"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        protected static IEnumerable<string> CreatePropertyNames<T>(params string[] ignore)
        {
            return CreatePropertyNames(typeof(T), ignore);
        }

        /// <summary>
        /// Maps propertys to database of given type
        /// </summary>
        /// <param name="type"></param>
        /// <param name="ignorePK"></param>
        /// <returns></returns>
        protected static IEnumerable<string> CreatePropertyNames(Type type, bool ignorePK = false)
        {
            return ignorePK ? CreatePropertyNames(type, type.GetPK()) : CreatePropertyNames(type, new string[0]);
        }

        /// <summary>
        /// Maps propertys to database of given type
        /// </summary>
        /// <param name="ignorePK"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        protected static IEnumerable<string> CreatePropertyNames<T>(bool ignorePK = false)
        {
            return ignorePK ? CreatePropertyNames<T>(typeof(T).GetPK()) : CreatePropertyNames<T>(new string[0]);
        }

        /// <summary>
        /// Gets all propertys that should be ignored due rules
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string[] CreateIgnoreList(Type type)
        {
            return
                type.GetProperties()
                    .Where(
                        s =>
                            s.GetGetMethod(false).IsVirtual ||
                            s.GetCustomAttributes().Any(e => e is IgnoreReflectionAttribute))
                    .Select(s => s.Name)
                    .ToArray();
        }
        
        private static IEnumerable<IDataRecord> EnumerateDataRecords(IDatabase database, IDbCommand query)
        {
            return database.Run(
                s =>
                {
                    var records = new List<IDataRecord>();

                    using (IDataReader dr = query.ExecuteReader())
                    {
                        while (dr.Read())
                            records.Add(dr.CreateEgarRecord());
                        dr.Close();
                    }
                    return records;
                });
        }
    }
}