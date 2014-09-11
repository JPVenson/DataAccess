using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using JPB.DataAccess.AdoWrapper;
using JPB.DataAccess.Helper;
using JPB.DataAccess.ModelsAnotations;
using JPB.DataAccess.QueryFactory;

namespace JPB.DataAccess.Manager
{
    [DebuggerDisplay("DB={Database}")]
#if !DEBUG
        [DebuggerStepThrough]
#endif
    public partial class DbAccessLayer
    {
        private IDatabase _database;
        
        /// <summary>
        /// Must set the Database Property immeditly
        /// Just for Intigration
        /// </summary>
        [Obsolete("Will maybe removed in future", false)]
        public DbAccessLayer()
        {

        }

        public DbAccessLayer(DbTypes dbType, string connection)
        {
            DbType = dbType;
            Database = new Database();

            if (dbType == DbTypes.Unknown)
            {
                throw new InvalidEnumArgumentException("dbType", (int)DbTypes.Unknown, typeof(DbTypes));
            }

            switch (dbType)
            {
                case DbTypes.MsSql:
                    Database.Attach(new DsMSSQL(connection));
                    break;
                case DbTypes.MySql:
                    Database.Attach(new Mysql(connection));
                    break;
            }
        }

        public DbAccessLayer(IDatabase database)
        {
            if (database == null)
                throw new ArgumentNullException("database");
            SelectDbAccessLayer();
            UpdateDbAccessLayer();

            DbType = DbTypes.Unknown;
            Database = database;
        }

        public DbTypes DbType { get; private set; }

        public IDatabase Database
        {
            get { return _database; }
            set
            {
                if(_database == null)
                _database = value;
                else
                {
                    throw new NotSupportedException("Runtime change of Database is not allowed. Create a new DbAccessLayer object");
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

        private static IDbCommand CheckInstanceForAttriute<T,TE>(T entry, IDatabase db, Func<T, IDatabase, IDbCommand> fallback)
            where TE : DataAccessAttribute
        {
            var type = entry.GetType();

            //try to get a Factory mehtod
            var methods =
                type.GetMethods()
                    .FirstOrDefault(s => s.GetCustomAttributes(false).Any(e => e is TE));
            if (methods != null)
            {
                //must be public static
                if (!methods.IsStatic)
                {
                    var invoke = methods.Invoke(entry, null);
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
            //screw that. Generate a select self!
            return createUpdate(entry, db);
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
            return ExecuteGenericCommand(query, (IEnumerable<IQueryParameter>)EnumarateFromDynamics(paramenter));
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
            Type type = typeof(T);
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
                listofQueryParamter.Add(new QueryParameter { Name = i.ToString(), Value = values[i] });
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

            PropertyInfo[] propertys = ((Type)parameter.GetType()).GetProperties();

            for (int i = 0; i < propertys.Length; i++)
            {
                PropertyInfo element = propertys[i];
                dynamic value = DataConverterExtensions.GetParamaterValue(parameter, element.Name);
                list.Add(new QueryParameter { Name = "@" + element.Name, Value = value });
            }

            return list;
        }

        protected static string CreatePropertyCSV(Type type, bool ignorePK = false)
        {
            return CreatePropertyNames(type, ignorePK).Aggregate((e, f) => e + ", " + f);
        }

        protected static string CreatePropertyCSV<T>(bool ignorePK = false)
        {
            return CreatePropertyCSV(typeof(T), ignorePK);
        }

        protected static string CreatePropertyCSV(Type type, params string[] ignore)
        {
            IEnumerable<string> propertyNames = CreatePropertyNames(type, ignore);
            return propertyNames.Aggregate((e, f) => e + ", " + f);
        }

        protected static string CreatePropertyCSV<T>(params string[] ignore)
        {
            return CreatePropertyCSV(typeof(T), ignore);
        }

        protected static IEnumerable<string> CreatePropertyNames(Type type, params string[] ignore)
        {
            return DataConverterExtensions.MapEntiyToSchema(type, ignore).ToList();
        }

        protected static IEnumerable<string> CreatePropertyNames<T>(params string[] ignore)
        {
            return CreatePropertyNames(typeof(T), ignore);
        }

        protected static IEnumerable<string> CreatePropertyNames(Type type, bool ignorePK = false)
        {
            return ignorePK ? CreatePropertyNames(type, type.GetPK()) : CreatePropertyNames(type, new string[0]);
        }

        protected static IEnumerable<string> CreatePropertyNames<T>(bool ignorePK = false)
        {
            return ignorePK ? CreatePropertyNames<T>(typeof(T).GetPK()) : CreatePropertyNames<T>(new string[0]);
        }
    }
}