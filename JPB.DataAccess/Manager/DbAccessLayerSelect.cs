using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using JPB.DataAccess.AdoWrapper;
using JPB.DataAccess.Helper;
using JPB.DataAccess.ModelsAnotations;
using JPB.DataAccess.QueryFactory;
using JPB.DataAccess.QueryProvider;

namespace JPB.DataAccess.Manager
{
    public partial class DbAccessLayer
    {
        #region BasicCommands

        /// <summary>
        ///     Execute select on a database with a standard Where [Primary Key] = <paramref name="pk"></paramref>
        /// </summary>
        /// <param name="type"></param>
        /// <param name="pk"></param>
        /// <returns></returns>
        public object Select(Type type, long pk)
        {
            return Select(type, pk, Database);
        }

        /// <summary>
        /// Selectes a Entry by its PrimaryKey
        /// Needs to define a PrimaryKey attribute inside <typeparam name="T"></typeparam>
        /// </summary>
        /// <param name="pk"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Select<T>(long pk)
        {
            return (T)Select(typeof(T), pk);
        }

        /// <summary> 
        /// Selectes a Entry by its PrimaryKey
        /// Needs to define a PrimaryKey attribute inside <paramref name="type"/>
        /// </summary>
        /// <param name="type"></param>
        /// <param name="pk"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        protected static object Select(Type type, long pk, IDatabase db)
        {
            return Select(type, db, CreateSelect(type, db, pk)).FirstOrDefault();
        }

        /// <summary>
        /// Selectes a Entry by its PrimaryKey
        /// Needs to define a PrimaryKey attribute inside <typeparam name="T"></typeparam>
        /// </summary>
        /// <param name="pk"></param>
        /// <param name="db"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        protected static T Select<T>(long pk, IDatabase db)
        {
            //return Select<T>(db, CreateSelect<T>(db, pk)).FirstOrDefault();
            return (T)Select(typeof(T), pk, db);
        }

        /// <summary>
        /// Creates and Executes a Plain select over a <param name="type"></param>
        /// </summary>
        /// <param name="type"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public List<object> Select(Type type, params object[] parameter)
        {
            return Select(type, Database, parameter);
        }

        /// <summary>
        ///     Uses a Factory method to Generate a new set of T
        ///     When no Factory is found an Reflection based Factory is used
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public List<T> Select<T>(params object[] parameter)
        {
            List<object> objects = Select(typeof(T), parameter);
            return objects.Cast<T>().ToList();
        }

        /// <summary>
        /// Creates and Executes a SelectStatement for a given <param name="type"></param> by using the <param name="parameter"></param>
        /// </summary>
        /// <param name="type"></param>
        /// <param name="db"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        protected static List<object> Select(Type type, IDatabase db, params object[] parameter)
        {
            return Select(type, db, CreateSelectQueryFactory(type, db, parameter));
        }

        /// <summary>
        /// Creates a selectStatement for a given <typeparam name="T"></typeparam>
        /// </summary>
        /// <param name="db"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        protected static List<T> Select<T>(IDatabase db)
        {
            return Select(typeof(T), db).Cast<T>().ToList();
        }

        /// <summary>
        /// Creates and Executes a Select Statement for a given <param name="type"></param> by using <param name="command"></param>
        /// </summary>
        /// <param name="type"></param>
        /// <param name="db"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        protected static List<object> Select(Type type, IDatabase db, IDbCommand command)
        {
            return SelectNative(type, db, command);
        }

        /// <summary>
        /// Creates and Executes a Select Statement for <typeparam name="T"></typeparam> by using <param name="command"></param>
        /// </summary>
        /// <param name="db"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        protected static List<T> Select<T>(IDatabase db, IDbCommand command)
        {
            return Select(typeof(T), db, command).Cast<T>().ToList();
        }

        #endregion

        #region CreateCommands

        /// <summary>
        /// Creates a Select with appended query
        /// </summary>
        /// <param name="type"></param>
        /// <param name="db"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public static IDbCommand CreateSelect(Type type, IDatabase db, string query)
        {
            return DbAccessLayerHelper.CreateCommand(db, CreateSelectQueryFactory(type, db).CommandText + " " + query);
        }


        /// <summary>
        /// Creates a Select with appended query
        /// </summary>
        /// <param name="type"></param>
        /// <param name="db"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public static IDbCommand CreateSelect<T>(IDatabase db, string query)
        {
            return CreateSelect(typeof(T), db, query);
        }

        /// <summary>
        /// Creates a Select with appended query and inclueded Query Paramater
        /// </summary>
        /// <param name="type"></param>
        /// <param name="db"></param>
        /// <param name="query"></param>
        /// <param name="paramenter"></param>
        /// <returns></returns>
        public static IDbCommand CreateSelect(Type type, IDatabase db, string query,
            IEnumerable<IQueryParameter> paramenter)
        {
            IDbCommand plainCommand = DbAccessLayerHelper.CreateCommand(db,
                CreateSelectQueryFactory(type, db).CommandText + " " + query);
            foreach (IQueryParameter para in paramenter)
                plainCommand.Parameters.AddWithValue(para.Name, para.Value, db);
            return plainCommand;
        }

        /// <summary>
        ///  Creates a Select with appended query and inclueded Query Paramater
        /// </summary>
        /// <param name="db"></param>
        /// <param name="query"></param>
        /// <param name="paramenter"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IDbCommand CreateSelect<T>(IDatabase db, string query,
            IEnumerable<IQueryParameter> paramenter)
        {
            return CreateSelect(typeof(T), db, query, paramenter);
        }

        private static IDbCommand CreateSelectQueryFactory(Type type, IDatabase db, params object[] parameter)
        {
            //try to get the attribute for static selection
            if (parameter != null && !parameter.Any())
            {
                var staticFactory = type.GetCustomAttributes().FirstOrDefault(s => s is SelectFactoryAttribute) as SelectFactoryAttribute;

                if (staticFactory != null)
                {
                    return DbAccessLayerHelper.CreateCommand(db, staticFactory.Query);
                }
            }

            //try to get a Factory mehtod
            //var methods =
            //    type.GetMethods()
            //        .FirstOrDefault(s => s.GetCustomAttributes(false).Any(e => e is TE /*&& (e as TE).DbQuery.HasFlag(DbType)*/));

            MethodInfo[] methods =
                type.GetMethods()
                    .Where(s => !s.IsConstructor && !s.IsSpecialName)
                    .Where(s => s.GetCustomAttributes(false).Any(e => e is SelectFactoryMehtodAttribute))
                    .ToArray();

            if (methods.Any())
            {
                MethodInfo[] searchMethodWithFittingParams = methods.Where(s =>
                {
                    ParameterInfo[] parameterInfos = s.GetParameters();

                    if (parameterInfos.Length != parameter.Length)
                    {
                        return false;
                    }

                    for (int i = 0; i < parameterInfos.Length; i++)
                    {
                        ParameterInfo para = parameterInfos[i];
                        object tryParam = parameter[i];
                        if (tryParam == null)
                            return false;
                        if (!(para.ParameterType == tryParam.GetType()))
                        {
                            return false;
                        }
                    }
                    return true;
                }).ToArray();

                if (searchMethodWithFittingParams.Length != 1)
                {
                    return DbAccessLayerHelper.CreateCommand(db, CreateSelect(type));
                }

                MethodInfo method = searchMethodWithFittingParams.First();

                //must be public static
                if (method.IsStatic)
                {
                    object[] cleanParams = parameter != null && parameter.Any() ? parameter : null;
                    object invoke = method.Invoke(null, cleanParams);
                    if (invoke != null)
                    {
                        if (invoke is string && !string.IsNullOrEmpty(invoke as string))
                        {
                            return DbAccessLayerHelper.CreateCommand(db, invoke as string);
                        }
                        if (invoke is IQueryFactoryResult)
                        {
                            var result = invoke as IQueryFactoryResult;
                            return db.CreateCommandWithParameterValues(result.Query, result.Parameters);
                        }
                    }
                }
            }
            return DbAccessLayerHelper.CreateCommand(db, CreateSelect(type));
        }

        /// <summary>
        ///  Creates a Select for one Item with appended query and inclueded Query Paramater
        /// </summary>
        /// <param name="type"></param>
        /// <param name="db"></param>
        /// <param name="pk"></param>
        /// <returns></returns>
        public static IDbCommand CreateSelect(Type type, IDatabase db, long pk)
        {
            string proppk = type.GetPK();
            string query = CreateSelectQueryFactory(type, db).CommandText + " WHERE " + proppk + " = @pk";
            IDbCommand cmd = DbAccessLayerHelper.CreateCommand(db, query);
            cmd.Parameters.AddWithValue("@pk", pk, db);
            return cmd;
        }

        /// <summary>
        ///  Creates a Select for one Item with appended query and inclueded Query Paramater
        /// </summary>
        /// <param name="db"></param>
        /// <param name="pk"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IDbCommand CreateSelect<T>(IDatabase db, long pk)
        {
            return CreateSelect(typeof(T), db, pk);
        }

        /// <summary>
        /// Creates a Plain Select statement by using <param name="type"></param>
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string CreateSelect(Type type)
        {
            var sb = new StringBuilder();
            sb.Append("SELECT ");
            sb.Append(type.CreatePropertyCSV(type.CreateIgnoreList()));
            sb.Append(" FROM ");
            sb.Append(type.GetTableName());
            return sb.ToString();
            //return "SELECT " + type.CreatePropertyCSV(type.CreateIgnoreList()) + " FROM " + type.GetTableName();
        }

        /// <summary>
        /// Creates a Select by using a Factory mehtod or auto generated querys
        /// </summary>
        /// <param name="db"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IDbCommand CreateSelect<T>(IDatabase db)
        {
            return CreateSelectQueryFactory(typeof(T), db);
        }

        #endregion

        #region Runs

        /// <summary>
        /// Executes a Selectstatement and Parse the Output into <param name="type"></param>
        /// </summary>
        /// <param name="type"></param>
        /// <param name="database"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public static IEnumerable RunDynamicSelect(Type type, IDatabase database, IDbCommand query)
        {
            RaiseUnknownSelect(query);
            return
                DbAccessLayerHelper.EnumerateDataRecords(database, query)
                    .Select(dataRecord => DataConverterExtensions.SetPropertysViaReflection(type, dataRecord))
                    .ToList();
        }

        /// <summary>
        /// Executes a Selectstatement and Parse the Output into <param name="type"></param>
        /// </summary>
        /// <param name="type"></param>
        /// <param name="database"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public static List<object> RunSelect(Type type, IDatabase database, IDbCommand query)
        {
            return RunDynamicSelect(type, database, query) as List<object>;
        }

        /// <summary>
        /// Executes a Selectstatement and Parse the Output into <typeparam name="T"></typeparam>
        /// </summary>
        /// <param name="database"></param>
        /// <param name="query"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static List<T> RunSelect<T>(IDatabase database, IDbCommand query)
        {
            return RunSelect(typeof(T), database, query).Cast<T>().ToList();
        }

        /// <summary>
        /// Executes <param name="query"></param> and Parse the Output into <param name="type"></param>
        /// </summary>
        /// <param name="type"></param>
        /// <param name="database"></param>
        /// <param name="query"></param>
        /// <param name="paramenter"></param>
        /// <returns></returns>
        public static List<object> RunSelect(Type type, IDatabase database, string query,
            IEnumerable<IQueryParameter> paramenter)
        {
            return
                database.Run(
                    s =>
                    {
                        IDbCommand command = DbAccessLayerHelper.CreateCommand(s, query);

                        foreach (IQueryParameter item in paramenter)
                            command.Parameters.AddWithValue(item.Name, item.Value, s);
                        return RunSelect(type, database, command);
                    }
                    );
        }

        /// <summary>
        /// Executes <param name="query"></param> and Parse the Output into <typeparam name="T"></typeparam>
        /// </summary>
        /// <param name="database"></param>
        /// <param name="query"></param>
        /// <param name="paramenter"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static List<T> RunSelect<T>(IDatabase database, string query, IEnumerable<IQueryParameter> paramenter)
        {
            return RunSelect(typeof(T), database, query, paramenter).Cast<T>().ToList();
        }

        private List<object> RunSelect(Type type, IDbCommand command)
        {
            return RunSelect(type, Database, command);
        }

        private List<T> RunSelect<T>(IDbCommand command)
        {
            return RunSelect(typeof(T), Database, command).Cast<T>().ToList();
        }

        #endregion

        #region SelectWhereCommands

        /// <summary>
        /// Executes a Select Statement and adds <param name="where"></param>
        /// </summary>
        /// <param name="type"></param>
        /// <param name="where"></param>
        /// <returns></returns>
        public List<object> SelectWhere(Type type, String @where)
        {
            IDbCommand query = CreateSelect(type, Database, @where);
            return RunSelect(type, query);
        }

        /// <summary>
        /// Executes a Select Statement and adds <param name="where"></param>
        /// </summary>
        /// <param name="where"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public List<T> SelectWhere<T>(String @where)
        {
            return SelectWhere(typeof(T), @where).Cast<T>().ToList();
        }

        /// <summary>
        /// Executes a Select Statement and adds <param name="where"></param>
        /// uses <param name="paramenter"></param>
        /// </summary>
        /// <param name="type"></param>
        /// <param name="where"></param>
        /// <param name="paramenter"></param>
        /// <returns></returns>
        public List<object> SelectWhere(Type type, String @where, IEnumerable<IQueryParameter> paramenter)
        {
            IDbCommand query = CreateSelect(type, Database, @where, paramenter);
            return RunSelect(type, query);
        }

        /// <summary>
        /// Executes a Select Statement and adds <param name="where"></param>
        /// uses <param name="paramenter"></param>
        /// </summary>
        /// <param name="where"></param>
        /// <param name="paramenter"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public List<T> SelectWhere<T>(String @where, IEnumerable<IQueryParameter> paramenter)
        {
            return SelectWhere(typeof(T), where, paramenter).Cast<T>().ToList();
        }

        /// <summary>
        /// Executes a Select Statement and adds <param name="where"></param>
        /// uses <param name="paramenter"></param>
        /// </summary>
        /// <param name="type"></param>
        /// <param name="where"></param>
        /// <param name="paramenter"></param>
        /// <returns></returns>
        public List<object> SelectWhere(Type type, String @where, dynamic paramenter)
        {
            IEnumerable<IQueryParameter> enumarateFromDynamics = DbAccessLayerHelper.EnumarateFromDynamics(paramenter);
            return SelectWhere(type, where, enumarateFromDynamics);
        }

        /// <summary>
        /// Executes a Select Statement and adds <param name="where"></param>
        /// uses <param name="paramenter"></param>
        /// </summary>
        /// <param name="where"></param>
        /// <param name="paramenter"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public List<T> SelectWhere<T>(String @where, dynamic paramenter)
        {
            List<object> selectWhere = SelectWhere(typeof(T), @where, paramenter);
            return selectWhere.Cast<T>().ToList();
        }

        #endregion

        #region PrimetivSelects

        /// <summary>
        /// Runs <param name="command"></param> and parses the first line of output into <param name="type"></param>
        /// </summary>
        /// <param name="type"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        public IEnumerable<object> RunPrimetivSelect(Type type, IDbCommand command)
        {
            RaiseKnownSelect(command);
            return DbAccessLayerHelper.EnumerateDataRecords(Database, command).Select(s => s[0]).ToList();
        }

        /// <summary>
        /// Runs <param name="query"></param> and parses the first line of output into <param name="type"></param>
        /// </summary>
        /// <param name="type"></param>
        /// <param name="query"></param>
        /// <param name="paramerter"></param>
        /// <returns></returns>
        public IEnumerable<object> RunPrimetivSelect(Type type, string query, IEnumerable<IQueryParameter> paramerter)
        {
            return RunPrimetivSelect(type, Database.CreateCommandWithParameterValues(query, paramerter));
        }

        /// <summary>
        /// Runs <param name="query"></param> and parses the first line of output into <param name="type"></param>
        /// </summary>
        /// <param name="type"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public IEnumerable<object> RunPrimetivSelect(Type type, string query)
        {
            return RunPrimetivSelect(type, query, new List<IQueryParameter>());
        }

        /// <summary>
        /// Runs <param name="query"></param> and parses the first line of output into <typeparam name="T"></typeparam>
        /// </summary>
        /// <param name="query"></param>
        /// <param name="parameters"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public List<T> RunPrimetivSelect<T>(string query, dynamic parameters) where T : class
        {
            IEnumerable<IQueryParameter> enumarateFromDynamics = DbAccessLayerHelper.EnumarateFromDynamics(parameters);
            var runPrimetivSelect = RunPrimetivSelect(typeof(T), query, enumarateFromDynamics);
            return runPrimetivSelect.Cast<T>().ToList();
        }

        /// <summary>
        /// Runs <param name="query"></param> and parses the first line of output into <typeparam name="T"></typeparam>
        /// </summary>
        /// <param name="query"></param>
        /// <param name="parameters"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public List<T> RunPrimetivSelect<T>(string query, IEnumerable<IQueryParameter> parameters) where T : class
        {
            return RunPrimetivSelect(typeof(T), query, parameters).Cast<T>().ToList();
        }

        /// <summary>
        /// Runs <param name="query"></param> and parses the first line of output into <typeparam name="T"></typeparam>
        /// </summary>
        /// <param name="query"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public List<T> RunPrimetivSelect<T>(string query) where T : class
        {
            return RunPrimetivSelect<T>(query, new List<IQueryParameter>());
        }

        /// <summary>
        /// Runs <param name="query"></param> and parses output into <typeparam name="T"></typeparam>
        /// </summary>
        /// <param name="query"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public List<T> SelectNative<T>(string query) where T : class
        {
            return SelectNative(typeof(T), query).Cast<T>().ToList();
        }

        /// <summary>
        /// Runs <param name="query"></param> and parses output into <param name="type"></param>
        /// </summary>
        /// <param name="type"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public List<object> SelectNative(Type type, string query)
        {
            return Select(type, Database, DbAccessLayerHelper.CreateCommand(Database, query));
        }


        /// <summary>
        /// Runs <param name="command"></param> and parses output into <param name="type"></param>
        /// </summary>
        /// <param name="type"></param>
        /// <param name="database"></param>
        /// <param name="command"></param>
        /// <param name="multiRow"></param>
        /// <returns></returns>
        public static List<object> SelectNative(Type type, IDatabase database, IDbCommand command, bool multiRow)
        {
            if (!multiRow)
                return SelectNative(type, database, command);

            var guessingRelations = new List<string>();

            var propertyInfos = type.GetFKs();

            foreach (var propertyInfo in propertyInfos)
            {
                CreateSelect(propertyInfo.PropertyType, database, "");
            }

            var objects = RunSelect(type, database, command);

            foreach (object model in objects)
                model.LoadNavigationProps(database);

            return objects;
        }

        /// <summary>
        /// Runs <param name="command"></param> and parses output into <param name="type"></param>
        /// </summary>
        /// <param name="type"></param>
        /// <param name="database"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        public static List<object> SelectNative(Type type, IDatabase database, IDbCommand command)
        {
            List<object> objects = RunSelect(type, database, command);

            foreach (object model in objects)
                model.LoadNavigationProps(database);

            return objects;
        }

        /// <summary>
        /// Runs <param name="command"></param> and parses output into <param name="type"></param>
        /// </summary>
        /// <param name="type"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        public List<object> SelectNative(Type type, IDbCommand command)
        {
            return SelectNative(type, Database, command);
        }

        /// <summary>
        /// Runs <param name="query"></param> and parses output into <param name="type"></param>
        /// </summary>
        /// <param name="type"></param>
        /// <param name="query"></param>
        /// <param name="paramenter"></param>
        /// <returns></returns>
        public List<object> SelectNative(Type type, string query, IEnumerable<IQueryParameter> paramenter)
        {
            IDbCommand dbCommand = Database.CreateCommandWithParameterValues(query, paramenter);
            return SelectNative(type, dbCommand);
        }

        /// <summary>
        /// Runs <param name="query"></param> and parses output into <typeparam name="T"></typeparam>
        /// </summary>
        /// <param name="query"></param>
        /// <param name="paramenter"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public List<T> SelectNative<T>(string query, IEnumerable<IQueryParameter> paramenter)
        {
            return RunSelect<T>(Database, query, paramenter);
        }

        /// <summary>
        /// Runs <param name="query"></param> and parses output into <param name="type"></param>
        /// </summary>
        /// <param name="type"></param>
        /// <param name="query"></param>
        /// <param name="paramenter"></param>
        /// <returns></returns>
        public List<object> SelectNative(Type type, string query, dynamic paramenter)
        {
            IEnumerable<IQueryParameter> enumarateFromDynamics = DbAccessLayerHelper.EnumarateFromDynamics(paramenter);
            return SelectNative(type, query, enumarateFromDynamics);
        }

        /// <summary>
        /// Runs <param name="query"></param> and parses output into <typeparam name="T"></typeparam>
        /// </summary>
        /// <param name="query"></param>
        /// <param name="paramenter"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public List<T> SelectNative<T>(string query, dynamic paramenter)
        {
            var objects = ((List<object>)SelectNative(typeof(T), query, paramenter));
            return objects.Cast<T>().ToList();
        }

        #endregion

        #region experimental

        private TestQueryProvider _testQueryProvider;

        private void SelectDbAccessLayer()
        {
            _testQueryProvider = new TestQueryProvider(this);
        }

        [Obsolete("Not implimented", true)]
        public IQueryable<T> SelectQuery<T>()
        {
            MethodInfo makeGenericMethod = ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(T));
            MethodCallExpression methodCallExpression = Expression.Call(Expression.Constant(this), makeGenericMethod);
            return _testQueryProvider.CreateQuery<T>(methodCallExpression);
        }

        #endregion
    }
}