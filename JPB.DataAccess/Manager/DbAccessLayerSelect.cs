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
using JPB.DataAccess.Config;
using JPB.DataAccess.Config.Model;
using JPB.DataAccess.Contacts;
using JPB.DataAccess.Helper;
using JPB.DataAccess.ModelsAnotations;
using JPB.DataAccess.QueryFactory;
using JPB.DataAccess.QueryProvider;
using JPB.DataAccess;

namespace JPB.DataAccess.Manager
{
    public partial class DbAccessLayer
    {
        /// <summary>
        /// If enabled Related structures will be loaded into the source object
        /// </summary>
        public static bool ProcessNavigationPropertys { get; set; }

        #region BasicCommands

        /// <summary>
        ///     Execute select on a database with a standard Where [Primary Key] = <paramref name="pk"></paramref>
        /// </summary>
        /// <param name="type"></param>
        /// <param name="pk"></param>
        /// <returns></returns>
        public object Select(Type type, object pk)
        {
            return Select(type, pk, Database, this.LoadCompleteResultBeforeMapping);
        }

        /// <summary>
        /// Selectes a Entry by its PrimaryKey
        /// Needs to define a PrimaryKey attribute inside <typeparam name="T"></typeparam>
        /// </summary>
        /// <param name="pk"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Select<T>(object pk)
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
        protected static object Select(Type type, object pk, IDatabase db, bool egarLoading)
        {
            return Select(type, db, CreateSelect(type, db, pk), egarLoading).FirstOrDefault();
        }

        /// <summary>
        /// Selectes a Entry by its PrimaryKey
        /// Needs to define a PrimaryKey attribute inside <typeparam name="T"></typeparam>
        /// </summary>
        /// <param name="pk"></param>
        /// <param name="db"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        protected static T Select<T>(object pk, IDatabase db, bool egarLoading)
        {
            //return Select<T>(db, CreateSelect<T>(db, pk)).FirstOrDefault();
            return (T)Select(typeof(T), pk, db, egarLoading);
        }

        /// <summary>
        /// Creates and Executes a Plain select over a <param name="type"></param>
        /// </summary>
        /// <param name="type"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public object[] Select(Type type, params object[] parameter)
        {
            return Select(type, Database, LoadCompleteResultBeforeMapping, parameter);
        }

        /// <summary>
        ///     Uses a Factory method to Generate a new set of T
        ///     When no Factory is found an Reflection based Factory is used
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public T[] Select<T>(params object[] parameter)
        {
            object[] objects = Select(typeof(T), parameter);
            return objects.Cast<T>().ToArray();
        }

        /// <summary>
        /// Creates and Executes a SelectStatement for a given <param name="type"></param> by using the <param name="parameter"></param>
        /// </summary>
        /// <param name="type"></param>
        /// <param name="db"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        protected static object[] Select(Type type, IDatabase db, bool egarLoading, params object[] parameter)
        {
            return Select(type, db, CreateSelectQueryFactory(type.GetClassInfo(), db, parameter), egarLoading);
        }

        /// <summary>
        /// Creates a selectStatement for a given <typeparam name="T"></typeparam>
        /// </summary>
        /// <param name="db"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        protected static T[] Select<T>(IDatabase db, bool egarLoading)
        {
            return Select(typeof(T), db, egarLoading).Cast<T>().ToArray();
        }

        /// <summary>
        /// Creates and Executes a Select Statement for a given <param name="type"></param> by using <param name="command"></param>
        /// </summary>
        /// <param name="type"></param>
        /// <param name="db"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        protected static object[] Select(Type type, IDatabase db, IDbCommand command, bool egarLoading)
        {
            return SelectNative(type, db, command, egarLoading);
        }

        /// <summary>
        /// Creates and Executes a Select Statement for <typeparam name="T"></typeparam> by using <param name="command"></param>
        /// </summary>
        /// <param name="db"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        protected static T[] Select<T>(IDatabase db, IDbCommand command, bool egarLoading)
        {
            return Select(typeof(T), db, command, egarLoading).Cast<T>().ToArray();
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
            return DbAccessLayerHelper.CreateCommand(db, CreateSelectQueryFactory(type.GetClassInfo(), db).CommandText + " " + query);
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
        public static IDbCommand CreateSelect(Type type, IDatabase db, string query, IEnumerable<IQueryParameter> paramenter)
        {
            IDbCommand plainCommand = DbAccessLayerHelper.CreateCommand(db,
                CreateSelectQueryFactory(type.GetClassInfo(), db).CommandText + " " + query);
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

        internal static IDbCommand CreateSelectQueryFactory(ClassInfoCache type, IDatabase db, params object[] parameter)
        {
            //try to get the attribute for static selection
            if (parameter != null && !parameter.Any())
            {
                var staticFactory = type.AttributeInfoCaches.FirstOrDefault(s => s.Attribute is SelectFactoryAttribute);

                if (staticFactory != null)
                {
                    return DbAccessLayerHelper.CreateCommand(db, (staticFactory.Attribute as SelectFactoryAttribute).Query);
                }
            }

            //try to get a Factory mehtod
            //var methods =
            //    type.GetMethods()
            //        .FirstOrDefault(s => s.GetCustomAttributes(false).Any(e => e is TE /*&& (e as TE).DbQuery.HasFlag(dbAccessType)*/));

            var methods =
                type.MethodInfoCaches
                    .Where(s => s.AttributeInfoCaches.Any(e => e.Attribute is SelectFactoryMethodAttribute))
                    .ToArray();

            if (methods.Any())
            {
                var searchMethodWithFittingParams = methods.Where(s =>
                {
                    ParameterInfo[] parameterInfos = s.MethodInfo.GetParameters();

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
                    return DbAccessLayerHelper.CreateCommand(db, CreateSelect(type.Type));
                }

                var method = searchMethodWithFittingParams.First();

                //must be public static
                if (method.MethodInfo.IsStatic)
                {
                    object[] cleanParams = parameter != null && parameter.Any() ? parameter : null;
                    object invoke = method.MethodInfo.Invoke(null, cleanParams);
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
            return DbAccessLayerHelper.CreateCommand(db, CreateSelect(type.Type));
        }

        /// <summary>
        ///  Creates a Select for one Item with appended query and inclueded Query Paramater
        /// </summary>
        /// <param name="type"></param>
        /// <param name="db"></param>
        /// <param name="pk"></param>
        /// <returns></returns>
        public static IDbCommand CreateSelect(Type type, IDatabase db, object pk)
        {
            string proppk = type.GetPK();
            string query = CreateSelectQueryFactory(type.GetClassInfo(), db).CommandText + " WHERE " + proppk + " = @pk";
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
        public static IDbCommand CreateSelect<T>(IDatabase db, object pk)
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
            return CreateSelectQueryFactory(typeof(T).GetClassInfo(), db);
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
        public static IEnumerable RunDynamicSelect(Type type, IDatabase database, IDbCommand query, bool egarLoading)
        {
            RaiseSelect(query, database);
            var typeInfo = type.GetClassInfo();
            return
                database.EnumerateDataRecords(query, egarLoading)
                    .Select(typeInfo.SetPropertysViaReflection)
                    .ToList();
        }

        /// <summary>
        /// Executes a Selectstatement and Parse the Output into <param name="type"></param>
        /// </summary>
        /// <param name="type"></param>
        /// <param name="database"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public static List<object> RunSelect(Type type, IDatabase database, IDbCommand query, bool egarLoading)
        {
            return RunDynamicSelect(type, database, query, egarLoading) as List<object>;
        }

        /// <summary>
        /// Executes a Selectstatement and Parse the Output into <typeparam name="T"></typeparam>
        /// </summary>
        /// <param name="database"></param>
        /// <param name="query"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static List<T> RunSelect<T>(IDatabase database, IDbCommand query, bool egarLoading)
        {
            return RunSelect(typeof(T), database, query, egarLoading).Cast<T>().ToList();
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
            IEnumerable<IQueryParameter> paramenter, bool egarLoading)
        {
            return
                database.Run(
                    s =>
                    {
                        IDbCommand command = DbAccessLayerHelper.CreateCommand(s, query);

                        foreach (IQueryParameter item in paramenter)
                            command.Parameters.AddWithValue(item.Name, item.Value, s);
                        return RunSelect(type, database, command, egarLoading);
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
        public static List<T> RunSelect<T>(IDatabase database, string query, IEnumerable<IQueryParameter> paramenter, bool egarLoading)
        {
            return RunSelect(typeof(T), database, query, paramenter, egarLoading).Cast<T>().ToList();
        }

        private List<object> RunSelect(Type type, IDbCommand command)
        {
            return RunSelect(type, Database, command, LoadCompleteResultBeforeMapping);
        }

        private List<T> RunSelect<T>(IDbCommand command)
        {
            return RunSelect(typeof(T), Database, command, LoadCompleteResultBeforeMapping).Cast<T>().ToList();
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
            //Concret declaration is nessesary because we are working with dynmaics, so the compiler has ne space to guess the type wrong
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
            RaiseSelect(command, Database);
            return Database.EnumerateDataRecords(command, LoadCompleteResultBeforeMapping).Select(s => s[0]).ToList();
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
        public List<T> RunPrimetivSelect<T>(string query, dynamic parameters)
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
        public List<T> RunPrimetivSelect<T>(string query, IEnumerable<IQueryParameter> parameters)
        {
            return RunPrimetivSelect(typeof(T), query, parameters).Cast<T>().ToList();
        }

        /// <summary>
        /// Runs <param name="query"></param> and parses the first line of output into <typeparam name="T"></typeparam>
        /// </summary>
        /// <param name="query"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public List<T> RunPrimetivSelect<T>(string query)
        {
            return RunPrimetivSelect<T>(query, new List<IQueryParameter>());
        }

        /// <summary>
        /// Runs <param name="query"></param> and parses output into <typeparam name="T"></typeparam>
        /// </summary>
        /// <param name="query"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T[] SelectNative<T>(string query) where T : class
        {
            return SelectNative(typeof(T), query).Cast<T>().ToArray();
        }

        /// <summary>
        /// Runs <param name="query"></param> and parses output into <param name="type"></param>
        /// </summary>
        /// <param name="type"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public object[] SelectNative(Type type, string query)
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
        public static object[] SelectNative(Type type, IDatabase database, IDbCommand command, bool multiRow, bool egarLoading)
        {
            if (!multiRow)
                return SelectNative(type, database, command, egarLoading);

            //var guessingRelations = new Dictionary<PropertyInfo, IDbCommand>();

            //var propertyInfos = type.GetFKs();
            //var primaryKeyName = type.GetPK();

            //foreach (var propertyInfo in propertyInfos)
            //{
            //    guessingRelations.Add(propertyInfo, database.CreateCommand(string.Format("JOIN {0} ON {0} = {1}", propertyInfo.DeclaringType.GetTableName(), primaryKeyName)));
            //}

            /*
             * Due the fact that you are not able to anylse the Query in a way to ensure its will not effect the query self we
             * are loading the result an then loading based on that the items             
             */

            return RunSelect(type, database, command, egarLoading).AsParallel().Select(s => s.LoadNavigationProps(database)).ToArray();
        }

        /// <summary>
        /// Runs <param name="command"></param> and parses output into <param name="type"></param>
        /// </summary>
        /// <param name="type"></param>
        /// <param name="database"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        public static object[] SelectNative(Type type, IDatabase database, IDbCommand command, bool egarLoading)
        {
            List<object> objects = RunSelect(type, database, command, egarLoading);

            if (ProcessNavigationPropertys && type.GetClassInfo().HasRelations)
                foreach (var model in objects)
                    model.LoadNavigationProps(database);

            return objects.ToArray();
        }

        /// <summary>
        /// Runs <param name="command"></param> and parses output into <param name="type"></param>
        /// </summary>
        /// <param name="type"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        public object[] SelectNative(Type type, IDbCommand command)
        {
            return SelectNative(type, Database, command, this.LoadCompleteResultBeforeMapping);
        }

        /// <summary>
        /// Runs <param name="query"></param> and parses output into <param name="type"></param>
        /// </summary>
        /// <param name="type"></param>
        /// <param name="query"></param>
        /// <param name="paramenter"></param>
        /// <returns></returns>
        public object[] SelectNative(Type type, string query, IEnumerable<IQueryParameter> paramenter)
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
            return RunSelect<T>(Database, query, paramenter, this.LoadCompleteResultBeforeMapping);
        }

        /// <summary>
        /// Runs <param name="query"></param> and parses output into <param name="type"></param>
        /// </summary>
        /// <param name="type"></param>
        /// <param name="query"></param>
        /// <param name="paramenter"></param>
        /// <returns></returns>
        public object[] SelectNative(Type type, string query, dynamic paramenter)
        {
            IEnumerable<IQueryParameter> enumarateFromDynamics = DbAccessLayerHelper.EnumarateFromDynamics(paramenter);
            return SelectNative(type, query, enumarateFromDynamics);
        }

        /// <summary>
        /// Runs <param name="query"></param> and parses output into <param name="type"></param>
        /// </summary>
        /// <param name="type"></param>
        /// <param name="query"></param>
        /// <param name="paramenter"></param>
        /// <returns></returns>
        public List<object> SelectNative(Type type, IDbCommand command, dynamic paramenter)
        {
            IEnumerable<IQueryParameter> enumarateFromDynamics = DbAccessLayerHelper.EnumarateFromDynamics(paramenter);

            foreach (var enumarateFromDynamic in enumarateFromDynamics)
            {
                command.Parameters.AddWithValue(enumarateFromDynamic.Name, enumarateFromDynamic.Value, Database);
            }

            return RunSelect(type, command);
        }

        /// <summary>
        /// Runs <param name="query"></param> and parses output into <typeparam name="T"></typeparam>
        /// </summary>
        /// <param name="query"></param>
        /// <param name="paramenter"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T[] SelectNative<T>(string query, dynamic paramenter)
        {
            var objects = (object[])SelectNative(typeof(T), query, (dynamic)paramenter);
            return objects.Cast<T>().ToArray();
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

        /// <summary>
        /// Executes a IDbCommand that will return multibe result sets that will be parsed to the marsTypes in order they are provided
        /// </summary>
        /// <param name="bulk"></param>
        /// <param name="marsTypes"></param>
        /// <returns></returns>
        public List<List<object>> ExecuteMARS(IDbCommand bulk, params Type[] marsTypes)
        {
            var mars = Database.EnumerateMarsDataRecords(bulk, true);
            var concatedMarsToType = new List<Tuple<ClassInfoCache, List<IDataRecord>>>();
            for (int index = 0; index < mars.Count; index++)
            {
                var dataRecord = mars[index];
                var expectedResult = marsTypes[index];
                concatedMarsToType.Add(new Tuple<ClassInfoCache, List<IDataRecord>>(expectedResult.GetClassInfo(), dataRecord));
            }
            var list = concatedMarsToType.Select(s => s.Item2.Select(e => s.Item1.SetPropertysViaReflection(e)).AsParallel().ToList()).AsParallel().ToList();
            return list;
        }
    }
}