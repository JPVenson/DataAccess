using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using JPB.DataAccess.AdoWrapper;
using JPB.DataAccess.Helper;
using JPB.DataAccess.ModelsAnotations;
using JPB.DataAccess.QueryFactory;

namespace JPB.DataAccess.Manager
{
    public class PreDefinedProviderCollection : IReadOnlyCollection<KeyValuePair<DbTypes, string>>
    {
        private readonly Dictionary<DbTypes, string> _preDefinedProvider = new Dictionary<DbTypes, string>
        {
            {DbTypes.MsSql, "JPB.DataAccess.MsSql.MsSql"},
            {DbTypes.OleDb, "JPB.DataAccess.OleDB.OleDb"},
            {DbTypes.Obdc, "JPB.DataAccess.Obdc.Obdc"},
            {DbTypes.MySql, "JPB.DataAccess.MySql.MySql"},
            {DbTypes.SqLite, "JPB.DataAccess.SqlLite.SqLite"},
        };

        public IEnumerator<KeyValuePair<DbTypes, string>> GetEnumerator()
        {
            return _preDefinedProvider.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count
        {
            get { return _preDefinedProvider.Count; }
        }
    }

    [DebuggerDisplay("DB={Database}")]
#if !DEBUG
        [DebuggerStepThrough]
#endif
    public partial class DbAccessLayer
    {
        private IDatabase _database;

        public PreDefinedProviderCollection ProviderCollection { get; set; }

        /// <summary>
        ///     Must set the Database Property immeditly
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
                throw new InvalidEnumArgumentException("dbType", (int) DbTypes.Unknown, typeof (DbTypes));
            }

            Database.Attach(GenerateStrategy(ProviderCollection.FirstOrDefault(s => s.Key == dbType).Value, connection));
        }

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
                ProviderCollection.Select(s => (KeyValuePair<DbTypes, string>?) s)
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

            if (!type.GetInterfaces().Contains(typeof (IDatabaseStrategy)))
            {
                throw new ArgumentException("Type was found but does not inhert from IDatabaseStrategy");
            }

            //try constructor injection
            ConstructorInfo ctOfType =
                type.GetConstructors()
                    .FirstOrDefault(
                        s => s.GetParameters().Length == 1 && s.GetParameters().First().ParameterType == typeof (string));
            if (ctOfType != null)
            {
                return ctOfType.Invoke(new object[] {connection}) as IDatabaseStrategy;
            }
            var instanceOfType = Activator.CreateInstance(type) as IDatabaseStrategy;
            if (instanceOfType == null)
                throw new ArgumentException("Type was found but does not inhert from IDatabaseStrategy");

            instanceOfType.ConnectionString = connection;

            return instanceOfType;
        }


        public DbTypes DbType { get; private set; }

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

        public bool CheckDatabase()
        {
            if (Database == null)
                return false;
            Database.Connect(false);
            Database.CloseConnection();
            return true;
        }

        private static IDbCommand CheckInstanceForAttriute<T, TE>(T entry, IDatabase db,
            Func<T, IDatabase, IDbCommand> fallback, params object[] param)
            where TE : DataAccessAttribute
        {
            Type type = entry.GetType();

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

        public int ExecuteGenericCommand(string query, IEnumerable<IQueryParameter> values)
        {
            IDbCommand command = CreateCommand(Database, query);

            foreach (IQueryParameter item in values)
                command.Parameters.AddWithValue(item.Name, item.Value, Database);

            return Database.Run(s => s.ExecuteNonQuery(command));
        }

        public int ExecuteGenericCommand(string query, dynamic paramenter)
        {
            return ExecuteGenericCommand(query, (IEnumerable<IQueryParameter>) EnumarateFromDynamics(paramenter));
        }

        public int ExecuteGenericCommand(IDbCommand command)
        {
            return Database.Run(s => s.ExecuteNonQuery(command));
        }

        public static int ExecuteGenericCommand(IDbCommand command, IDatabase db)
        {
            return db.Run(s => s.ExecuteNonQuery(command));
        }

        public static List<T> ExecuteGenericCreateModelsCommand<T>(IDbCommand command, IDatabase db)
            where T : class, new()
        {
            return db.Run(
                s =>
                    s.GetEntitiesList(command, DataConverterExtensions.SetPropertysViaRefection<T>)
                        .ToList());
        }

        public static object GetDataValue(object value)
        {
            return value ?? DBNull.Value;
        }

        protected static IDbCommand CreateCommand(IDatabase db, string query)
        {
            return db.CreateCommand(query);
        }

        public static IDbCommand CreateCommandWithParameterValues<T>(string query, string[] propertyInfos, T entry,
            IDatabase db)
        {
            Type type = typeof (T);
            object[] propertyvalues =
                propertyInfos.Select(
                    propertyInfo =>
                    {
                        PropertyInfo property =
                            type.GetProperty(DataConverterExtensions.ReMapSchemaToEntiysProp<T>(propertyInfo));
                        object dataValue = GetDataValue(property.GetValue(entry, null));
                        return dataValue;
                    }).ToArray();
            return CreateCommandWithParameterValues(query, db, propertyvalues);
        }

        public static IDbCommand CreateCommandWithParameterValues(string query, IDatabase db,
            object[] values)
        {
            var listofQueryParamter = new List<IQueryParameter>();
            for (int i = 0; i < values.Count(); i++)
                listofQueryParamter.Add(new QueryParameter {Name = i.ToString(), Value = values[i]});
            return CreateCommandWithParameterValues(query, db, listofQueryParamter);
        }

        public static IDbCommand CreateCommandWithParameterValues(string query, IDatabase db,
            IEnumerable<IQueryParameter> values)
        {
            IDbCommand cmd = CreateCommand(db, query);
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

            PropertyInfo[] propertys = ((Type) parameter.GetType()).GetProperties();

            for (int i = 0; i < propertys.Length; i++)
            {
                PropertyInfo element = propertys[i];
                dynamic value = DataConverterExtensions.GetParamaterValue(parameter, element.Name);
                list.Add(new QueryParameter {Name = "@" + element.Name, Value = value});
            }

            return list;
        }

        protected static string CreatePropertyCSV(Type type, bool ignorePK = false)
        {
            return CreatePropertyNames(type, ignorePK).Aggregate((e, f) => e + ", " + f);
        }

        protected static string CreatePropertyCSV<T>(bool ignorePK = false)
        {
            return CreatePropertyCSV(typeof (T), ignorePK);
        }

        protected static string CreatePropertyCSV(Type type, params string[] ignore)
        {
            IEnumerable<string> propertyNames = CreatePropertyNames(type, ignore);
            return propertyNames.Aggregate((e, f) => e + ", " + f);
        }

        protected static string CreatePropertyCSV<T>(params string[] ignore)
        {
            return CreatePropertyCSV(typeof (T), ignore);
        }

        protected static IEnumerable<string> CreatePropertyNames(Type type, params string[] ignore)
        {
            return DataConverterExtensions.MapEntiyToSchema(type, ignore).ToList();
        }

        protected static IEnumerable<string> CreatePropertyNames<T>(params string[] ignore)
        {
            return CreatePropertyNames(typeof (T), ignore);
        }

        protected static IEnumerable<string> CreatePropertyNames(Type type, bool ignorePK = false)
        {
            return ignorePK ? CreatePropertyNames(type, type.GetPK()) : CreatePropertyNames(type, new string[0]);
        }

        protected static IEnumerable<string> CreatePropertyNames<T>(bool ignorePK = false)
        {
            return ignorePK ? CreatePropertyNames<T>(typeof (T).GetPK()) : CreatePropertyNames<T>(new string[0]);
        }
    }
}